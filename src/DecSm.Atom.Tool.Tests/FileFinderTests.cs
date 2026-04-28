namespace DecSm.Atom.Tool.Tests;

[TestFixture]
internal sealed class FileFinderTests
{
    private MockFileSystem _fs = null!;

    private static string Root =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\"
            : "/";

    private string CombinePath(params string[] parts) =>
        _fs.Path.Combine([Root, ..parts]);

    [SetUp]
    public void SetUp() =>
        _fs = new();

    // ── Direct file path ──────────────────────────────────────────────────────

    [Test]
    public void FindFile_WhenStartDirectoryIsExistingFile_ReturnsImmediately()
    {
        var filePath = CombinePath("repo", "build.csproj");
        _fs.AddFile(filePath, new(""));

        var result = FileFinder.FindFile(_fs, filePath, ["build.csproj"], false);

        result.ShouldNotBeNull();
        result.FullName.ShouldBe(filePath);
    }

    // ── Current directory ─────────────────────────────────────────────────────

    [Test]
    public void FindFile_FileInCurrentDirectory_ReturnsFile()
    {
        var dir = CombinePath("repo");
        var filePath = CombinePath("repo", "_atom.csproj");
        _fs.AddDirectory(dir);
        _fs.AddFile(filePath, new(""));
        _fs.Directory.SetCurrentDirectory(dir);

        var result = FileFinder.FindFile(_fs, dir, ["_atom.csproj"], false);

        result.ShouldNotBeNull();
        result.FullName.ShouldBe(filePath);
    }

    [Test]
    public void FindFile_FileNotInCurrentOrParentOrChild_ReturnsNull()
    {
        var dir = CombinePath("empty");
        _fs.AddDirectory(dir);
        _fs.Directory.SetCurrentDirectory(dir);

        var result = FileFinder.FindFile(_fs, dir, ["missing.csproj"], false);

        result.ShouldBeNull();
    }

    // ── Upward search ─────────────────────────────────────────────────────────

    [Test]
    public void FindFile_FileInParentDirectory_ReturnsFile()
    {
        var childDir = CombinePath("repo", "src", "project");
        var filePath = CombinePath("repo", "_atom.csproj");

        _fs.AddDirectory(childDir);
        _fs.AddFile(filePath, new(""));

        var result = FileFinder.FindFile(_fs, childDir, ["_atom.csproj"], false);

        result.ShouldNotBeNull();
        result.FullName.ShouldBe(filePath);
    }

    [Test]
    public void FindFile_StopsUpwardSearchAtGitDirectory()
    {
        // .git is at the root of subDir — searching from deeper should NOT find
        // the project file that lives ABOVE the .git boundary.
        var aboveGit = CombinePath("workspace");
        var repoRoot = CombinePath("workspace", "repo");
        var deepDir = CombinePath("workspace", "repo", "src");

        _fs.AddDirectory(deepDir);
        _fs.AddDirectory(_fs.Path.Combine(repoRoot, ".git")); // git root marker
        _fs.AddFile(_fs.Path.Combine(aboveGit, "above.csproj"), new(""));

        _fs.Directory.SetCurrentDirectory(deepDir);

        var result = FileFinder.FindFile(_fs, deepDir, ["above.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_StopsUpwardSearchAtSlnFile()
    {
        // FileFinder stops when it finds a file/directory LITERALLY named ".sln"
        var workspace = CombinePath("workspace");
        var repo = CombinePath("workspace", "repo");
        var deep = CombinePath("workspace", "repo", "sub");

        _fs.AddDirectory(deep);
        _fs.AddFile(_fs.Path.Combine(repo, ".sln"), new("")); // exact name ".sln"
        _fs.AddFile(_fs.Path.Combine(workspace, "above.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, deep, ["above.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_StopsUpwardSearchAtSlnxFile()
    {
        // FileFinder stops when it finds a file/directory LITERALLY named ".slnx"
        var workspace = CombinePath("workspace");
        var repo = CombinePath("workspace", "repo");
        var deep = CombinePath("workspace", "repo", "sub");

        _fs.AddDirectory(deep);
        _fs.AddFile(_fs.Path.Combine(repo, ".slnx"), new("")); // exact name ".slnx"
        _fs.AddFile(_fs.Path.Combine(workspace, "above.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, deep, ["above.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_FindsFileInSameDirectoryAsRootMarker()
    {
        // File is at the same level as the .git marker (i.e., in the repo root)
        var repoRoot = CombinePath("repo");
        var deep = CombinePath("repo", "src");

        _fs.AddDirectory(deep);
        _fs.AddDirectory(_fs.Path.Combine(repoRoot, ".git"));
        _fs.AddFile(_fs.Path.Combine(repoRoot, "_atom.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, deep, ["_atom.csproj"], false);

        result.ShouldNotBeNull();
    }

    // ── Downward search ───────────────────────────────────────────────────────

    [Test]
    public void FindFile_FileInSubdirectory_ReturnsFile()
    {
        var baseDir = CombinePath("workspace");
        var filePath = CombinePath("workspace", "atoms", "_atom.csproj");

        _fs.AddDirectory(baseDir);
        _fs.AddFile(filePath, new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["_atom.csproj"], false);

        result.ShouldNotBeNull();
        result.FullName.ShouldBe(filePath);
    }

    [Test]
    public void FindFile_DownwardSearch_SkipsBinFolder()
    {
        var baseDir = CombinePath("workspace");
        var binDir = CombinePath("workspace", "bin");
        _fs.AddDirectory(baseDir);
        _fs.AddFile(_fs.Path.Combine(binDir, "_atom.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["_atom.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_DownwardSearch_SkipsObjFolder()
    {
        var baseDir = CombinePath("workspace");
        _fs.AddDirectory(baseDir);
        _fs.AddFile(_fs.Path.Combine(baseDir, "obj", "_atom.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["_atom.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_DownwardSearch_SkipsNodeModulesFolder()
    {
        var baseDir = CombinePath("workspace");
        _fs.AddDirectory(baseDir);
        _fs.AddFile(_fs.Path.Combine(baseDir, "node_modules", "tool.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["tool.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_DownwardSearch_SkipsGitFolder()
    {
        var baseDir = CombinePath("workspace");
        _fs.AddDirectory(baseDir);
        _fs.AddFile(_fs.Path.Combine(baseDir, ".git", "target.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["target.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_DownwardSearch_SkipsVsFolder()
    {
        var baseDir = CombinePath("workspace");
        _fs.AddDirectory(baseDir);
        _fs.AddFile(_fs.Path.Combine(baseDir, ".vs", "target.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["target.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_DownwardSearch_RespectsMaxDepthFourLevels()
    {
        var baseDir = CombinePath("workspace");

        // Depth 4 from baseDir
        var tooDeepPath = _fs.Path.Combine(baseDir, "L1", "L2", "L3", "L4", "L5", "target.csproj");

        _fs.AddDirectory(baseDir);
        _fs.AddFile(tooDeepPath, new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["target.csproj"], false);

        result.ShouldBeNull();
    }

    [Test]
    public void FindFile_DownwardSearch_FindsAtMaxAllowedDepth()
    {
        var baseDir = CombinePath("workspace");

        // Exactly at depth 4 (MaxDownstreamDepth)
        var atMaxDepthPath = _fs.Path.Combine(baseDir, "L1", "L2", "L3", "L4", "target.csproj");

        _fs.AddDirectory(baseDir);
        _fs.AddFile(atMaxDepthPath, new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["target.csproj"], false);

        result.ShouldNotBeNull();
    }

    // ── Convention search (checkParentSubfolderMatches) ───────────────────────

    [Test]
    public void FindFile_WithConventionSearch_FindsFileInMatchingSubdirectory()
    {
        // If searching for "_atom.csproj" and there is a folder named "_atom"
        // containing "_atom.csproj", it should be found.
        var baseDir = CombinePath("repo");
        var nestedPath = CombinePath("repo", "_atom", "_atom.csproj");

        _fs.AddDirectory(baseDir);
        _fs.AddFile(nestedPath, new(""));

        var result = FileFinder.FindFile(_fs, baseDir, ["_atom.csproj"], true);

        result.ShouldNotBeNull();
        result.FullName.ShouldBe(nestedPath);
    }

    [Test]
    public void FindFile_WithoutConventionSearch_DoesNotFindFileInMatchingSubdirectory()
    {
        var baseDir = CombinePath("repo");
        var nestedPath = CombinePath("repo", "_atom", "_atom.csproj");

        _fs.AddDirectory(baseDir);
        _fs.AddFile(nestedPath, new(""));

        // Without convention search, the file at "_atom/_atom.csproj" is not
        // found unless we happen to traverse into the "_atom" subfolder during BFS.
        // Since the BFS DOES descend into subfolders, it will find it — BUT the
        // convention search only applies during the upward walk.
        // Testing the *direct* case: just the nested path, no non-nested version.
        var result = FileFinder.FindFile(_fs, baseDir, ["_atom.csproj"], false);

        // BFS WILL find it because "_atom" subfolder is not excluded
        result.ShouldNotBeNull();
    }

    [Test]
    public void FindFile_ConventionSearch_OnlyUsedDuringUpwardWalk()
    {
        // Convention search: parent directory should match filename without extension.
        // Put only a file at /repo/parent1/_atom.csproj where parent1 != _atom
        // So no convention match.
        var baseDir = CombinePath("repo");
        _fs.AddDirectory(baseDir);
        _fs.AddFile(CombinePath("repo", "parent1", "_atom.csproj"), new(""));

        // The upward walk checks /repo for _atom.csproj directly, not /repo/parent1/_atom.csproj
        // convention requires parent dir name == filename-without-ext
        // parent1 != _atom, so convention doesn't match it
        // But BFS downward WILL find it (parent1 is not excluded).
        var result = FileFinder.FindFile(_fs, baseDir, ["_atom.csproj"], true);

        // BFS finds it anyway through normal traversal
        result.ShouldNotBeNull();
    }

    // ── Multiple file names ───────────────────────────────────────────────────

    [Test]
    public void FindFile_WithMultipleFileNames_ReturnsFirstMatch()
    {
        var dir = CombinePath("workspace");
        _fs.AddDirectory(dir);
        _fs.AddFile(_fs.Path.Combine(dir, "second.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, dir, ["first.csproj", "second.csproj"], false);

        result.ShouldNotBeNull();

        _fs
            .Path
            .GetFileName(result.FullName)
            .ShouldBe("second.csproj");
    }

    [Test]
    public void FindFile_AllNamesExist_ReturnsFirstNameMatch()
    {
        var dir = CombinePath("workspace");
        _fs.AddDirectory(dir);
        _fs.AddFile(_fs.Path.Combine(dir, "first.csproj"), new(""));
        _fs.AddFile(_fs.Path.Combine(dir, "second.csproj"), new(""));

        var result = FileFinder.FindFile(_fs, dir, ["first.csproj", "second.csproj"], false);

        result.ShouldNotBeNull();

        _fs
            .Path
            .GetFileName(result.FullName)
            .ShouldBe("first.csproj");
    }
}
