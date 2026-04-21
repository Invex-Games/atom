namespace DecSm.Atom.Module.GitVersion.Flags;

[PublicAPI]
public sealed record GitVersionProvideBuildIdFlag(bool Enabled) : IBuildFlag;
