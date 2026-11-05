namespace Invex.Atom.Module.GitVersion.Tests;

[BuildDefinition]
public partial class GitVersionTestBuild : BuildDefinition, IGitVersion, IDotnetToolInstallHelper
{
    public IReadOnlyList<IBuildOption> Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId, BuildOptions.GitVersion.ProvideBuildVersion,
    ];
}
