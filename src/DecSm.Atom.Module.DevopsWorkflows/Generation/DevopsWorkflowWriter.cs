namespace DecSm.Atom.Module.DevopsWorkflows.Generation;

internal sealed partial class DevopsWorkflowWriter(
    IAtomFileSystem fileSystem,
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    IWorkflowExpressionResolver workflowExpressionResolver,
    ILogger<DevopsWorkflowWriter> logger
) : WorkflowFileWriter<DevopsWorkflowType>(fileSystem, logger)
{
    private readonly IAtomFileSystem _fileSystem = fileSystem;

    protected override string FileExtension => "yml";

    protected override int TabSize => 2;

    protected override RootedPath FileLocation => _fileSystem.AtomRootDirectory / ".devops" / "workflows";

    [GeneratedRegex("-+")]
    private static partial Regex HyphenReductionRegex();

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        Writer.WriteLine($"name: {workflow.Name}");
        Writer.WriteLine();

        var manualTrigger = workflow
            .Triggers
            .OfType<ManualTrigger>()
            .FirstOrDefault();

        if (manualTrigger is { Inputs.Count: > 0 })
            using (Writer.WriteSection("parameters:"))
            {
                foreach (var input in manualTrigger.Inputs)
                    using (Writer.WriteSection($"- name: {input.Name}"))
                    {
                        Writer.WriteLine($"displayName: '{input.Name} | {input.Description}'");

                        switch (input)
                        {
                            case ManualBoolInput boolInput:

                                Writer.WriteLine("type: boolean");

                                if (boolInput.DefaultValue is not null)
                                    Writer.WriteLine($"default: '{boolInput.DefaultValue.Value}'");

                                break;

                            case ManualStringInput stringInput:

                                Writer.WriteLine("type: string");

                                if (stringInput.DefaultValue is not null)
                                    Writer.WriteLine($"default: '{stringInput.DefaultValue}'");

                                break;

                            case ManualChoiceInput choiceInput:

                                Writer.WriteLine("type: string");

                                Writer.WriteLine(choiceInput.DefaultValue is not null
                                    ? $"default: {choiceInput.DefaultValue}"
                                    : $"default: '{choiceInput.Choices[0]}'");

                                using (Writer.WriteSection("values:"))
                                {
                                    foreach (var choice in choiceInput.Choices)
                                        Writer.WriteLine($"- '{choice}'");
                                }

                                break;
                        }
                    }
            }

        // TODO: Variables

        var variableGroups = workflow
            .Options
            .OfType<DevopsVariableGroup>()
            .ToArray();

        if (variableGroups.Length > 0)
            using (Writer.WriteSection("variables:"))
            {
                foreach (var variableGroup in variableGroups)
                    Writer.WriteLine($"- group: {variableGroup.Name}");
            }

        var pushTriggers = workflow
            .Triggers
            .OfType<GitPushTrigger>()
            .ToArray();

        if (pushTriggers.Length > 0)
            using (Writer.WriteSection("trigger:"))
            {
                foreach (var pushTrigger in pushTriggers)
                {
                    using (Writer.WriteSection("branches:"))
                    {
                        if (pushTrigger.IncludedBranches.Count > 0)
                            using (Writer.WriteSection("include:"))
                            {
                                foreach (var branch in pushTrigger.IncludedBranches)
                                    Writer.WriteLine($"- '{branch}'");
                            }

                        if (pushTrigger.ExcludedBranches.Count > 0)
                            using (Writer.WriteSection("exclude:"))
                            {
                                foreach (var branch in pushTrigger.ExcludedBranches)
                                    Writer.WriteLine($"- '{branch}'");
                            }
                    }

                    if (pushTrigger.IncludedPaths.Count > 0 || pushTrigger.ExcludedPaths.Count > 0)
                        using (Writer.WriteSection("paths:"))
                        {
                            if (pushTrigger.IncludedPaths.Count > 0)
                                using (Writer.WriteSection("include:"))
                                {
                                    foreach (var path in pushTrigger.IncludedPaths)
                                        Writer.WriteLine($"- '{path}'");
                                }

                            if (pushTrigger.ExcludedPaths.Count > 0)
                                using (Writer.WriteSection("exclude:"))
                                {
                                    foreach (var path in pushTrigger.ExcludedPaths)
                                        Writer.WriteLine($"- '{path}'");
                                }
                        }

                    // ReSharper disable once InvertIf
                    if (pushTrigger.IncludedTags.Count > 0 || pushTrigger.ExcludedTags.Count > 0)
                        using (Writer.WriteSection("tags:"))
                        {
                            if (pushTrigger.IncludedTags.Count > 0)
                                using (Writer.WriteSection("include:"))
                                {
                                    foreach (var tag in pushTrigger.IncludedTags)
                                        Writer.WriteLine($"- '{tag}'");
                                }

                            // ReSharper disable once InvertIf
                            if (pushTrigger.ExcludedTags.Count > 0)
                                using (Writer.WriteSection("exclude:"))
                                {
                                    foreach (var tag in pushTrigger.ExcludedTags)
                                        Writer.WriteLine($"- '{tag}'");
                                }
                        }
                }
            }

        if (manualTrigger is null && pushTriggers.Length is 0)
            Writer.WriteLine("trigger: none");

        Writer.WriteLine();

        using (Writer.WriteSection("jobs:"))
        {
            foreach (var job in workflow.Jobs)
            {
                Writer.WriteLine();
                WriteJob(workflow, job);
            }
        }
    }

    private void WriteJob(WorkflowModel workflow, WorkflowJobModel job)
    {
        using (Writer.WriteSection($"- job: {job.Name}"))
        {
            var jobRequirementNames = job.JobDependencies;

            if (jobRequirementNames.Count > 0)
                Writer.WriteLine($"dependsOn: [ {string.Join(", ", jobRequirementNames)} ]");

            if (job.MatrixDimensions.Count > 0)
                using (Writer.WriteSection("strategy:"))
                using (Writer.WriteSection("matrix:"))
                {
                    var dimensions = job
                        .MatrixDimensions
                        .Select(d => new MatrixDimension(d.Name)
                        {
                            Values = d.Values,
                        })
                        .ToArray();

                    var dimensionValues = dimensions
                        .Select(d => d.Values)
                        .ToArray();

                    var dimensionNames = dimensions
                        .Select(d => d.Name)
                        .ToArray();

                    var dimensionValueCounts = dimensionValues
                        .Select(d => d.Count)
                        .ToArray();

                    var dimensionValueIndices = new int[dimensionValues.Length];

                    var counter = 1;

                    while (true)
                    {
                        // Compute the current dimension values based on the indices
                        var currentDimensionValues = dimensionValueIndices
                            .Select((index, i) => dimensionValues[i][index])

                            // Replace any invalid characters in the dimension value with a hyphen
                            .Select(value => new string(workflowExpressionResolver
                                .Resolve(value)
                                .Select(c => char.IsLetterOrDigit(c)
                                    ? c
                                    : '-')
                                .ToArray()))

                            // Replace multiple hyphens with a single hyphen
                            .Select(value => HyphenReductionRegex()
                                .Replace(value, "-"))

                            // Trim hyphens from the start and end of the dimension value
                            .Select(value => value.Trim('-'))
                            .ToArray();

                        var dimensionValueName = $"{counter++:D3}_{string.Join("_", currentDimensionValues)}";

                        using (Writer.WriteSection($"{dimensionValueName}:"))
                        {
                            for (var i = 0; i < dimensions.Length; i++)
                                Writer.WriteLine(
                                    $"{buildDefinition.ParamDefinitions[dimensionNames[i]].ArgName}: '{dimensionValues[i][dimensionValueIndices[i]]}'");
                        }

                        var dimensionIndex = 0;

                        while (dimensionIndex < dimensionValues.Length)
                        {
                            if (dimensionValueIndices[dimensionIndex] < dimensionValueCounts[dimensionIndex] - 1)
                            {
                                dimensionValueIndices[dimensionIndex]++;

                                break;
                            }

                            dimensionValueIndices[dimensionIndex] = 0;
                            dimensionIndex++;
                        }

                        if (dimensionIndex == dimensionValues.Length)
                            break;
                    }
                }

            var poolOption = workflow
                                 .Options
                                 .OfType<DevopsPool>()
                                 .FirstOrDefault() ??
                             WorkflowOptions.Devops.DevopsPool.Ubuntu_Latest;

            using (Writer.WriteSection("pool:"))
            {
                var hostedImage = workflowExpressionResolver.Resolve(poolOption.HostedImage);
                var name = workflowExpressionResolver.Resolve(poolOption.Name);

                var demands = poolOption
                    .Demands
                    .Select(workflowExpressionResolver.Resolve)
                    .ToArray();

                if (hostedImage is { Length: > 0 })
                    Writer.WriteLine($"vmImage: {hostedImage}");
                else if (name is { Length: > 0 })
                    Writer.WriteLine($"name: {name}");

                if (demands.Length > 0)
                    using (Writer.WriteSection("demands:"))
                    {
                        foreach (var demand in demands)
                            Writer.WriteLine($"- {demand}");
                    }
            }

            var environmentOption = GetOption<DeployToEnvironment>(workflow, job.TargetStep);

            if (environmentOption is not null)
                using (Writer.WriteSection("environment:"))
                    Writer.WriteLine($"name: {workflowExpressionResolver.Resolve(environmentOption.EnvironmentName)}");

            var variables = new Dictionary<string, string>();

            var targetsForConsumedVariableDeclaration = new List<TargetModel>();

            var target = buildModel.GetTarget(job.TargetStep.Name);
            targetsForConsumedVariableDeclaration.Add(target);

            if (GetOption<UseCustomArtifactProvider>(workflow) is { Value: true } &&
                GetOption<SuppressArtifactPublishingOption>(workflow, job.TargetStep) is not { Value: true })
            {
                if (target.ConsumedArtifacts.Count > 0)
                    targetsForConsumedVariableDeclaration.Add(
                        buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact)));

                if (target.ProducedArtifacts.Count > 0)
                    targetsForConsumedVariableDeclaration.Add(
                        buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact)));
            }

            foreach (var consumedVariable in targetsForConsumedVariableDeclaration.SelectMany(x => x.ConsumedVariables))
            {
                var variableName = buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName;

                variables[variableName] =
                    $"$[ dependencies.{consumedVariable.TargetName}.outputs['{consumedVariable.TargetName}.{variableName}'] ]";
            }

            if (variables.Count > 0)
                using (Writer.WriteSection("variables:"))
                {
                    foreach (var (name, value) in variables)
                        Writer.WriteLine($"{name}: {value}");
                }

            using (Writer.WriteSection("steps:"))
            {
                Writer.WriteLine();
                WriteStep(workflow, job.TargetStep, job);
            }
        }
    }

    private void WriteStep(WorkflowModel workflow, WorkflowStepModel step, WorkflowJobModel job)
    {
        if (workflow
                .Options
                .Concat(step.Options)
                .OfType<DevopsCheckoutOption>()
                .FirstOrDefault() is { } checkoutOption)
            using (Writer.WriteSection("- checkout: self"))
            {
                Writer.WriteLine("fetchDepth: 0");

                if (checkoutOption.Lfs)
                    Writer.WriteLine("lfs: true");

                if (!string.IsNullOrWhiteSpace(checkoutOption.Submodules))
                    Writer.WriteLine($"submodules: {checkoutOption.Submodules}");
            }
        else
            using (Writer.WriteSection("- checkout: self"))
                Writer.WriteLine("fetchDepth: 0");

        Writer.WriteLine();

        var commandStepTarget = buildModel.GetTarget(step.Name);

        var matrixParams = job
            .MatrixDimensions
            .Select(dimension => buildDefinition.ParamDefinitions[dimension.Name].ArgName)
            .Select(name => (Name: name, Value: $"$({name})"))
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
                using (Writer.WriteSection("- task: UseDotNet@2"))
                {
                    if (workflowExpressionResolver.Resolve(setupDotnetStep.DotnetVersion) is not
                        {
                            Length: > 0,
                        } dotnetVersion)
                        continue;

                    using (Writer.WriteSection("inputs:"))
                    {
                        Writer.WriteLine($"version: '{workflowExpressionResolver.Resolve(dotnetVersion)}'\n");

                        if (setupDotnetStep.Quality is not null)
                            Writer.WriteLine("includePreviewVersions: 'true'");
                    }
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

            // If we know the SetupDotnet step was run for dotnet 10+,
            // then we can use the dotnet tool exec command instead of installing the tool to run it
            if (setupDotnetSteps.Any(x => SemVer.TryParse(workflowExpressionResolver
                                                  .Resolve(x.DotnetVersion)
                                                  ?.Replace("x", "0"),
                                              out var version) &&
                                          version.Major >= 10))
            {
                using (Writer.WriteSection("- script: |"))
                {
                    foreach (var feedToAdd in feedsToAdd)
                        Writer.WriteLine(
                            $"dotnet tool exec decsm.atom.tool -y -- nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"");

                    Writer.WriteLine("displayName: 'Setup NuGet'");

                    using (Writer.WriteSection("env:"))
                    {
                        foreach (var feedToAdd in feedsToAdd)
                            Writer.WriteLine(
                                $"{AddNugetFeedsStep.GetEnvVarNameForFeed(feedToAdd.FeedName)}: $({feedToAdd.SecretName})");
                    }

                    Writer.WriteLine();
                }
            }
            else
            {
                using (Writer.WriteSection("- script: dotnet tool update --global DecSm.Atom.Tool"))
                    Writer.WriteLine("displayName: 'Install atom tool'");

                Writer.WriteLine();

                using (Writer.WriteSection("- script: |"))
                {
                    foreach (var feedToAdd in feedsToAdd)
                        Writer.WriteLine(
                            $"  atom nuget-add --name \"{feedToAdd.FeedName}\" --url \"{feedToAdd.FeedUrl}\"");

                    Writer.WriteLine("displayName: 'Setup NuGet'");

                    using (Writer.WriteSection("env:"))
                    {
                        foreach (var feedToAdd in feedsToAdd)
                            Writer.WriteLine(
                                $"{AddNugetFeedsStep.GetEnvVarNameForFeed(feedToAdd.FeedName)}: $({feedToAdd.SecretName})");
                    }

                    Writer.WriteLine();
                }
            }
        }

        if (commandStepTarget.ConsumedArtifacts.Count > 0)
        {
            foreach (var consumedArtifact in commandStepTarget.ConsumedArtifacts)
            {
                if (SuppressArtifactPublishingOption.IsOptionEnabled(workflow, step))
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} consumes artifact {ArtifactName} from target {SourceTargetName}, which has artifact publishing suppressed; this may cause the workflow to fail",
                        workflow.Name,
                        step.Name,
                        consumedArtifact.ArtifactName,
                        consumedArtifact.TargetName);
            }

            if (GetOption<UseCustomArtifactProvider>(workflow) is { Value: true })
            {
                WriteCommandStep(workflow,
                    new(nameof(IRetrieveArtifact.RetrieveArtifact)),
                    buildModel.GetTarget(nameof(IRetrieveArtifact.RetrieveArtifact)),
                    [
                        ("atom-artifacts",
                            string.Join(",", commandStepTarget.ConsumedArtifacts.Select(x => x.ArtifactName))),
                        !string.IsNullOrWhiteSpace(buildSlice.Value)
                            ? buildSlice
                            : default,
                    ],
                    false);

                Writer.WriteLine();
            }
            else
            {
                foreach (var artifact in commandStepTarget.ConsumedArtifacts)
                {
                    using (Writer.WriteSection("- task: DownloadPipelineArtifact@2"))
                    {
                        Writer.WriteLine($"displayName: {artifact.ArtifactName}");

                        using (Writer.WriteSection("inputs:"))
                        {
                            Writer.WriteLine(artifact.BuildSlice is { Length: > 0 }
                                ? $"artifact: {artifact.ArtifactName}-{artifact.BuildSlice}"
                                : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                    ? $"artifact: {artifact.ArtifactName}-{buildSlice.Value}"
                                    : $"artifact: {artifact.ArtifactName}");

                            Writer.WriteLine($"path: \"{Devops.PipelineArtifactDirectory}/{artifact.ArtifactName}\"");
                        }
                    }

                    Writer.WriteLine();
                }
            }
        }

        WriteCommandStep(workflow, step, commandStepTarget, matrixParams, true);

        // ReSharper disable once InvertIf
        if (commandStepTarget.ProducedArtifacts.Count > 0 &&
            !SuppressArtifactPublishingOption.IsOptionEnabled(workflow, step))
        {
            if (UseCustomArtifactProvider.IsOptionEnabled(workflow))
            {
                Writer.WriteLine();

                WriteCommandStep(workflow,
                    new(nameof(IStoreArtifact.StoreArtifact)),
                    buildModel.GetTarget(nameof(IStoreArtifact.StoreArtifact)),
                    [
                        ("atom-artifacts",
                            string.Join(",", commandStepTarget.ProducedArtifacts.Select(x => x.ArtifactName))),
                        !string.IsNullOrWhiteSpace(buildSlice.Value)
                            ? buildSlice
                            : default,
                    ],
                    false);
            }
            else
            {
                if (commandStepTarget.ProducedArtifacts.Count > 0)
                    Writer.WriteLine();

                foreach (var artifact in commandStepTarget.ProducedArtifacts)
                {
                    using (Writer.WriteSection("- task: PublishPipelineArtifact@1"))
                    {
                        Writer.WriteLine($"displayName: {artifact.ArtifactName}");

                        using (Writer.WriteSection("inputs:"))
                        {
                            Writer.WriteLine(artifact.BuildSlice is { Length: > 0 }
                                ? $"artifactName: {artifact.ArtifactName}-{artifact.BuildSlice}"
                                : !string.IsNullOrWhiteSpace(buildSlice.Value)
                                    ? $"artifactName: {artifact.ArtifactName}-{buildSlice.Value}"
                                    : $"artifactName: {artifact.ArtifactName}");

                            Writer.WriteLine(
                                $"targetPath: \"{Devops.PipelinePublishDirectory}/{artifact.ArtifactName}\"");
                        }
                    }

                    Writer.WriteLine();
                }
            }
        }
    }

    private void WriteCommandStep(
        WorkflowModel workflow,
        WorkflowStepModel workflowStep,
        TargetModel target,
        (string name, string value)[] extraParams,
        bool includeName)
    {
        string runScript;

        if (_fileSystem.IsFileBasedApp)
        {
            if (AppContext.GetData("EntryPointFilePath") is not string fileName)
                throw new InvalidOperationException("EntryPointFilePath is null");

            var filePathRelativeToRoot =
                _fileSystem.FileSystem.Path.GetRelativePath(_fileSystem.AtomRootDirectory, fileName);

            runScript = $"- script: dotnet run --file {filePathRelativeToRoot} {workflowStep.Name} --skip --headless";
        }
        else
        {
            var projectPath = FindProjectPath(_fileSystem, _fileSystem.ProjectName);

            runScript = $"- script: dotnet run --project {projectPath} {workflowStep.Name} --skip --headless";
        }

        using (Writer.WriteSection(runScript))
        {
            if (includeName)
                Writer.WriteLine($"name: {workflowStep.Name}");

            var env = new Dictionary<string, string>();

            foreach (var githubManualTrigger in workflow.Triggers.OfType<ManualTrigger>())
            {
                if (githubManualTrigger.Inputs is null or [])
                    continue;

                foreach (var input in githubManualTrigger.Inputs.Where(i => target
                             .Params
                             .Select(p => p.Param.ArgName)
                             .Any(p => p == i.Name)))
                    env[input.Name] = $"${{{{ parameters.{input.Name} }}}}";
            }

            foreach (var consumedVariable in target.ConsumedVariables)
                env[buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName] =
                    $"$({buildDefinition.ParamDefinitions[consumedVariable.VariableName].ArgName})";

            var requiredSecrets = target
                .Params
                .Where(x => x.Param.IsSecret)
                .Select(x => x)
                .ToArray();

            if (requiredSecrets.Any(x => x.Param.IsSecret))
            {
                foreach (var injectedSecret in workflow.Options.OfType<WorkflowSecretInjectionForSecretProvider>())
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - Better UX
                    if (injectedSecret.SecretName is null)
                    {
                        logger.LogWarning(
                            "Workflow {WorkflowName} command {CommandName} has a secret injection with a null value",
                            workflow.Name,
                            workflowStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedSecret.SecretName);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] = $"$({paramDefinition.EnvVarName})";
                }

                foreach (var injectedEnvVar in workflow.Options.OfType<WorkflowSecretsInjectionFromEnvironment>())
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - Better UX
                    if (injectedEnvVar.SecretName is null)
                    {
                        logger.LogWarning(
                            "Workflow {WorkflowName} command {CommandName} has a secret environment variable injection with a null value",
                            workflow.Name,
                            workflowStep.Name);

                        continue;
                    }

                    var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(injectedEnvVar.SecretName);

                    if (paramDefinition is not null)
                        env[paramDefinition.ArgName] = $"$({paramDefinition.EnvVarName})";
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
                    env[requiredSecret.Param.ArgName] = $"$({requiredSecret.Param.EnvVarName})";
            }

            var paramInjections = GetOptions<WorkflowParamInjection>(workflow, workflowStep)
                .ToList();

            var environmentInjections = GetOptions<WorkflowParamInjectionFromEnvironment>(workflow, workflowStep)
                .Where(e => paramInjections.All(p => p.Name != e.Value));

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

                env[paramDefinition.ArgName] = $"$({paramDefinition.EnvVarName})";
            }

            foreach (var paramInjection in paramInjections)
            {
                if (!buildDefinition.ParamDefinitions.TryGetValue(paramInjection.Name, out var paramDefinition))
                {
                    logger.LogWarning(
                        "Workflow {WorkflowName} command {CommandName} has an injection for parameter {ParamName} that does not exist",
                        workflow.Name,
                        workflowStep.Name,
                        paramInjection.Name);

                    continue;
                }

                env[paramDefinition.ArgName] = workflowExpressionResolver.Resolve(paramInjection.InjectionExpression);
            }

            var validEnv = env
                .Where(static x => x.Value is { Length: > 0 })
                .ToList();

            var validExtraParams = extraParams
                .Where(static x => x.value is { Length: > 0 })
                .ToList();

            // ReSharper disable once InvertIf
            if (validEnv.Count > 0 || validExtraParams.Count > 0)
                using (Writer.WriteSection("env:"))
                {
                    foreach (var (key, value) in validEnv)
                        Writer.WriteLine($"{key}: {value}");

                    foreach (var (key, value) in validExtraParams)
                        Writer.WriteLine($"{key}: {value}");
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
