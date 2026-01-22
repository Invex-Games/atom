namespace DecSm.Atom.Build;

/// <summary>
///     Resolves the complete <see cref="BuildModel" /> by processing target definitions, dependencies, and parameters.
/// </summary>
/// <param name="buildDefinition">The core build definition.</param>
/// <param name="paramService">The service for resolving parameters.</param>
/// <param name="commandLineArgs">The parsed command-line arguments.</param>
/// <param name="logger">The logger for diagnostics.</param>
internal sealed class BuildResolver(
    IBuildDefinition buildDefinition,
    IParamService paramService,
    CommandLineArgs commandLineArgs,
    ILogger<BuildResolver> logger
)
{
    /// <summary>
    ///     Resolves and constructs the <see cref="BuildModel" /> for the current build.
    /// </summary>
    /// <returns>A fully resolved <see cref="BuildModel" /> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if duplicate or circular target dependencies are detected.</exception>
    public BuildModel Resolve()
    {
        var startTime = Stopwatch.GetTimestamp();

        var defaultValuesOnlyScope = paramService.CreateDefaultValuesOnlyScope();

        var paramModels = buildDefinition
            .ParamDefinitions
            .Select(x => new ParamModel(x.Key)
            {
                ArgName = x.Value.ArgName,
                Description = x.Value.Description,
                DefaultValue = buildDefinition
                    .AccessParam(x.Value.Name)
                    ?.ToString(),
                Sources = x.Value.Sources,
                IsSecret = x.Value.IsSecret,
                ChainedParams = x.Value.ChainedParams,
            })
            .ToDictionary(x => x.Name);

        defaultValuesOnlyScope.Dispose();

        var specifiedTargets = commandLineArgs
            .Commands
            .Select(x => x.Name)
            .ToArray();

        // Whether to first execute dependencies of a target before the target itself
        var includeDependencies = !commandLineArgs.HasSkip;

        // Extract target definitions from build definition
        var targetDefinitions = buildDefinition
            .TargetDefinitions
            .Select(x => x
                .Value(new()
                {
                    Name = x.Key,
                })
                .ApplyExtensions(buildDefinition))
            .ToArray();

        // Duplicate target names could result in undefined behavior,
        // So we fail early if any are found
        // Note: This doesn't include overriden or extended targets
        var duplicateTargetNames = targetDefinitions
            .GroupBy(x => x.Name)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();

        if (duplicateTargetNames.Length > 0)
            throw new(
                $"One or more targets are defined multiple times, which is not allowed: {string.Join(", ", $"'{duplicateTargetNames}'")}.");

        // Allows mutation of target model dependencies within this scope
        var targetModelDependencyMap = new Dictionary<string, List<TargetModel>>();

        // Transform target definitions into target models
        var targetModels = targetDefinitions
            .Select(x =>
            {
                var dependencies = new List<TargetModel>();
                targetModelDependencyMap.Add(x.Name, dependencies);

                // We want to get not only the used/required params, but also recursively the params that are used by those params ('chained params')
                // The logic is basic for now - a chained param is required if the root is required
                var usedParams = new List<UsedParam>();

                foreach (var param in x.Params)
                    AddParamAndChildren(param.Param, param.Required, usedParams, paramModels);

                return new TargetModel(x.Name, x.Description, x.Hidden)
                {
                    Tasks = x.Tasks,
                    Params = usedParams,
                    ConsumedArtifacts = x.ConsumedArtifacts,
                    ProducedArtifacts = x.ProducedArtifacts,
                    ConsumedVariables = x.ConsumedVariables,
                    ProducedVariables = x.ProducedVariables,
                    Dependencies = dependencies,
                    DeclaringAssembly = buildDefinition.TargetDefinitions[x.Name].Method.ReflectedType!.Assembly,
                };
            })
            .ToArray();

        // Copy target definition dependencies to their respective target models
        foreach (var targetDefinition in targetDefinitions)
        {
            var targetModel = targetModels.First(x => x.Name == targetDefinition.Name);

            var dependencyNames = targetDefinition
                .Dependencies
                .Concat(targetDefinition.ConsumedArtifacts.Select(x => x.TargetName))
                .Concat(targetDefinition.ConsumedVariables.Select(x => x.TargetName))
                .Distinct();

            foreach (var dependencyName in dependencyNames)
            {
                var dependencyTargetDefinition = targetDefinitions.FirstOrDefault(x => x.Name == dependencyName);

                if (dependencyTargetDefinition is null)
                    throw new(
                        $"Target '{targetModel.Name}' depends on target '{dependencyName}' which does not exist.");

                targetModelDependencyMap[targetModel.Name]
                    .Add(targetModels.First(x => x.Name == dependencyName));
            }
        }

        // Sort targets and find circular dependencies
        var depthFirstTargets = new List<TargetModel>();
        var targetMarks = targetModels.ToDictionary(x => x, _ => (Temporary: false, Permenant: false));

        for (var target = targetModels.FirstOrDefault(x => !targetMarks[x].Permenant);
             target is not null;
             target = targetModels.FirstOrDefault(x => !targetMarks[x].Permenant))
            Visit(target);

        depthFirstTargets.Reverse();

        // Initialize target states
        var targetStates = depthFirstTargets.ToDictionary(x => x, x => new TargetState(x.Name));

        foreach (var specifiedTarget in specifiedTargets)
        {
            var target = depthFirstTargets.First(x => x.Name == specifiedTarget);

            targetStates[target] = targetStates[target] with
            {
                Status = TargetRunState.PendingRun,
            };
        }

        // If we're including dependencies, mark specified targets' dependencies as pending run
        if (includeDependencies)
        {
            var modified = true;

            while (modified)
            {
                modified = false;

                foreach (var target in depthFirstTargets
                             .Where(target => targetStates[target].Status is TargetRunState.PendingRun)
                             .ToArray())
                foreach (var dependentTarget in target
                             .Dependencies
                             .Where(dependency => targetStates[dependency].Status is not TargetRunState.PendingRun)
                             .ToArray())
                {
                    targetStates[dependentTarget] = targetStates[dependentTarget] with
                    {
                        Status = TargetRunState.PendingRun,
                    };

                    modified = true;
                }
            }
        }

        // Mark all targets that are not pending run as pending skip
        foreach (var state in depthFirstTargets
                     .Select(target => targetStates[target])
                     .Where(state => state.Status is not TargetRunState.PendingRun))
            state.Status = TargetRunState.Skipped;

        // ReSharper disable once InvertIf
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var endTime = Stopwatch.GetTimestamp();
            var duration = TimeSpan.FromTicks((endTime - startTime) * TimeSpan.TicksPerSecond / Stopwatch.Frequency);
            logger.LogDebug("Build resolution completed in {Duration}", duration);
        }

        return new()
        {
            Targets = depthFirstTargets,
            TargetStates = targetStates,
            DeclaringAssembly = buildDefinition.GetType()
                .Assembly,
        };

        void Visit(TargetModel target)
        {
            var marks = targetMarks[target];

            if (marks.Permenant)
                return;

            if (marks.Temporary)
                throw new(
                    $"Circular dependency detected: {string.Join(" -> ", depthFirstTargets.Select(x => x.Name))}.");

            targetMarks[target] = (true, marks.Permenant);

            foreach (var dependency in target.Dependencies)
                Visit(dependency);

            targetMarks[target] = (false, true);
            depthFirstTargets.Insert(0, target);
        }
    }

    /// <summary>
    ///     Recursively adds a parameter and its chained dependencies to the list of used parameters for a target.
    /// </summary>
    /// <param name="param">The name of the parameter to add.</param>
    /// <param name="required">A value indicating whether the parameter is required.</param>
    /// <param name="usedParams">The list of used parameters to add to.</param>
    /// <param name="paramModels">A dictionary of all available parameter models.</param>
    private static void AddParamAndChildren(
        string param,
        bool required,
        List<UsedParam> usedParams,
        Dictionary<string, ParamModel> paramModels)
    {
        var model = paramModels[param];
        usedParams.Add(new(model, required));

        foreach (var chainedParam in model.ChainedParams)
            AddParamAndChildren(chainedParam, required, usedParams, paramModels);
    }
}
