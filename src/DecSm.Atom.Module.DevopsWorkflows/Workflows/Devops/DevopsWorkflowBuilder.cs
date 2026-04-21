namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops;

internal sealed partial class DevopsWorkflowBuilder(
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    IParamService paramService,
    IAtomFileSystem atomFileSystem,
    AtomProjectData atomProjectData,
    IWorkflowExpressionResolver expressionResolver,
    ILogger<DevopsWorkflowBuilder> logger
)
{
    public Pipeline Build(WorkflowModel workflow) =>
        new Pipeline.PipelineWithJobs
        {
            Name = WorkflowExpressions.Raw(workflow.Name),
            Jobs = workflow
                .Jobs
                .Select(x => BuildJob(workflow, x))
                .ToList(),
            Trigger = BuildTrigger(workflow),
            Pr = BuildPr(workflow),
            Parameters = BuildParameters(workflow),
            Variables = BuildVariables(workflow),
        };

    private static Trigger? BuildTrigger(WorkflowModel workflow)
    {
        var pushTriggers = workflow
            .Triggers
            .OfType<GitPushTrigger>()
            .ToArray();

        if (pushTriggers.Length is 0)
            return new Trigger.None();

        // Combine all push triggers into a single full trigger
        var includedBranches = pushTriggers
            .SelectMany(t => t.IncludedBranches)
            .ToArray();

        var excludedBranches = pushTriggers
            .SelectMany(t => t.ExcludedBranches)
            .ToArray();

        var includedPaths = pushTriggers
            .SelectMany(t => t.IncludedPaths)
            .ToArray();

        var excludedPaths = pushTriggers
            .SelectMany(t => t.ExcludedPaths)
            .ToArray();

        var includedTags = pushTriggers
            .SelectMany(t => t.IncludedTags)
            .ToArray();

        var excludedTags = pushTriggers
            .SelectMany(t => t.ExcludedTags)
            .ToArray();

        var hasBranches = includedBranches.Length > 0 || excludedBranches.Length > 0;
        var hasPaths = includedPaths.Length > 0 || excludedPaths.Length > 0;
        var hasTags = includedTags.Length > 0 || excludedTags.Length > 0;

        if (!hasBranches && !hasPaths && !hasTags)
        {
            if (includedBranches.Length > 0)
                return new Trigger.BranchList
                {
                    Branches = new(includedBranches.Select(WorkflowExpressions.Raw)),
                };

            return null;
        }

        return new Trigger.Full
        {
            Branches = hasBranches
                ? new IncludeExcludeFilters
                {
                    Include = includedBranches.Length > 0
                        ? new WorkflowExpressionCollection(includedBranches.Select(WorkflowExpressions.Raw))
                        : null,
                    Exclude = excludedBranches.Length > 0
                        ? new WorkflowExpressionCollection(excludedBranches.Select(WorkflowExpressions.Raw))
                        : null,
                }
                : null,
            Paths = hasPaths
                ? new IncludeExcludeFilters
                {
                    Include = includedPaths.Length > 0
                        ? new WorkflowExpressionCollection(includedPaths.Select(WorkflowExpressions.Raw))
                        : null,
                    Exclude = excludedPaths.Length > 0
                        ? new WorkflowExpressionCollection(excludedPaths.Select(WorkflowExpressions.Raw))
                        : null,
                }
                : null,
            Tags = hasTags
                ? new IncludeExcludeFilters
                {
                    Include = includedTags.Length > 0
                        ? new WorkflowExpressionCollection(includedTags.Select(WorkflowExpressions.Raw))
                        : null,
                    Exclude = excludedTags.Length > 0
                        ? new WorkflowExpressionCollection(excludedTags.Select(WorkflowExpressions.Raw))
                        : null,
                }
                : null,
        };
    }

    private static Pr? BuildPr(WorkflowModel workflow)
    {
        var prTriggers = workflow
            .Triggers
            .OfType<GitPullRequestTrigger>()
            .ToArray();

        if (prTriggers.Length is 0)
            return null;

        var includedBranches = prTriggers
            .SelectMany(t => t.IncludedBranches)
            .ToArray();

        var excludedBranches = prTriggers
            .SelectMany(t => t.ExcludedBranches)
            .ToArray();

        var includedPaths = prTriggers
            .SelectMany(t => t.IncludedPaths)
            .ToArray();

        var excludedPaths = prTriggers
            .SelectMany(t => t.ExcludedPaths)
            .ToArray();

        var hasBranches = includedBranches.Length > 0 || excludedBranches.Length > 0;
        var hasPaths = includedPaths.Length > 0 || excludedPaths.Length > 0;

        if (!hasBranches && !hasPaths)
            return null;

        if (!hasPaths && includedBranches.Length > 0 && excludedBranches.Length is 0)
            return new Pr.BranchList
            {
                Branches = new(includedBranches.Select(WorkflowExpressions.Raw)),
            };

        return new Pr.Full
        {
            Branches = hasBranches
                ? new IncludeExcludeFilters
                {
                    Include = includedBranches.Length > 0
                        ? new WorkflowExpressionCollection(includedBranches.Select(WorkflowExpressions.Raw))
                        : null,
                    Exclude = excludedBranches.Length > 0
                        ? new WorkflowExpressionCollection(excludedBranches.Select(WorkflowExpressions.Raw))
                        : null,
                }
                : null,
            Paths = hasPaths
                ? new IncludeExcludeFilters
                {
                    Include = includedPaths.Length > 0
                        ? new WorkflowExpressionCollection(includedPaths.Select(WorkflowExpressions.Raw))
                        : null,
                    Exclude = excludedPaths.Length > 0
                        ? new WorkflowExpressionCollection(excludedPaths.Select(WorkflowExpressions.Raw))
                        : null,
                }
                : null,
        };
    }

    private List<Parameter>? BuildParameters(WorkflowModel workflow)
    {
        var manualTrigger = workflow
            .Triggers
            .OfType<ManualTrigger>()
            .FirstOrDefault();

        if (manualTrigger?.Inputs is not { Count: > 0 })
            return null;

        return manualTrigger
            .Inputs
            .Select<ManualInput, Parameter>(input =>
            {
                var inputParamName = buildDefinition.ParamDefinitions.FirstOrDefault(x => x.Value.ArgName == input.Name)
                    .Key;

                if (inputParamName is null)
                    throw new InvalidOperationException(
                        $"Workflow {workflow.Name} has a manual trigger input named {input.Name} that does not correspond to any parameter in the build definition");

                switch (input)
                {
                    case ManualBoolInput boolInput:
                    {
                        bool? defaultBoolValue = null;

                        if (boolInput.DefaultValue.HasValue)
                            defaultBoolValue = boolInput.DefaultValue.Value;
                        else
                            using (paramService.CreateDefaultValuesOnlyScope())
                                switch (buildDefinition.AccessParam(inputParamName))
                                {
                                    case bool boolParam:
                                        defaultBoolValue = boolParam;

                                        break;
                                    case string stringParam when bool.TryParse(stringParam, out var parsedBool):
                                        defaultBoolValue = parsedBool;

                                        break;
                                }

                        return new()
                        {
                            Name = WorkflowExpressions.Raw(input.Name),
                            DisplayName = WorkflowExpressions.Raw($"{input.Name} | {input.Description}"),
                            Type = WorkflowExpressions.Raw("boolean"),
                            Default = defaultBoolValue is not null
                                ? WorkflowExpressions.Raw(defaultBoolValue.Value
                                    ? "true"
                                    : "false")
                                : null,
                        };
                    }

                    case ManualChoiceInput choiceInput:
                    {
                        using (paramService.CreateDefaultValuesOnlyScope())
                        {
                            var defaultChoiceValue = choiceInput.DefaultValue is { Length: > 0 }
                                ? choiceInput.DefaultValue
                                : buildDefinition
                                    .AccessParam(inputParamName)
                                    ?.ToString();

                            return new()
                            {
                                Name = WorkflowExpressions.Raw(input.Name),
                                DisplayName = WorkflowExpressions.Raw($"{input.Name} | {input.Description}"),
                                Type = WorkflowExpressions.Raw("string"),
                                Default = defaultChoiceValue is { Length: > 0 }
                                    ? WorkflowExpressions.Raw(defaultChoiceValue)
                                    : null,
                                Values = new(choiceInput.Choices.Select(c =>
                                    (WorkflowExpression)WorkflowExpressions.Raw(c))),
                            };
                        }
                    }

                    case ManualStringInput stringInput:
                    {
                        using (paramService.CreateDefaultValuesOnlyScope())
                        {
                            var defaultStringValue = stringInput.DefaultValue is { Length: > 0 }
                                ? stringInput.DefaultValue
                                : buildDefinition
                                    .AccessParam(inputParamName)
                                    ?.ToString();

                            return new()
                            {
                                Name = WorkflowExpressions.Raw(input.Name),
                                DisplayName = WorkflowExpressions.Raw($"{input.Name} | {input.Description}"),
                                Type = WorkflowExpressions.Raw("string"),
                                Default = defaultStringValue is { Length: > 0 }
                                    ? WorkflowExpressions.Raw(defaultStringValue)
                                    : null,
                            };
                        }
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(input));
                }
            })
            .ToList();
    }

    private static Variables.VariableList? BuildVariables(WorkflowModel workflow)
    {
        var variableGroups = workflow
            .Options
            .OfType<DevopsVariableGroup>()
            .ToArray();

        if (variableGroups.Length is 0)
            return null;

        return new()
        {
            Values = variableGroups
                .Select(g => (Variable)new Variable.Group
                {
                    GroupName = WorkflowExpressions.Raw(g.Name),
                })
                .ToList(),
        };
    }

    private Job BuildJob(WorkflowModel workflow, WorkflowJobModel job)
    {
        var poolOption = job
            .TargetStep
            .Options
            .Concat(workflow.Options)
            .OfType<DevopsPool>()
            .FirstOrDefault();

        var pool = BuildPool(poolOption);

        var condition = TargetCondition
            .GetOptions(workflow, job.TargetStep)
            .ToList();

        var conditionExpression = condition switch
        {
            { Count: > 1 } options => options[0]
                .Value
                .And(options
                    .Skip(1)
                    .Select(x => x.Value)
                    .ToArray()),
            { Count: 1 } option => option[0].Value,
            _ => null,
        };

        var environment = DeployToEnvironment.GetOption(workflow, job.TargetStep);

        if (environment is not null)
            return new Job.Deployment
            {
                DeploymentId = WorkflowExpressions.Raw(job.Name),
                DisplayName = WorkflowExpressions.Raw(job.Name),
                DependsOn = job.JobDependencies.Count > 0
                    ? job.JobDependencies
                    : null,
                Condition = conditionExpression,
                Pool = pool,
                Environment = new DeploymentEnvironment.EnvironmentName
                {
                    Name = environment.EnvironmentName,
                },
                Strategy = new DeploymentStrategy.RunOnce
                {
                    Deploy = new()
                    {
                        Steps = BuildSteps(workflow, job),
                    },
                },
            };

        return new Job.RegularJob
        {
            JobId = WorkflowExpressions.Raw(job.Name),
            DisplayName = WorkflowExpressions.Raw(job.Name),
            DependsOn = job.JobDependencies.Count > 0
                ? job.JobDependencies
                : null,
            Condition = conditionExpression,
            Pool = pool,
            Strategy = job.MatrixDimensions.Count > 0
                ? new JobStrategy
                {
                    Matrix = BuildMatrix(job.MatrixDimensions),
                }
                : null,
            Steps = BuildSteps(workflow, job),
        };
    }

    private static Pool.PoolSpec BuildPool(DevopsPool? poolOption)
    {
        if (poolOption is null)
            return new()
            {
                VmImage = WorkflowExpressions.Raw("ubuntu-latest"),
            };

        if (poolOption.HostedImage is not null)
            return new()
            {
                VmImage = poolOption.HostedImage,
                Name = poolOption.Name,
                Demands = poolOption.Demands.Count > 0
                    ? poolOption.Demands
                    : null,
            };

        if (poolOption.Name is not null)
            return new()
            {
                Name = poolOption.Name,
                Demands = poolOption.Demands.Count > 0
                    ? poolOption.Demands
                    : null,
            };

        return new()
        {
            VmImage = WorkflowExpressions.Raw("ubuntu-latest"),
        };
    }

    private Dictionary<string, IReadOnlyDictionary<string, WorkflowExpression>> BuildMatrix(
        IReadOnlyList<MatrixDimension> matrixDimensions)
    {
        var dimensions = matrixDimensions
            .Select(d => (buildDefinition.ParamDefinitions[d.Name].ArgName, d.Values))
            .ToArray();

        var result = new Dictionary<string, IReadOnlyDictionary<string, WorkflowExpression>>();
        var indices = new int[dimensions.Length];
        var counter = 1;

        while (true)
        {
            // Compute the current dimension values and sanitize for the combination name
            var currentValues = indices
                .Select((idx, i) => dimensions[i]
                    .Values[idx])
                .ToArray();

            var sanitizedValues = currentValues
                .Select(v => new string(expressionResolver
                    .Resolve(v)
                    .Select(c => char.IsLetterOrDigit(c)
                        ? c
                        : '-')
                    .ToArray()))
                .Select(v => HyphenReductionRegex()
                    .Replace(v, "-"))
                .Select(v => v.Trim('-'))
                .ToArray();

            var combinationName = $"{counter++:D3}_{string.Join("_", sanitizedValues)}";

            var entry = new Dictionary<string, WorkflowExpression>();

            for (var i = 0; i < dimensions.Length; i++)
                entry[dimensions[i].ArgName] = currentValues[i];

            result[combinationName] = entry;

            // Advance indices (odometer-style)
            var dim = 0;

            while (dim < dimensions.Length)
            {
                if (indices[dim] < dimensions[dim].Values.Count - 1)
                {
                    indices[dim]++;

                    break;
                }

                indices[dim] = 0;
                dim++;
            }

            if (dim == dimensions.Length)
                break;
        }

        return result;
    }

    [GeneratedRegex("-+")]
    private static partial Regex HyphenReductionRegex();

    private List<Step> BuildSteps(WorkflowModel workflow, WorkflowJobModel job)
    {
        var additionalSteps = IAdditionalStepOption
            .GetOptions(workflow, job.TargetStep)
            .ToList();

        // Special case: Add default checkout step if there isn't one
        if (!additionalSteps.Any(x => x is CheckoutStep))
            additionalSteps.Add(new DevopsCheckoutStep
            {
                Value = true,
                Repository = WorkflowExpressions.Raw("self"),
                FetchDepth = WorkflowExpressions.From(0),
            });

        additionalSteps = additionalSteps
            .Where<IAdditionalStepOption>(x => x is not IToggleWorkflowOption or IToggleWorkflowOption { Value: true })
            .ToList();

        // Add pre-target additional steps
        var steps = new List<Step>(additionalSteps
            .Where(x => x.Order < 0)
            .OrderBy(x => x.Order)
            .Select(BuildAdditionalStep));

        // Matrix params
        var matrixParams = job
            .MatrixDimensions
            .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].ArgName)
            .Select(name => (Name: name, Value: $"$({name})"))
            .ToList();

        var buildSliceValue = matrixParams switch
        {
            { Count: 0 } => null,
            { Count: 1 } => matrixParams[0].Value,
            { Count: > 1 } => string.Join("-", matrixParams.Select(x => x.Value)),
        };

        if (buildSliceValue is not null)
            matrixParams.Add(("build-slice", buildSliceValue));

        var target = buildModel.GetTarget(job.TargetStep.Name);

        // Consume artifacts
        if (target.ConsumedArtifacts.Count > 0)
        {
            foreach (var consumedArtifact in target.ConsumedArtifacts)
            {
                var consumedStep = workflow
                    .Jobs
                    .Select(x => x.TargetStep)
                    .Single(x => x.Name == consumedArtifact.TargetName);

                if (SuppressArtifactPublishingOption.GetOption(workflow, consumedStep) is { Value: true })
                    logger.LogWarning(
                        "Workflow {WorkflowName} target {TargetName} consumes artifact {ArtifactName} from target {SourceTargetName}, which has artifact publishing suppressed; this may cause the workflow to fail",
                        workflow.Name,
                        job.TargetStep.Name,
                        consumedArtifact.ArtifactName,
                        consumedArtifact.TargetName);
            }

            if (UseCustomArtifactProvider.GetOption(workflow) is { Value: true })
                foreach (var slice in target.ConsumedArtifacts.GroupBy(a => a.BuildSlice))
                {
                    var artifactNames = slice
                        .Select(x => x.ArtifactName)
                        .ToArray();

                    var retrieveTarget = buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact));

                    var env = BuildTargetStepEnv(workflow,
                        job,
                        retrieveTarget.Params,
                        retrieveTarget.ConsumedVariables,
                        matrixParams);

                    env["atom-artifacts"] = WorkflowExpressions.Raw(string.Join(",", artifactNames));

                    if (!env.ContainsKey("build-slice"))
                    {
                        if (slice.Key is { Length: > 0 })
                            env.Add("build-slice", WorkflowExpressions.Raw(slice.Key));
                        else if (buildSliceValue is not null)
                            env.Add("build-slice", WorkflowExpressions.Raw(buildSliceValue));
                    }

                    steps.Add(BuildScriptStep(artifactNames switch
                        {
                            [var name] => $"Retrieve artifact `{name}`",
                            _ => artifactNames.Length < 60
                                ? $"Retrieve artifacts `{string.Join(", ", artifactNames)}`"
                                : "Retrieve multiple artifacts",
                        },
                        "RetrieveArtifact",
                        env));
                }
            else
                foreach (var artifact in target.ConsumedArtifacts)
                {
                    var artifactName = artifact.BuildSlice is { Length: > 0 }
                        ? $"{artifact.ArtifactName}-{artifact.BuildSlice}"
                        : buildSliceValue is not null
                            ? $"{artifact.ArtifactName}-{buildSliceValue}"
                            : artifact.ArtifactName;

                    steps.Add(new Step.Task
                    {
                        TaskName = WorkflowExpressions.Raw("DownloadPipelineArtifact@2"),
                        DisplayName = WorkflowExpressions.Raw(artifact.ArtifactName),
                        Inputs = new Dictionary<string, WorkflowExpression>
                        {
                            ["artifact"] = WorkflowExpressions.Raw(artifactName),
                            ["path"] = WorkflowExpressions.Raw(
                                $"{DevopsWorkflows.Devops.PipelineArtifactDirectory}/{artifact.ArtifactName}"),
                        },
                    });
                }
        }

        // Target step
        var targetStepEnv = BuildTargetStepEnv(workflow, job, target.Params, target.ConsumedVariables, matrixParams);

        steps.Add(BuildScriptStep(job.TargetStep.Name, job.TargetStep.Name, targetStepEnv, job.TargetStep.Name));

        // Produce artifacts
        if (target.ProducedArtifacts.Count > 0 &&
            SuppressArtifactPublishingOption.GetOption(workflow, job.TargetStep) is not { Value: true })
        {
            if (UseCustomArtifactProvider.GetOption(workflow) is { Value: true })
                foreach (var slice in target.ProducedArtifacts.GroupBy(a => a.BuildSlice))
                {
                    var artifactNames = slice
                        .Select(x => x.ArtifactName)
                        .ToArray();

                    var storeTarget = buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact));

                    var env = BuildTargetStepEnv(workflow,
                        job,
                        storeTarget.Params,
                        storeTarget.ConsumedVariables,
                        matrixParams);

                    env["atom-artifacts"] = WorkflowExpressions.Raw(string.Join(",", artifactNames));

                    if (!env.ContainsKey("build-slice"))
                    {
                        if (slice.Key is { Length: > 0 })
                            env.Add("build-slice", WorkflowExpressions.Raw(slice.Key));
                        else if (buildSliceValue is not null)
                            env.Add("build-slice", WorkflowExpressions.Raw(buildSliceValue));
                    }

                    steps.Add(BuildScriptStep(artifactNames switch
                        {
                            [var name] => $"Store artifact `{name}`",
                            _ => artifactNames.Length < 60
                                ? $"Store artifacts `{string.Join(", ", artifactNames)}`"
                                : "Store multiple artifacts",
                        },
                        "StoreArtifact",
                        env));
                }
            else
                foreach (var artifact in target.ProducedArtifacts)
                {
                    var artifactName = artifact.BuildSlice is { Length: > 0 }
                        ? $"{artifact.ArtifactName}-{artifact.BuildSlice}"
                        : buildSliceValue is not null
                            ? $"{artifact.ArtifactName}-{buildSliceValue}"
                            : artifact.ArtifactName;

                    steps.Add(new Step.Task
                    {
                        TaskName = WorkflowExpressions.Raw("PublishPipelineArtifact@1"),
                        DisplayName = WorkflowExpressions.Raw(artifact.ArtifactName),
                        Inputs = new Dictionary<string, WorkflowExpression>
                        {
                            ["artifactName"] = WorkflowExpressions.Raw(artifactName),
                            ["targetPath"] =
                                WorkflowExpressions.Raw(
                                    $"{DevopsWorkflows.Devops.PipelinePublishDirectory}/{artifact.ArtifactName}"),
                        },
                    });
                }
        }

        // Add post-target additional steps
        steps.AddRange(additionalSteps
            .Where(x => x.Order > 0)
            .OrderBy(x => x.Order)
            .Select(BuildAdditionalStep));

        return steps;
    }

    private Step BuildAdditionalStep(IAdditionalStepOption additionalStep) =>
        additionalStep switch
        {
            IDevopsAdditionalStepOption devopsStep => devopsStep.Build(),
            SetupDotnetStep setupDotnetStep => BuildSetupDotnetStep(setupDotnetStep),
            AddNugetFeedsStep addNugetFeedsStep => BuildAddNugetFeedsStep(addNugetFeedsStep),
            _ => throw new InvalidOperationException(
                $"Unknown additional step type: {additionalStep.GetType().FullName}"),
        };

    private static Step.Task BuildSetupDotnetStep(SetupDotnetStep step)
    {
        var inputs = new Dictionary<string, WorkflowExpression>();

        if (step.DotnetVersion is not null)
            inputs["version"] = step.DotnetVersion;

        return new()
        {
            TaskName = WorkflowExpressions.Raw("UseDotNet@2"),
            DisplayName = step.DotnetVersion is not null
                ? WorkflowExpressions.Concat(["Setup .NET ", step.DotnetVersion])
                : WorkflowExpressions.Raw("Setup .NET"),
            Inputs = inputs.Count > 0
                ? inputs
                : null,
        };
    }

    private Step.Script BuildAddNugetFeedsStep(AddNugetFeedsStep step)
    {
        var feedsToAdd = step.FeedsToAdd.ToList();

        var toolVersion = "";

        if (step.SyncAtomToolVersionToLibraryVersion)
        {
            if (SemVer.TryParse(typeof(AtomHost).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                    ?.InformationalVersion ??
                                "",
                    out var semVer))
                toolVersion =
                    SemVer.Parse($"{semVer.Prefix}{(semVer.IsPreRelease ? $"-{semVer.PreRelease}" : string.Empty)}");
            else
                throw new InvalidOperationException(
                    "Failed to parse DecSm.Atom.Host assembly version as SemVer for syncing atom tool version");
        }

        var env = feedsToAdd.ToDictionary<NugetFeedOptions, string, WorkflowExpression>(
            k => AddNugetFeedsStep.GetEnvVarNameForFeed(k.FeedName),
            v => WorkflowExpressions.Raw($"$({v.SecretName})"));

        // If we are using .net 10+ then we can use the dotnet tool exec command instead of installing the tool to run it
        if (SemVer.TryParse(RuntimeInformation
                    .FrameworkDescription
                    .Replace(".NET ", "")
                    .Replace("x", "0"),
                out var version) &&
            version.Major >= 10)
            return new()
            {
                ScriptContent = WorkflowExpressions.Raw(string.Join("\n",
                    feedsToAdd.Select(feedToAdd => step.SyncAtomToolVersionToLibraryVersion
                        ? $"dotnet tool exec decsm.atom.tool@{toolVersion} -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\""
                        : $"dotnet tool exec decsm.atom.tool -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\""))),
                DisplayName = WorkflowExpressions.Raw("Setup NuGet"),
                Env = env.Count > 0
                    ? env
                    : null,
            };

        return new()
        {
            ScriptContent = WorkflowExpressions.Raw(string.Join("\n",
                feedsToAdd
                    .Select(feedToAdd => step.SyncAtomToolVersionToLibraryVersion
                        ? $"atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\" --tool-version \"{toolVersion}\""
                        : $"atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"")
                    .Prepend("dotnet tool update --global DecSm.Atom.Tool"))),
            DisplayName = WorkflowExpressions.Raw("Setup NuGet"),
            Env = env.Count > 0
                ? env
                : null,
        };
    }

    private Step.Script BuildScriptStep(
        string displayName,
        string targetName,
        Dictionary<string, WorkflowExpression> env,
        string? stepName = null)
    {
        var runScript = GetRunScript(targetName);

        return new()
        {
            ScriptContent = WorkflowExpressions.Raw(runScript),
            DisplayName = WorkflowExpressions.Raw(displayName),
            Name = stepName is not null
                ? WorkflowExpressions.Raw(stepName)
                : null,
            Env = env.Count > 0
                ? env
                : null,
        };
    }

    private string GetRunScript(string targetName)
    {
        if (atomProjectData.IsFileBasedApp)
        {
            if (AppContext.GetData("EntryPointFilePath") is not string fileName)
                throw new InvalidOperationException(
                    "AtomFileSystem reports file-based app but AppContext.EntryPointFilePath is null, cannot determine file path to run");

            var filePathRelativeToRoot =
                atomFileSystem.FileSystem.Path.GetRelativePath(atomFileSystem.AtomRootDirectory, fileName);

            return $"dotnet run --file {filePathRelativeToRoot} {targetName} --skip --headless";
        }

        var projectPath = FindProjectPath(atomFileSystem, atomProjectData.ProjectName);

        return $"dotnet run --project {projectPath} {targetName} --skip --headless";
    }

    private Dictionary<string, WorkflowExpression> BuildTargetStepEnv(
        WorkflowModel workflow,
        WorkflowJobModel job,
        IReadOnlyList<UsedParam> usedParams,
        IReadOnlyList<ConsumedVariable> consumedVariables,
        List<(string Name, string Value)> matrixParams)
    {
        var targetStepEnv = new Dictionary<string, WorkflowExpression>();

        foreach (var manualTrigger in workflow.Triggers.OfType<ManualTrigger>())
        foreach (var input in manualTrigger.Inputs?.Where(i => usedParams
                     .Select(p => p.Param.ArgName)
                     .Any(p => p == i.Name)) ?? [])
            targetStepEnv[input.Name] = WorkflowExpressions
                .Raw("parameters")[input.Name]
                .Evaluate();

        foreach (var consumedVariable in consumedVariables)
        {
            var argName = buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName;

            targetStepEnv[argName] = new DevopsRuntimeExpression(new RawExpression(
                $"dependencies.{consumedVariable.TargetName}.outputs['{consumedVariable.TargetName}.{argName}']"));
        }

        var requiredSecrets = usedParams
            .Where(x => x.Param.IsSecret)
            .ToArray();

        if (requiredSecrets.Length > 0)
        {
            foreach (var injectedSecret in workflow.Options.OfType<WorkflowSecretInjectionForSecretProvider>())
                if (buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.SecretName) is
                    { } paramDefinition)
                    targetStepEnv[paramDefinition.ArgName] =
                        new DevopsMacroExpression(WorkflowExpressions.Raw(paramDefinition.EnvVarName));

            foreach (var injectedEnvVar in workflow.Options.OfType<WorkflowSecretsInjectionFromEnvironment>())
                if (buildDefinition.ParamDefinitions.GetValueOrDefault(injectedEnvVar.SecretName) is
                    { } paramDefinition)
                    targetStepEnv[paramDefinition.ArgName] =
                        new DevopsMacroExpression(WorkflowExpressions.Raw(paramDefinition.EnvVarName));
        }

        foreach (var requiredSecret in requiredSecrets)
            if (WorkflowSecretInjection
                .GetOptions(workflow, job.TargetStep)
                .Any(x => x.Value == requiredSecret.Param.Name))
                targetStepEnv[requiredSecret.Param.ArgName] =
                    new DevopsMacroExpression(WorkflowExpressions.Raw(requiredSecret.Param.EnvVarName));

        var environmentInjections = WorkflowParamInjectionFromEnvironment.GetOptions(workflow, job.TargetStep);
        var paramInjections = WorkflowParamInjection.GetOptions(workflow, job.TargetStep);
        var environmentVariableInjections = WorkflowEnvironmentVariableInjection.GetOptions(workflow, job.TargetStep);

        environmentInjections = environmentInjections.Where(e => paramInjections.All(p => p.Name != e.Value));

        foreach (var environmentInjection in environmentInjections)
        {
            if (!buildDefinition.ParamDefinitions.TryGetValue(environmentInjection.Value, out var paramDefinition))
            {
                logger.LogWarning(
                    "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that does not exist",
                    workflow.Name,
                    job.TargetStep.Name,
                    environmentInjection.Value);

                continue;
            }

            targetStepEnv[paramDefinition.ArgName] =
                new DevopsMacroExpression(WorkflowExpressions.Raw(paramDefinition.EnvVarName));
        }

        foreach (var paramInjection in paramInjections)
        {
            if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.Name, out var paramDefinition))
            {
                logger.LogWarning(
                    "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that is not consumed by the command",
                    workflow.Name,
                    job.TargetStep.Name,
                    paramInjection.Name);

                continue;
            }

            targetStepEnv[paramDefinition.ArgName] = paramInjection.InjectionExpression;
        }

        foreach (var environmentVariableInjection in environmentVariableInjections)
            targetStepEnv[environmentVariableInjection.Name] = environmentVariableInjection.Value;

        foreach (var matrixParam in matrixParams)
            targetStepEnv[matrixParam.Name] = WorkflowExpressions.Raw(matrixParam.Value);

        return targetStepEnv;
    }

    private static string FindProjectPath(IAtomFileSystem atomFileSystem, string projectName)
    {
        var projectPath = atomFileSystem
            .FileSystem
            .DirectoryInfo
            .New(atomFileSystem.AtomRootDirectory)
            .EnumerateFiles("*.csproj",
                new EnumerationOptions
                {
                    IgnoreInaccessible = true,
                    MaxRecursionDepth = 4,
                    RecurseSubdirectories = true,
                    ReturnSpecialDirectories = false,
                })
            .FirstOrDefault(f => f.Name.Equals($"{projectName}.csproj", StringComparison.OrdinalIgnoreCase));

        if (projectPath?.FullName is null)
            throw new InvalidOperationException($"Project '{projectName}' not found in current directory.");

        return atomFileSystem
            .FileSystem
            .Path
            .GetRelativePath(atomFileSystem.AtomRootDirectory, projectPath.FullName)
            .Replace("\\", "/");
    }
}
