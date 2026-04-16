namespace DecSm.Atom.Tests.ClassTests.Build;

[TestFixture]
public class DefaultBuildVersionProviderTests
{
    private static readonly string OsAgnosticRoot = OperatingSystem.IsWindows()
        ? @"C:\Solution"
        : "/Solution";

    private static readonly char Ps = Path.DirectorySeparatorChar;

    private static AtomFileSystem NewFileSystem(IFileSystem fileSystem)
    {
        var result = new AtomFileSystem(A.Fake<ILogger<AtomFileSystem>>())
        {
            FileSystem = fileSystem,
            PathLocators = [],
            ProjectName = "Atom",
        };

        return result;
    }

    [Test]
    public void Version_Returns_VersionInfo()
    {
        const string directoryBuildProps = """
                                           <Project>
                                               <PropertyGroup>
                                               <Version>1.2.3</Version>
                                               </PropertyGroup>
                                           </Project>
                                           """;

        // Arrange
        var fileSystem = NewFileSystem(new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{OsAgnosticRoot}{Ps}Solution.sln", new("<!-- -->") },
                { $"{OsAgnosticRoot}{Ps}Project", new MockDirectoryData() },
                { $"{OsAgnosticRoot}{Ps}Directory.Build.props", new(directoryBuildProps) },
            },
            OsAgnosticRoot));

        var provider = new DefaultBuildVersionProvider(fileSystem);

        // Act
        var version = provider.Version;

        // Assert
        version
            .ShouldNotBeNull()
            .ShouldSatisfyAllConditions(x => x
                .ToString()
                .ShouldBe("1.2.3"));
    }

    [Test]
    [NonParallelizable]
    [SuppressMessage("ReSharper", "MoveLocalFunctionAfterJumpStatement")]
    public void Version_WhenDirectoryBuildPropsDoesNotExist_ReturnsDefaultVersion()
    {
        // Arrange

        var fileSystem = NewFileSystem(new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { $"{OsAgnosticRoot}{Ps}Solution.sln", new("<!-- -->") },
                { $"{OsAgnosticRoot}{Ps}Project", new MockDirectoryData() },
            },
            OsAgnosticRoot));

        var provider = new DefaultBuildVersionProvider(fileSystem);

        // Act
        var version = provider.Version;

        // Assert
        version.ShouldBe(SemVer.Parse("1.0.0"));
    }
}
