namespace DecSm.Atom.Build.Definition;

/// <summary>
///     A minimal abstract base class for creating build definitions, providing default implementations for
///     <see cref="IBuildDefinition" />.
/// </summary>
/// <remarks>
///     <para>
///         While developers should typically inherit from the more comprehensive <see cref="BuildDefinition" />,
///         this class can be used for a leaner setup.
///     </para>
///     <para>
///         The <see cref="TargetDefinitions" /> and <see cref="ParamDefinitions" /> properties are populated by source
///         generators based on the interfaces and attributes used in the derived class. A derived class must be
///         decorated with <see cref="BuildDefinitionAttribute" />.
///     </para>
/// </remarks>
/// <example>
///     A minimal build definition:
///     <code>
/// [BuildDefinition]
/// internal partial class MyBuild : MinimalBuildDefinition, IMyTargets
/// {
///     // ...
/// }
///     </code>
/// </example>
/// <param name="services">The service provider for dependency injection.</param>
[PublicAPI]
public abstract class MinimalBuildDefinition(IServiceProvider services) : IBuildDefinition
{
    /// <summary>
    ///     Provides access to the service provider associated with the build definition.
    ///     This property allows resolving dependencies and accessing services registered in the application.
    /// </summary>
    public IServiceProvider Services => services;

    /// <inheritdoc />
    public abstract IReadOnlyDictionary<string, Target> TargetDefinitions { get; }

    /// <inheritdoc />
    public abstract IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions { get; }

    /// <inheritdoc />
    public abstract object? AccessParam(string paramName);

    /// <inheritdoc />
    public virtual IReadOnlyList<WorkflowDefinition> Workflows => [];

    /// <inheritdoc />
    public virtual IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions => [];
}
