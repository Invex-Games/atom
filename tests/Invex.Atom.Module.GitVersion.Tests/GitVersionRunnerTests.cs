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
    public void BuildInfo_WithConfig_AfterBuildResolution_UsesConfig()
    {
        // Arrange: resolving the build model reads every param's default (including BuildId and BuildVersion) in
        // a default-values-only scope, where the config param is not visible.
        var host = CreateHost(AppAConfig);
        host.Services.GetRequiredService<BuildModel>();

        var buildInfo = (IBuildInfo)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        var buildId = buildInfo.BuildId;
        var buildVersion = buildInfo.BuildVersion;

        // Assert
        buildId.ShouldBe("1.2.3-beta.4");
        buildVersion.ToString()
            .ShouldBe("1.2.3-beta.4");

        _processRunner.GitVersionCalls.ShouldContain($"dotnet gitversion /config {RootPath(AppAConfig)} /output json");
    }

    [Test]
    public void BuildInfo_WithConfigAndFailingDefaultConfig_ResolvesAndUsesConfig()
    {
        // Arrange: the root configuration is unusable, but the selected product configuration is fine
        _processRunner.FailDefaultConfigRun = true;
        var host = CreateHost(AppAConfig);

        // Act: resolving the build model must not fail on the default-configuration run
        host.Services.GetRequiredService<BuildModel>();
        var buildId = ((IBuildInfo)host.Services.GetRequiredService<IBuildDefinition>()).BuildId;

        // Assert
        buildId.ShouldBe("1.2.3-beta.4");
        _processRunner.GitVersionCalls.ShouldContain($"dotnet gitversion /config {RootPath(AppAConfig)} /output json");
    }

    [Test]
    public void Target_RequiringBuildVersion_WithConfigAndFailingDefaultConfig_Executes()
    {
        // Arrange: the build version's default can't be computed during build resolution (the root configuration
        // fails there), so required-parameter validation must compute it with the selected configuration
        _processRunner.FailDefaultConfigRun = true;

        var host = CreateTestHost<GitVersionTargetTestBuild>(fileSystem: _fileSystem,
            commandLineArgs: new(true,
            [
                new CommandArg(nameof(IVersionTarget.ReportVersion)),
                new ParamArg("gitversion-config", nameof(IGitVersion.GitVersionConfig), AppAConfig),
            ]),
            configure: builder => builder.Services.AddSingleton<IProcessRunner>(_processRunner));

        var build = (GitVersionTargetTestBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.ExecutedVersion.ShouldBe("1.2.3-beta.4");
    }

    [Test]
    public void Target_RequiringBuildVersion_Interactive_WithConfigAndFailingDefaultConfig_OffersComputedVersion()
    {
        // Arrange: in interactive mode, the prompt must offer the version computed with the selected configuration,
        // so accepting it (an empty answer) runs the target with that version
        _processRunner.FailDefaultConfigRun = true;

        using var console = new TestConsole();
        console.Interactive();

        for (var i = 0; i < 4; i++)
            console.Input.PushTextWithEnter(string.Empty);

        var host = CreateTestHost<GitVersionTargetTestBuild>(console,
            _fileSystem,
            new(true,
            [
                new CommandArg(nameof(IVersionTarget.ReportVersion)),
                new ParamArg("gitversion-config", nameof(IGitVersion.GitVersionConfig), AppAConfig),
                new InteractiveArg(),
            ]),
            configure: builder => builder.Services.AddSingleton<IProcessRunner>(_processRunner));

        var build = (GitVersionTargetTestBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.ExecutedVersion.ShouldBe("1.2.3-beta.4");
    }

    [Test]
    public void GetOutput_WithConfigPathContainingSpace_QuotesPath()
    {
        // Arrange
        const string config = "src/My App/GitVersion.yml";
        _fileSystem.AddFile(RootPath(config), new("tag-prefix: 'myapp/v'\n"));
        var host = CreateHost(config);

        // Act
        _ = host
            .Services
            .GetRequiredService<IBuildIdProvider>()
            .BuildId;

        // Assert
        _processRunner
            .GitVersionCalls
            .ShouldHaveSingleItem()
            .ShouldBe($"dotnet gitversion /config \"{RootPath(config)}\" /output json");
    }

    [Test]
    public void GetOutput_WithBuildOverridingConfig_UsesOverride()
    {
        // Arrange: no param is given; the build definition overrides IGitVersion.GitVersionConfig
        var runner = CreateTestHost<GitVersionOverrideTestBuild>(fileSystem: _fileSystem,
                commandLineArgs: new(true, []),
                configure: builder => builder.Services.AddSingleton<IProcessRunner>(_processRunner))
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        runner.GetOutput();

        // Assert
        _processRunner
            .GitVersionCalls
            .ShouldHaveSingleItem()
            .ShouldBe($"dotnet gitversion /config {RootPath(AppAConfig)} /output json");
    }

    [Test]
    public void BuildInfo_WithMissingConfig_AfterBuildResolution_Throws()
    {
        // Arrange
        var host = CreateHost("src/Missing/GitVersion.yml");
        host.Services.GetRequiredService<BuildModel>();

        var buildInfo = (IBuildInfo)host.Services.GetRequiredService<IBuildDefinition>();

        // Act & Assert
        Should
            .Throw<InvalidOperationException>(() => buildInfo.BuildId)
            .Message
            .ShouldContain("gitversion-config");
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
            .ShouldBe([$"dotnet gitversion /config {RootPath(AppAConfig)} /output json"]);
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
            .ShouldBe([$"dotnet gitversion /config {RootPath(AppAConfig)} /output json"]);
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
        exception.Message.ShouldContain("which must be a path relative to the Atom root directory");
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
    public void GetOutput_AfterConfigEditInTheSameRun_RunsGitVersionAgain()
    {
        // Arrange: as when build resolution reads a product configuration that the run then regenerates
        var runner = CreateHost(AppAConfig)
            .Services
            .GetRequiredService<GitVersionRunner>();

        runner.GetOutput();
        _fileSystem.File.WriteAllText(RootPath(AppAConfig), "tag-prefix: 'appa-v'\n");

        // Act
        runner.GetOutput();
        runner.GetOutput();

        // Assert
        _processRunner
            .GitVersionCalls
            .Count()
            .ShouldBe(2);
    }

    [TestCase("GitVersion.yml")]
    [TestCase("GitVersion.yaml")]
    [TestCase(".GitVersion.yml")]
    public void GetOutput_WithoutConfig_AfterDefaultConfigEdit_RunsGitVersionAgain(string fileName)
    {
        // Arrange
        _fileSystem.AddFile(RootPath(fileName), new("next-version: 1.0.0\n"));

        CreateHost()
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        _fileSystem.File.WriteAllText(RootPath(fileName), "next-version: 2.0.0\n");

        var runner = CreateHost()
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
    public void GetOutput_WithoutConfig_KeysCacheOnDefaultConfigContents()
    {
        // Arrange
        _fileSystem.AddFile(RootPath("GitVersion.yml"), new("mode: ContinuousDelivery\n"));
        var host = CreateHost();

        // Act
        host
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        // Assert
        var cacheFile = GitVersionCache.GetCacheFile(host.Services.GetRequiredService<IRootedFileSystem>(),
            GitVersionCache.DefaultCacheFileName);

        _fileSystem
            .File
            .ReadAllText(cacheFile)
            .ShouldStartWith($"0123456789abcdef||{Sha256("mode: ContinuousDelivery\n")}\n");
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
    public void GetOutput_WithUnsupportedToolAfterUpdate_Throws()
    {
        // Arrange: for example, a mirrored feed that only has 6.7.x.
        _processRunner.InstalledToolVersion = null;
        _processRunner.UpdatedToolVersion = "6.7.0";

        var runner = CreateHost()
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => runner.GetOutput());

        // Assert
        exception.Message.ShouldContain("6.7.0");
        exception.Message.ShouldContain("6.8.0");
        _processRunner.GitVersionCalls.ShouldBeEmpty();
    }

    [TestCase("6.8.2")]
    [TestCase("6.9.0")]
    public void GetOutput_WithSupportedLocalTool_UsesItWithoutCheckingGlobalTool(string localVersion)
    {
        // Arrange
        _processRunner.LocalToolVersion = localVersion;
        _processRunner.InstalledToolVersion = null;

        var runner = CreateHost()
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        runner.GetOutput();

        // Assert
        _processRunner
            .Calls
            .Where(x => x.StartsWith("dotnet tool", StringComparison.Ordinal))
            .ShouldBe(["dotnet tool list GitVersion.Tool"]);

        _processRunner.GitVersionCalls.ShouldHaveSingleItem();
    }

    [TestCase("6.4.0")]
    [TestCase("7.0.0")]
    public void GetOutput_WithUnsupportedLocalTool_ThrowsEvenIfGlobalToolIsSupported(string localVersion)
    {
        // Arrange: 'dotnet gitversion' would run the local manifest's version, not the global 6.8.2.
        _processRunner.LocalToolVersion = localVersion;

        var runner = CreateHost()
            .Services
            .GetRequiredService<GitVersionRunner>();

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => runner.GetOutput());

        // Assert
        exception.Message.ShouldContain(localVersion);
        exception.Message.ShouldContain("local tool manifest");
        _processRunner.GitVersionCalls.ShouldBeEmpty();
        _processRunner.Calls.ShouldNotContain(x => x.StartsWith("dotnet tool update", StringComparison.Ordinal));
    }

    [Test]
    public void GetOutput_ForTwoConfigsInOneRunner_RunsGitVersionOncePerConfigAndChecksToolOnce()
    {
        // Arrange
        var host = CreateHost();
        var buildDefinition = A.Fake<IBuildDefinition>(options => options.Implements<IGitVersion>());
        var configs = new Queue<string>([AppAConfig, AppBConfig, AppAConfig, AppBConfig]);

        A
            .CallTo(() => ((IGitVersion)buildDefinition).GitVersionConfig)
            .ReturnsLazily(_ => configs.Dequeue());

        var runner = new GitVersionRunner(host.Services.GetRequiredService<IDotnetToolInstallHelper>(),
            _processRunner,
            buildDefinition,
            host.Services.GetRequiredService<IRootedFileSystem>(),
            NullLogger<GitVersionRunner>.Instance);

        // Act
        for (var i = 0; i < 4; i++)
            runner.GetOutput();

        // Assert
        _processRunner
            .GitVersionCalls
            .ShouldBe([
                $"dotnet gitversion /config {RootPath(AppAConfig)} /output json",
                $"dotnet gitversion /config {RootPath(AppBConfig)} /output json",
            ]);

        _processRunner
            .Calls
            .Count(x => x.StartsWith("dotnet tool list", StringComparison.Ordinal))
            .ShouldBe(2);
    }

    [TestCase("Shared", true)]
    [TestCase("AppA", false)]
    public void GetOutput_WithProduct_WarnsOnlyWhenProductHasNoConfig(string product, bool expectWarning)
    {
        // Arrange
        var loggerProvider = new TestLoggerProvider();

        var host = CreateTestHost<GitVersionProductsTestBuild>(fileSystem: _fileSystem,
            commandLineArgs: new(true, [new ParamArg("product", nameof(IProductsDefinition.ProductName), product)]),
            configure: builder =>
            {
                builder.Services.AddSingleton<IProcessRunner>(_processRunner);
                builder.Logging.AddProvider(loggerProvider);
            });

        // Act
        host
            .Services
            .GetRequiredService<GitVersionRunner>()
            .GetOutput();

        // Assert
        loggerProvider
            .Logger
            .LogContent
            .ToString()
            .Contains("Product Shared has no GitVersion configuration (Product.GitVersionConfig)", StringComparison.Ordinal)
            .ShouldBe(expectWarning);
    }

    [Test]
    public void BuildId_WithProductWithoutConfig_AfterBuildResolution_Warns()
    {
        // Arrange: build resolution caches GitVersion's output with no configuration and no product visible first
        var loggerProvider = new TestLoggerProvider();

        var host = CreateTestHost<GitVersionProductsTestBuild>(fileSystem: _fileSystem,
            commandLineArgs: new(true, [new ParamArg("product", nameof(IProductsDefinition.ProductName), "Shared")]),
            configure: builder =>
            {
                builder.Services.AddSingleton<IProcessRunner>(_processRunner);
                builder.Logging.AddProvider(loggerProvider);
            });

        host.Services.GetRequiredService<BuildModel>();

        // Act
        host
            .Services
            .GetRequiredService<IBuildIdProvider>()
            .BuildId
            .ShouldBe("1.2.3-beta.4");

        // Assert
        loggerProvider
            .Logger
            .LogContent
            .ToString()
            .ShouldContain("Product Shared has no GitVersion configuration (Product.GitVersionConfig)");
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
