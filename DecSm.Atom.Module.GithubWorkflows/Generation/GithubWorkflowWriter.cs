namespace DecSm.Atom.Module.GithubWorkflows.Generation;

internal sealed class GithubWorkflowWriter(
    IAtomFileSystem fileSystem,
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    IParamService paramService,
    ILogger<GithubWorkflowWriter> logger,
    IWorkflowExpressionGenerator workflowExpressionGenerator
) : WorkflowFileWriter<GithubWorkflowType>(fileSystem, logger)
{
    private readonly IAtomFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".github" / "workflows";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        WriteLine($"name: {workflow.Name}");
        WriteLine();

        WritePermissions(workflow.Options);

        using (WriteSection("on:"))
        {
            var manualTrigger = workflow
                .Triggers
                .OfType<ManualTrigger>()
                .FirstOrDefault();

            if (manualTrigger is not null)
                using (WriteSection("workflow_dispatch:"))
                {
                    if (manualTrigger.Inputs?.Count > 0)
                        using (WriteSection("inputs:"))
                        {
                            foreach (var input in manualTrigger.Inputs)
                                using (WriteSection($"{input.Name}:"))
                                {
                                    WriteLine($"description: {input.Description}");

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
                                            {
                                                defaultBoolValue = boolInput.DefaultValue.Value;
                                            }
                                            else
                                            {
                                                using var defaultValuesOnlyScope =
                                                    paramService.CreateDefaultValuesOnlyScope();

                                                var accessedParam = buildDefinition.AccessParam(inputParamName);

                                                switch (accessedParam)
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
                                            }

                                            var isBoolRequired = input.Required ?? defaultBoolValue is null
                                                ? "true"
                                                : "false";

                                            WriteLine($"required: {isBoolRequired}");

                                            WriteLine("type: boolean");

                                            if (defaultBoolValue.HasValue)
                                                WriteLine($"default: {(defaultBoolValue.Value ? "true" : "false")}");

                                            break;
                                        }

                                        case ManualStringInput stringInput:
                                        {
                                            using var defaultValuesOnlyScope =
                                                paramService.CreateDefaultValuesOnlyScope();

                                            var defaultStringValue = stringInput.DefaultValue is { Length: > 0 }
                                                ? stringInput.DefaultValue
                                                : buildDefinition
                                                    .AccessParam(inputParamName)
                                                    ?.ToString();

                                            var isStringRequired =
                                                input.Required ?? defaultStringValue is not { Length: > 0 }
                                                    ? "true"
                                                    : "false";

                                            WriteLine($"required: {isStringRequired}");

                                            WriteLine("type: string");

                                            if (defaultStringValue is not null)
                                                WriteLine($"default: {defaultStringValue}");

                                            break;
                                        }

                                        case ManualChoiceInput choiceInput:
                                        {
                                            using var defaultValuesOnlyScope =
                                                paramService.CreateDefaultValuesOnlyScope();

                                            var defaultChoiceValue = choiceInput.DefaultValue is { Length: > 0 }
                                                ? choiceInput.DefaultValue
                                                : buildDefinition
                                                    .AccessParam(inputParamName)
                                                    ?.ToString();

                                            var isChoiceRequired =
                                                input.Required ?? defaultChoiceValue is not { Length: > 0 }
                                                    ? "true"
                                                    : "false";

                                            WriteLine($"required: {isChoiceRequired}");

                                            WriteLine("type: choice");

                                            using (WriteSection("options:"))
                                            {
                                                foreach (var choice in choiceInput.Choices)
                                                    WriteLine($"- {choice}");
                                            }

                                            if (defaultChoiceValue is not null)
                                                WriteLine($"default: {defaultChoiceValue}");

                                            break;
                                        }
                                    }
                                }
                        }
                }

            var releaseTriggers = workflow
                .Triggers
                .OfType<GithubReleaseTrigger>()
                .ToList();

            if (releaseTriggers.Count > 0)
            {
                using (WriteSection("release:"))
                    WriteLine($"types: [ {string.Join(", ", releaseTriggers.SelectMany(x => x.Types).Distinct())} ]");

                WriteLine();
            }

            foreach (var pullRequestTrigger in workflow.Triggers.OfType<GitPullRequestTrigger>())
                using (WriteSection("pull_request:"))
                {
                    if (pullRequestTrigger.IncludedBranches.Count > 0)
                        using (WriteSection("branches:"))
                        {
                            foreach (var branch in pullRequestTrigger.IncludedBranches)
                                WriteLine($"- '{branch}'");
                        }

                    if (pullRequestTrigger.ExcludedBranches.Count > 0)
                        using (WriteSection("branches-ignore:"))
                        {
                            foreach (var branch in pullRequestTrigger.ExcludedBranches)
                                WriteLine($"- '{branch}'");
                        }

                    if (pullRequestTrigger.IncludedPaths.Count > 0)
                        using (WriteSection("paths:"))
                        {
                            foreach (var path in pullRequestTrigger.IncludedPaths)
                                WriteLine($"- '{path}'");
                        }

                    if (pullRequestTrigger.ExcludedPaths.Count > 0)
                        using (WriteSection("paths-ignore:"))
                        {
                            foreach (var path in pullRequestTrigger.ExcludedPaths)
                                WriteLine($"- '{path}'");
                        }

                    // ReSharper disable once InvertIf
                    if (pullRequestTrigger.Types.Count > 0)
                        using (WriteSection("types:"))
                        {
                            foreach (var type in pullRequestTrigger.Types)
                                WriteLine($"- '{type}'");
                        }
                }

            foreach (var pushTrigger in workflow.Triggers.OfType<GitPushTrigger>())
                using (WriteSection("push:"))
                {
                    if (pushTrigger.IncludedBranches.Count > 0)
                        using (WriteSection("branches:"))
                        {
                            foreach (var branch in pushTrigger.IncludedBranches)
                                WriteLine($"- '{branch}'");
                        }

                    if (pushTrigger.ExcludedBranches.Count > 0)
                        using (WriteSection("branches-ignore:"))
                        {
                            foreach (var branch in pushTrigger.ExcludedBranches)
                                WriteLine($"- '{branch}'");
                        }

                    if (pushTrigger.IncludedPaths.Count > 0)
                        using (WriteSection("paths:"))
                        {
                            foreach (var path in pushTrigger.IncludedPaths)
                                WriteLine($"- '{path}'");
                        }

                    if (pushTrigger.ExcludedPaths.Count > 0)
                        using (WriteSection("paths-ignore:"))
                        {
                            foreach (var path in pushTrigger.ExcludedPaths)
                                WriteLine($"- '{path}'");
                        }

                    if (pushTrigger.IncludedTags.Count > 0)
                        using (WriteSection("tags:"))
                        {
                            foreach (var tag in pushTrigger.IncludedTags)
                                WriteLine($"- '{tag}'");
                        }

                    // ReSharper disable once InvertIf
                    if (pushTrigger.ExcludedTags.Count > 0)
                        using (WriteSection("tags-ignore:"))
                        {
                            foreach (var tag in pushTrigger.ExcludedTags)
                                WriteLine($"- '{tag}'");
                        }
                }
        }

        WriteLine();

        using (WriteSection("jobs:"))
        {
            foreach (var job in workflow.Jobs)
            {
                WriteLine();
                WriteJob(workflow, job);
            }
        }
    }

    private void WriteJob(WorkflowModel workflow, WorkflowJobModel job)
    {
        using (WriteSection($"{job.Name}:"))
        {
            var jobRequirementNames = job
                .JobDependencies
                .Distinct()
                .ToList();

            if (jobRequirementNames.Count > 0)
                WriteLine($"needs: [ {string.Join(", ", jobRequirementNames)} ]");

            if (job.MatrixDimensions.Count > 0)
                using (WriteSection("strategy:"))
                using (WriteSection("matrix:"))
                {
                    foreach (var dimension in job.MatrixDimensions)
                        WriteLine(
                            $"{buildDefinition.ParamDefinitions[dimension.Name].ArgName}: [ {string.Join(", ", dimension.Values)} ]");
                }

            var githubPlatformOption = job
                                           .Options
                                           .Concat(workflow.Options)
                                           .OfType<GithubRunsOn>()
                                           .FirstOrDefault() ??
                                       WorkflowOptions.Github.RunsOn.Ubuntu_Latest;

            var runOnLabels = githubPlatformOption.Labels.Count is 1
                ? workflowExpressionGenerator.Write(githubPlatformOption.Labels[0])
                : string.Join(", ", githubPlatformOption.Labels.Select(workflowExpressionGenerator.Write));

            var runOnGroup = workflowExpressionGenerator.Write(githubPlatformOption.Group);

            if (runOnGroup is { Length: > 0 })
                using (WriteSection("runs-on:"))
                {
                    WriteLine($"group: {runOnGroup}");
                    WriteLine($"labels: {runOnLabels}");
                }
            else
                WriteLine($"runs-on: {runOnLabels}");

            var snapshotImageOption = job
                .Options
                .Concat(workflow.Options)
                .OfType<GithubSnapshotImageOption>()
                .FirstOrDefault();

            if (snapshotImageOption is not null)
                using (WriteSection("snapshot:"))
                {
                    WriteLine($"image-name: {snapshotImageOption.ImageName}");

                    if (!string.IsNullOrWhiteSpace(snapshotImageOption.Version))
                        WriteLine($"version: {snapshotImageOption.Version}");
                }

            var environmentOptions = job
                .Options
                .Concat(workflow.Options)
                .OfType<DeployToEnvironment>()
                .ToList();

            foreach (var environmentOption in environmentOptions)
                WriteLine($"environment: {workflowExpressionGenerator.Write(environmentOption.EnvironmentName)}");

            var githubIfOptions = job
                .Options
                .Concat(workflow.Options)
                .OfType<RunTargetIf>()
                .ToList();

            foreach (var githubIfOption in githubIfOptions)
                WriteLine($"if: {workflowExpressionGenerator.Write(githubIfOption.Condition)}");

            WritePermissions(job.Options);

            var outputs = new List<string>();

            foreach (var step in job.Steps)
                outputs.AddRange(buildModel.GetTarget(step.Name)
                    .ProducedVariables);

            if (outputs.Count > 0)
                using (WriteSection("outputs:"))
                {
                    foreach (var output in outputs)
                        WriteLine(
                            $"{buildDefinition.ParamDefinitions[output].ArgName}: ${{{{ steps.{job.Name}.outputs.{buildDefinition.ParamDefinitions[output].ArgName} }}}}");
                }

            using (WriteSection("steps:"))
            {
                foreach (var step in job.Steps)
                {
                    WriteLine();
                    WriteStep(workflow, step, job);
                }
            }
        }
    }

    private void WritePermissions(IReadOnlyList<IWorkflowOption> options)
    {
        var githubPermissionsOption = options
            .OfType<GithubTokenPermissionsOption>()
            .FirstOrDefault();

        if (githubPermissionsOption is null)
            return;

        if (githubPermissionsOption == WorkflowOptions.Github.TokenPermissions.WriteAll)
            WriteLine("permissions: write-all");
        else if (githubPermissionsOption == WorkflowOptions.Github.TokenPermissions.ReadAll)
            WriteLine("permissions: read-all");
        else if (githubPermissionsOption == WorkflowOptions.Github.TokenPermissions.NoneAll)
            WriteLine("permissions: { }");
        else
            using (WriteSection("permissions:"))
            {
                foreach (var (key, value) in githubPermissionsOption.GetStrings)
                    WriteLine($"{key}: {value}");
            }
    }

    private void WriteStep(WorkflowModel workflow, WorkflowStepModel step, WorkflowJobModel job)
    {
        if (workflow
                .Options
                .Concat(step.Options)
                .OfType<GithubCheckoutOption>()
                .FirstOrDefault() is { } checkoutOption)
            using (WriteSection("- name: Checkout"))
            {
                WriteLine($"uses: actions/checkout@{checkoutOption.Version}");

                using (WriteSection("with:"))
                {
                    WriteLine("fetch-depth: 0");

                    if (checkoutOption.Lfs)
                        WriteLine("lfs: true");

                    if (!string.IsNullOrWhiteSpace(checkoutOption.Submodules))
                        WriteLine($"submodules: {checkoutOption.Submodules}");

                    if (!string.IsNullOrWhiteSpace(checkoutOption.Token))
                        WriteLine($"token: {checkoutOption.Token}");
                }
            }
        else
            using (WriteSection("- name: Checkout"))
            {
                WriteLine("uses: actions/checkout@v4");

                using (WriteSection("with:"))
                    WriteLine("fetch-depth: 0");
            }

        WriteLine();

        var commandStepTarget = buildModel.GetTarget(step.Name);

        var matrixParams = job
            .MatrixDimensions
            .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].ArgName)
            .Select(name => (Name: name, Value: $"${{{{ matrix.{name} }}}}"))
            .ToArray();

        var buildSlice = (Name: "build-slice", Value: string.Join("-", matrixParams.Select(x => x.Value)));

        if (!string.IsNullOrWhiteSpace(buildSlice.Value))
            matrixParams = matrixParams
                .Append(buildSlice)
                .ToArray();

        var setupDotnetSteps = workflow
            .Options
            .Concat(step.Options)
            .OfType<SetupDotnetStep>()
            .ToList();

        if (setupDotnetSteps.Count > 0)
            foreach (var setupDotnetStep in setupDotnetSteps)
            {
                using (WriteSection("- uses: actions/setup-dotnet@v4"))
                {
                    if (setupDotnetStep.DotnetVersion is not { Length: > 0 })
                        continue;

                    using (WriteSection("with:"))
                    {
                        // TODO: Use this to correctly handle quotes throughout the rest of the writer
                        var versionQuote = setupDotnetStep.DotnetVersion.Contains('\'')
                            ? '"'
                            : '\'';

                        WriteLine($"dotnet-version: {versionQuote}{setupDotnetStep.DotnetVersion}{versionQuote}");

                        var qualityQuote = setupDotnetStep.Quality.ToString()!.Contains('\'')
                            ? '"'
                            : '\'';

                        if (setupDotnetStep.Quality is not null)
                            WriteLine(
                                $"dotnet-quality: {qualityQuote}{setupDotnetStep.Quality.ToString()!.ToLower()}{qualityQuote}");

                        if (setupDotnetStep.Cache)
                            WriteLine("cache: true");

                        if (setupDotnetStep.LockFile is { Length: > 0 })
                            WriteLine($"cache-dependency-path: {setupDotnetStep.LockFile}");
                    }
                }

                WriteLine();
            }

        var setupNugetSteps = workflow
            .Options
            .Concat(step.Options)
            .OfType<AddNugetFeedsStep>()
            .ToList();

        if (setupNugetSteps.Count > 0)
        {
            var feedsToAdd = setupNugetSteps
                .SelectMany(x => x.FeedsToAdd)
                .DistinctBy(x => x.FeedName)
                .ToList();

            var syncAtomToolVersionToLibraryVersion = setupNugetSteps.Any(x => x.SyncAtomToolVersionToLibraryVersion);
            var toolVersion = "";

            if (syncAtomToolVersionToLibraryVersion)
            {
                if (SemVer.TryParse(typeof(AtomHost).Assembly
                                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                        ?.InformationalVersion ??
                                    "",
                        out var semVer))
                    toolVersion =
                        SemVer.Parse(
                            $"{semVer.Prefix}{(semVer.IsPreRelease ? $"-{semVer.PreRelease}" : string.Empty)}");
                else
                    throw new InvalidOperationException(
                        "Failed to parse DecSm.Atom.Host assembly version as SemVer for syncing atom tool version");
            }

            // If we know the SetupDotnet step was run for dotnet 10+,
            // then we can use the dotnet tool exec command instead of installing the tool to run it
            if (setupDotnetSteps.Any(x =>
                    SemVer.TryParse(x.DotnetVersion?.Replace("x", "0"), out var version) && version.Major >= 10))
            {
                using (WriteSection("- name: Setup NuGet"))
                {
                    using (WriteSection("run: |"))
                    {
                        foreach (var feedToAdd in feedsToAdd)
                            WriteLine(syncAtomToolVersionToLibraryVersion
                                ? $"dotnet tool exec decsm.atom.tool@{toolVersion} -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\""
                                : $"dotnet tool exec decsm.atom.tool -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"");
                    }

                    WriteLine("shell: bash");

                    using (WriteSection("env:"))
                    {
                        foreach (var feedToAdd in feedsToAdd)
                            WriteLine(
                                $$$"""{{{AddNugetFeedsStep.GetEnvVarNameForFeed(feedToAdd.FeedName)}}}: ${{ secrets.{{{feedToAdd.SecretName}}} }}""");
                    }
                }
            }
            else
            {
                using (WriteSection("- name: Install atom tool"))
                {
                    WriteLine("run: dotnet tool update --global DecSm.Atom.Tool");
                    WriteLine("shell: bash");
                }

                WriteLine();

                using (WriteSection("- name: Setup NuGet"))
                {
                    using (WriteSection("run: |"))
                    {
                        foreach (var feedToAdd in feedsToAdd)
                            WriteLine(
                                $"  atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"");
                    }

                    WriteLine("shell: bash");

                    using (WriteSection("env:"))
                    {
                        foreach (var feedToAdd in feedsToAdd)
                            WriteLine(
                                $$$"""{{{AddNugetFeedsStep.GetEnvVarNameForFeed(feedToAdd.FeedName)}}}: ${{ secrets.{{{feedToAdd.SecretName}}} }}""");
                    }
                }
            }
        }

        if (commandStepTarget.ConsumedArtifacts.Count > 0)
        {
            foreach (var consumedArtifact in commandStepTarget.ConsumedArtifacts)
                if (workflow
                    .Jobs
                    .SelectMany(x => x.Steps)
                    .Single(x => x.Name == consumedArtifact.TargetName)
                    .SuppressArtifactPublishing)
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} consumes artifact {ArtifactName} from target {SourceTargetName}, which has artifact publishing suppressed; this may cause the workflow to fail",
                        workflow.Name,
                        step.Name,
                        consumedArtifact.ArtifactName,
                        consumedArtifact.TargetName);

            if (workflow.Options.HasEnabledToggle<UseCustomArtifactProvider>())
                foreach (var slice in commandStepTarget.ConsumedArtifacts.GroupBy(a => a.BuildSlice))
                {
                    WriteLine();

                    WriteCommandStep(workflow,
                        new(nameof(IRetrieveArtifact.RetrieveArtifact)),
                        buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact)),
                        [
                            ("atom-artifacts", string.Join(",",
                                slice
                                    .AsEnumerable()
                                    .Select(x => x.ArtifactName))),
                            slice.Key is { Length: > 0 }
                                ? (Name: "build-slice", Value: slice.Key)
                                : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                    ? buildSlice
                                    : default,
                        ],
                        false);
                }
            else
                foreach (var artifact in commandStepTarget.ConsumedArtifacts)
                {
                    WriteLine();

                    using (WriteSection($"- name: Download {artifact.ArtifactName}"))
                    {
                        WriteLine("uses: actions/download-artifact@v4");

                        using (WriteSection("with:"))
                        {
                            WriteLine(artifact.BuildSlice is { Length: > 0 }
                                ? $"name: {artifact.ArtifactName}-{artifact.BuildSlice}"
                                : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                    ? $"name: {artifact.ArtifactName}-{buildSlice.Value}"
                                    : $"name: {artifact.ArtifactName}");

                            WriteLine($"path: \"{Github.PipelineArtifactDirectory}/{artifact.ArtifactName}\"");
                        }
                    }
                }
        }

        WriteCommandStep(workflow, step, commandStepTarget, matrixParams, true);

        // ReSharper disable once InvertIf
        if (commandStepTarget.ProducedArtifacts.Count > 0 && !step.SuppressArtifactPublishing)
        {
            if (workflow.Options.HasEnabledToggle<UseCustomArtifactProvider>())
                foreach (var slice in commandStepTarget.ProducedArtifacts.GroupBy(a => a.BuildSlice))
                {
                    WriteLine();

                    WriteCommandStep(workflow,
                        new(nameof(IStoreArtifact.StoreArtifact)),
                        buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact)),
                        [
                            ("atom-artifacts", string.Join(",",
                                slice
                                    .AsEnumerable()
                                    .Select(x => x.ArtifactName))),
                            slice.Key is { Length: > 0 }
                                ? (Name: "build-slice", Value: slice.Key)
                                : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                    ? buildSlice
                                    : default,
                        ],
                        false);
                }
            else
                foreach (var artifact in commandStepTarget.ProducedArtifacts)
                {
                    WriteLine();

                    using (WriteSection($"- name: Upload {artifact.ArtifactName}"))
                    {
                        WriteLine("uses: actions/upload-artifact@v4");

                        using (WriteSection("with:"))
                        {
                            WriteLine(artifact.BuildSlice is { Length: > 0 }
                                ? $"name: {artifact.ArtifactName}-{artifact.BuildSlice}"
                                : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                    ? $"name: {artifact.ArtifactName}-{buildSlice.Value}"
                                    : $"name: {artifact.ArtifactName}");

                            WriteLine($"path: \"{Github.PipelinePublishDirectory}/{artifact.ArtifactName}\"");
                        }
                    }
                }
        }
    }

    private void WriteCommandStep(
        WorkflowModel workflow,
        WorkflowStepModel workflowStep,
        TargetModel target,
        (string name, string value)[] extraParams,
        bool includeId)
    {
        var stepWriter = new GithubStepWriter(workflowExpressionGenerator, _fileSystem, StringBuilder, IndentLevel);

        if (workflow
            .Options
            .Concat(workflowStep.Options)
            .HasEnabledToggle<UseGithubForAtomBuildCache>())
        {
            var buildCacheRestoreSteps = workflow
                .Options
                .Concat(workflowStep.Options)
                .OfType<WorkflowCacheRestoreOption>()
                .ToList();

            foreach (var buildCacheRestoreStep in buildCacheRestoreSteps)
            {
                UseGithubForAtomBuildCache.WriteRestoreStep(stepWriter, buildCacheRestoreStep);
                stepWriter.ResetIndent();
                WriteLine();
            }
        }

        var customPreTargetSteps = workflowStep
            .Options
            .Concat(workflow.Options)
            .OfType<IGithubCustomStepOption>()
            .Where(x => x.Order is GithubCustomStepOrder.BeforeTarget)
            .OrderBy(x => x.Priority)
            .ToList();

        if (customPreTargetSteps.Count > 0)
            foreach (var customPostStep in customPreTargetSteps)
            {
                customPostStep.WriteStep(stepWriter);
                stepWriter.ResetIndent();
                WriteLine();
            }

        using (WriteSection($"- name: {workflowStep.Name}"))
        {
            if (includeId)
                WriteLine($"id: {workflowStep.Name}");

            var runTargetStepIf = workflow
                .Options
                .Concat(workflowStep.Options)
                .OfType<RunTargetStepIf>()
                .ToList();

            if (runTargetStepIf.Count > 0)
            {
                var condition = WorkflowExpressions.True.And(runTargetStepIf
                    .Select(x => x.Condition)
                    .ToArray());

                WriteLine($"if: {condition}");
            }

            var customAtomCommand = workflow
                .Options
                .Concat(workflowStep.Options)
                .OfType<CustomAtomCommand>()
                .FirstOrDefault();

            if (customAtomCommand is not null)
            {
                WriteLine(customAtomCommand.Write(workflow, workflowStep, _fileSystem));
            }
            else
            {
                if (_fileSystem.IsFileBasedApp)
                {
                    if (AppContext.GetData("EntryPointFilePath") is not string fileName)
                        throw new InvalidOperationException("EntryPointFilePath is null");

                    var filePathRelativeToRoot =
                        _fileSystem.FileSystem.Path.GetRelativePath(_fileSystem.AtomRootDirectory, fileName);

                    WriteLine(
                        $"run: dotnet run --file {filePathRelativeToRoot} -- {workflowStep.Name} --skip --headless");
                }
                else
                {
                    var projectPath = FindProjectPath(_fileSystem, _fileSystem.ProjectName);

                    WriteLine($"run: dotnet run --project {projectPath} -- {workflowStep.Name} --skip --headless");
                }
            }

            var env = new Dictionary<string, string>();

            foreach (var githubManualTrigger in workflow.Triggers.OfType<ManualTrigger>())
            {
                if (githubManualTrigger.Inputs is null or [])
                    continue;

                foreach (var input in githubManualTrigger.Inputs.Where(i => target
                             .Params
                             .Select(p => p.Param.ArgName)
                             .Any(p => p == i.Name)))
                    env[input.Name] = $"${{{{ inputs.{input.Name} }}}}";
            }

            foreach (var consumedVariable in target.ConsumedVariables)
                env[buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName] =
                    $"${{{{ needs.{consumedVariable.TargetName}.outputs.{buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName} }}}}";

            var requiredSecrets = target
                .Params
                .Where(x => x.Param.IsSecret)
                .Select(x => x)
                .ToArray();

            if (requiredSecrets.Any(x => x.Param.IsSecret))
            {
                foreach (var injectedSecret in workflow.Options.OfType<WorkflowSecretInjectionForSecretProvider>())
                {
                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.SecretName);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] = $"${{{{ secrets.{paramDefinition.EnvVarName} }}}}";
                }

                foreach (var injectedEvVar in workflow.Options.OfType<WorkflowSecretsInjectionFromEnvironment>())
                {
                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedEvVar.SecretName);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] = $"${{{{ vars.{paramDefinition.EnvVarName} }}}}";
                }
            }

            foreach (var requiredSecret in requiredSecrets)
            {
                var injectedSecret = workflow
                    .Options
                    .Concat(workflowStep.Options)
                    .OfType<WorkflowSecretInjection>()
                    .FirstOrDefault(x => x.Value == requiredSecret.Param.Name);

                if (injectedSecret is not null)
                    env[requiredSecret.Param.ArgName] = $"${{{{ secrets.{requiredSecret.Param.EnvVarName} }}}}";
            }

            var environmentInjections = workflow
                .Options
                .Concat(workflowStep.Options)
                .OfType<WorkflowParamInjectionFromEnvironment>()
                .Distinct();

            var paramInjections = workflow
                .Options
                .Concat(workflowStep.Options)
                .OfType<WorkflowParamInjection>()
                .Distinct();

            var environmentVariableInjections = workflow
                .Options
                .Concat(workflowStep.Options)
                .OfType<WorkflowEnvironmentVariableInjection>()
                .Distinct();

            environmentInjections = environmentInjections.Where(e => paramInjections.All(p => p.Name != e.Value));

            foreach (var environmentInjection in environmentInjections)
            {
                if (!buildDefinition.ParamDefinitions.TryGetValue(environmentInjection.Value, out var paramDefinition))
                {
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that does not exist",
                        workflow.Name,
                        workflowStep.Name,
                        environmentInjection.Value);

                    continue;
                }

                env[paramDefinition.ArgName] = $"${{{{ vars.{paramDefinition.EnvVarName} }}}}";
            }

            foreach (var paramInjection in paramInjections)
            {
                if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.Name, out var paramDefinition))
                {
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that is not consumed by the command",
                        workflow.Name,
                        workflowStep.Name,
                        paramInjection.Name);

                    continue;
                }

                env[paramDefinition.ArgName] =
                    $"${{{{ {workflowExpressionGenerator.Write(paramInjection.InjectionExpression)} }}}}";
            }

            foreach (var environmentVariableInjection in environmentVariableInjections)
                env[environmentVariableInjection.Name] = environmentVariableInjection.Value is LiteralExpression
                    ? $"${{{{ {workflowExpressionGenerator.Write(environmentVariableInjection.Value)} }}}}"
                    : workflowExpressionGenerator.Write(environmentVariableInjection.Value);

            var validEnv = env
                .Where(static x => x.Value is { Length: > 0 })
                .ToList();

            var validExtraParams = extraParams
                .Where(static x => x.value is { Length: > 0 })
                .ToList();

            // ReSharper disable once InvertIf
            if (validEnv.Count > 0 || validExtraParams.Count > 0)
                using (WriteSection("env:"))
                {
                    foreach (var (key, value) in validEnv)
                        WriteLine($"{key}: {value}");

                    foreach (var (key, value) in validExtraParams)
                        WriteLine($"{key}: {value}");
                }
        }

        var customPostTargetSteps = workflowStep
            .Options
            .Concat(workflow.Options)
            .OfType<IGithubCustomStepOption>()
            .Where(x => x.Order is GithubCustomStepOrder.AfterTarget)
            .OrderBy(x => x.Priority)
            .ToList();

        // ReSharper disable once InvertIf
        if (customPostTargetSteps.Count > 0)
        {
            var writer = new GithubStepWriter(workflowExpressionGenerator, _fileSystem, StringBuilder, IndentLevel);

            foreach (var customPostStep in customPostTargetSteps)
            {
                WriteLine();
                customPostStep.WriteStep(writer);
                writer.ResetIndent();
            }
        }

        foreach (var _ in workflow
                     .Options
                     .Concat(workflowStep.Options)
                     .OfType<CleanAtomDirectory>())
        {
            WriteLine();
            WriteLine("- run: |");
            WriteLine("    git reset --hard HEAD");
            WriteLine("    git clean -xdf \"_atom\"");
        }

        if (workflow
            .Options
            .Concat(workflowStep.Options)
            .HasEnabledToggle<UseGithubForAtomBuildCache>())
        {
            var buildCacheSaveSteps = workflow
                .Options
                .Concat(workflowStep.Options)
                .OfType<WorkflowCacheSaveOption>()
                .ToList();

            foreach (var buildCacheSaveStep in buildCacheSaveSteps)
            {
                WriteLine();
                UseGithubForAtomBuildCache.WriteSaveStep(stepWriter, buildCacheSaveStep);
                stepWriter.ResetIndent();
            }
        }
    }

    private static string FindProjectPath(IAtomFileSystem fileSystem, string projectName)
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
