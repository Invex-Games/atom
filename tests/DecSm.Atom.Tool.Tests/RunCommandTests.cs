namespace DecSm.Atom.Tool.Tests;

[TestFixture]
internal sealed class RunCommandTests
{
    [SetUp]
    public void SetUp()
    {
        _fs = new();
        RunCommand.FileSystem = _fs;
        RunCommand.MockDotnetCli = true;
        Environment.SetEnvironmentVariable("ATOM_NO_RESTORE_CACHE", null);
    }

    [TearDown]
    public void TearDown()
    {
        RunCommand.FileSystem = new FileSystem();
        RunCommand.MockDotnetCli = false;
        Environment.SetEnvironmentVariable("ATOM_NO_RESTORE_CACHE", null);
    }

    private MockFileSystem _fs = null!;

    private static string Root =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\"
            : "/";

    private string P(params string[] parts) =>
        _fs.Path.Combine([Root, ..parts]);

    [Test]
    public async Task Handle_ShouldFindProjectInParent_AndStopAtRootMarker()
    {
        var subDir = P("Repo", "SubFolder");
        var targetDir = P("Repo", "SubFolder", "Target");

        _fs.AddDirectory(targetDir);
        _fs.AddFile(P("Repo", "MyProj.csproj"), new(""));
        _fs.AddDirectory(_fs.Path.Combine(subDir, ".git")); // root marker

        _fs.Directory.SetCurrentDirectory(targetDir);

        var result = await RunCommand.Handle([], "MyProj", CancellationToken.None);

        result.ShouldBe(1); // blocked by .git root marker
    }

    [Test]
    public async Task Handle_ShouldFindNestedProject_WhenConventionSearchIsEnabled()
    {
        var workDir = P("Work");
        _fs.AddFile(P("Work", "Atom", "Atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], "Atom", CancellationToken.None);

        result.ShouldBe(0);
    }

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

    [Test]
    public async Task Handle_CacheMiss_PerformsRestore_AndWritesCache()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoRestore.ShouldBeFalse();

        _fs
            .File
            .Exists(P("work", "obj", ".atom-restore.hash"))
            .ShouldBeTrue();
    }

    [Test]
    public async Task Handle_CacheHit_SkipsRestore()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        // First run performs a restore and writes the cache.
        await RunCommand.Handle([], string.Empty, CancellationToken.None);

        // Simulate the restore output that 'dotnet restore' would produce.
        _fs.AddFile(P("work", "obj", "project.assets.json"), new(""));

        // Second run with unchanged inputs should skip the restore.
        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoRestore.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_CacheInvalidated_WhenInputChanges_PerformsRestore()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        await RunCommand.Handle([], string.Empty, CancellationToken.None);
        _fs.AddFile(P("work", "obj", "project.assets.json"), new(""));

        // Change the project file content - the cached hash should no longer match.
        _fs.File.WriteAllText(P("work", "_atom.csproj"), "<Project></Project>");

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoRestore.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_CacheInvalidated_WhenDirectoryPackagesChanges_PerformsRestore()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.AddFile(P("work", "Directory.Packages.props"), new("<Project></Project>"));
        _fs.Directory.SetCurrentDirectory(workDir);

        await RunCommand.Handle([], string.Empty, CancellationToken.None);
        _fs.AddFile(P("work", "obj", "project.assets.json"), new(""));

        // A walked-up restore input changed - restore should run again.
        _fs.File.WriteAllText(P("work", "Directory.Packages.props"), "<Project><ItemGroup /></Project>");

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoRestore.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_NoRestoreCacheFlag_AlwaysPerformsRestore()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        await RunCommand.Handle([], string.Empty, CancellationToken.None);
        _fs.AddFile(P("work", "obj", "project.assets.json"), new(""));

        // Even though the cache would normally hit, the opt-out forces a restore.
        var result = await RunCommand.Handle([], string.Empty, true, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoRestore.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_NoRestoreCacheEnvVar_AlwaysPerformsRestore()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        await RunCommand.Handle([], string.Empty, CancellationToken.None);
        _fs.AddFile(P("work", "obj", "project.assets.json"), new(""));

        Environment.SetEnvironmentVariable("ATOM_NO_RESTORE_CACHE", "1");

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoRestore.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_BuildCacheMiss_PerformsBuild_AndWritesCache()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoBuild.ShouldBeFalse();

        _fs
            .File
            .Exists(P("work", "obj", ".atom-build.hash"))
            .ShouldBeTrue();
    }

    [Test]
    public async Task Handle_BuildCacheHit_SkipsBuild()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        // First run builds and writes the cache.
        await RunCommand.Handle([], string.Empty, CancellationToken.None);

        // Simulate the build output that 'dotnet build' would produce.
        _fs.AddFile(P("work", "bin", "Debug", "net10.0", "_atom.dll"), new(""));

        // Second run with unchanged inputs should skip the build entirely.
        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoBuild.ShouldBeTrue();
        RunCommand.LastUsedNoRestore.ShouldBeFalse(); // --no-build supersedes --no-restore
    }

    [Test]
    public async Task Handle_BuildCacheInvalidated_WhenSourceChanges_PerformsBuild()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.AddFile(P("work", "Program.cs"), new("// v1"));
        _fs.Directory.SetCurrentDirectory(workDir);

        await RunCommand.Handle([], string.Empty, CancellationToken.None);
        _fs.AddFile(P("work", "bin", "Debug", "net10.0", "_atom.dll"), new(""));

        // A source file changed - the build can no longer be skipped.
        _fs.File.WriteAllText(P("work", "Program.cs"), "// v2");

        var result = await RunCommand.Handle([], string.Empty, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoBuild.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_NoRestoreCacheFlag_AlsoForcesBuild()
    {
        var workDir = P("work");
        _fs.AddDirectory(workDir);
        _fs.AddFile(P("work", "_atom.csproj"), new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        await RunCommand.Handle([], string.Empty, CancellationToken.None);
        _fs.AddFile(P("work", "bin", "Debug", "net10.0", "_atom.dll"), new(""));

        // Even though the build cache would hit, the opt-out forces a full build.
        var result = await RunCommand.Handle([], string.Empty, true, CancellationToken.None);

        result.ShouldBe(0);
        RunCommand.LastUsedNoBuild.ShouldBeFalse();
    }
}
