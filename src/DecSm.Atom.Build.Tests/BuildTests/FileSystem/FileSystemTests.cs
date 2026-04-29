namespace DecSm.Atom.Build.Tests.BuildTests.FileSystem;

[TestFixture]
public sealed class FileSystemTests
{
    [Test]
    public void Default_BuildDefinition_WithDefaultLocation_Locates_AtomRootDirectory()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>();

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomRootDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\Atom\"
                : "/Atom/");
    }

    [Test]
    public void Default_BuildDefinition_WithCustomLocator_Locates_AtomRootDirectory()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>(configure: builder =>
            builder.Services.AddSingleton<IPathProvider>(provider => new FunctionPathProvider
            {
                Provider = key => key is AtomPaths.Root
                    ? provider
                        .GetRequiredService<IAtomFileSystem>()
                        .CreateRootedPath(Environment.OSVersion.Platform is PlatformID.Win32NT
                            ? @"C:\CustomAtomRoot"
                            : "/CustomAtomRoot")
                    : null,
                Priority = 1,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomRootDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\CustomAtomRoot"
                : "/CustomAtomRoot");
    }

    [Test]
    public void Default_BuildDefinition_WithDefaultLocation_Locates_AtomArtifactsDirectory()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>();

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomArtifactsDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\Atom\atom-publish"
                : "/Atom/atom-publish");
    }

    [Test]
    public void Default_BuildDefinition_WithCustomLocator_Locates_AtomArtifactsDirectory()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>(configure: builder =>
            builder.Services.AddSingleton<IPathProvider>(provider => new FunctionPathProvider
            {
                Provider = key => key is AtomPaths.Artifacts
                    ? provider
                        .GetRequiredService<IAtomFileSystem>()
                        .CreateRootedPath(Environment.OSVersion.Platform is PlatformID.Win32NT
                            ? @"C:\CustomAtomArtifacts"
                            : "/CustomAtomArtifacts")
                    : null,
                Priority = 1,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomArtifactsDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\CustomAtomArtifacts"
                : "/CustomAtomArtifacts");
    }

    [Test]
    public void Default_BuildDefinition_WithDefaultLocation_Locates_AtomPublishDirectory()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>();

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomPublishDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\Atom\atom-publish"
                : "/Atom/atom-publish");
    }

    [Test]
    public void Default_BuildDefinition_WithCustomLocator_Locates_AtomPublishDirectory()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>(configure: builder =>
            builder.Services.AddSingleton<IPathProvider>(provider => new FunctionPathProvider
            {
                Provider = key => key is AtomPaths.Publish
                    ? provider
                        .GetRequiredService<IAtomFileSystem>()
                        .CreateRootedPath(Environment.OSVersion.Platform is PlatformID.Win32NT
                            ? @"C:\CustomAtomPublish"
                            : "/CustomAtomPublish")
                    : null,
                Priority = 1,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomPublishDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\CustomAtomPublish"
                : "/CustomAtomPublish");
    }

    [Test]
    public void Default_BuildDefinition_WithDefaultLocation_Locates_AtomTempDirectory()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>();

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomTempDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\temp\"
                : "/temp/");
    }

    [Test]
    public void Default_BuildDefinition_WithCustomLocator_Locates_AtomTempDirectory()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>(configure: builder =>
            builder.Services.AddSingleton<IPathProvider>(provider => new FunctionPathProvider
            {
                Provider = key => key is AtomPaths.Temp
                    ? provider
                        .GetRequiredService<IAtomFileSystem>()
                        .CreateRootedPath(Environment.OSVersion.Platform is PlatformID.Win32NT
                            ? @"C:\CustomAtomTemp"
                            : "/CustomAtomTemp")
                    : null,
                Priority = 1,
            }));

        // Act
        var atomFileSystem = host.Services.GetRequiredService<IAtomFileSystem>();
        var atomRootDirectory = atomFileSystem.AtomTempDirectory;

        // Assert
        atomRootDirectory
            .ToString()
            .ShouldBe(Environment.OSVersion.Platform is PlatformID.Win32NT
                ? @"C:\CustomAtomTemp"
                : "/CustomAtomTemp");
    }
}
