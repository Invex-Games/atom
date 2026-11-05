namespace Invex.Atom.Module.GitVersion.Tests;

[BuildDefinition]
public partial class GitVersionTestBuild : BuildDefinition, IGitVersion, IBuildInfo
{
    public IReadOnlyList<IBuildOption> Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId, BuildOptions.GitVersion.ProvideBuildVersion,
    ];
}

[BuildDefinition]
public partial class GitVersionOverrideTestBuild : BuildDefinition, IGitVersion, IBuildInfo
{
    public IReadOnlyList<IBuildOption> Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId, BuildOptions.GitVersion.ProvideBuildVersion,
    ];

    string? IGitVersion.GitVersionConfig => "src/AppA/GitVersion.yml";
}
