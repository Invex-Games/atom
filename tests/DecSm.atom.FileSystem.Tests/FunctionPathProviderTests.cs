namespace DecSm.Atom.FileSystem.Tests;

[TestFixture]
internal sealed class FunctionPathProviderTests
{
    private static readonly string Root = OperatingSystem.IsWindows()
        ? @"C:\"
        : "/";

    [Test]
    public void GetPath_WhenDelegateReturnsPath_ReturnsThatPath()
    {
        // Arrange
        var mockFs = new MockFileSystem();
        var expectedPath = new RootedPath(mockFs, $"{Root}result");

        var provider = new FunctionPathProvider
        {
            Priority = 5,
            Provider = _ => expectedPath,
        };

        // Act
        var result = provider.GetPath("AnyKey");

        // Assert
        result.ShouldBe(expectedPath);
    }

    [Test]
    public void GetPath_WhenDelegateReturnsNull_ReturnsNull()
    {
        // Arrange
        var provider = new FunctionPathProvider
        {
            Priority = 0,
            Provider = _ => null,
        };

        // Act
        var result = provider.GetPath("AnyKey");

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetPath_ForwardsKeyToDelegate()
    {
        // Arrange
        string? capturedKey = null;

        var provider = new FunctionPathProvider
        {
            Priority = 0,
            Provider = key =>
            {
                capturedKey = key;

                return null;
            },
        };

        // Act
        provider.GetPath("ExpectedKey");

        // Assert
        capturedKey.ShouldBe("ExpectedKey");
    }

    [Test]
    public void Priority_ReturnsInitializedValue()
    {
        // Arrange
        var provider = new FunctionPathProvider
        {
            Priority = 42,
            Provider = _ => null,
        };

        // Act / Assert
        provider.Priority.ShouldBe(42);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(-1)]
    public void Priority_ReturnsCorrectValueForVariousPriorities(int priority)
    {
        // Arrange
        var provider = new FunctionPathProvider
        {
            Priority = priority,
            Provider = _ => null,
        };

        // Act / Assert
        provider.Priority.ShouldBe(priority);
    }
}
