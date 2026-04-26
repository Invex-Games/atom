namespace DecSm.Atom.FileSystem.Tests;

[TestFixture]
internal sealed class FileSystemHostExtensionsTests
{
    private static readonly string Root = OperatingSystem.IsWindows()
        ? @"C:\"
        : "/";

    [Test]
    public void AddAtomFileSystem_RegistersIAtomFileSystem()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAtomFileSystem();

        // Act
        using var sp = services.BuildServiceProvider();
        var atomFs = sp.GetService<IAtomFileSystem>();

        // Assert
        atomFs.ShouldNotBeNull();
    }

    [Test]
    public void AddAtomFileSystem_IFileSystem_ResolvesToSameInstanceAsIAtomFileSystem()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAtomFileSystem();

        // Act
        using var sp = services.BuildServiceProvider();
        var atomFs = sp.GetRequiredService<IAtomFileSystem>();
        var fileSystem = sp.GetRequiredService<IFileSystem>();

        // Assert
        fileSystem.ShouldBeSameAs(atomFs);
    }

    [Test]
    public void ProvidePath_WithAtomFileSystemDelegate_IsResolvedViaGetPath()
    {
        // Arrange
        const string key = "MyCustomKey";
        var expectedPath = $"{Root}custom-path";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAtomFileSystem();

        services.ProvidePath((queryKey, atomFs) => queryKey == key
            ? new RootedPath(atomFs, expectedPath)
            : null);

        // Act
        using var sp = services.BuildServiceProvider();
        var atomFs = sp.GetRequiredService<IAtomFileSystem>();
        var result = atomFs.GetPath(key);

        // Assert
        result.Path.ShouldBe(expectedPath);
    }

    [Test]
    public void ProvidePath_WithRawDelegate_IsResolvedViaGetPath()
    {
        // Arrange
        const string key = "RawKey";
        var expectedPath = $"{Root}raw-path";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAtomFileSystem();

        // The raw delegate overload does not receive the IAtomFileSystem;
        // the RootedPath's FileSystem will be whatever was passed to the RootedPath constructor.
        // We use the registered IFileSystem via a closure after building the provider.
        services.ProvidePath(queryKey => queryKey == key
            ? new RootedPath(new MockFileSystem(), expectedPath)
            : null);

        // Act
        using var sp = services.BuildServiceProvider();
        var atomFs = sp.GetRequiredService<IAtomFileSystem>();
        var result = atomFs.GetPath(key);

        // Assert
        result.Path.ShouldBe(expectedPath);
    }

    [Test]
    public void ProvidePath_WhenMultipleProvidersRegistered_HigherPriorityWins()
    {
        // Arrange
        const string key = "SharedKey";
        var highPriorityPath = $"{Root}high-priority";
        var lowPriorityPath = $"{Root}low-priority";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAtomFileSystem();

        services.ProvidePath((queryKey, atomFs) => queryKey == key
                ? new RootedPath(atomFs, lowPriorityPath)
                : null,
            1);

        services.ProvidePath((queryKey, atomFs) => queryKey == key
                ? new RootedPath(atomFs, highPriorityPath)
                : null,
            10);

        // Act
        using var sp = services.BuildServiceProvider();
        var atomFs = sp.GetRequiredService<IAtomFileSystem>();
        var result = atomFs.GetPath(key);

        // Assert
        result.Path.ShouldBe(highPriorityPath);
    }

    [Test]
    public void AddAtomFileSystem_CurrentDirectory_ReturnsNonNullPath()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAtomFileSystem();

        // Act
        using var sp = services.BuildServiceProvider();
        var atomFs = sp.GetRequiredService<IAtomFileSystem>();

        // Assert
        atomFs.CurrentDirectory.Path.ShouldNotBeNullOrEmpty();
    }
}
