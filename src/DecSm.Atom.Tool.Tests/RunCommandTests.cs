using System.IO.Abstractions;

namespace DecSm.Atom.Tool.Tests;

[TestFixture]
public class RunCommandTests
{
    [SetUp]
    public void SetUp()
    {
        _fs = new();
        RunCommand.FileSystem = _fs;
        RunCommand.MockDotnetCli = true;
    }

    [TearDown]
    public void TearDown()
    {
        RunCommand.FileSystem = new FileSystem();
        RunCommand.MockDotnetCli = false;
    }

    private MockFileSystem _fs = null!;

    private static string Root =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\"
            : "/";

    private string P(params string[] parts) =>
        _fs.Path.Combine([Root, ..parts]);

    // ── Upward-search / root-marker boundary ─────────────────────────────────

    [Test]
    public async Task Handle_ShouldFindProjectInParent_AndStopAtRootMarker()
    {
        var repoDir = P("Repo");
        var subDir = P("Repo", "SubFolder");
        var targetDir = P("Repo", "SubFolder", "Target");

        _fs.AddDirectory(targetDir);
        _fs.AddFile(P("Repo", "MyProj.csproj"), new(""));
        _fs.AddDirectory(_fs.Path.Combine(subDir, ".git")); // root marker

        _fs.Directory.SetCurrentDirectory(targetDir);

        var result = await RunCommand.Handle([], "MyProj", CancellationToken.None);

        result.ShouldBe(1); // blocked by .git root marker
    }

    // ── Downward convention search ────────────────────────────────────────────

    [Test]
    public async Task Handle_ShouldFindNestedProject_WhenConventionSearchIsEnabled()
    {
        var workDir = P("Work");
        _fs.AddFile(P("Work", "Atom", "Atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "Atom", CancellationToken.None);

        result.ShouldBe(0);
    }

    // ── Breadth-first ordering ────────────────────────────────────────────────

    [Test]
    public async Task Handle_ShouldPrioritizeBreadthFirst_InDownwardSearch()
    {
        var searchRoot = P("SearchRoot");

        _fs.AddFile(P("SearchRoot", "Level1", "Level2", "Target.csproj"), new(""));
        _fs.AddFile(P("SearchRoot", "Level1_Sibling", "Target.csproj"), new(""));
        _fs.AddDirectory(searchRoot);

        _fs.Directory.SetCurrentDirectory(searchRoot);

        var result = await RunCommand.Handle([], "Target", CancellationToken.None);

        result.ShouldBe(0);
    }

    // ── .csproj extension subject ─────────────────────────────────────────────

    [Test]
    public async Task Handle_WithDotCsprojSubject_FindsProject_ReturnsZero()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "MyBuild.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "MyBuild.csproj", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithDotCsprojSubject_ProjectNotFound_ReturnsOne()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "Missing.csproj", CancellationToken.None);

        result.ShouldBe(1);
    }

    // ── .cs extension subject ─────────────────────────────────────────────────

    [Test]
    public async Task Handle_WithDotCsSubject_FindsCsFile_ReturnsZero()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "build.cs"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "build.cs", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithDotCsSubject_FileNotFound_ReturnsOne()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "missing.cs", CancellationToken.None);

        result.ShouldBe(1);
    }

    // ── Name-only subject (Either) ────────────────────────────────────────────

    [Test]
    public async Task Handle_WithEitherSubject_FindsProject_ReturnsZero()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "mybuild.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEitherSubject_FindsCsFile_ReturnsZero()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "mybuild.cs"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEitherSubject_NotFound_ReturnsOne()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(1);
    }

    [Test]
    public async Task Handle_WithEitherSubject_PrefersProjectOverCsFile()
    {
        // Both exist — project wins (project is checked first)
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "mybuild.csproj"), new(""));
        _fs.AddFile(P("work", "mybuild.cs"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        // Should succeed regardless of which one it picks
        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    // ── Empty subject (None — default discovery) ──────────────────────────────

    [Test]
    public async Task Handle_WithEmptySubject_FindsAtomProject_ReturnsZero()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEmptySubject_FindsBuildProject_ReturnsZero()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_build.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEmptySubject_NothingFound_ReturnsOne()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(1);
    }

    // ── Argument sanitization ─────────────────────────────────────────────────

    [Test]
    public async Task Handle_SubjectWithNewlines_IsSanitizedBeforeSearch()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "mybuild.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        // Newlines in subject should be stripped
        var result = await RunCommand.Handle([], "my\nbuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_SubjectWithCarriageReturn_IsSanitizedBeforeSearch()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "mybuild.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "my\rbuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_SubjectWithSurroundingQuotes_IsTrimmed()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "mybuild.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "\"mybuild\"", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_SubjectWithSingleQuotes_IsTrimmed()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "mybuild.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "'mybuild'", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_SubjectWithSurroundingSpaces_IsTrimmed()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "mybuild.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "  mybuild  ", CancellationToken.None);

        result.ShouldBe(0);
    }

    // ── Run-args are forwarded ────────────────────────────────────────────────

    [Test]
    public async Task Handle_WithRunArgs_DoesNotAffectSearchOrReturnCode()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        // Extra run args should not affect discovery or mock return
        var result = await RunCommand.Handle(["--target", "Build", "--param", "Foo=Bar"],
            string.Empty,
            CancellationToken.None);

        result.ShouldBe(0);
    }
}
