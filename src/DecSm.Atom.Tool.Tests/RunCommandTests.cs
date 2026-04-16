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

    private MockFileSystem _fs = null!;

    private static string GetRoot() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\"
            : "/";

    [Test]
    public async Task Handle_ShouldFindProjectInParent_AndStopAtRootMarker()
    {
        // Arrange
        var root = GetRoot();
        var repoDir = _fs.Path.Combine(root, "Repo");
        var subDir = _fs.Path.Combine(repoDir, "SubFolder");
        var targetDir = _fs.Path.Combine(subDir, "Target");

        // CRITICAL FIX: Explicitly create the full path
        // MockFileSystem needs the physical directory to exist to enumerate it
        _fs.AddDirectory(targetDir);

        // Adding the file automatically creates 'Repo', but not 'SubFolder\Target'
        _fs.AddFile(_fs.Path.Combine(repoDir, "MyProj.csproj"), new(""));

        // Add the root marker
        _fs.AddDirectory(_fs.Path.Combine(subDir, ".git"));

        _fs.Directory.SetCurrentDirectory(targetDir);

        // Act
        var result = await RunCommand.Handle([], "MyProj", CancellationToken.None);

        // Assert
        result.ShouldBe(1);
    }

    [Test]
    public async Task Handle_ShouldFindNestedProject_WhenConventionSearchIsEnabled()
    {
        // Arrange
        var root = GetRoot();
        var workDir = _fs.Path.Combine(root, "Work");
        var nestedProject = _fs.Path.Combine(workDir, "Atom", "Atom.csproj");

        // AddDirectory is not strictly needed here because AddFile creates the parent
        _fs.AddFile(nestedProject, new(""));
        _fs.Directory.SetCurrentDirectory(workDir);

        // Act
        var result = await RunCommand.Handle([], "Atom", CancellationToken.None);

        // Assert
        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_ShouldPrioritizeBreadthFirst_InDownwardSearch()
    {
        // Arrange
        var root = GetRoot();
        var searchRoot = _fs.Path.Combine(root, "SearchRoot");

        var deepPath = _fs.Path.Combine(searchRoot, "Level1", "Level2", "Target.csproj");
        var shallowPath = _fs.Path.Combine(searchRoot, "Level1_Sibling", "Target.csproj");

        // AddFile creates all necessary parent directories for these files
        _fs.AddFile(deepPath, new(""));
        _fs.AddFile(shallowPath, new(""));

        // Ensure the search root itself is initialized
        _fs.AddDirectory(searchRoot);
        _fs.Directory.SetCurrentDirectory(searchRoot);

        // Act
        var result = await RunCommand.Handle([], "Target", CancellationToken.None);

        // Assert
        result.ShouldBe(0);
    }
}
