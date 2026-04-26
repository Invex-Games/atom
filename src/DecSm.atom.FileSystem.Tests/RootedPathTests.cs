namespace DecSm.Atom.FileSystem.Tests;

[TestFixture]
internal sealed class RootedPathTests
{
    private static readonly string Root = OperatingSystem.IsWindows()
        ? @"C:\"
        : "/";

    private static readonly char Ps = Path.DirectorySeparatorChar;

    [Test]
    public void Parent_WhenPathIsRoot_ReturnsNull()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, Root);

        // Act / Assert
        path.Parent.ShouldBeNull();
    }

    [Test]
    public void Parent_WhenPathIsDirectChildOfRoot_ReturnsRoot()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}foo");

        // Act
        var parent = path.Parent;

        // Assert
        parent.ShouldNotBeNull();
        parent.Path.ShouldBe(Root);
    }

    [Test]
    public void Parent_WhenPathHasTrailingSlash_StripsItAndReturnsParent()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}foo{Ps}");

        // Act
        var parent = path.Parent;

        // Assert
        parent.ShouldNotBeNull();
        parent.Path.ShouldBe(Root);
    }

    [Test]
    public void Parent_WhenPathIsNestedDirectory_ReturnsImmediateParent()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}foo{Ps}bar");

        // Act
        var parent = path.Parent;

        // Assert
        parent.ShouldNotBeNull();
        parent.Path.ShouldBe($"{Root}foo{Ps}");
    }

    [Test]
    public void Parent_ReturnedPath_SharesSameFileSystem()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}foo{Ps}bar");

        // Act
        var parent = path.Parent;

        // Assert
        parent!.FileSystem.ShouldBeSameAs(mockFs);
    }

    [Test]
    public void FileExists_WhenFileExistsInFileSystem_ReturnsTrue()
    {
        // Arrange
        var filePath = $"{Root}file.txt";

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new("content") },
        });

        var path = new RootedPath(mockFs, filePath);

        // Act / Assert
        path.FileExists.ShouldBeTrue();
    }

    [Test]
    public void FileExists_WhenFileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}missing.txt");

        // Act / Assert
        path.FileExists.ShouldBeFalse();
    }

    [Test]
    public void FileExists_WhenPathIsDirectory_ReturnsFalse()
    {
        // Arrange
        var dirPath = $"{Root}mydir";

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { dirPath, new MockDirectoryData() },
        });

        var path = new RootedPath(mockFs, dirPath);

        // Act / Assert
        path.FileExists.ShouldBeFalse();
    }

    [Test]
    public void DirectoryExists_WhenDirectoryExistsInFileSystem_ReturnsTrue()
    {
        // Arrange
        var dirPath = $"{Root}mydir";

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { dirPath, new MockDirectoryData() },
        });

        var path = new RootedPath(mockFs, dirPath);

        // Act / Assert
        path.DirectoryExists.ShouldBeTrue();
    }

    [Test]
    public void DirectoryExists_WhenDirectoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}missing");

        // Act / Assert
        path.DirectoryExists.ShouldBeFalse();
    }

    [Test]
    public void DirectoryExists_WhenPathIsFile_ReturnsFalse()
    {
        // Arrange
        var filePath = $"{Root}file.txt";

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new("content") },
        });

        var path = new RootedPath(mockFs, filePath);

        // Act / Assert
        path.DirectoryExists.ShouldBeFalse();
    }

    [Test]
    public void PathExists_WhenFileExists_ReturnsTrue()
    {
        // Arrange
        var filePath = $"{Root}file.txt";

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new("content") },
        });

        var path = new RootedPath(mockFs, filePath);

        // Act / Assert
        path.PathExists.ShouldBeTrue();
    }

    [Test]
    public void PathExists_WhenDirectoryExists_ReturnsTrue()
    {
        // Arrange
        var dirPath = $"{Root}mydir";

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { dirPath, new MockDirectoryData() },
        });

        var path = new RootedPath(mockFs, dirPath);

        // Act / Assert
        path.PathExists.ShouldBeTrue();
    }

    [Test]
    public void PathExists_WhenNeitherFileNorDirectoryExists_ReturnsFalse()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}nothing");

        // Act / Assert
        path.PathExists.ShouldBeFalse();
    }

    [Test]
    public void FileName_WhenFileExists_ReturnsFileName()
    {
        // Arrange
        var filePath = $"{Root}folder{Ps}file.txt";

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { filePath, new("content") },
        });

        var path = new RootedPath(mockFs, filePath);

        // Act / Assert
        path.FileName.ShouldBe("file.txt");
    }

    [Test]
    public void FileName_WhenFileDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}folder{Ps}missing.txt");

        // Act / Assert
        path.FileName.ShouldBeNull();
    }

    [Test]
    public void FileNameWithoutExtension_ReturnsNameWithoutExtension()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}folder{Ps}file.txt");

        // Act / Assert
        path.FileNameWithoutExtension.ShouldBe("file");
    }

    [Test]
    public void FileNameWithoutExtension_WhenNoExtension_ReturnsFullName()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}folder{Ps}noextension");

        // Act / Assert
        path.FileNameWithoutExtension.ShouldBe("noextension");
    }

    [Test]
    public void DirectoryName_WhenDirectoryExists_ReturnsNonNullValue()
    {
        // Arrange
        var dirPath = $"{Root}parent{Ps}child";

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { dirPath, new MockDirectoryData() },
        });

        var path = new RootedPath(mockFs, dirPath);

        // Act / Assert
        path.DirectoryName.ShouldNotBeNull();
    }

    [Test]
    public void DirectoryName_WhenDirectoryDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var path = new RootedPath(mockFs, $"{Root}missing");

        // Act / Assert
        path.DirectoryName.ShouldBeNull();
    }

    [Test]
    public void SlashOperator_CombinesPaths()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var basePath = new RootedPath(mockFs, $"{Root}base");

        // Act
        var combined = basePath / "sub";

        // Assert
        combined.Path.ShouldBe(mockFs.Path.Combine($"{Root}base", "sub"));
    }

    [Test]
    public void SlashOperator_PreservesFileSystemReference()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var basePath = new RootedPath(mockFs, $"{Root}base");

        // Act
        var combined = basePath / "sub";

        // Assert
        combined.FileSystem.ShouldBeSameAs(mockFs);
    }

    [Test]
    public void SlashOperator_CanChain()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var basePath = new RootedPath(mockFs, $"{Root}a");

        // Act
        var combined = basePath / "b" / "c";

        // Assert
        combined.Path.ShouldBe(mockFs.Path.Combine(mockFs.Path.Combine($"{Root}a", "b"), "c"));
    }

    [Test]
    public void ImplicitStringConversion_ReturnsPathString()
    {
        // Arrange
        var expected = $"{Root}foo{Ps}bar.txt";
        var mockFs = new MockFileSystem();
        var rootedPath = new RootedPath(mockFs, expected);

        // Act
        string actual = rootedPath;

        // Assert
        actual.ShouldBe(expected);
    }

    [Test]
    public void ToString_ReturnsPathString()
    {
        // Arrange
        var expected = $"{Root}foo{Ps}bar.txt";
        var mockFs = new MockFileSystem();
        var rootedPath = new RootedPath(mockFs, expected);

        // Act / Assert
        rootedPath
            .ToString()
            .ShouldBe(expected);
    }
}
