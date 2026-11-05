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

[BuildDefinition]
public partial class GitVersionTargetTestBuild : BuildDefinition, IGitVersion, IVersionTarget
{
    public IReadOnlyList<IBuildOption> Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId, BuildOptions.GitVersion.ProvideBuildVersion,
    ];

    public string? ExecutedVersion { get; set; }
}

public interface IVersionTarget : IBuildInfo
{
    string? ExecutedVersion { get; set; }

    Target ReportVersion =>
        t => t
            .RequiresParam(nameof(BuildVersion))
            .Executes(() =>
            {
                ExecutedVersion = BuildVersion.ToString();

                return Task.CompletedTask;
            });
}
