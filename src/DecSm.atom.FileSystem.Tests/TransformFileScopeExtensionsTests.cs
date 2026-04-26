namespace DecSm.Atom.FileSystem.Tests;

[TestFixture]
internal sealed class TransformFileScopeExtensionsTests
{
    private static readonly string Root = OperatingSystem.IsWindows()
        ? @"C:\"
        : "/";

    private static string FilePath(string name) =>
        $"{Root}{name}";

    private static RootedPath CreateRootedPath(IFileSystem fileSystem, string name) =>
        new(fileSystem, FilePath(name));

    [Test]
    public async Task AddAsync_OnTransformFileScopeTask_AppliesChainedTransform()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("original") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");

        // Act
        await using var scope = await TransformFileScope
            .CreateAsync(file, _ => "step-1")
            .AddAsync(c => $"{c}-step-2");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file.txt"))).ShouldBe("step-1-step-2");
    }

    [Test]
    public async Task AddAsync_OnTransformFileScopeTask_ReturnsSameScopeInstance()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("content") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");

        var originalScope = await TransformFileScope.CreateAsync(file, content => content);

        // Act
        var returnedScope = await Task
            .FromResult(originalScope)
            .AddAsync(c => c);

        // Assert
        returnedScope.ShouldBeSameAs(originalScope);

        await originalScope.DisposeAsync();
    }

    [Test]
    public async Task AddAsync_OnTransformFileScopeTask_CanChainMultipleTimes()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("a") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");

        // Act
        await using var scope = await TransformFileScope
            .CreateAsync(file, _ => "1")
            .AddAsync(c => $"{c}2")
            .AddAsync(c => $"{c}3");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file.txt"))).ShouldBe("123");
    }

    [Test]
    public async Task AddAsync_OnTransformMultiFileScopeTask_AppliesChainedTransformToAllFiles()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("a") },
            { FilePath("file2.txt"), new("b") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        // Act
        await using var scope = await TransformMultiFileScope
            .CreateAsync(files, content => $"[{content}]")
            .AddAsync(content => $"<{content}>");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file1.txt"))).ShouldBe("<[a]>");
        (await mockFs.File.ReadAllTextAsync(FilePath("file2.txt"))).ShouldBe("<[b]>");
    }

    [Test]
    public async Task AddAsync_OnTransformMultiFileScopeTask_ReturnsSameScopeInstance()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("content") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt") };
        var originalScope = await TransformMultiFileScope.CreateAsync(files, content => content);

        // Act
        var returnedScope = await Task
            .FromResult(originalScope)
            .AddAsync(c => c);

        // Assert
        returnedScope.ShouldBeSameAs(originalScope);

        await originalScope.DisposeAsync();
    }

    [Test]
    public async Task AddAsync_OnTransformMultiFileScopeTask_CanChainMultipleTimes()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("x") },
            { FilePath("file2.txt"), new("y") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        // Act
        await using var scope = await TransformMultiFileScope
            .CreateAsync(files, _ => "1")
            .AddAsync(c => $"{c}2")
            .AddAsync(c => $"{c}3");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file1.txt"))).ShouldBe("123");
        (await mockFs.File.ReadAllTextAsync(FilePath("file2.txt"))).ShouldBe("123");
    }
}
