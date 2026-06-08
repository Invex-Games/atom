namespace Invex.Atom.Build.Definition;

/// <summary>
///     The standard abstract base class for creating build definitions, providing default implementations for
///     <see cref="IBuildDefinition" />.
/// </summary>
/// <remarks>
///     The <see cref="TargetDefinitions" /> and <see cref="ParamDefinitions" /> properties are populated by source
///     generators based on the interfaces and attributes used in the derived class. A derived class must be
///     decorated with <see cref="BuildDefinitionAttribute" />.
/// </remarks>
/// <example>
///     A typical build definition:
///     <code>
/// [BuildDefinition]
/// internal partial class MyBuild : BuildDefinition, IMyTargets
/// {
///     // ...
/// }
///     </code>
/// </example>
/// <param name="services">The service provider for dependency injection.</param>
[PublicAPI]
public abstract class BuildDefinition(IServiceProvider services) : IBuildDefinition
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
    public virtual void ConfigureDefinitionHost(IHostApplicationBuilder builder) { }
}
