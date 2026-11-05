namespace Invex.Atom.Module.GitVersion.Tests;

[TestFixture]
public sealed class GitVersionCacheTests
{
    [TestCase(null)]
    [TestCase("")]
    public void GetCacheFileName_ForDefaultConfig_ReturnsDefaultName(string? configRelativePath) =>
        GitVersionCache
            .GetCacheFileName(configRelativePath)
            .ShouldBe(".gitversioncache");

    [Test]
    public void GetCacheFileName_ForCustomConfig_IsStableAndDistinctPerPath()
    {
        // Act
        var appA = GitVersionCache.GetCacheFileName("src/AppA/GitVersion.yml");
        var appAAgain = GitVersionCache.GetCacheFileName("src/AppA/GitVersion.yml");
        var appB = GitVersionCache.GetCacheFileName("src/AppB/GitVersion.yml");

        // Assert
        appA.ShouldMatch("^\\.gitversioncache-[0-9a-f]{12}$");
        appAAgain.ShouldBe(appA);
        appB.ShouldNotBe(appA);
    }

    [Test]
    public void CreateKey_ForDefaultConfig_HasEmptyConfigSegments() =>
        GitVersionCache
            .CreateKey("abc", null, null)
            .ShouldBe("abc||");

    [Test]
    public void CreateKey_ForCustomConfig_IncludesPathAndContentHash() =>
        GitVersionCache
            .CreateKey("abc", "src/AppA/GitVersion.yml", "0f0f")
            .ShouldBe("abc|src/AppA/GitVersion.yml|0f0f");
}
