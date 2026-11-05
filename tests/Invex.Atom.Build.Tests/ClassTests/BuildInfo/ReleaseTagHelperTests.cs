namespace Invex.Atom.Build.Tests.ClassTests.BuildInfo;

[TestFixture]
internal sealed class ReleaseTagHelperTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";

    private sealed class TestReleaseTagHelper(IServiceProvider services) : IReleaseTagHelper
    {
        public IServiceProvider Services { get; } = services;
    }

    private static (IReleaseTagHelper Helper, List<ProcessRunOptions> Calls) CreateSut(
        string logOutput = Commit,
        string shallowOutput = "false")
    {
        var calls = new List<ProcessRunOptions>();
        var processRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) =>
            {
                calls.Add(options);

                var output = options.Args.StartsWith("rev-parse", StringComparison.Ordinal)
                    ? $"{shallowOutput}\n"
                    : $"{logOutput}\n";

                return Task.FromResult(new ProcessRunResult(options, 0, output, string.Empty));
            });

        var host = CreateTestHost<DefaultAtomBuild>(configure: builder =>
            builder.Services.AddSingleton(processRunner));

        return (new TestReleaseTagHelper(host.Services), calls);
    }

    private static string ExpectedRoot =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\"
            : "/Atom/";

    [Test]
    public async Task GetReleaseTagCommit_WithNoPaths_ReturnsHeadCommit()
    {
        var (helper, calls) = CreateSut();

        var commit = await helper.GetReleaseTagCommit([]);

        commit.ShouldBe(Commit);

        calls
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(c => c.Name.ShouldBe("git"),
                c => c.Args.ShouldBe("log -1 --format=%H"),
                c => c.WorkingDirectory.ShouldBe(ExpectedRoot));
    }

    [Test]
    public async Task GetReleaseTagCommit_WithPaths_ChecksShallowThenLogsPaths()
    {
        var (helper, calls) = CreateSut();

        var commit = await helper.GetReleaseTagCommit(["src/AppA/", "src/Shared/"]);

        commit.ShouldBe(Commit);

        calls
            .Select(c => c.Args)
            .ShouldBe(["rev-parse --is-shallow-repository", "log -1 --format=%H -- src/AppA/ src/Shared/"]);

        calls.ShouldAllBe(c => c.Name == "git" && c.WorkingDirectory == ExpectedRoot);
    }

    [Test]
    public async Task GetReleaseTagCommit_WithPathContainingSpace_QuotesPath()
    {
        var (helper, calls) = CreateSut();

        await helper.GetReleaseTagCommit(["src/My App/"]);

        calls[^1]
            .Args
            .ShouldBe("log -1 --format=%H -- \"src/My App/\"");
    }

    [Test]
    public async Task GetReleaseTagCommit_WithEmptyOutput_Throws()
    {
        var (helper, _) = CreateSut(string.Empty);

        var exception = await Should.ThrowAsync<StepFailedException>(() => helper.GetReleaseTagCommit(["src/AppA/"]));

        exception.Message.ShouldContain("src/AppA/");
        exception.Message.ShouldContain("fetch-depth: 0");
    }

    [Test]
    public async Task GetReleaseTagCommit_WithNoPathsAndEmptyOutput_Throws()
    {
        var (helper, _) = CreateSut(string.Empty);

        var exception = await Should.ThrowAsync<StepFailedException>(() => helper.GetReleaseTagCommit([]));

        exception.Message.ShouldContain("fetch-depth: 0");
    }

    [Test]
    public async Task GetReleaseTagCommit_WithPathsInShallowClone_Throws()
    {
        var (helper, calls) = CreateSut(shallowOutput: "true");

        var exception = await Should.ThrowAsync<StepFailedException>(() => helper.GetReleaseTagCommit(["src/AppA/"]));

        exception.Message.ShouldContain("shallow");
        exception.Message.ShouldContain("fetch-depth: 0");

        calls
            .ShouldHaveSingleItem()
            .Args
            .ShouldBe("rev-parse --is-shallow-repository");
    }

    [Test]
    public async Task GetReleaseTagCommit_WithNoPathsInShallowClone_ReturnsHeadCommit()
    {
        var (helper, calls) = CreateSut(shallowOutput: "true");

        var commit = await helper.GetReleaseTagCommit([]);

        commit.ShouldBe(Commit);
        calls.ShouldHaveSingleItem();
    }

    [TestCase("")]
    [TestCase(" ")]
    public async Task GetReleaseTagCommit_WithBlankPath_Throws(string path)
    {
        var (helper, calls) = CreateSut();

        await Should.ThrowAsync<ArgumentException>(() => helper.GetReleaseTagCommit(["src/AppA/", path]));

        calls.ShouldBeEmpty();
    }

    [Test]
    public async Task GetReleaseTagCommit_WithNullPaths_Throws()
    {
        var (helper, _) = CreateSut();

        await Should.ThrowAsync<ArgumentNullException>(() => helper.GetReleaseTagCommit(null!));
    }
}
