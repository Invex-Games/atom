namespace Invex.Atom.Build.Tests.ClassTests.BuildInfo;

[TestFixture]
internal sealed class ReleaseTagTests
{
    [Test]
    public void DefaultPrefix_IsV() =>
        ReleaseTag.DefaultPrefix.ShouldBe("v");

    [TestCase("1.2.0", "v1.2.0")]
    [TestCase("1.2.0-rc.1", "v1.2.0-rc.1")]
    [TestCase("1.2.0+build.5", "v1.2.0+build.5")]
    [TestCase("1.2.0-rc.1+build.5", "v1.2.0-rc.1+build.5")]
    public void For_WithDefaultPrefix_PrefixesV(string version, string expected) =>
        ReleaseTag
            .For(SemVer.Parse(version))
            .ShouldBe(expected);

    [TestCase("appa/v", "1.2.0", "appa/v1.2.0")]
    [TestCase("appa/v", "1.2.0-rc.1", "appa/v1.2.0-rc.1")]
    [TestCase("", "1.2.0", "1.2.0")]
    [TestCase("release-", "2.0.0", "release-2.0.0")]
    public void For_WithPrefix_UsesPrefix(string prefix, string version, string expected) =>
        ReleaseTag
            .For(SemVer.Parse(version), prefix)
            .ShouldBe(expected);

    [Test]
    public void For_WithNullArguments_Throws()
    {
        Should.Throw<ArgumentNullException>(() => ReleaseTag.For(null!));
        Should.Throw<ArgumentNullException>(() => ReleaseTag.For(SemVer.Parse("1.0.0"), null!));
    }

    [TestCase("v1.2.0", "v", "1.2.0")]
    [TestCase("v1.2.0-rc.1", "v", "1.2.0-rc.1")]
    [TestCase("v1.2.0+build.5", "v", "1.2.0+build.5")]
    [TestCase("v1.2.0-rc.1+build.5", "v", "1.2.0-rc.1+build.5")]
    [TestCase("appa/v1.2.0", "appa/v", "1.2.0")]
    [TestCase("appa/v1.2.0-rc.1", "appa/v", "1.2.0-rc.1")]
    [TestCase("1.2.0", "", "1.2.0")]
    public void TryParse_WithMatchingPrefix_ReturnsVersion(string tag, string prefix, string expected)
    {
        ReleaseTag
            .TryParse(tag, out var version, prefix)
            .ShouldBeTrue();

        version.ShouldBe(SemVer.Parse(expected));
    }

    [Test]
    public void TryParse_WithDefaultPrefix_ReturnsVersion()
    {
        ReleaseTag
            .TryParse("v1.2.0", out var version)
            .ShouldBeTrue();

        version.ShouldBe(SemVer.Parse("1.2.0"));
    }

    [TestCase("appa/v1.2.0", "v")]
    [TestCase("v1.2.0", "appa/v")]
    [TestCase("appb/v1.2.0", "appa/v")]
    [TestCase("V1.2.0", "v")]
    [TestCase("APPA/v1.2.0", "appa/v")]
    [TestCase("1.2.0", "v")]
    [TestCase("appa/v1.2.0", "")]
    [TestCase("vnext", "v")]
    [TestCase("v", "v")]
    [TestCase("vv1.2.0", "v")]
    [TestCase("v1.2", "v")]
    [TestCase("v1.2.0.0", "v")]
    [TestCase("v 1.2.0", "v")]
    [TestCase("v1.2.0 ", "v")]
    [TestCase("", "v")]
    [TestCase("", "")]
    [TestCase("garbage", "v")]
    public void TryParse_WithMismatchedPrefixOrInvalidVersion_ReturnsFalse(string tag, string prefix)
    {
        ReleaseTag
            .TryParse(tag, out var version, prefix)
            .ShouldBeFalse();

        version.ShouldBeNull();
    }

    [Test]
    public void TryParse_WithNullTag_ReturnsFalse()
    {
        ReleaseTag
            .TryParse(null, out var version)
            .ShouldBeFalse();

        version.ShouldBeNull();
    }

    [Test]
    public void TryParse_WithNullPrefix_Throws() =>
        Should.Throw<ArgumentNullException>(() => ReleaseTag.TryParse("v1.2.0", out _, null!));

    [TestCase("1.2.0", "v")]
    [TestCase("1.2.0-rc.1+build.5", "v")]
    [TestCase("1.2.0-rc.1", "appa/v")]
    [TestCase("1.2.0", "")]
    public void TryParse_RoundTripsFor(string version, string prefix)
    {
        var semVer = SemVer.Parse(version);

        ReleaseTag
            .TryParse(ReleaseTag.For(semVer, prefix), out var parsed, prefix)
            .ShouldBeTrue();

        parsed.ShouldBe(semVer);
    }

    [Test]
    public void Glob_WithDefaultPrefix_IsVStar() =>
        ReleaseTag
            .Glob()
            .ShouldBe("v*");

    [TestCase("appa/v", "appa/v*")]
    [TestCase("", "*")]
    public void Glob_WithPrefix_AppendsStar(string prefix, string expected) =>
        ReleaseTag
            .Glob(prefix)
            .ShouldBe(expected);

    [Test]
    public void Glob_WithNullPrefix_Throws() =>
        Should.Throw<ArgumentNullException>(() => ReleaseTag.Glob(null!));
}
