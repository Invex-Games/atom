namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Defines a target that can be modeled and executed, including its tasks, dependencies, and parameters.
/// </summary>
[PublicAPI]
public sealed class TargetDefinition
{
    /// <summary>
    ///     Gets the name of the target. Target names must be unique.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets the description of the target, used for help text.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the target should be hidden from help output unless verbose mode is enabled.
    /// </summary>
    public bool Hidden { get; private set; }

    /// <summary>
    ///     Gets the list of asynchronous tasks that will be executed when the target runs.
    /// </summary>
    public List<Func<CancellationToken, Task>> Tasks { get; private set; } = [];

    /// <summary>
    ///     Gets the list of target names that must be executed before this target.
    /// </summary>
    public List<string> Dependencies { get; private set; } = [];

    /// <summary>
    ///     Gets the list of parameters that are used or required by this target.
    /// </summary>
    public List<DefinedParam> Params { get; private set; } = [];

    /// <summary>
    ///     Gets the list of artifacts that must be produced by other targets before this target can run.
    /// </summary>
    public List<ConsumedArtifact> ConsumedArtifacts { get; private set; } = [];

    /// <summary>
    ///     Gets the list of artifacts that will be produced by this target.
    /// </summary>
    public List<ProducedArtifact> ProducedArtifacts { get; private set; } = [];

    /// <summary>
    ///     Gets the list of variables that must be produced by other targets before this target can run.
    /// </summary>
    public List<ConsumedVariable> ConsumedVariables { get; private set; } = [];

    /// <summary>
    ///     Gets the list of variables that will be produced by this target.
    /// </summary>
    public List<string> ProducedVariables { get; private set; } = [];

    private List<(Func<IBuildDefinition, Target> GetExtension, bool RunExtensionAfter)> Extensions { get; } = [];

    /// <summary>
    ///     Marks this target as an extension of another, inheriting its tasks, dependencies, and other properties.
    /// </summary>
    /// <param name="targetToExtend">The base target to extend.</param>
    /// <param name="runExtensionAfter">If true, the tasks of this target will run after the base target's tasks.</param>
    /// <typeparam name="T">The type of the build definition containing the base target.</typeparam>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition Extends<T>(Func<T, Target> targetToExtend, bool runExtensionAfter = false)
    {
        Extensions.Insert(0, (d => targetToExtend((T)d), runExtensionAfter));

        return this;
    }

    /// <summary>
    ///     Applies all extensions to this target definition, merging properties from the base targets.
    /// </summary>
    /// <param name="buildDefinition">The build definition to resolve extensions against.</param>
    /// <returns>The updated <see cref="TargetDefinition" />.</returns>
    internal TargetDefinition ApplyExtensions(IBuildDefinition buildDefinition)
    {
        foreach (var extension in Extensions)
        {
            var targetToExtend = extension
                .GetExtension(buildDefinition)(new()
                {
                    Name = Name,
                })
                .ApplyExtensions(buildDefinition);

            if (extension.RunExtensionAfter)
            {
                Tasks.AddRange(targetToExtend.Tasks);

                Dependencies = Dependencies
                    .Concat(targetToExtend.Dependencies)
                    .Distinct()
                    .ToList();

                Params = Params
                    .Concat(targetToExtend.Params)
                    .GroupBy(p => p.Param)
                    .Select(g => new DefinedParam(g.Key, g.Any(p => p.Required)))
                    .ToList();

                ConsumedArtifacts = ConsumedArtifacts
                    .Concat(targetToExtend.ConsumedArtifacts)
                    .Distinct()
                    .ToList();

                ProducedArtifacts = ProducedArtifacts
                    .Concat(targetToExtend.ProducedArtifacts)
                    .Distinct()
                    .ToList();

                ConsumedVariables = ConsumedVariables
                    .Concat(targetToExtend.ConsumedVariables)
                    .Distinct()
                    .ToList();

                ProducedVariables = ProducedVariables
                    .Concat(targetToExtend.ProducedVariables)
                    .Distinct()
                    .ToList();
            }
            else
            {
                Tasks = targetToExtend
                    .Tasks
                    .Concat(Tasks)
                    .ToList();

                Dependencies = targetToExtend
                    .Dependencies
                    .Concat(Dependencies)
                    .Distinct()
                    .ToList();

                Params = targetToExtend
                    .Params
                    .Concat(Params)
                    .GroupBy(p => p.Param)
                    .Select(g => new DefinedParam(g.Key, g.Any(p => p.Required)))
                    .ToList();

                ConsumedArtifacts = targetToExtend
                    .ConsumedArtifacts
                    .Concat(ConsumedArtifacts)
                    .Distinct()
                    .ToList();

                ProducedArtifacts = targetToExtend
                    .ProducedArtifacts
                    .Concat(ProducedArtifacts)
                    .Distinct()
                    .ToList();

                ConsumedVariables = targetToExtend
                    .ConsumedVariables
                    .Concat(ConsumedVariables)
                    .Distinct()
                    .ToList();

                ProducedVariables = targetToExtend
                    .ProducedVariables
                    .Concat(ProducedVariables)
                    .Distinct()
                    .ToList();
            }
        }

        return this;
    }

    /// <summary>
    ///     Sets the description for the target.
    /// </summary>
    /// <param name="description">A human-readable description of the target's purpose.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition DescribedAs(string description)
    {
        Description = description;

        return this;
    }

    /// <summary>
    ///     Sets whether the target should be hidden from help output.
    /// </summary>
    /// <param name="hidden">If true, the target will be hidden.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition IsHidden(bool hidden = true)
    {
        Hidden = hidden;

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous task to be executed when the target runs.
    /// </summary>
    /// <param name="task">The asynchronous task to execute.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition Executes(Func<CancellationToken, Task> task)
    {
        Tasks.Add(task);

        return this;
    }

    /// <summary>
    ///     Adds an asynchronous task to be executed when the target runs.
    /// </summary>
    /// <param name="task">The asynchronous task to execute.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition Executes(Func<Task> task)
    {
        Tasks.Add(_ => task());

        return this;
    }

    /// <summary>
    ///     Adds a synchronous action to be executed when the target runs.
    /// </summary>
    /// <param name="action">The synchronous action to execute.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition Executes(Action action)
    {
        Tasks.Add(_ =>
        {
            action();

            return Task.CompletedTask;
        });

        return this;
    }

    /// <summary>
    ///     Adds a dependency on another target by its name.
    /// </summary>
    /// <param name="targetName">The name of the target to depend on.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition DependsOn(string targetName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        Dependencies.Add(targetName);

        return this;
    }

    /// <summary>
    ///     Adds a dependency on another target.
    /// </summary>
    /// <param name="target">The target to depend on.</param>
    /// <param name="targetName">The name of the target, inferred from the argument expression.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    [SuppressMessage("ReSharper", "LocalizableElement")]
    public TargetDefinition DependsOn(Target target, [CallerArgumentExpression("target")] string? targetName = null)
    {
        if (string.IsNullOrWhiteSpace(targetName))
            throw new ArgumentException("""
                                        Unable to infer target name from argument expression.
                                        This usually happens when passing a target through a variable.
                                        Instead of: var t = Build.MyTarget; DependsOn(t), use: DependsOn(nameof(IMyTargets.MyTarget)) or DependsOn("MyTarget")
                                        """,
                nameof(target));

        Dependencies.Add(targetName);

        return this;
    }

    /// <summary>
    ///     Adds a dependency on another target.
    /// </summary>
    /// <param name="workflowTarget">The workflow target definition to depend on.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition DependsOn(WorkflowTargetDefinition workflowTarget)
    {
        Dependencies.Add(workflowTarget.Name);

        return this;
    }

    /// <summary>
    ///     Specifies that this target may use the provided parameters.
    /// </summary>
    /// <param name="paramNames">An array of parameter names that may be used.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition UsesParam(params IEnumerable<string> paramNames)
    {
        ArgumentNullException.ThrowIfNull(paramNames);
        Params.AddRange(paramNames.Select(x => new DefinedParam(x, false)));

        return this;
    }

    /// <summary>
    ///     Specifies that this target requires the provided parameters.
    /// </summary>
    /// <param name="paramNames">An array of parameter names that are required.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition RequiresParam(params IEnumerable<string> paramNames)
    {
        ArgumentNullException.ThrowIfNull(paramNames);
        Params.AddRange(paramNames.Select(x => new DefinedParam(x, true)));

        return this;
    }

    /// <summary>
    ///     Declares an artifact that this target produces.
    /// </summary>
    /// <param name="artifactName">The name of the artifact.</param>
    /// <param name="buildSlice">An optional build slice associated with the artifact.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition ProducesArtifact(string artifactName, string? buildSlice = null)
    {
        ProducedArtifacts.Add(new(artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Declares multiple artifacts that this target produces.
    /// </summary>
    /// <param name="artifactName">The names of the artifacts.</param>
    /// <param name="buildSlice">An optional build slice associated with the artifacts.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition ProducesArtifacts(IEnumerable<string> artifactName, string? buildSlice = null)
    {
        ProducedArtifacts.AddRange(artifactName.Select(x => new ProducedArtifact(x, buildSlice)));

        return this;
    }

    /// <summary>
    ///     Declares an artifact that this target consumes.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the artifact.</param>
    /// <param name="artifactName">The name of the artifact to consume.</param>
    /// <param name="buildSlice">An optional build slice associated with the artifact.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition ConsumesArtifact(string targetName, string artifactName, string? buildSlice = null)
    {
        ConsumedArtifacts.Add(new(targetName, artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Declares multiple artifacts from a single target that this target consumes.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the artifacts.</param>
    /// <param name="artifactNames">The names of the artifacts to consume.</param>
    /// <param name="buildSlice">An optional build slice associated with the artifacts.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition ConsumesArtifacts(
        string targetName,
        IEnumerable<string> artifactNames,
        string? buildSlice = null)
    {
        ConsumedArtifacts.AddRange(artifactNames.Select(artifactName =>
            new ConsumedArtifact(targetName, artifactName, buildSlice)));

        return this;
    }

    /// <summary>
    ///     Declares an artifact from multiple build slices that this target consumes.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the artifact.</param>
    /// <param name="artifactName">The name of the artifact to consume.</param>
    /// <param name="buildSlices">The build slices to consume the artifact from.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition ConsumesArtifact(string targetName, string artifactName, IEnumerable<string> buildSlices)
    {
        foreach (var buildSlice in buildSlices)
            ConsumedArtifacts.Add(new(targetName, artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Declares multiple artifacts from multiple build slices that this target consumes.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the artifacts.</param>
    /// <param name="artifactNames">The names of the artifacts to consume.</param>
    /// <param name="buildSlices">The build slices to consume the artifacts from.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition ConsumesArtifacts(
        string targetName,
        IEnumerable<string> artifactNames,
        IEnumerable<string> buildSlices)
    {
        var buildSlicesArray = buildSlices.ToArray();

        foreach (var artifactName in artifactNames)
        foreach (var buildSlice in buildSlicesArray)
            ConsumedArtifacts.Add(new(targetName, artifactName, buildSlice));

        return this;
    }

    /// <summary>
    ///     Declares a variable that this target produces.
    /// </summary>
    /// <param name="variableName">The name of the variable.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    /// <remarks>
    ///     This only declares the variable; it must be written using <see cref="IWorkflowVariableService.WriteVariable" />.
    /// </remarks>
    public TargetDefinition ProducesVariable(string variableName)
    {
        ProducedVariables.Add(variableName);

        return this;
    }

    /// <summary>
    ///     Declares a variable that this target consumes.
    /// </summary>
    /// <param name="targetName">The name of the target that produces the variable.</param>
    /// <param name="outputName">The name of the variable to consume.</param>
    /// <returns>The current <see cref="TargetDefinition" /> for fluent chaining.</returns>
    public TargetDefinition ConsumesVariable(string targetName, string outputName)
    {
        ConsumedVariables.Add(new(targetName, outputName));

        return this;
    }
}
