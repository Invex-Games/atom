namespace Invex.Atom.Build.Tests.BuildTests.Products;

[BuildDefinition]
public sealed partial class ProductsBuild : BuildDefinition, IProductsTestTargets
{
    public IReadOnlyList<Product> TestProducts { get; set; } =
    [
        new("Shared")
        {
            Paths = ["src/Shared/"],
        },
        new("AppA")
        {
            Paths = ["src/AppA/**"],
            DependsOn = ["Shared"],
            Directory = "src/AppA",
        },
        new("AppB")
        {
            Paths = ["src/AppB/**", "docs/appb/*.md"],
            DependsOn = ["Shared"],
            TagPrefix = "app-b-",
        },
    ];

    IReadOnlyList<Product> IProductsDefinition.Products => TestProducts;

    public string? BuildNameResult { get; set; }

    public SemVer? BuildVersionResult { get; set; }

    public string? ReleaseTagResult { get; set; }
}

public interface IProductsTestTargets : IProductsDefinition, IBuildInfo, IReleaseTagHelper, IValidateBuild
{
    Target ProductInfoTarget =>
        t => t.Executes(() =>
        {
            var build = (ProductsBuild)this;
            build.BuildNameResult = BuildName;
            build.BuildVersionResult = BuildVersion;
            build.ReleaseTagResult = ReleaseTagFor(SemVer.Parse("1.2.0"));
        });
}

/// <summary>
///     Captures the variables a build writes.
/// </summary>
internal sealed class CapturingVariableProvider : IVariableProvider
{
    public Dictionary<string, string> Variables { get; } = [];

    public Task<bool> WriteVariable(
        string variableName,
        string variableValue,
        CancellationToken cancellationToken = default)
    {
        Variables[variableName] = variableValue;

        return Task.FromResult(true);
    }

    public Task<string?> ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default) =>
        Task.FromResult(Variables.GetValueOrDefault(variableName));
}
