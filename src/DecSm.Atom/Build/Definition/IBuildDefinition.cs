namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Defines the core structure and components of an Atom build process.
/// </summary>
/// <remarks>
///     This interface outlines the fundamental elements of a build, including its targets, parameters,
///     workflow configurations, and global options. Implementations, typically derived from
///     <see cref="MinimalBuildDefinition" /> or <see cref="BuildDefinition" />, serve as the central point
///     for Atom to understand and execute a build.
/// </remarks>
[PublicAPI]
public interface IBuildDefinition
{
    /// <summary>
    ///     Gets the collection of workflow definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Workflows define how targets are orchestrated, potentially across different CI/CD platforms.
    /// </remarks>
    IReadOnlyList<WorkflowDefinition> Workflows { get; }

    /// <summary>
    ///     Gets the collection of target definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Targets represent individual units of work. This collection is populated by source generation
    ///     based on <see cref="Target" /> properties in interfaces that inherit <see cref="IBuildAccessor" />
    /// </remarks>
    IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <summary>
    ///     Gets the collection of parameter definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Parameters allow for external input to customize the build process. This collection is populated by
    ///     source generation based on properties marked with <see cref="ParamDefinitionAttribute" /> or
    ///     <see cref="SecretDefinitionAttribute" />.
    /// </remarks>
    IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <summary>
    ///     Gets the collection of global workflow options that apply to all workflows.
    /// </summary>
    /// <remarks>
    ///     These options can be overridden at the individual workflow level.
    /// </remarks>
    IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions { get; }

    /// <summary>
    ///     Retrieves the value of a build parameter by its name.
    /// </summary>
    /// <param name="paramName">The name of the parameter to access.</param>
    /// <returns>The value of the specified parameter, or <c>null</c> if not defined or has no value.</returns>
    object? AccessParam(string paramName);
}
