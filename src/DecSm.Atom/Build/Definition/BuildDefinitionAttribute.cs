namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Marks a class as a build definition, triggering source generation to implement <see cref="IBuildDefinition" />.
/// </summary>
/// <remarks>
///     This attribute should be applied to a class that inherits from <see cref="MinimalBuildDefinition" /> or
///     <see cref="BuildDefinition" />.
///     The source generator uses this attribute to identify the main build class and generate the necessary code to
///     discover and register targets and parameters.
/// </remarks>
/// <example>
///     <code>
/// [BuildDefinition]
/// public partial class MyBuild : BuildDefinition, IMyTargets
/// {
///     // ...
/// }
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class BuildDefinitionAttribute : Attribute;
