namespace Invex.Atom.Build.Util;

/// <summary>
///     Indicates that the annotated API is unstable and may change or be removed in any release
///     without following semantic versioning guarantees.
/// </summary>
/// <remarks>
///     Consumers should avoid depending on members marked with this attribute in production code,
///     or be prepared to update their usage when upgrading.
/// </remarks>
[UnstableAPI]
[AttributeUsage(AttributeTargets.All, Inherited = false)]
[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnstableAPIAttribute : Attribute;
