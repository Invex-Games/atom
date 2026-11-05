using Invex.Atom.Module.Dotnet.Util;

namespace Invex.Atom.Module.Dotnet.Tests.Util;

[TestFixture]
public sealed class DotnetToolVersionTests
{
    private const string GlobalListOutput = """
                                            Package Id           Version      Commands
                                            ---------------------------------------------------
                                            gitversion.tool      6.8.2        dotnet-gitversion

                                            """;

    [Test]
    public void FindInstalledVersion_MatchesPackageIdCaseInsensitively() =>
        DotnetToolVersion
            .FindInstalledVersion(GlobalListOutput, "GitVersion.Tool")
            .ShouldBe("6.8.2");

    [Test]
    public void FindInstalledVersion_WithCrLfLineEndings_ReturnsVersion() =>
        DotnetToolVersion
            .FindInstalledVersion(GlobalListOutput.ReplaceLineEndings("\r\n"), "gitversion.tool")
            .ShouldBe("6.8.2");

    [Test]
    public void FindInstalledVersion_WithLocalManifestColumn_ReturnsVersion() =>
        DotnetToolVersion
            .FindInstalledVersion("""
                                  Package Id           Version      Commands             Manifest
                                  ---------------------------------------------------------------------------------
                                  gitversion.tool      6.8.2        dotnet-gitversion    /repo/.config/dotnet-tools.json
                                  """,
                "GitVersion.Tool")
            .ShouldBe("6.8.2");

    [TestCase("GitVersion")]
    [TestCase("GitVersion.Tool.Extra")]
    [TestCase("Package")]
    public void FindInstalledVersion_WithoutExactPackageIdRow_ReturnsNull(string packageId) =>
        DotnetToolVersion
            .FindInstalledVersion(GlobalListOutput, packageId)
            .ShouldBeNull();

    [Test]
    public void FindInstalledVersion_WithOnlyHeader_ReturnsNull() =>
        DotnetToolVersion
            .FindInstalledVersion("""
                                  Package Id      Version      Commands
                                  -------------------------------------
                                  """,
                "GitVersion.Tool")
            .ShouldBeNull();

    [Test]
    public void FindInstalledVersion_WithEmptyOutput_ReturnsNull() =>
        DotnetToolVersion
            .FindInstalledVersion(string.Empty, "GitVersion.Tool")
            .ShouldBeNull();

    [TestCase("6.8.2", null, true)]
    [TestCase("6.8.2", "", true)]
    [TestCase("6.8.2", " ", true)]
    [TestCase("6.8.2", "6.8.2", true)]
    [TestCase("6.8.2", " 6.8.2 ", true)]
    [TestCase("1.2.0", "1.2", true)]
    [TestCase("1.2", "1.2.0", true)]
    [TestCase("1.2.3.0", "1.2.3", true)]
    [TestCase("1.2.3", "1.2", false)]
    [TestCase("1.2.3", "1.2.30", false)]
    [TestCase("1.2.30", "1.2.3", false)]
    [TestCase("6.8.2", "6.8.3", false)]
    [TestCase("1.0.0-beta.1", "1.0.0-BETA.1", true)]
    [TestCase("1.0.0-beta.1", "1.0.0", false)]
    [TestCase("1.0.0", "1.0.0-beta.1", false)]
    [TestCase("1.0.0+abc", "1.0.0", true)]
    public void IsMatch_WithExactVersion_ComparesComponents(
        string installedVersion,
        string? requestedVersion,
        bool expected) =>
        DotnetToolVersion
            .IsMatch(installedVersion, requestedVersion)
            .ShouldBe(expected);

    [TestCase("6.8.2", "6.*", true)]
    [TestCase("6.0.0", "6.*", true)]
    [TestCase("6.8.2", "6.8.*", true)]
    [TestCase("6.8.2", "6.9.*", false)]
    [TestCase("7.0.0", "6.*", false)]
    [TestCase("16.0.0", "6.*", false)]
    [TestCase("6.8.2", "*", true)]
    [TestCase("6.9.0-beta.1", "6.*", false)]
    [TestCase("6.9.0-beta.1", "*", false)]
    [TestCase("6", "6.8.*", false)]
    public void IsMatch_WithFloatingVersion_MatchesStablePrefix(
        string installedVersion,
        string requestedVersion,
        bool expected) =>
        DotnetToolVersion
            .IsMatch(installedVersion, requestedVersion)
            .ShouldBe(expected);
}
