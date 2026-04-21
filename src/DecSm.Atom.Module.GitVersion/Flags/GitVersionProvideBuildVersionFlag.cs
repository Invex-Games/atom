namespace DecSm.Atom.Module.GitVersion.Flags;

[PublicAPI]
public sealed record GitVersionProvideBuildVersionFlag(bool Enabled) : IBuildFlag;
