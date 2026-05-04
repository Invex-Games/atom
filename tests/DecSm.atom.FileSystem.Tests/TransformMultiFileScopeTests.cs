namespace DecSm.Atom.FileSystem.Tests;

[TestFixture]
internal sealed class TransformMultiFileScopeTests
{
    private static readonly string Root = OperatingSystem.IsWindows()
        ? @"C:\"
        : "/";

    private static string FilePath(string name) =>
        $"{Root}{name}";

    private static RootedPath CreateRootedPath(IFileSystem fileSystem, string name) =>
        new(fileSystem, FilePath(name));

    [Test]
    public async Task CreateAsync_WhenFilesDoNotExist_CreatesFilesWithTransformedContent()
    {
        // Arrange
        var mockFs = new MockFileSystem();

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        // Act
        await using var scope = await TransformMultiFileScope.CreateAsync(files, _ => "transformed");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file1.txt"))).ShouldBe("transformed");
        (await mockFs.File.ReadAllTextAsync(FilePath("file2.txt"))).ShouldBe("transformed");
    }

    [Test]
    public async Task CreateAsync_WhenFilesExist_OverwritesWithTransformedContent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("content-1") },
            { FilePath("file2.txt"), new("content-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        // Act
        await using var scope = await TransformMultiFileScope.CreateAsync(files, content => $"[{content}]");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file1.txt"))).ShouldBe("[content-1]");
        (await mockFs.File.ReadAllTextAsync(FilePath("file2.txt"))).ShouldBe("[content-2]");
    }

    [Test]
    public void Create_WhenFilesDoNotExist_CreatesFilesWithTransformedContent()
    {
        // Arrange
        var mockFs = new MockFileSystem();

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        // Act
        using var scope = TransformMultiFileScope.Create(files, _ => "transformed");

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file1.txt"))
            .ShouldBe("transformed");

        mockFs
            .File
            .ReadAllText(FilePath("file2.txt"))
            .ShouldBe("transformed");
    }

    [Test]
    public void Create_WhenFilesExist_OverwritesWithTransformedContent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("content-1") },
            { FilePath("file2.txt"), new("content-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        // Act
        using var scope = TransformMultiFileScope.Create(files, content => $"[{content}]");

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file1.txt"))
            .ShouldBe("[content-1]");

        mockFs
            .File
            .ReadAllText(FilePath("file2.txt"))
            .ShouldBe("[content-2]");
    }

    [Test]
    public async Task DisposeAsync_WhenFilesExisted_RestoresAllOriginalContents()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("original-1") },
            { FilePath("file2.txt"), new("original-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        var scope = await TransformMultiFileScope.CreateAsync(files, _ => "transformed");

        // Act
        await scope.DisposeAsync();

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file1.txt"))).ShouldBe("original-1");
        (await mockFs.File.ReadAllTextAsync(FilePath("file2.txt"))).ShouldBe("original-2");
    }

    [Test]
    public async Task DisposeAsync_WhenFilesDidNotExist_DeletesAllFiles()
    {
        // Arrange
        var mockFs = new MockFileSystem();

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        var scope = await TransformMultiFileScope.CreateAsync(files, _ => "transformed");

        // Act
        await scope.DisposeAsync();

        // Assert
        mockFs
            .File
            .Exists(FilePath("file1.txt"))
            .ShouldBeFalse();

        mockFs
            .File
            .Exists(FilePath("file2.txt"))
            .ShouldBeFalse();
    }

    [Test]
    public async Task DisposeAsync_WithMixedNewAndExistingFiles_HandlesEachCorrectly()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("existing.txt"), new("original") },
        });

        var files = new[] { CreateRootedPath(mockFs, "existing.txt"), CreateRootedPath(mockFs, "new.txt") };

        var scope = await TransformMultiFileScope.CreateAsync(files, _ => "transformed");

        // Act
        await scope.DisposeAsync();

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("existing.txt"))
            .ShouldBe("original");

        mockFs
            .File
            .Exists(FilePath("new.txt"))
            .ShouldBeFalse();
    }

    [Test]
    public void Dispose_WhenFilesExisted_RestoresAllOriginalContents()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("original-1") },
            { FilePath("file2.txt"), new("original-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        var scope = TransformMultiFileScope.Create(files, _ => "transformed");

        // Act
        scope.Dispose();

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file1.txt"))
            .ShouldBe("original-1");

        mockFs
            .File
            .ReadAllText(FilePath("file2.txt"))
            .ShouldBe("original-2");
    }

    [Test]
    public void Dispose_WhenFilesDidNotExist_DeletesAllFiles()
    {
        // Arrange
        var mockFs = new MockFileSystem();

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        var scope = TransformMultiFileScope.Create(files, _ => "transformed");

        // Act
        scope.Dispose();

        // Assert
        mockFs
            .File
            .Exists(FilePath("file1.txt"))
            .ShouldBeFalse();

        mockFs
            .File
            .Exists(FilePath("file2.txt"))
            .ShouldBeFalse();
    }

    [Test]
    public async Task DisposeAsync_WhenCalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("original") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt") };
        var scope = await TransformMultiFileScope.CreateAsync(files, _ => "transformed");

        // Act
        await scope.DisposeAsync();
        await scope.DisposeAsync();

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file1.txt"))
            .ShouldBe("original");
    }

    [Test]
    public void Dispose_WhenCalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("original") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt") };
        var scope = TransformMultiFileScope.Create(files, _ => "transformed");

        // Act
        scope.Dispose();
        scope.Dispose();

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file1.txt"))
            .ShouldBe("original");
    }

    [Test]
    public async Task AddAsync_AppliesTransformToAllFiles()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("content-1") },
            { FilePath("file2.txt"), new("content-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        await using var scope = await TransformMultiFileScope.CreateAsync(files, content => $"[{content}]");

        // Act
        await scope.AddAsync(content => $"<{content}>");

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file1.txt"))).ShouldBe("<[content-1]>");
        (await mockFs.File.ReadAllTextAsync(FilePath("file2.txt"))).ShouldBe("<[content-2]>");
    }

    [Test]
    public void Add_AppliesTransformToAllFiles()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("content-1") },
            { FilePath("file2.txt"), new("content-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        using var scope = TransformMultiFileScope.Create(files, content => $"[{content}]");

        // Act
        scope.Add(content => $"<{content}>");

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file1.txt"))
            .ShouldBe("<[content-1]>");

        mockFs
            .File
            .ReadAllText(FilePath("file2.txt"))
            .ShouldBe("<[content-2]>");
    }

    [Test]
    public async Task AddAsync_WhenScopeIsDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("content") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt") };
        var scope = await TransformMultiFileScope.CreateAsync(files, _ => "transformed");
        await scope.DisposeAsync();

        // Act / Assert
        Should.Throw<ObjectDisposedException>(() => scope.AddAsync(_ => "more"));
    }

    [Test]
    public void Add_WhenScopeIsDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("content") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt") };
        var scope = TransformMultiFileScope.Create(files, _ => "transformed");
        scope.Dispose();

        // Act / Assert
        Should.Throw<ObjectDisposedException>(() => scope.Add(_ => "more"));
    }

    [Test]
    public async Task CancelRestore_PreventsRestorationOnDisposeAsync()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("original-1") },
            { FilePath("file2.txt"), new("original-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        var scope = await TransformMultiFileScope.CreateAsync(files, _ => "transformed");

        // Act
        scope.CancelRestore();
        await scope.DisposeAsync();

        // Assert
        (await mockFs.File.ReadAllTextAsync(FilePath("file1.txt"))).ShouldBe("transformed");
        (await mockFs.File.ReadAllTextAsync(FilePath("file2.txt"))).ShouldBe("transformed");
    }

    [Test]
    public void CancelRestore_PreventsRestorationOnDispose()
    {
        // Arrange
        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { FilePath("file1.txt"), new("original-1") },
            { FilePath("file2.txt"), new("original-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        var scope = TransformMultiFileScope.Create(files, _ => "transformed");

        // Act
        scope.CancelRestore();
        scope.Dispose();

        // Assert
        mockFs
            .File
            .ReadAllText(FilePath("file1.txt"))
            .ShouldBe("transformed");

        mockFs
            .File
            .ReadAllText(FilePath("file2.txt"))
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
            { FilePath("file1.txt"), new("original-1") },
            { FilePath("file2.txt"), new("original-2") },
        });

        var files = new[] { CreateRootedPath(mockFs, "file1.txt"), CreateRootedPath(mockFs, "file2.txt") };

        // Act / Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            TransformMultiFileScope.CreateAsync(files, content => content, cts.Token));
    }
}
