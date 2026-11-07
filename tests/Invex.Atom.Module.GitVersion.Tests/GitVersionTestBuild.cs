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

[BuildDefinition]
public partial class GitVersionProductsTestBuild : BuildDefinition, IGitVersion, IProductsDefinition
{
    public IReadOnlyList<IBuildOption> Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId, BuildOptions.GitVersion.ProvideBuildVersion,
    ];

    /// <summary>
    ///     Gets or sets the products to declare instead of the default Shared, AppA and App.B.
    /// </summary>
    public IReadOnlyList<Product>? TestProducts { get; set; }

    IReadOnlyList<Product> IProductsDefinition.Products =>
        TestProducts ??
        [
            new("Shared")
            {
                Paths = ["src/Shared/"],
            },
            new("AppA")
            {
                Paths = ["src/AppA/**"],
                DependsOn = ["Shared"],
                GitVersionConfig = FileSystem.AtomRootDirectory / "src/AppA/GitVersion.yml",
            },
            new("App.B")
            {
                Paths = ["src/AppB/**"],
                GitVersionConfig = FileSystem.AtomRootDirectory / "src/AppB/GitVersion.yml",
            },
        ];
}
