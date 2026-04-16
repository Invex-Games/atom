namespace DecSm.Atom.Tests.ClassTests.Paths;

[TestFixture]
internal sealed class TransformFileScopeTests
{
    // Explicitly return IAtomFileSystem as it's better in this context
#pragma warning disable CA1859
    private static IAtomFileSystem CreateFileSystem(
        IDictionary<string, MockFileData> files,
        string currentDirectory = "") =>
        new AtomFileSystem(A.Fake<ILogger<AtomFileSystem>>())
        {
            PathLocators = [],
            FileSystem = new MockFileSystem(files, currentDirectory),
        };
#pragma warning restore CA1859

    [Test]
    public async Task CreateAsync_WhenFileDoesNotExist_CreatesFileAndWritesTransformedContent()
    {
        // Arrange
        var fs = CreateFileSystem(new Dictionary<string, MockFileData>());

        // Act
        await using var scope = await TransformFileScope.CreateAsync(fs.CreateRootedPath("file.txt"), _ => "test-text");

        // Assert
        (await fs.File.ReadAllTextAsync("file.txt")).ShouldBe("test-text");
    }

    [Test]
    public async Task CreateAsync_WhenFileExists_OverwritesFileWithTransformedContent()
    {
        // Arrange
        var fs = CreateFileSystem(new Dictionary<string, MockFileData>
        {
            { "file.txt", new("existing-content") },
        });

        // Act
        await using var scope = await TransformFileScope.CreateAsync(fs.CreateRootedPath("file.txt"), _ => "test-text");

        // Assert
        (await fs.File.ReadAllTextAsync("file.txt")).ShouldBe("test-text");
    }

    [Test]
    public async Task AddAsync_WhenScopeIsDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var fs = CreateFileSystem(new Dictionary<string, MockFileData>
        {
            { "file.txt", new("existing-content") },
        });

        var scope = await TransformFileScope.CreateAsync(fs.CreateRootedPath("file.txt"), _ => "test-text");

        // Act
        await scope
            .DisposeAsync()
            .ConfigureAwait(false);

        // Assert
        Should.Throw<ObjectDisposedException>(() => scope.AddAsync(_ => "test-text"));
    }

    [Test]
    public async Task AddAsync_AddsTransformationToExistingContent()
    {
        // Arrange
        var fs = CreateFileSystem(new Dictionary<string, MockFileData>
        {
            { "file.txt", new("existing-content") },
        });

        await using var scope = await TransformFileScope.CreateAsync(fs.CreateRootedPath("file.txt"), _ => "test-text");

        // Act
        await scope.AddAsync(c => $"{c}-additional-text");

        // Assert
        (await fs.File.ReadAllTextAsync("file.txt")).ShouldBe("test-text-additional-text");
    }

    [Test]
    public void CreateAndRestore_ResetsFileContentToOriginal()
    {
        // Arrange
        var fs = CreateFileSystem(new Dictionary<string, MockFileData>
        {
            { "file.txt", new("existing-content") },
        });

        // Act
        var scope = TransformFileScope.Create(fs.CreateRootedPath("file.txt"), _ => "test-text");
        scope.Dispose();

        // Assert
        fs
            .File
            .ReadAllText("file.txt")
            .ShouldBe("existing-content");
    }
}
