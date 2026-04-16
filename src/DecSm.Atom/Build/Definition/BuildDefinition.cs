namespace DecSm.Atom.Build.Definition;

/// <summary>
///     A comprehensive base class for creating build definitions, pre-configuring a rich set of common build targets and
///     options.
/// </summary>
/// <remarks>
///     <para>
///         This class is the recommended starting point for most Atom build projects. It implements several standard
///         interfaces,
///         providing targets for common operations such as setting up build info (<see cref="ISetupBuildInfo" />),
///         validating the build (<see cref="IValidateBuild" />), and managing .NET user secrets (
///         <see cref="IDotnetUserSecrets" />).
///     </para>
///     <para>
///         A project's main build definition class typically inherits from this class.
///     </para>
/// </remarks>
/// <example>
///     A typical build definition class:
///     <code>
/// [BuildDefinition]
/// [GenerateEntryPoint]
/// internal partial class Build : BuildDefinition, IMyTargets
/// {
///     // ...
/// }
///     </code>
/// </example>
/// <seealso cref="MinimalBuildDefinition" />
/// <seealso cref="IBuildDefinition" />
[PublicAPI]
public abstract class BuildDefinition(IServiceProvider services)
    : MinimalBuildDefinition(services), ISetupBuildInfo, IDotnetUserSecrets;
