namespace DecSm.Atom.Module.GithubWorkflows.GithubActions;

internal sealed class GithubWorkflowFileWriter(
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    IParamService paramService,
    IAtomFileSystem atomFileSystem,
    AtomProjectData atomProjectData,
    ILogger<GithubWorkflowFileWriter> logger
) : WorkflowFileWriter<GithubWorkflowType>(atomFileSystem, logger)
{
    private readonly IAtomFileSystem _atomFileSystem = atomFileSystem;

    protected override string FileExtension => "yml";

    protected override RootedPath FileLocation => _atomFileSystem.AtomRootDirectory / ".github" / "workflows";

    protected override string WriteWorkflow(WorkflowModel workflow)
    {
        var resolvedWorkflow = BuildWorkflow(workflow);

        var actionWriter = new GithubActionWriter();
        actionWriter.Write(resolvedWorkflow);

        return actionWriter.TextWriter.ToString();
    }

    private GithubAction BuildWorkflow(WorkflowModel workflow) =>
        new()
        {
            Name = workflow.Name,
            On = BuildTriggers(workflow),
            Jobs = workflow
                .Jobs
                .Select(x => BuildJob(workflow, x))
                .ToList(),
            Permissions = BuildPermissions(GithubTokenPermissionsOption.Get(workflow.Options) ??
                                           BuildOptions.Github.TokenPermissions.NoneAll),
        };

    private List<On> BuildTriggers(WorkflowModel workflow) =>
        workflow
            .Triggers
            .Select<IWorkflowTrigger, On>(trigger => trigger switch
            {
                GithubTrigger githubTrigger => githubTrigger.On,
                GitPullRequestTrigger gitPullRequestTrigger => new On.PullRequest(gitPullRequestTrigger
                    .Types
                    .Select(type => Enum.TryParse<On.PullRequest.PullRequestType>(type, out var result)
                        ? result
                        : throw new ArgumentOutOfRangeException(nameof(type)))
                    .ToList())
                {
                    Branches = gitPullRequestTrigger.IncludedBranches,
                    BranchesIgnore = gitPullRequestTrigger.ExcludedBranches,
                    Tags = null,
                    TagsIgnore = null,
                    Paths = gitPullRequestTrigger.IncludedPaths,
                    PathsIgnore = gitPullRequestTrigger.ExcludedPaths,
                },

                GitPushTrigger gitPushTrigger => new On.Push
                {
                    Branches = gitPushTrigger.IncludedBranches,
                    BranchesIgnore = gitPushTrigger.ExcludedBranches,
                    Tags = gitPushTrigger.IncludedTags,
                    TagsIgnore = gitPushTrigger.ExcludedTags,
                    Paths = gitPushTrigger.IncludedPaths,
                    PathsIgnore = gitPushTrigger.ExcludedPaths,
                },

                ManualTrigger manualTrigger => new On.WorkflowDispatch(manualTrigger
                    .Inputs
                    ?.Select<ManualInput, WorkflowDispatchInput>(input =>
                    {
                        var inputParamName = buildDefinition.ParamDefinitions
                            .FirstOrDefault(x => x.Value.ArgName == input.Name)
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
                                            {
                                                defaultBoolValue = boolParam;

                                                break;
                                            }

                                            case string stringParam:
                                            {
                                                if (bool.TryParse(stringParam, out var parsedBool))
                                                    defaultBoolValue = parsedBool;

                                                break;
                                            }
                                        }

                                return new WorkflowDispatchInput.Boolean
                                {
                                    Name = input.Name,
                                    Description = input.Description,
                                    Required = input.Required ?? defaultBoolValue is null,
                                    Default = defaultBoolValue switch
                                    {
                                        true => "true",
                                        false => "false",
                                        null => null,
                                    },
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

                                    return new WorkflowDispatchInput.Choice
                                    {
                                        Name = input.Name,
                                        Description = input.Description,
                                        Required = input.Required ?? defaultChoiceValue is not { Length: > 0 },
                                        Default = defaultChoiceValue,
                                        Options = choiceInput.Choices,
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

                                    return new WorkflowDispatchInput.String
                                    {
                                        Name = stringInput.Name,
                                        Description = stringInput.Description,
                                        Required = input.Required ?? defaultStringValue is not { Length: > 0 },
                                        Default = defaultStringValue,
                                    };
                                }
                            }

                            default:
                            {
                                throw new ArgumentOutOfRangeException(nameof(input));
                            }
                        }
                    })
                    .ToList() ?? []),
                _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null),
            })
            .ToList();

    private Job BuildJob(WorkflowModel workflow, WorkflowJobModel job) =>
        new()
        {
            Name = job.Name,
            Permissions = BuildPermissions(GithubTokenPermissionsOption.Get(job.TargetStep.Options)),
            Needs = job
                .JobDependencies
                .Distinct()
                .ToList(),
            If = TargetCondition
                    .GetOptions(job.TargetStep.Options)
                    .ToList() switch
                {
                    { Count: > 1 } options => options[0]
                        .Value
                        .And(options
                            .Skip(1)
                            .Select(x => x.Value)
                            .ToArray()),
                    { Count: 1 } option => option[0].Value,
                    _ => null,
                },
            RunsOn = GithubRunsOn.Get(job.TargetStep.Options) is { } runsOn
                ? new()
                {
                    Labels = runsOn.Labels,
                    Group = runsOn.Group,
                }
                : new()
                {
                    Labels = ["ubuntu-latest"],
                },
            Snapshot = null,
            Environment = DeployToEnvironment.Get(job.TargetStep.Options) is { } environment
                ? new()
                {
                    Name = environment.EnvironmentName,
                }
                : null,
            Concurrency = null,
            Outputs = buildModel.GetTarget(job.TargetStep.Name)
                .ProducedVariables is { Count: > 0 } outputs
                ? outputs.ToDictionary<string, string, TextExpression>(x => buildDefinition.ParamDefinitions[x].ArgName,
                    x => TextExpressions
                        .Raw("steps")[job.Name]["outputs"][buildDefinition.ParamDefinitions[x].ArgName]
                        .Evaluate())
                : null,
            Env = null,
            Strategy = job.TargetStep.MatrixDimensions.Count > 0
                ? new()
                {
                    Matrix = new()
                    {
                        Map = job.TargetStep.MatrixDimensions.ToDictionary(
                            x => buildDefinition.ParamDefinitions[x.Name].ArgName,
                            x => x.Values),
                    },
                }
                : null,
            ContinueOnError = null,
            Container = null,
            Services = null,
            Steps = BuildSteps(workflow, job),
        };

    private static Permissions? BuildPermissions(GithubTokenPermissionsOption? permissionOption) =>
        permissionOption?.Permissions switch
        {
            Permissions.All all => all,
            Permissions.Exact exact => exact.Permissions.IsAll(PermissionsLevel.Write)
                ? new Permissions.All(PermissionsLevel.Write)
                : exact.Permissions.IsAll(PermissionsLevel.Read)
                    ? new Permissions.All(PermissionsLevel.Read)
                    : exact.Permissions.IsAll(PermissionsLevel.None)
                        ? new Permissions.All(PermissionsLevel.None)
                        : exact.Permissions,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(permissionOption), permissionOption, null),
        };

    private List<Step> BuildSteps(WorkflowModel workflow, WorkflowJobModel job)
    {
        var additionalSteps = IAdditionalStepOption
            .GetOptions(job.TargetStep.Options)
            .ToList();

        // Special case: Add default checkout step if there isn't one
        if (!additionalSteps.Any(x => x is CheckoutStep))
            additionalSteps.Add(new GithubCheckoutStep
            {
                Enabled = true,
                FetchDepth = TextExpressions.From(0),
            });

        additionalSteps = additionalSteps.ToList();

        // Add pre-target additional steps
        var steps = new List<Step>(additionalSteps
            .Where(x => x.Order < 0)
            .OrderBy(x => x.Order)
            .Select(BuildAdditionalStep)
            .OfType<Step>());

        var matrixParams = job
            .TargetStep
            .MatrixDimensions
            .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].ArgName)
            .Select(name => (Name: name, Value: (TextExpression)TextExpressions
                .Raw("matrix")[name]
                .Evaluate()))
            .ToList();

        var buildSliceValue = matrixParams switch
        {
            { Count: 0 } => null,
            { Count: 1 } => matrixParams[0].Value,
            { Count: > 1 } => new ConcatExpression(matrixParams
                .Select((x, i) => i == matrixParams.Count - 1
                    ? x.Value
                    : new ConcatExpression([x.Value, "-"]))
                .ToArray()),
        };

        if (buildSliceValue is not null)
            matrixParams.Add(new("build-slice", buildSliceValue));

        var target = buildModel.GetTarget(job.TargetStep.Name);

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
                        .AsEnumerable()
                        .Select(x => x.ArtifactName)
                        .ToArray();

                    var retrieveTarget = buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact));

                    var retrieveStepEnv = BuildTargetStepEnv(workflow,
                        job,
                        retrieveTarget.Params,
                        retrieveTarget.ConsumedVariables,
                        matrixParams);

                    retrieveStepEnv["atom-artifacts"] = string.Join(",", artifactNames);

                    if (!retrieveStepEnv.ContainsKey("build-slice"))
                        if (slice.Key is { Length: > 0 })
                            retrieveStepEnv.Add("build-slice", slice.Key);
                        else if (buildSliceValue is not null)
                            retrieveStepEnv.Add("build-slice", buildSliceValue);

                    var retrieveArtifactsName = artifactNames switch
                    {
                        [var artifactName] => $"Retrieve artifact `{artifactName}`",
                        _ => artifactNames.Length < 60
                            ? $"Retrieve artifacts `{string.Join(", ", artifactNames)}`"
                            : "Retrieve multiple artifacts",
                    };

                    if (atomProjectData.IsFileBasedApp)
                    {
                        if (AppContext.GetData("EntryPointFilePath") is not string fileName)
                            throw new InvalidOperationException(
                                "AtomFileSystem reports file-based app but AppContext.EntryPointFilePath is null, cannot determine file path to run");

                        var filePathRelativeToRoot =
                            _atomFileSystem.FileSystem.Path.GetRelativePath(_atomFileSystem.AtomRootDirectory,
                                fileName);

                        steps.Add(new Step.RunStep
                        {
                            Name = retrieveArtifactsName,
                            Run = $"dotnet run --file {filePathRelativeToRoot} -- RetrieveArtifact --skip --headless",
                            Env = retrieveStepEnv,
                        });
                    }
                    else
                    {
                        var projectPath = FindProjectPath(_atomFileSystem, atomProjectData.ProjectName);

                        steps.Add(new Step.RunStep
                        {
                            Name = retrieveArtifactsName,
                            Run = $"dotnet run --project {projectPath} -- RetrieveArtifact --skip --headless",
                            Env = retrieveStepEnv,
                        });
                    }
                }
            else
                steps.AddRange(target.ConsumedArtifacts.Select(artifact => new Step.UsesStep
                {
                    Name = $"Retrieve {artifact.ArtifactName}",
                    Uses = "actions/download-artifact@v8",
                    With = new Dictionary<string, TextExpressionCollection>
                    {
                        ["name"] = artifact.BuildSlice is { Length: > 0 }
                            ? new ConcatExpression([artifact.ArtifactName, "-", artifact.BuildSlice])
                            : buildSliceValue is not null
                                ? new ConcatExpression([artifact.ArtifactName, "-", buildSliceValue])
                                : artifact.ArtifactName,
                        ["path"] = $"${{{{ github.workspace }}}}/.github/artifacts/{artifact.ArtifactName}",
                    },
                }));
        }

        var targetStepCondition = TargetStepCondition
                .GetOptions(job.TargetStep.Options)
                .ToList() switch
            {
                { Count: > 1 } multiple => new AndExpression(multiple
                    .Select(x => x.Condition)
                    .ToArray()),
                { Count: 1 } single => single[0].Condition,
                _ => null,
            };

        var targetStepEnv = BuildTargetStepEnv(workflow, job, target.Params, target.ConsumedVariables, matrixParams);

        if (atomProjectData.IsFileBasedApp)
        {
            if (AppContext.GetData("EntryPointFilePath") is not string fileName)
                throw new InvalidOperationException(
                    "AtomFileSystem reports file-based app but AppContext.EntryPointFilePath is null, cannot determine file path to run");

            var filePathRelativeToRoot =
                _atomFileSystem.FileSystem.Path.GetRelativePath(_atomFileSystem.AtomRootDirectory, fileName);

            steps.Add(new Step.RunStep
            {
                Id = job.TargetStep.Name,
                Name = job.TargetStep.Name,
                If = targetStepCondition,
                Run = $"dotnet run --file {filePathRelativeToRoot} -- {job.TargetStep.Name} --skip --headless",
                Env = targetStepEnv,
            });
        }
        else
        {
            var projectPath = FindProjectPath(_atomFileSystem, atomProjectData.ProjectName);

            steps.Add(new Step.RunStep
            {
                Id = job.TargetStep.Name,
                Name = job.TargetStep.Name,
                If = targetStepCondition,
                Run = $"dotnet run --project {projectPath} -- {job.TargetStep.Name} --skip --headless",
                Env = targetStepEnv,
            });
        }

        if (target.ProducedArtifacts.Count > 0 && !SuppressArtifactPublishingOption.IsEnabled(job.TargetStep.Options))
        {
            if (UseCustomArtifactProvider.Get(job.TargetStep.Options) is { Enabled: true })
                foreach (var slice in target.ProducedArtifacts.GroupBy(a => a.BuildSlice))
                {
                    var artifactNames = slice
                        .AsEnumerable()
                        .Select(x => x.ArtifactName)
                        .ToArray();

                    var storeTarget = buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact));

                    var storeStepEnv = BuildTargetStepEnv(workflow,
                        job,
                        storeTarget.Params,
                        storeTarget.ConsumedVariables,
                        matrixParams);

                    storeStepEnv["atom-artifacts"] = string.Join(",", artifactNames);

                    if (!storeStepEnv.ContainsKey("build-slice"))
                        if (slice.Key is { Length: > 0 })
                            storeStepEnv.Add("build-slice", slice.Key);
                        else if (buildSliceValue is not null)
                            storeStepEnv.Add("build-slice", buildSliceValue);

                    var storeArtifactsName = artifactNames switch
                    {
                        [var artifactName] => $"Store artifact `{artifactName}`",
                        _ => artifactNames.Length < 60
                            ? $"Store artifacts `{string.Join(", ", artifactNames)}`"
                            : "Store multiple artifacts",
                    };

                    if (atomProjectData.IsFileBasedApp)
                    {
                        if (AppContext.GetData("EntryPointFilePath") is not string fileName)
                            throw new InvalidOperationException(
                                "AtomFileSystem reports file-based app but AppContext.EntryPointFilePath is null, cannot determine file path to run");

                        var filePathRelativeToRoot =
                            _atomFileSystem.FileSystem.Path.GetRelativePath(_atomFileSystem.AtomRootDirectory,
                                fileName);

                        steps.Add(new Step.RunStep
                        {
                            Name = storeArtifactsName,
                            Run = $"dotnet run --file {filePathRelativeToRoot} -- StoreArtifact --skip --headless",
                            Env = storeStepEnv,
                        });
                    }
                    else
                    {
                        var projectPath = FindProjectPath(_atomFileSystem, atomProjectData.ProjectName);

                        steps.Add(new Step.RunStep
                        {
                            Name = storeArtifactsName,
                            Run = $"dotnet run --project {projectPath} -- StoreArtifact --skip --headless",
                            Env = storeStepEnv,
                        });
                    }
                }
            else
                steps.AddRange(target.ProducedArtifacts.Select(artifact => new Step.UsesStep
                {
                    Name = $"Store {artifact.ArtifactName}",
                    Uses = "actions/upload-artifact@v7",
                    With = new Dictionary<string, TextExpressionCollection>
                    {
                        ["name"] = artifact.BuildSlice is { Length: > 0 }
                            ? new ConcatExpression([artifact.ArtifactName, "-", artifact.BuildSlice])
                            : buildSliceValue is not null
                                ? new ConcatExpression([artifact.ArtifactName, "-", buildSliceValue])
                                : artifact.ArtifactName,
                        ["path"] = $"${{{{ github.workspace }}}}/.github/publish/{artifact.ArtifactName}",
                    },
                }));
        }

        steps.AddRange(additionalSteps
            .Where(x => x.Order > 0)
            .OrderBy(x => x.Order)
            .Select(BuildAdditionalStep)
            .OfType<Step>());

        return steps;
    }

    private static Step? BuildAdditionalStep(IAdditionalStepOption additionalStep) =>
        additionalStep switch
        {
            IGithubAdditionalStepOption { Enabled: true } githubStep => githubStep.Build(),
            IGithubAdditionalStepOption => null,
            SetupDotnetStep setupDotnetStep => BuildSetupDotnetStep(setupDotnetStep),
            AddNugetFeedsStep addNugetFeedsStep => BuildAddNugetFeedsStep(addNugetFeedsStep),
            _ => throw new InvalidOperationException(
                $"Unknown additional step type: {additionalStep.GetType().FullName}"),
        };

    private static Step.UsesStep BuildSetupDotnetStep(SetupDotnetStep step)
    {
        var with = new Dictionary<string, TextExpressionCollection>();

        if (step.DotnetVersion is not null)
        {
            with.Add("dotnet-version", step.DotnetVersion);

            if (step.Quality is not null)
                with.Add("quality", step.Quality.ToString()!.ToLowerInvariant());
        }

        return new()
        {
            Name = step.DotnetVersion is not null
                ? TextExpressions.Concat(["Setup .NET ", step.DotnetVersion])
                : "Setup .NET",
            Uses = "actions/setup-dotnet@v5",
            With = with.Count > 0
                ? with
                : null,
        };
    }

    private static Step.RunStep BuildAddNugetFeedsStep(AddNugetFeedsStep step)
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

        // If we are using .net 10+ then we can use the dotnet tool exec command instead of installing the tool to run it
        if (SemVer.TryParse(RuntimeInformation
                    .FrameworkDescription
                    .Replace(".NET ", "")
                    .Replace("x", "0"),
                out var version) &&
            version.Major >= 10)
            return new()
            {
                Name = "Setup NuGet",
                Shell = "bash",
                Env = feedsToAdd.ToDictionary<NugetFeedOptions, string, TextExpression>(
                    k => AddNugetFeedsStep.GetEnvVarNameForFeed(k.FeedName),
                    v => TextExpressions
                        .Raw("secrets")[v.SecretName]
                        .Evaluate()),
                Run = string.Join("\n",
                    feedsToAdd.Select(feedToAdd => step.SyncAtomToolVersionToLibraryVersion
                        ? $"dotnet tool exec decsm.atom.tool@{toolVersion} -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\""
                        : $"dotnet tool exec decsm.atom.tool -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"")),
            };

        return new()
        {
            Name = "Setup NuGet",
            Shell = "bash",
            Env = feedsToAdd.ToDictionary<NugetFeedOptions, string, TextExpression>(
                k => AddNugetFeedsStep.GetEnvVarNameForFeed(k.FeedName),
                v => TextExpressions
                    .Raw("secrets")[v.SecretName]
                    .Evaluate()),
            Run = string.Join("\n",
                feedsToAdd
                    .Select(feedToAdd => step.SyncAtomToolVersionToLibraryVersion
                        ? $"atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\" --tool-version \"{toolVersion}\""
                        : $"atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"")
                    .Prepend("dotnet tool update --global DecSm.Atom.Tool")),
        };
    }

    private Dictionary<string, TextExpression> BuildTargetStepEnv(
        WorkflowModel workflow,
        WorkflowJobModel job,
        IReadOnlyList<UsedParam> usedParams,
        IReadOnlyList<ConsumedVariable> consumedVariables,
        List<(string Name, TextExpression Value)> matrixParams)
    {
        var targetStepEnv = new Dictionary<string, TextExpression>();

        foreach (var githubManualTrigger in workflow.Triggers.OfType<ManualTrigger>())
        foreach (var input in githubManualTrigger.Inputs?.Where(i => usedParams
                     .Select(p => p.Param.ArgName)
                     .Any(p => p == i.Name)) ?? [])
            targetStepEnv[input.Name] = TextExpressions
                .Raw("inputs")[input.Name]
                .Evaluate();

        foreach (var consumedVariable in consumedVariables)
            targetStepEnv[buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName] = TextExpressions
                .Raw("needs")[consumedVariable.TargetName]["outputs"][buildDefinition
                    .ParamDefinitions[consumedVariable.VariableName].ArgName]
                .Evaluate();

        var requiredSecrets = usedParams
            .Where(x => x.Param.IsSecret)
            .Select(x => x)
            .ToArray();

        if (requiredSecrets.Any(x => x.Param.IsSecret))
        {
            foreach (var injectedSecret in workflow.Options.OfType<WorkflowSecretInjectionForSecretProvider>())
                if (buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.SecretName) is
                    { } paramDefinition)
                    targetStepEnv[paramDefinition.ArgName] = TextExpressions
                        .Raw("secrets")[paramDefinition.EnvVarName]
                        .Evaluate();

            foreach (var injectedEvVar in workflow.Options.OfType<WorkflowSecretsInjectionFromEnvironment>())
                if (buildDefinition.ParamDefinitions.GetValueOrDefault(injectedEvVar.SecretName) is { } paramDefinition)
                    targetStepEnv[paramDefinition.ArgName] = TextExpressions
                        .Raw("vars")[paramDefinition.EnvVarName]
                        .Evaluate();
        }

        foreach (var requiredSecret in requiredSecrets)
            if (WorkflowSecretInjection
                .GetOptions(job.TargetStep.Options)
                .Any(x => x.Value == requiredSecret.Param.Name))
                targetStepEnv[requiredSecret.Param.ArgName] = TextExpressions
                    .Raw("secrets")[requiredSecret.Param.EnvVarName]
                    .Evaluate();

        var environmentInjections = WorkflowParamInjectionFromEnvironment.GetOptions(job.TargetStep.Options);
        var paramInjections = WorkflowParamInjection.GetOptions(job.TargetStep.Options);
        var environmentVariableInjections = WorkflowEnvironmentVariableInjection.GetOptions(job.TargetStep.Options);

        environmentInjections = environmentInjections
            .Where(e => paramInjections.All(p => p.Name != e.Value))
            .ToList();

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

            targetStepEnv[paramDefinition.ArgName] = TextExpressions
                .Raw("vars")[paramDefinition.EnvVarName]
                .Evaluate();
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

            targetStepEnv[paramDefinition.ArgName] = paramInjection.InjectionExpression is EvaluateExpression
                ? paramInjection.InjectionExpression
                : paramInjection.InjectionExpression.Evaluate();
        }

        foreach (var environmentVariableInjection in environmentVariableInjections)
            targetStepEnv[environmentVariableInjection.Name] = environmentVariableInjection.Value is EvaluateExpression
                ? environmentVariableInjection.Value
                : environmentVariableInjection.Value.Evaluate();

        foreach (var matrixParam in matrixParams)
            targetStepEnv[matrixParam.Name] = matrixParam.Value;

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
