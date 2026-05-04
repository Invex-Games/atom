using JetBrains.Annotations;

namespace DecSm.Atom.FileSystem.Tests;

[TestFixture]
internal sealed class AtomFileSystemTests
{
    private static readonly string Root = OperatingSystem.IsWindows()
        ? @"C:\"
        : "/";

    [SuppressMessage("Performance",
        "CA1859:Use concrete types when possible for improved performance",
        Justification =
            "Returns AtomFileSystem as IAtomFileSystem to access default interface members (e.g. GetPath<T>)")]
    private static IAtomFileSystem CreateAtomFileSystem(
        IReadOnlyList<IPathProvider>? providers = null,
        IFileSystem? fileSystem = null) =>
        new AtomFileSystem(A.Fake<ILogger<AtomFileSystem>>())
        {
            PathProviders = providers ?? [],
            FileSystem = fileSystem ?? new MockFileSystem(),
        };

    [Test]
    public void GetPath_WhenProviderResolvesKey_ReturnsThatPath()
    {
        // Arrange
        var expectedPath = $"{Root}test";
        var mockFs = new MockFileSystem();

        var provider = new FunctionPathProvider
        {
            Priority = 0,
            Provider = key => key == "TestKey"
                ? new RootedPath(mockFs, expectedPath)
                : null,
        };

        var atomFs = CreateAtomFileSystem([provider], mockFs);

        // Act
        var result = atomFs.GetPath("TestKey");

        // Assert
        result.Path.ShouldBe(expectedPath);
    }

    [Test]
    public void GetPath_WhenMultipleProviders_UsesFirstNonNullByListOrder()
    {
        // Arrange
        // AtomFileSystem picks first non-null from the list in order;
        // callers are responsible for ordering by descending priority (as done by DI registration).
        var mockFs = new MockFileSystem();

        var firstProvider = new FunctionPathProvider
        {
            Priority = 10,
            Provider = _ => new(mockFs, $"{Root}first"),
        };

        var secondProvider = new FunctionPathProvider
        {
            Priority = 0,
            Provider = _ => new(mockFs, $"{Root}second"),
        };

        var atomFs = CreateAtomFileSystem([firstProvider, secondProvider], mockFs);

        // Act
        var result = atomFs.GetPath("AnyKey");

        // Assert
        result.Path.ShouldBe($"{Root}first");
    }

    [Test]
    public void GetPath_WhenFirstProviderReturnsNull_FallsBackToNextProvider()
    {
        // Arrange
        var mockFs = new MockFileSystem();

        var nullProvider = new FunctionPathProvider
        {
            Priority = 10,
            Provider = _ => null,
        };

        var fallbackProvider = new FunctionPathProvider
        {
            Priority = 0,
            Provider = _ => new(mockFs, $"{Root}fallback"),
        };

        var atomFs = CreateAtomFileSystem([nullProvider, fallbackProvider], mockFs);

        // Act
        var result = atomFs.GetPath("AnyKey");

        // Assert
        result.Path.ShouldBe($"{Root}fallback");
    }

    [Test]
    public void GetPath_CachesResultOnSubsequentCalls()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var callCount = 0;

        var provider = new FunctionPathProvider
        {
            Priority = 0,
            Provider = _ =>
            {
                callCount++;

                return new(mockFs, $"{Root}test");
            },
        };

        var atomFs = CreateAtomFileSystem([provider], mockFs);

        // Act
        var first = atomFs.GetPath("CachedKey");
        var second = atomFs.GetPath("CachedKey");

        // Assert
        callCount.ShouldBe(1);
        second.ShouldBeSameAs(first);
    }

    [Test]
    public void GetPath_WhenNoProviderResolvesKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var provider = new FunctionPathProvider
        {
            Priority = 0,
            Provider = _ => null,
        };

        var atomFs = CreateAtomFileSystem([provider]);

        // Act / Assert
        Should
            .Throw<InvalidOperationException>(() => atomFs.GetPath("MissingKey"))
            .Message
            .ShouldContain("MissingKey");
    }

    [Test]
    public void GetPath_WhenNoProvidersRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var atomFs = CreateAtomFileSystem([]);

        // Act / Assert
        Should.Throw<InvalidOperationException>(() => atomFs.GetPath("AnyKey"));
    }

    [Test]
    public void GetPath_WhenCircularDependency_ThrowsInvalidOperationException()
    {
        // Arrange
        AtomFileSystem? atomFs = null;

        var provider = new FunctionPathProvider
        {
            Priority = 0,
            Provider = key =>
            {
                // This call re-enters GetPath for the same key, creating infinite recursion
                // ReSharper disable once AccessToModifiedClosure
                atomFs!.GetPath(key);

                return null;
            },
        };

        atomFs = new(A.Fake<ILogger<AtomFileSystem>>())
        {
            PathProviders = [provider],
            FileSystem = new MockFileSystem(),
        };

        // Act / Assert
        Should
            .Throw<InvalidOperationException>(() => atomFs.GetPath("CircularKey"))
            .Message
            .ShouldContain("circular");
    }

    [Test]
    public void GetPathT_CallsPathMarkerStaticAbstractMethod()
    {
        // Arrange
        var atomFs = CreateAtomFileSystem();

        // Act
        // GetPath<T>() is a default interface member that calls T.Path(this)
        // It does NOT go through GetPath(string key) and does not require a registered provider
        var result = atomFs.GetPath<TestPathMarker>();

        // Assert
        result.Path.ShouldBe(TestPathMarker.ExpectedPath);
    }

    [Test]
    public void CreateRootedPath_ReturnsRootedPathWithCorrectPathAndFileSystem()
    {
        // Arrange
        var atomFs = CreateAtomFileSystem();
        var expectedPath = $"{Root}some{Path.DirectorySeparatorChar}path";

        // Act
        var result = atomFs.CreateRootedPath(expectedPath);

        // Assert
        result.ShouldSatisfyAllConditions(() => result.Path.ShouldBe(expectedPath),
            () => result.FileSystem.ShouldBeSameAs(atomFs));
    }

    [UsedImplicitly(Reason = "Test code")]
    private sealed class TestPathMarker : IPathMarker
    {
        public static readonly string ExpectedPath = OperatingSystem.IsWindows()
            ? @"C:\marker-path"
            : "/marker-path";

        public static RootedPath Path(IFileSystem fileSystem) =>
            new(fileSystem, ExpectedPath);
    }
}
