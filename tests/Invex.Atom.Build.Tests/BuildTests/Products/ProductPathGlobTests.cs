namespace Invex.Atom.Build.Tests.BuildTests.Products;

[TestFixture]
internal sealed class ProductPathGlobTests
{
    [TestCase("src/AppA/", "src/AppA/Program.cs", true)]
    [TestCase("src/AppA/", "src/AppA/nested/deep/File.cs", true)]
    [TestCase("src/AppA/**", "src/AppAB/Program.cs", false)]
    [TestCase("src/*/bin", "src/AppA/bin", true)]
    [TestCase("src/*/bin", "src/AppA/x/bin", false)]
    [TestCase("src/**/bin", "src/bin", true)]
    [TestCase("src/**/bin", "src/a/b/bin", true)]
    [TestCase("**/*.md", "README.md", true)]
    [TestCase("**/*.md", "docs/a/README.md", true)]
    [TestCase("Directory.Build.*", "Directory.Build.props", true)]
    [TestCase("Directory.Build.*", "src/Directory.Build.props", false)]
    [TestCase("global.json", "global.json", true)]
    [TestCase("global.json", "globalxjson", false)]
    [TestCase("src/App A/", "src/App A/x.cs", true)]
    [TestCase("src/AppA/**", @"src\AppA\x.cs", true)]
    [TestCase("src/AppA/**", "SRC/AppA/x.cs", false)]
    [TestCase("src/AppA/", "src/AppA/file\nname.cs", true)]
    [TestCase("src/**/bin", "src/a\nb/bin", true)]
    [TestCase("global.json", "global.json\n", false)]
    public void IsMatch_FollowsTheGlobDialect(string glob, string path, bool expected) =>
        ProductPathGlob
            .IsMatch(glob, path)
            .ShouldBe(expected);

    [Test]
    public void ToGitVersionIgnoreRegex_IgnoresEverythingOutsideTheGlobs()
    {
        var pattern = ProductPathGlob.ToGitVersionIgnoreRegex(["src/AppA/", "global.json", "Directory.Build.*", "src/**/x"]);

        pattern.ShouldBe(@"^(?!(?:src[/\\]AppA[/\\].*|global\.json|Directory\.Build\.[^/\\]*|src[/\\](?:.*[/\\])?x)$).*");

        var regex = new System.Text.RegularExpressions.Regex(pattern);

        regex
            .IsMatch("src/AppA/Program.cs")
            .ShouldBeFalse();

        regex
            .IsMatch(@"src\AppA\Program.cs")
            .ShouldBeFalse();

        regex
            .IsMatch("global.json")
            .ShouldBeFalse();

        regex
            .IsMatch("src/a/b/x")
            .ShouldBeFalse();

        regex
            .IsMatch("src/AppB/Program.cs")
            .ShouldBeTrue();

        regex
            .IsMatch("global.json.bak")
            .ShouldBeTrue();

        regex
            .IsMatch("src/Directory.Build.props")
            .ShouldBeTrue();
    }

    [TestCase("src/App?/", "it contains '?' or '+', which GitHub path filters treat as quantifiers of the preceding character")]
    [TestCase("src/C++/", "it contains '?' or '+', which GitHub path filters treat as quantifiers of the preceding character")]
    [TestCase("src/{a,b}/", "it uses glob syntax other than '*' and '**'")]
    [TestCase("!src/AppA/", "it uses glob syntax other than '*' and '**'")]
    [TestCase("src/AppA", null)]
    [TestCase("src/AppA/", null)]
    public void GetError_ReportsUnsupportedSyntax(string glob, string? expected) =>
        ProductPathGlob
            .GetError(glob)
            .ShouldBe(expected);

    [TestCase("global.json", true)]
    [TestCase("src/AppA", true)]
    [TestCase("src/AppA/", false)]
    [TestCase("src/*.cs", false)]
    [TestCase("src/**", false)]
    public void IsLiteral_IsTrueForPathsWithoutWildcardOrTrailingSlash(string glob, bool expected) =>
        ProductPathGlob
            .IsLiteral(glob)
            .ShouldBe(expected);

    [Test]
    public void ToGitVersionIgnoreRegex_WithRepositoryPrefix_AnchorsBelowIt()
    {
        var pattern = ProductPathGlob.ToGitVersionIgnoreRegex(["src/AppA/", "global.json"], "samples/My.Sample");

        pattern.ShouldBe(@"^(?!samples[/\\]My\.Sample[/\\](?:src[/\\]AppA[/\\].*|global\.json)$).*");

        var regex = new System.Text.RegularExpressions.Regex(pattern);

        regex
            .IsMatch("samples/My.Sample/src/AppA/Program.cs")
            .ShouldBeFalse();

        regex
            .IsMatch(@"samples\My.Sample\global.json")
            .ShouldBeFalse();

        regex
            .IsMatch("src/AppA/Program.cs")
            .ShouldBeTrue();

        regex
            .IsMatch("samples/My.Sample/src/AppB/Program.cs")
            .ShouldBeTrue();
    }

    [Test]
    public void ToGitPathspec_UsesGlobMagic() =>
        ProductPathGlob
            .ToGitPathspec("src/AppA/")
            .ShouldBe(":(glob)src/AppA/**");
}
