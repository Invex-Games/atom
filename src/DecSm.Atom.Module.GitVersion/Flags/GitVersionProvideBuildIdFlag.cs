using DecSm.Atom.Build.BuildOptions;

namespace DecSm.Atom.Module.GitVersion.Flags;

[PublicAPI]
public sealed record GitVersionProvideBuildIdFlag(bool Enabled) : IBuildOption;
