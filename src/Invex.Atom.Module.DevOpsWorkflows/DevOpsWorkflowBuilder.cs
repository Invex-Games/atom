namespace Invex.Atom.Module.DevOpsWorkflows;

internal sealed partial class DevOpsWorkflowBuilder(
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    IParamService paramService,
    IRootedFileSystem fileSystem,
    AtomProjectData atomProjectData,
    ILogger<DevOpsWorkflowBuilder> logger
)
{
    public DevopsPipeline Build(WorkflowModel workflow) =>
        new DevopsPipeline.DevopsPipelineWithJobs
        {
            Name = workflow.Name,
            Jobs = workflow
                .Jobs
                .Select(x => BuildJob(workflow, x))
                .ToList(),
            Trigger = BuildTrigger(workflow),
            Pr = BuildPr(workflow),
            Parameters = BuildParameters(workflow),
            Variables = BuildVariables(workflow),
            LockBehavior = DevOpsConcurrencyOption.Get(workflow.Options)
                ?.LockBehavior,
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
                    Branches = [..includedBranches.Select(x => (Template)x)],
                };

            return null;
        }

        return new Trigger.Full
        {
            Branches = hasBranches
                ? new IncludeExcludeFilters
                {
                    Include = includedBranches.Length > 0
                        ? [..includedBranches.Select(x => (Template)x)]
                        : null,
                    Exclude = excludedBranches.Length > 0
                        ? [..excludedBranches.Select(x => (Template)x)]
                        : null,
                }
                : null,
            Paths = hasPaths
                ? new IncludeExcludeFilters
                {
                    Include = includedPaths.Length > 0
                        ? [..includedPaths.Select(x => (Template)x)]
                        : null,
                    Exclude = excludedPaths.Length > 0
                        ? [..excludedPaths.Select(x => (Template)x)]
                        : null,
                }
                : null,
            Tags = hasTags
                ? new IncludeExcludeFilters
                {
                    Include = includedTags.Length > 0
                        ? [..includedTags.Select(x => (Template)x)]
                        : null,
                    Exclude = excludedTags.Length > 0
                        ? [..excludedTags.Select(x => (Template)x)]
                        : null,
                }
                : null,
        };
    }

    private static Pr? BuildPr(WorkflowModel workflow)
    {
        var pullRequestOption = DevOpsPullRequestOption.Get(workflow.Options);

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

        if (pullRequestOption is null && !hasPaths && includedBranches.Length > 0 && excludedBranches.Length is 0)
            return new Pr.BranchList
            {
                Branches = [..includedBranches.Select(x => (Template)x)],
            };

        return new Pr.Full
        {
            Branches = hasBranches
                ? new IncludeExcludeFilters
                {
                    Include = includedBranches.Length > 0
                        ? [..includedBranches.Select(x => (Template)x)]
                        : null,
                    Exclude = excludedBranches.Length > 0
                        ? [..excludedBranches.Select(x => (Template)x)]
                        : null,
                }
                : null,
            Paths = hasPaths
                ? new IncludeExcludeFilters
                {
                    Include = includedPaths.Length > 0
                        ? [..includedPaths.Select(x => (Template)x)]
                        : null,
                    Exclude = excludedPaths.Length > 0
                        ? [..excludedPaths.Select(x => (Template)x)]
                        : null,
                }
                : null,
            AutoCancel = pullRequestOption is not null
                ? (pullRequestOption.AutoCancel)
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
                                defaultBoolValue = buildDefinition.AccessParam(inputParamName) switch
                                {
                                    bool boolParam => boolParam,
                                    string stringParam when bool.TryParse(stringParam, out var parsedBool) =>
                                        parsedBool,
                                    _ => defaultBoolValue,
                                };

                        return new()
                        {
                            Name = input.Name,
                            DisplayName = $"{input.Name} | {input.Description}",
                            Type = "boolean",
                            Default = defaultBoolValue is not null
                                ? (Template)(defaultBoolValue.Value
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
                                Name = input.Name,
                                DisplayName = $"{input.Name} | {input.Description}",
                                Type = "string",
                                Default = defaultChoiceValue is { Length: > 0 }
                                    ? (Template)defaultChoiceValue
                                    : null,
                                Values = [..choiceInput.Choices.Select(x => (Template)x)],
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
                                Name = input.Name,
                                DisplayName = $"{input.Name} | {input.Description}",
                                Type = "string",
                                Default = defaultStringValue is { Length: > 0 }
                                    ? (Template)defaultStringValue
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
            .OfType<DevOpsVariableGroup>()
            .ToArray();

        if (variableGroups.Length is 0)
            return null;

        return new()
        {
            Values = variableGroups
                .Select(Variable (g) => new Variable.Group
                {
                    GroupName = g.Name,
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
            .OfType<DevOpsPool>()
            .FirstOrDefault();

        var pool = BuildPool(poolOption);

        var condition = TargetCondition
            .GetOptions(job.TargetStep.Options)
            .ToList();

        var conditionExpression = condition switch
        {
            { Count: > 1 } => (Tex.All(condition.Select(x => x.Condition))),
            [_] => (condition[0].Condition),
            _ => null,
        };

        var environment = DeployToEnvironment.Get(job.TargetStep.Options);
        var jobVariables = BuildJobConsumedVariables(job);

        if (environment is not null)
            return new Job.Deployment
            {
                DeploymentId = job.Name,
                DisplayName = job.Name,
                DependsOn = job.JobDependencies.Count > 0
                    ? [..job.JobDependencies.Select(x => (Template)x)]
                    : null,
                Condition = conditionExpression,
                Pool = pool,
                Variables = jobVariables,
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
            JobId = job.Name,
            DisplayName = job.Name,
            DependsOn = job.JobDependencies.Count > 0
                ? [..job.JobDependencies.Select(x => (Template)x)]
                : null,
            Condition = conditionExpression,
            Pool = pool,
            Variables = jobVariables,
            Strategy = job.TargetStep.MatrixDimensions.Count > 0
                ? new JobStrategy
                {
                    Matrix = BuildMatrix(job.TargetStep.MatrixDimensions),
                }
                : null,
            Steps = BuildSteps(workflow, job),
        };
    }

    private Variables.VariableList? BuildJobConsumedVariables(WorkflowJobModel job)
    {
        var target = buildModel.GetTarget(job.TargetStep.Name);

        var targets = new List<TargetModel>
        {
            target,
        };

        if (UseCustomArtifactProvider.Get(job.TargetStep.Options) is { Enabled: true })
        {
            if (target.ConsumedArtifacts.Count > 0)
                targets.Add(buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact)));

            if (target.ProducedArtifacts.Count > 0 &&
                SuppressArtifactPublishingOption.Get(job.TargetStep.Options) is not { Enabled: true })
                targets.Add(buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact)));
        }

        var consumedVariables = targets
            .SelectMany(t => t.ConsumedVariables)
            .DistinctBy(v => v.VariableName)
            .ToList();

        if (consumedVariables.Count is 0)
            return null;

        return new()
        {
            Values = consumedVariables.ConvertAll(consumedVariable =>
            {
                var argName = buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName;

                return new Variable.Name
                {
                    VariableName = argName,
                    Value = Tex.DevOps.Runtime(Tex.Raw(
                        $"dependencies.{consumedVariable.TargetName}.outputs['{consumedVariable.TargetName}.{argName}']")),
                };
            }),
        };
    }

    private static Pool.PoolSpec BuildPool(DevOpsPool? poolOption)
    {
        if (poolOption is null)
            return new()
            {
                VmImage = "ubuntu-latest",
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
            VmImage = "ubuntu-latest",
        };
    }

    private Dictionary<string, IReadOnlyDictionary<string, Template>> BuildMatrix(
        IReadOnlyList<MatrixDimension> matrixDimensions)
    {
        var dimensions = matrixDimensions
            .Select(d => (buildDefinition.ParamDefinitions[d.Name].ArgName, d.Values))
            .ToArray();

        var result = new Dictionary<string, IReadOnlyDictionary<string, Template>>();
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
                .Select(v => new string(DevopsExpressionPrinter.Instance
                    .Render(v)
                    .Select(c => char.IsLetterOrDigit(c)
                        ? c
                        : '-')
                    .ToArray()))
                .Select(v => HyphenReductionRegex()
                    .Replace(v, "-"))
                .Select(v => v.Trim('-'))
                .ToArray();

            var combinationName = $"{counter++:D3}_{string.Join("_", sanitizedValues)}";

            var entry = new Dictionary<string, Template>();

            for (var i = 0; i < dimensions.Length; i++)
                entry[dimensions[i].ArgName] = (currentValues[i]);

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
            .GetOptions(job.TargetStep.Options)
            .ToList();

        // Special case: Add default checkout step if there isn't one
        if (!additionalSteps.Any(x => x is CheckoutStep))
            additionalSteps.Add(new DevOpsCheckoutStep
            {
                TaskEnabled = true,
                Repository = "self",
                FetchDepth = 0,
            });

        additionalSteps = additionalSteps.ToList();

        // Add pre-target additional steps
        var steps = new List<Step>(additionalSteps
            .Where(x => x.Order < 0)
            .OrderBy(x => x.Order)
            .Select(BuildAdditionalStep));

        // Matrix params
        var matrixParams = job
            .TargetStep
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

                if (SuppressArtifactPublishingOption.Get(consumedStep.Options) is { Enabled: true })
                    logger.LogWarning(
                        "Workflow {WorkflowName} target {TargetName} consumes artifact {ArtifactName} from target {SourceTargetName}, which has artifact publishing suppressed; this may cause the workflow to fail",
                        workflow.Name,
                        job.TargetStep.Name,
                        consumedArtifact.ArtifactName,
                        consumedArtifact.TargetName);
            }

            if (UseCustomArtifactProvider.Get(job.TargetStep.Options) is { Enabled: true })
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

                    env["atom-artifacts"] = (string.Join(",", artifactNames));

                    if (!env.ContainsKey("build-slice"))
                    {
                        if (slice.Key is { Length: > 0 })
                            env.Add("build-slice", (slice.Key));
                        else if (buildSliceValue is not null)
                            env.Add("build-slice", (buildSliceValue));
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
                        TaskName = "DownloadPipelineArtifact@2",
                        DisplayName = artifact.ArtifactName,
                        Inputs = new Dictionary<string, Template>
                        {
                            ["artifact"] = artifactName,
                            ["path"] = (
                                $"{DevOpsWorkflows.DevOps.PipelineArtifactDirectory}/{artifact.ArtifactName}"),
                        },
                    });
                }
        }

        // Target step
        var targetStepEnv = BuildTargetStepEnv(workflow, job, target.Params, target.ConsumedVariables, matrixParams);

        steps.Add(BuildScriptStep(job.TargetStep.Name, job.TargetStep.Name, targetStepEnv, job.TargetStep.Name));

        // Produce artifacts
        if (target.ProducedArtifacts.Count > 0 &&
            SuppressArtifactPublishingOption.Get(job.TargetStep.Options) is not { Enabled: true })
        {
            if (UseCustomArtifactProvider.Get(job.TargetStep.Options) is { Enabled: true })
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

                    env["atom-artifacts"] = (string.Join(",", artifactNames));

                    if (!env.ContainsKey("build-slice"))
                    {
                        if (slice.Key is { Length: > 0 })
                            env.Add("build-slice", (slice.Key));
                        else if (buildSliceValue is not null)
                            env.Add("build-slice", (buildSliceValue));
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
                        TaskName = "PublishPipelineArtifact@1",
                        DisplayName = artifact.ArtifactName,
                        Inputs = new Dictionary<string, Template>
                        {
                            ["artifactName"] = artifactName,
                            ["targetPath"] = (
                                $"{DevOpsWorkflows.DevOps.PipelinePublishDirectory}/{artifact.ArtifactName}"),
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

    private static Step BuildAdditionalStep(IAdditionalStepOption additionalStep) =>
        additionalStep switch
        {
            IDevOpsAdditionalStepOption devopsStep => devopsStep.Build(),
            SetupDotnetStep setupDotnetStep => BuildSetupDotnetStep(setupDotnetStep),
            AddNugetFeedsStep addNugetFeedsStep => BuildAddNugetFeedsStep(addNugetFeedsStep),
            _ => throw new InvalidOperationException(
                $"Unknown additional step type: {additionalStep.GetType().FullName}"),
        };

    private static Step.Task BuildSetupDotnetStep(SetupDotnetStep step)
    {
        var inputs = new Dictionary<string, Template>();

        if (step.DotnetVersion is not null)
            inputs["version"] = (step.DotnetVersion);

        return new()
        {
            TaskName = "UseDotNet@2",
            DisplayName = step.DotnetVersion is not null
                ? (Template)$"Setup .NET {step.DotnetVersion}"
                : "Setup .NET",
            Inputs = inputs.Count > 0
                ? inputs
                : null,
        };
    }

    private static Step.Script BuildAddNugetFeedsStep(AddNugetFeedsStep step)
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
                    "Failed to parse Invex.Atom.Host assembly version as SemVer for syncing atom tool version");
        }

        var env = feedsToAdd.ToDictionary<NugetFeedOptions, string, Template>(
            k => AddNugetFeedsStep.GetEnvVarNameForFeed(k.FeedName),
            v => ($"$({v.SecretName})"));

        // If we are using .net 10+ then we can use the dotnet tool exec command instead of installing the tool to run it
        if (SemVer.TryParse(RuntimeInformation
                    .FrameworkDescription
                    .Replace(".NET ", "")
                    .Replace("x", "0"),
                out var version) &&
            version.Major >= 10)
            return new()
            {
                ScriptContent = (string.Join("\n",
                    feedsToAdd.Select(feedToAdd => step.SyncAtomToolVersionToLibraryVersion
                        ? $"dotnet tool exec invex.atom.tool@{toolVersion} -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\""
                        : $"dotnet tool exec invex.atom.tool -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\""))),
                DisplayName = "Setup NuGet",
                Env = env.Count > 0
                    ? env
                    : null,
            };

        return new()
        {
            ScriptContent = (string.Join("\n",
                feedsToAdd
                    .Select(feedToAdd => step.SyncAtomToolVersionToLibraryVersion
                        ? $"atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\" --tool-version \"{toolVersion}\""
                        : $"atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"")
                    .Prepend("dotnet tool update --global Invex.Atom.Tool"))),
            DisplayName = "Setup NuGet",
            Env = env.Count > 0
                ? env
                : null,
        };
    }

    private Step.Script BuildScriptStep(
        string displayName,
        string targetName,
        Dictionary<string, Template> env,
        string? stepName = null)
    {
        var runScript = GetRunScript(targetName);

        return new()
        {
            ScriptContent = runScript,
            DisplayName = displayName,
            Name = stepName is not null
                ? (Template)stepName
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
                fileSystem.FileSystem.Path.GetRelativePath(fileSystem.AtomRootDirectory, fileName);

            return $"dotnet run --file {filePathRelativeToRoot} {targetName} --skip --headless";
        }

        var projectPath = FindProjectPath(fileSystem, atomProjectData.ProjectName);

        return $"dotnet run --project {projectPath} {targetName} --skip --headless";
    }

    private Dictionary<string, Template> BuildTargetStepEnv(
        WorkflowModel workflow,
        WorkflowJobModel job,
        IReadOnlyList<UsedParam> usedParams,
        IReadOnlyList<ConsumedVariable> consumedVariables,
        List<(string Name, string Value)> matrixParams)
    {
        var targetStepEnv = new Dictionary<string, Template>();

        foreach (var manualTrigger in workflow.Triggers.OfType<ManualTrigger>())
        foreach (var input in manualTrigger.Inputs?.Where(i => usedParams
                     .Select(p => p.Param.ArgName)
                     .Any(p => p == i.Name)) ?? [])
            targetStepEnv[input.Name] = Tex.Input(input.Name);

        foreach (var consumedVariable in consumedVariables)
        {
            var argName = buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName;

            // Consumed variables are declared as job-level variables (using runtime expressions) in BuildJobConsumedVariables.
            // Here we reference them via the macro $(varName) syntax, which is the correct way to read job variables in step envs.
            targetStepEnv[argName] = Tex.Variable(argName);
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
                        Tex.Variable(paramDefinition.EnvVarName);

            foreach (var injectedEnvVar in workflow.Options.OfType<WorkflowSecretInjectionFromEnvironment>())
                if (buildDefinition.ParamDefinitions.GetValueOrDefault(injectedEnvVar.SecretName) is
                    { } paramDefinition)
                    targetStepEnv[paramDefinition.ArgName] =
                        Tex.Variable(paramDefinition.EnvVarName);
        }

        foreach (var requiredSecret in requiredSecrets)
            if (WorkflowSecretInjection
                .GetOptions(job.TargetStep.Options)
                .Any(x => x.SecretName == requiredSecret.Param.Name))
                targetStepEnv[requiredSecret.Param.ArgName] =
                    Tex.Variable(requiredSecret.Param.EnvVarName);

        var environmentInjections = WorkflowParamInjectionFromEnvironment.GetOptions(job.TargetStep.Options);
        var paramInjections = WorkflowParamInjection.GetOptions(job.TargetStep.Options);
        var environmentVariableInjections = WorkflowEnvironmentVariableInjection.GetOptions(job.TargetStep.Options);

        environmentInjections = environmentInjections
            .Where(e => paramInjections.All(p => p.ParamName != e.ParamName))
            .ToList();

        foreach (var environmentInjection in environmentInjections)
        {
            if (!buildDefinition.ParamDefinitions.TryGetValue(environmentInjection.ParamName, out var paramDefinition))
            {
                logger.LogWarning(
                    "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that does not exist",
                    workflow.Name,
                    job.TargetStep.Name,
                    environmentInjection.ParamName);

                continue;
            }

            targetStepEnv[paramDefinition.ArgName] =
                Tex.Variable(paramDefinition.EnvVarName);
        }

        foreach (var paramInjection in paramInjections)
        {
            if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.ParamName, out var paramDefinition))
            {
                logger.LogWarning(
                    "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that is not consumed by the command",
                    workflow.Name,
                    job.TargetStep.Name,
                    paramInjection.ParamName);

                continue;
            }

            targetStepEnv[paramDefinition.ArgName] = (paramInjection.Value);
        }

        foreach (var environmentVariableInjection in environmentVariableInjections)
            targetStepEnv[environmentVariableInjection.Name] =
                (environmentVariableInjection.Value);

        foreach (var matrixParam in matrixParams)
            targetStepEnv[matrixParam.Name] = (matrixParam.Value);

        return targetStepEnv;
    }

    private static string FindProjectPath(IRootedFileSystem fileSystem, string projectName)
    {
        var projectPath = fileSystem
            .FileSystem
            .DirectoryInfo
            .New(fileSystem.AtomRootDirectory)
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

        return fileSystem
            .FileSystem
            .Path
            .GetRelativePath(fileSystem.AtomRootDirectory, projectPath.FullName)
            .Replace("\\", "/");
    }
}
