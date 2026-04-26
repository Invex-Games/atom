namespace DecSm.Atom.Process.Tests;

[TestFixture]
internal sealed class ProcessRunResultTests
{
    [Test]
    public void Constructor_StoresAllProperties()
    {
        // Arrange
        var options = new ProcessRunOptions("dotnet", "--version");

        // Act
        var result = new ProcessRunResult(options, 0, "1.0.0", string.Empty);

        // Assert
        result.ShouldSatisfyAllConditions(() => result.RunOptions.ShouldBeSameAs(options),
            () => result.ExitCode.ShouldBe(0),
            () => result.Output.ShouldBe("1.0.0"),
            () => result.Error.ShouldBe(string.Empty));
    }

    [Test]
    public void ExitCode_WithNonZeroValue_IsStoredCorrectly()
    {
        // Arrange
        var options = new ProcessRunOptions("dotnet", "--version");

        // Act
        var result = new ProcessRunResult(options, 1, string.Empty, "error message");

        // Assert
        result.ExitCode.ShouldBe(1);
    }

    [Test]
    public void Output_WithMultilineContent_IsStoredAsIs()
    {
        // Arrange
        var options = new ProcessRunOptions("dotnet", "--version");
        const string multiline = "line1\nline2\nline3";

        // Act
        var result = new ProcessRunResult(options, 0, multiline, string.Empty);

        // Assert
        result.Output.ShouldBe(multiline);
    }

    [Test]
    public void Error_WithContent_IsStoredAsIs()
    {
        // Arrange
        var options = new ProcessRunOptions("dotnet", "--version");
        const string errorText = "Build failed: some error";

        // Act
        var result = new ProcessRunResult(options, 1, string.Empty, errorText);

        // Assert
        result.Error.ShouldBe(errorText);
    }
}
