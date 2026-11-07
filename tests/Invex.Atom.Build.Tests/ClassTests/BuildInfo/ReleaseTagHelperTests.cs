namespace Invex.Atom.Build.Tests.ClassTests.BuildInfo;

[TestFixture]
internal sealed class ReleaseTagHelperTests
{
    private const string Commit = "0123456789abcdef0123456789abcdef01234567";

    private sealed class TestReleaseTagHelper(IServiceProvider services) : IReleaseTagHelper
    {
        public IServiceProvider Services { get; } = services;
    }

    private static (IReleaseTagHelper Helper, List<ProcessRunOptions> Calls, RootedPath Root) CreateSut(
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

        return (new TestReleaseTagHelper(host.Services), calls, host
            .Services
            .GetRequiredService<IRootedFileSystem>()
            .AtomRootDirectory);
    }

    private static string ExpectedRoot =>
        Environment.OSVersion.Platform is PlatformID.Win32NT
            ? @"C:\Atom\"
            : "/Atom/";

    [TestCase("1.2.0", "v1.2.0")]
    [TestCase("1.2.0-rc.1+build.5", "v1.2.0-rc.1+build.5")]
    public void ReleaseTagFor_UsesDefaultPrefix(string version, string expected) =>
        CreateSut()
            .Helper
            .ReleaseTagFor(SemVer.Parse(version))
            .ShouldBe(expected);

    [TestCase("v1.2.0", true)]
    [TestCase("v1.2.0-rc.1", true)]
    [TestCase("appa/v1.2.0", false)]
    [TestCase("vnext", false)]
    [TestCase(null, false)]
    public void TryParseReleaseTag_UsesDefaultPrefix(string? tag, bool expected)
    {
        var helper = CreateSut().Helper;

        helper
            .TryParseReleaseTag(tag, out var version)
            .ShouldBe(expected);

        version.ShouldBe(expected
            ? SemVer.Parse(tag![1..])
            : null);
    }

    [Test]
    public void ReleaseTagGlob_UsesDefaultPrefix() =>
        CreateSut()
            .Helper
            .ReleaseTagGlob
            .ShouldBe("v*");

    [Test]
    public async Task GetReleaseTagCommit_WithNoPaths_ReturnsHeadCommit()
    {
        var (helper, calls, _) = CreateSut();

        var commit = await helper.GetReleaseTagCommit([]);

        commit.ShouldBe(Commit);

        calls
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(c => c.Name.ShouldBe("git"),
                c => c.Args.ShouldBe("log -1 --no-show-signature --format=%H"),
                c => c.WorkingDirectory.ShouldBe(ExpectedRoot));
    }

    [Test]
    public async Task GetReleaseTagCommit_WithPaths_ChecksShallowThenLogsLiteralPathspecs()
    {
        var (helper, calls, root) = CreateSut();

        var commit = await helper.GetReleaseTagCommit([root / "src/AppA", root / "src/Shared/"]);

        commit.ShouldBe(Commit);

        calls
            .Select(c => c.Args)
            .ShouldBe([
                "rev-parse --is-shallow-repository",
                "log -1 --first-parent --no-show-signature --format=%H -- :(literal)src/AppA :(literal)src/Shared",
            ]);

        calls.ShouldAllBe(c => c.Name == "git" && c.WorkingDirectory == ExpectedRoot);
    }

    [TestCase("src/*.cs", ":(literal)src/*.cs")]
    [TestCase("src/**", ":(literal)src/**")]
    [TestCase("src/AppA/../Shared", ":(literal)src/Shared")]
    [TestCase("./README.md", ":(literal)README.md")]
    public async Task GetReleaseTagCommit_WithPath_PassesItLiterallyRelativeToRoot(string path, string expectedPathspec)
    {
        var (helper, calls, root) = CreateSut();

        await helper.GetReleaseTagCommit([root / path]);

        calls[^1]
            .Args
            .ShouldBe($"log -1 --first-parent --no-show-signature --format=%H -- {expectedPathspec}");
    }

    [Test]
    public async Task GetReleaseTagCommit_WithPathContainingSpace_QuotesPath()
    {
        var (helper, calls, root) = CreateSut();

        await helper.GetReleaseTagCommit([root / "src/My App/"]);

        calls[^1]
            .Args
            .ShouldBe("log -1 --first-parent --no-show-signature --format=%H -- \":(literal)src/My App\"");
    }

    [TestCase(".")]
    [TestCase("src/..")]
    public async Task GetReleaseTagCommit_WithAtomRoot_PassesDotPathspec(string path)
    {
        var (helper, calls, root) = CreateSut();

        // Everything below the Atom root, which is not the whole repository when the root is a subfolder of it
        await helper.GetReleaseTagCommit([root / path]);

        calls[^1]
            .Args
            .ShouldBe("log -1 --first-parent --no-show-signature --format=%H -- :(literal).");
    }

    [Test]
    public async Task GetReleaseTagCommit_WithEmptyOutput_Throws()
    {
        var (helper, _, root) = CreateSut(string.Empty);

        var exception =
            await Should.ThrowAsync<StepFailedException>(() => helper.GetReleaseTagCommit([root / "src/AppA/"]));

        exception.Message.ShouldContain(":(literal)src/AppA");
        exception.Message.ShouldContain("fetch-depth: 0");
    }

    [Test]
    public async Task GetReleaseTagCommit_WithNoPathsAndEmptyOutput_Throws()
    {
        var (helper, _, _) = CreateSut(string.Empty);

        var exception = await Should.ThrowAsync<StepFailedException>(() => helper.GetReleaseTagCommit([]));

        exception.Message.ShouldContain("fetch-depth: 0");
    }

    [Test]
    public async Task GetReleaseTagCommit_WithPathsInShallowClone_Throws()
    {
        var (helper, calls, root) = CreateSut(shallowOutput: "true");

        var exception =
            await Should.ThrowAsync<StepFailedException>(() => helper.GetReleaseTagCommit([root / "src/AppA/"]));

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
        var (helper, calls, _) = CreateSut(shallowOutput: "true");

        var commit = await helper.GetReleaseTagCommit([]);

        commit.ShouldBe(Commit);
        calls.ShouldHaveSingleItem();
    }

    [TestCase("..")]
    [TestCase("../AtomOther/src")]
    [TestCase("src/../../x")]
    public async Task GetReleaseTagCommit_WithPathNotBelowRoot_Throws(string path)
    {
        var (helper, calls, root) = CreateSut();

        var exception = await Should.ThrowAsync<ArgumentException>(() =>
            helper.GetReleaseTagCommit([root / "src/AppA/", root / path]));

        exception.ShouldSatisfyAllConditions(e => e.ParamName.ShouldBe("paths"),
            e => e.Message.ShouldStartWith(
                $"Path '{root / path}' is not at or below the Atom root directory '{root}'."));

        calls.ShouldBeEmpty();
    }

    [Test]
    public async Task GetReleaseTagCommit_WithNullPath_Throws()
    {
        var (helper, calls, root) = CreateSut();

        await Should.ThrowAsync<ArgumentException>(() => helper.GetReleaseTagCommit([root / "src/AppA/", null!]));

        calls.ShouldBeEmpty();
    }

    [Test]
    public async Task GetReleaseTagCommit_WithNullPaths_Throws()
    {
        var (helper, _, _) = CreateSut();

        await Should.ThrowAsync<ArgumentNullException>(() =>
            helper.GetReleaseTagCommit((IEnumerable<RootedPath>)null!));
    }

    [Test]
    public async Task GetReleaseTagCommit_WithProductOnBuildWithoutProducts_Throws()
    {
        var (helper, calls, _) = CreateSut();

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            helper.GetReleaseTagCommit(new Product("AppA")
            {
                Paths = ["src/AppA/"],
            }));

        exception.Message.ShouldBe("GetReleaseTagCommit(Product) requires a build that implements IProductsDefinition.");
        calls.ShouldBeEmpty();
    }

    [Test]
    public async Task GetReleaseTagCommit_WithNullProduct_Throws()
    {
        var (helper, _, _) = CreateSut();

        await Should.ThrowAsync<ArgumentNullException>(() => helper.GetReleaseTagCommit((Product)null!));
    }
}
