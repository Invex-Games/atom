namespace DecSm.Atom.FileSystem.Tests;

[TestFixture]
internal sealed class TransformFileScopeTests
{
    private static readonly string Root = OperatingSystem.IsWindows()
        ? @"C:\"
        : "/";

    private static string FilePath(string name) =>
        $"{Root}{name}";

    private static RootedPath CreateRootedPath(IFileSystem fileSystem, string name) =>
        new(fileSystem, FilePath(name));

    [Test]
    public async Task CreateAsync_WhenFileDoesNotExist_CreatesFileAndWritesTransformedContent()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var file = CreateRootedPath(mockFs, "file.txt");

        // Act
        await using var scope = await TransformFileScope.CreateAsync(file, _ => "test-text");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file.txt"))).ShouldBe("test-text");
    }

    [Test]
    public async Task CreateAsync_WhenFileExists_OverwritesWithTransformedContent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("existing-content") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");

        // Act
        await using var scope = await TransformFileScope.CreateAsync(file, _ => "test-text");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file.txt"))).ShouldBe("test-text");
    }

    [Test]
    public async Task CreateAsync_WhenFileExists_PassesExistingContentToTransform()
    {
        // Arrange
        string? capturedContent = null;

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("existing-content") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");

        // Act
        await using var scope = await TransformFileScope.CreateAsync(file,
            content =>
            {
                capturedContent = content;

                return "new-content";
            });

        // Assert
        capturedContent.ShouldBe("existing-content");
    }

    [Test]
    public async Task CreateAsync_WhenFileDoesNotExist_PassesEmptyStringToTransform()
    {
        // Arrange
        string? capturedContent = null;

        var mockFs = new MockFileSystem();
        var file = CreateRootedPath(mockFs, "file.txt");

        // Act
        await using var scope = await TransformFileScope.CreateAsync(file,
            content =>
            {
                capturedContent = content;

                return "new-content";
            });

        // Assert
        capturedContent.ShouldBe(string.Empty);
    }

    [Test]
    public async Task DisposeAsync_WhenFileExisted_RestoresOriginalContent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("existing-content") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = await TransformFileScope.CreateAsync(file, _ => "test-text");

        // Act
        await scope.DisposeAsync();

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file.txt"))).ShouldBe("existing-content");
    }

    [Test]
    public async Task DisposeAsync_WhenFileDidNotExist_DeletesFile()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = await TransformFileScope.CreateAsync(file, _ => "test-text");

        // Act
        await scope.DisposeAsync();

        // Assert
        mockFs
            .File
            .Exists(FilePath("file.txt"))
            .ShouldBeFalse();
    }

    [Test]
    public async Task DisposeAsync_WhenCalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("original") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = await TransformFileScope.CreateAsync(file, _ => "transformed");

        // Act
        await scope.DisposeAsync();
        await scope.DisposeAsync();

        // Assert - content should be restored (from first dispose), not double-restored
        (await mockFs.File.ReadAllTextAsync(FilePath("file.txt"))).ShouldBe("original");
    }

    [Test]
    public void Create_WhenFileDoesNotExist_CreatesFileAndWritesTransformedContent()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var file = CreateRootedPath(mockFs, "file.txt");

        // Act
        using var scope = TransformFileScope.Create(file, _ => "test-text");

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file.txt"))
            .ShouldBe("test-text");
    }

    [Test]
    public void Create_WhenFileExists_OverwritesWithTransformedContent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("existing-content") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");

        // Act
        using var scope = TransformFileScope.Create(file, _ => "test-text");

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file.txt"))
            .ShouldBe("test-text");
    }

    [Test]
    public void Dispose_WhenFileExisted_RestoresOriginalContent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("existing-content") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = TransformFileScope.Create(file, _ => "test-text");

        // Act
        scope.Dispose();

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file.txt"))
            .ShouldBe("existing-content");
    }

    [Test]
    public void Dispose_WhenFileDidNotExist_DeletesFile()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = TransformFileScope.Create(file, _ => "test-text");

        // Act
        scope.Dispose();

        // Assert
        mockFs
            .File
            .Exists(FilePath("file.txt"))
            .ShouldBeFalse();
    }

    [Test]
    public void Dispose_WhenCalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("original") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = TransformFileScope.Create(file, _ => "transformed");

        // Act
        scope.Dispose();
        scope.Dispose();

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file.txt"))
            .ShouldBe("original");
    }

    [Test]
    public async Task AddAsync_AppliesAdditionalTransformToCurrentContent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("original") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        await using var scope = await TransformFileScope.CreateAsync(file, _ => "step-1");

        // Act
        await scope.AddAsync(c => $"{c}-step-2");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file.txt"))).ShouldBe("step-1-step-2");
    }

    [Test]
    public void Add_AppliesAdditionalTransformToCurrentContent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("original") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        using var scope = TransformFileScope.Create(file, _ => "step-1");

        // Act
        scope.Add(c => $"{c}-step-2");

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file.txt"))
            .ShouldBe("step-1-step-2");
    }

    [Test]
    public async Task AddAsync_WhenScopeIsDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("content") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = await TransformFileScope.CreateAsync(file, _ => "test-text");
        await scope.DisposeAsync();

        // Act / Assert
        Should.Throw<ObjectDisposedException>(() => scope.AddAsync(_ => "more-text"));
    }

    [Test]
    public void Add_WhenScopeIsDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("content") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = TransformFileScope.Create(file, _ => "test-text");
        scope.Dispose();

        // Act / Assert
        Should.Throw<ObjectDisposedException>(() => scope.Add(_ => "more-text"));
    }

    [Test]
    public async Task CancelRestore_PreventsRestorationOnDisposeAsync()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("original") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = await TransformFileScope.CreateAsync(file, _ => "transformed");

        // Act
        scope.CancelRestore();
        await scope.DisposeAsync();

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file.txt"))).ShouldBe("transformed");
    }

    [Test]
    public void CancelRestore_PreventsRestorationOnDispose()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("original") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");
        var scope = TransformFileScope.Create(file, _ => "transformed");

        // Act
        scope.CancelRestore();
        scope.Dispose();

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file.txt"))
            .ShouldBe("transformed");
    }

    [Test]
    public async Task CreateAsync_WhenOperationCancelled_RethrowsOperationCanceledException()
    {
        // Arrange
        // Use a pre-cancelled token; ReadAllTextAsync on an existing file will throw before the scope is created
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file.txt"), new("existing") },
        });

        var file = CreateRootedPath(mockFs, "file.txt");

        // Act / Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            TransformFileScope.CreateAsync(file, content => content, cts.Token));
    }
}
