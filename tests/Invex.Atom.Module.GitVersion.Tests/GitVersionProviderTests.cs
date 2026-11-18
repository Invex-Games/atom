namespace Invex.Atom.Module.GitVersion.Tests;

[TestFixture]
internal sealed class GitVersionProviderTests
{
    [SetUp]
    public void SetUp()
    {
        _host = CreateTestHost<TestBuildDefinition>();
        _fileSystem = _host.Services.GetRequiredService<IRootedFileSystem>();
        _toolInstallHelper = A.Fake<IDotnetToolInstallHelper>();
        _processRunner = A.Fake<IProcessRunner>();
        _buildOptionService = A.Fake<IBuildOptionService>();
        _gitVersionRuns = [];

        A
            .CallTo(() => _buildOptionService.Options)
            .Returns([
                new GitVersionProvideBuildIdOption
                {
                    Enabled = true,
                },
                new GitVersionProvideBuildVersionOption
                {
                    Enabled = true,
                },
            ]);

        A
            .CallTo(() => _processRunner.Run(A<ProcessRunOptions>._))
            .ReturnsLazily((ProcessRunOptions options) =>
            {
                if (options.Name is "git")
                    return new(options, 0, GitHash, string.Empty);

                _gitVersionRuns.Add(options);

                return new(options, 0, V7Output, string.Empty);
            });
    }

    [TearDown]
    public void TearDown() =>
        _host.Dispose();

    private const string GitHash = "0123456789abcdef0123456789abcdef01234567";

    private const string V7Output = """
                                    {
                                      "Major": 1,
                                      "Minor": 2,
                                      "Patch": 3,
                                      "FullSemVer": "1.2.3-beta.feat-thing.4",
                                      "PreReleaseLabel": "beta.feat-thing.4",
                                      "PreReleaseLabelName": "beta.feat-thing",
                                      "PreReleaseNumber": 4
                                    }
                                    """;

    private IHost _host = null!;
    private IRootedFileSystem _fileSystem = null!;
    private IDotnetToolInstallHelper _toolInstallHelper = null!;
    private IProcessRunner _processRunner = null!;
    private IBuildOptionService _buildOptionService = null!;
    private List<ProcessRunOptions> _gitVersionRuns = null!;

    [Test]
    public void Version_UsesGitVersion7PreReleaseLabel()
    {
        // Act
        var version = CreateVersionProvider()
            .Version;

        // Assert
        version.ShouldBe(SemVer.Parse("1.2.3-beta.feat-thing.4"));
    }

    [Test]
    public void BuildId_UsesFullSemVer()
    {
        // Act
        var buildId = CreateBuildIdProvider()
            .BuildId;

        // Assert
        buildId.ShouldBe("1.2.3-beta.feat-thing.4");
    }

    [Test]
    public void Version_InstallsPinnedGitVersionTool_AndRunsItWithGitVersion7Arguments()
    {
        // Act
        _ = CreateVersionProvider()
            .Version;

        // Assert
        A
            .CallTo(() => _toolInstallHelper.InstallTool("GitVersion.Tool", "7.0.0", A<bool>._, A<bool>._))
            .MustHaveHappenedOnceExactly();

        var run = _gitVersionRuns.ShouldHaveSingleItem();
        run.Name.ShouldBe("dotnet");

        string
            .Join(' ', run.Args)
            .ShouldBe("gitversion --output json");
    }

    [Test]
    public void Version_ReusesOutputCachedForTheSameCommitAndToolVersion()
    {
        // Arrange
        _ = CreateBuildIdProvider()
            .BuildId;

        // Act
        var version = CreateVersionProvider()
            .Version;

        // Assert
        version.ShouldBe(SemVer.Parse("1.2.3-beta.feat-thing.4"));
        _gitVersionRuns.Count.ShouldBe(1);
    }

    [Test]
    public void Version_IgnoresOutputCachedWithoutTheToolVersion()
    {
        // Arrange - a cache entry in the pre-7 format, keyed by the Git hash alone, holding GitVersion 6 output
        // in which 'PreReleaseLabel' is the label without the pre-release number.
        var objDirectory = _fileSystem.AtomRootDirectory / "obj";
        _fileSystem.Directory.CreateDirectory(objDirectory);

        _fileSystem.File.WriteAllText(objDirectory / ".gitversioncache",
            $$"""
              {{GitHash}}
              { "Major": 1, "Minor": 2, "Patch": 3, "FullSemVer": "1.2.3-beta.feat-thing.4", "PreReleaseTag": "beta.feat-thing.4", "PreReleaseLabel": "beta.feat-thing" }
              """);

        // Act
        var version = CreateVersionProvider()
            .Version;

        // Assert
        version.ShouldBe(SemVer.Parse("1.2.3-beta.feat-thing.4"));
        _gitVersionRuns.Count.ShouldBe(1);
    }

    private GitVersionBuildVersionProvider CreateVersionProvider() =>
        new(_toolInstallHelper,
            _processRunner,
            _buildOptionService,
            _fileSystem,
            NullLogger<GitVersionBuildVersionProvider>.Instance);

    private GitVersionBuildIdProvider CreateBuildIdProvider() =>
        new(_toolInstallHelper,
            _processRunner,
            _buildOptionService,
            _fileSystem,
            NullLogger<GitVersionBuildIdProvider>.Instance);

    [UsedImplicitly]
    private sealed class TestBuildDefinition(IServiceProvider services) : BuildDefinition(services), IConfigureHost
    {
        public override IReadOnlyDictionary<string, Target> TargetDefinitions => new Dictionary<string, Target>();

        public override IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions =>
            new Dictionary<string, ParamDefinition>();

        public static void ConfigureHostBuilder(IHostApplicationBuilder builder) { }

        public static void ConfigureHost(IHost host) { }

        public override object AccessParam(string paramName) =>
            throw new NotImplementedException();
    }
}
