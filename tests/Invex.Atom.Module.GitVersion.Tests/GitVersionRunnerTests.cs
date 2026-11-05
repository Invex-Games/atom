namespace Invex.Atom.Module.GitVersion.Tests;

[TestFixture]
public sealed class GitVersionRunnerTests
{
    private const string AppAConfig = "src/AppA/GitVersion.yml";
    private const string AppBConfig = "src/AppB/GitVersion.yml";

    private MockFileSystem _fileSystem = null!;
    private FakeGitVersionProcessRunner _processRunner = null!;

    [SetUp]
    public void SetUp()
    {
        _fileSystem = FileSystemUtils.DefaultMockFileSystem;
        _processRunner = new();

        _fileSystem.AddFile(RootPath(AppAConfig), new("tag-prefix: 'appa/v'\n"));
        _fileSystem.AddFile(RootPath(AppBConfig), new("tag-prefix: 'appb/v'\n"));
    }

    [Test]
    public void BuildIdAndBuildVersion_RunGitVersionOnce()
    {
        // Arrange
        var host = CreateHost();

        // Act
        var buildId = host
            .Services
            .GetRequiredService<IBuildIdProvider>()
            .BuildId;

        var buildVersion = host
            .Services
            .GetRequiredService<IBuildVersionProvider>()
            .Version;

        // Assert
        buildId.ShouldBe("1.2.3-beta.4");
        buildVersion.ToString()
            .ShouldBe("1.2.3-beta.4");

        _processRunner.GitVersionCalls.ShouldHaveSingleItem();
    }

    [Test]
    public void GetOutput_CalledConcurrently_RunsGitVersionOnce()
    {
        // Arrange
        var runner = CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        Parallel.For(0, 8, _ => runner.GetOutput());

        // Assert
        _processRunner.GitVersionCalls.ShouldHaveSingleItem();
    }

    [Test]
    public void GetOutput_WithoutConfig_RunsGitVersionWithoutConfigArgument()
    {
        // Arrange
        var runner = CreateHost()
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        runner.GetOutput();

        // Assert
        _processRunner
            .GitVersionCalls
            .ShouldBe(["dotnet gitversion /output json"]);
    }

    [TestCase(AppAConfig)]
    [TestCase("./src/AppA/../AppA/GitVersion.yml")]
    [TestCase(" src/AppA/GitVersion.yml ")]
    public void GetOutput_WithConfig_PassesQuotedAbsoluteConfigPath(string configParam)
    {
        // Arrange
        var runner = CreateHost(configParam)
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        runner.GetOutput();

        // Assert
        _processRunner
            .GitVersionCalls
            .ShouldBe([$"dotnet gitversion /config \"{RootPath(AppAConfig)}\" /output json"]);
    }

    [Test]
    public void GetOutput_WithConfigFromEnvironmentVariable_UsesConfig()
    {
        // Arrange: workflow writers inject a param into the step environment keyed by its argument name.
        Environment.SetEnvironmentVariable("gitversion-config", AppAConfig);

        try
        {
            var runner = CreateHost()
                .Services
                .GetRequiredService<GitVersionRunner>();

            // Act
            runner.GetOutput();
        }
        finally
        {
            Environment.SetEnvironmentVariable("gitversion-config", null);
        }

        // Assert
        _processRunner
            .GitVersionCalls
            .ShouldBe([$"dotnet gitversion /config \"{RootPath(AppAConfig)}\" /output json"]);
    }

    [Test]
    public void GetOutput_WithMissingConfig_ThrowsNamingParamAndResolvedPath()
    {
        // Arrange
        var runner = CreateHost("src/Missing/GitVersion.yml")
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => runner.GetOutput());

        // Assert
        exception.Message.ShouldContain("gitversion-config");
        exception.Message.ShouldContain(RootPath("src/Missing/GitVersion.yml"));
        _processRunner.Calls.ShouldBeEmpty();
    }

    [Test]
    public void BuildVersion_WithMissingConfig_Throws()
    {
        // Arrange
        var provider = CreateHost("src/Missing/GitVersion.yml")
            .Services
            .GetRequiredService<IBuildVersionProvider>();

        // Act & Assert
        Should
            .Throw<InvalidOperationException>(() => provider.Version)
            .Message
            .ShouldContain("gitversion-config");
    }

    [Test]
    public void GetOutput_WithCacheForSameCommitAndConfig_DoesNotRunGitVersion()
    {
        // Arrange
        CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        _processRunner.Calls.Clear();

        var runner = CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        var output = runner.GetOutput();

        // Assert
        output
            .GetProperty("FullSemVer")
            .GetString()
            .ShouldBe("1.2.3-beta.4");

        _processRunner.Calls.ShouldBe(["git rev-parse HEAD"]);
    }

    [Test]
    public void GetOutput_AfterConfigEdit_RunsGitVersionAgain()
    {
        // Arrange
        CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        _fileSystem.File.WriteAllText(RootPath(AppAConfig), "tag-prefix: 'appa-v'\n");

        var runner = CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        runner.GetOutput();

        // Assert
        _processRunner
            .GitVersionCalls
            .Count()
            .ShouldBe(2);
    }

    [Test]
    public void GetOutput_AfterNewCommit_RunsGitVersionAgain()
    {
        // Arrange
        CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        _processRunner.GitHash = "fedcba9876543210";

        var runner = CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        runner.GetOutput();

        // Assert
        _processRunner
            .GitVersionCalls
            .Count()
            .ShouldBe(2);
    }

    [Test]
    public void GetOutput_ForEachConfig_WritesSeparateCacheFiles()
    {
        // Arrange & Act
        CreateHost()
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        var host = CreateHost(AppBConfig);

        host
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        // Assert
        var rootedFileSystem = host.Services.GetRequiredService<IRootedFileSystem>();

        var defaultCacheFile = GitVersionCache.GetCacheFile(rootedFileSystem, GitVersionCache.DefaultCacheFileName);

        var appACacheFile =
            GitVersionCache.GetCacheFile(rootedFileSystem, GitVersionCache.GetCacheFileName(AppAConfig));

        var appBCacheFile =
            GitVersionCache.GetCacheFile(rootedFileSystem, GitVersionCache.GetCacheFileName(AppBConfig));

        _fileSystem
            .File
            .ReadAllText(defaultCacheFile)
            .ShouldStartWith("0123456789abcdef||\n");

        _fileSystem
            .File
            .ReadAllText(appACacheFile)
            .ShouldStartWith($"0123456789abcdef|{AppAConfig}|{Sha256("tag-prefix: 'appa/v'\n")}\n");

        _fileSystem
            .File
            .ReadAllText(appBCacheFile)
            .ShouldStartWith($"0123456789abcdef|{AppBConfig}|");

        _processRunner
            .GitVersionCalls
            .Count()
            .ShouldBe(3);
    }

    [Test]
    public void GetOutput_WithCorruptCache_RunsGitVersionAgain()
    {
        // Arrange
        var host = CreateHost();
        var rootedFileSystem = host.Services.GetRequiredService<IRootedFileSystem>();
        var cacheFile = GitVersionCache.GetCacheFile(rootedFileSystem, GitVersionCache.DefaultCacheFileName);

        _fileSystem.AddFile(cacheFile, new("0123456789abcdef||\n{ not json"));

        // Act
        var output = host
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        // Assert
        output
            .GetProperty("Major")
            .GetInt32()
            .ShouldBe(1);

        _processRunner.GitVersionCalls.ShouldHaveSingleItem();
    }

    [TestCase("6.8.2")]
    [TestCase("6.9.0")]
    public void GetOutput_WithSupportedToolInstalled_DoesNotUpdateTool(string installedVersion)
    {
        // Arrange
        _processRunner.InstalledToolVersion = installedVersion;

        var runner = CreateHost()
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        runner.GetOutput();

        // Assert
        _processRunner.Calls.ShouldNotContain(x => x.StartsWith("dotnet tool update", StringComparison.Ordinal));
    }

    [TestCase(null)]
    [TestCase("6.3.0")]
    [TestCase("7.0.0")]
    [TestCase("5.12.0")]
    public void GetOutput_WithUnsupportedTool_UpdatesToPinnedVersion(string? installedVersion)
    {
        // Arrange
        _processRunner.InstalledToolVersion = installedVersion;

        var runner = CreateHost()
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        runner.GetOutput();

        // Assert
        _processRunner
            .Calls
            .Where(x => x.StartsWith("dotnet tool update", StringComparison.Ordinal))
            .ShouldBe(["dotnet tool update GitVersion.Tool --version 6.* --allow-downgrade -g"]);

        _processRunner.GitVersionCalls.ShouldHaveSingleItem();
    }

    [Test]
    public void GetOutput_ForTwoConfigsInOneRunner_RunsGitVersionOncePerConfigAndChecksToolOnce()
    {
        // Arrange
        var host = CreateHost();
        var paramService = A.Fake<IParamService>();
        var configs = new Queue<string>([AppAConfig, AppBConfig, AppAConfig, AppBConfig]);

        A
            .CallTo(() => paramService.GetParam(nameof(IGitVersion.GitVersionConfig), A<string?>._))
            .ReturnsLazily(_ => configs.Dequeue());

        var runner = new GitVersionRunner(host.Services.GetRequiredService<IDotnetToolInstallHelper>(),
            _processRunner,
            paramService,
            host.Services.GetRequiredService<IRootedFileSystem>(),
            NullLogger<GitVersionRunner>.Instance);

        // Act
        for (var i = 0; i < 4; i++)
            runner.GetOutput();

        // Assert
        _processRunner
            .GitVersionCalls
            .ShouldBe([
                $"dotnet gitversion /config \"{RootPath(AppAConfig)}\" /output json",
                $"dotnet gitversion /config \"{RootPath(AppBConfig)}\" /output json",
            ]);

        _processRunner
            .Calls
            .Count(x => x.StartsWith("dotnet tool list", StringComparison.Ordinal))
            .ShouldBe(1);
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("6.7.9", false)]
    [TestCase("6.8.0", true)]
    [TestCase("6.8.2", true)]
    [TestCase("6.12.0", true)]
    [TestCase("7.0.0", false)]
    [TestCase("not-a-version", false)]
    public void IsSupportedToolVersion_RequiresSixPointEightOrLaterSix(string? installedVersion, bool expected) =>
        GitVersionRunner
            .IsSupportedToolVersion(installedVersion)
            .ShouldBe(expected);

    private IHost CreateHost(string? gitVersionConfig = null) =>
        CreateTestHost<GitVersionTestBuild>(fileSystem: _fileSystem,
            commandLineArgs: new(true,
                gitVersionConfig is null
                    ? []
                    : [new ParamArg("gitversion-config", nameof(IGitVersion.GitVersionConfig), gitVersionConfig)]),
            configure: builder => builder.Services.AddSingleton<IProcessRunner>(_processRunner));

    private static string RootPath(string relativePath)
    {
        var path = new MockFileSystem().Path;

        return path.GetFullPath(path.Combine(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\Atom"
                : "/Atom",
            relativePath));
    }

    private static string Sha256(string content) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
}
