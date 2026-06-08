namespace Invex.Atom.Build.Tests.BuildTests.Console;

[TestFixture]
internal sealed class ConsoleTests
{
    [Test]
    public async Task MinimalBuildDefinition_Displays_DefaultConsoleMessage()
    {
        // Arrange
        var testConsole = new TestConsole();
        var host = CreateTestHost<DefaultAtomBuild>(testConsole);

        // Act
        await host.RunAsync();

        // Assert
        await Verify(testConsole.Output);
    }

    [Test]
    public async Task DefaultBuildDefinition_Displays_DefaultConsoleMessage()
    {
        // Arrange
        var testConsole = new TestConsole();
        var host = CreateTestHost<DefaultAtomBuild>(testConsole);

        // Act
        await host.RunAsync();

        // Assert
        await Verify(testConsole.Output);
    }

    [Test]
    public async Task ConsoleBuildDefinition_Displays_DefaultConsoleMessage()
    {
        // Arrange
        var testConsole = new TestConsole();
        var host = CreateTestHost<ConsoleBuild>(testConsole);

        // Act
        await host.RunAsync();

        // Assert
        await Verify(testConsole.Output);
    }
}
