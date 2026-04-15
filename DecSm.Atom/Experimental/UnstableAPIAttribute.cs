namespace DecSm.Atom.Experimental;

[UnstableAPI]
[AttributeUsage(AttributeTargets.All, Inherited = false)]
[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class UnstableAPIAttribute : Attribute;
