namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Triggers source generation to implement members from interfaces applied to a class.
/// </summary>
/// <remarks>
///     When this attribute is applied to a partial class, the source generator scans all interfaces implemented by the
///     class.
///     For each interface, it generates explicit implementations of its members, delegating the calls to the base class.
///     This is primarily used to avoid boilerplate code when a build definition implements many target interfaces.
/// </remarks>
/// <example>
///     <code>
/// [GenerateInterfaceMembers]
/// public partial class MyBuild : BuildDefinition, IMyTargets, IYourTargets
/// {
///     // The source generator will automatically implement all members
///     // from IMyTargets and IYourTargets.
/// }
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateInterfaceMembersAttribute : Attribute;
