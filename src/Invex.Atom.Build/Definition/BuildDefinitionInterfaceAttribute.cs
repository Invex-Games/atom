namespace Invex.Atom.Build.Definition;

/// <summary>
///     Marks an interface as a build definition interface, enabling source generation of its
///     target and parameter collections.
/// </summary>
/// <seealso cref="BuildDefinitionAttribute" />
[PublicAPI]
[AttributeUsage(AttributeTargets.Interface)]
public sealed class BuildDefinitionInterfaceAttribute : Attribute;
