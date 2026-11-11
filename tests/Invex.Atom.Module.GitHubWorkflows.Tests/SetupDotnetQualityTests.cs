namespace Invex.Atom.Module.GitHubWorkflows.Tests;

[TestFixture]
public class SetupDotnetQualityTests
{
    [TestCase(SetupDotnetStep.DotnetQuality.Daily)]
    [TestCase(SetupDotnetStep.DotnetQuality.Preview)]
    [TestCase(SetupDotnetStep.DotnetQuality.Ga)]
    public void BuildSetupDotnetStep_WithSupportedQuality_EmitsDotnetQuality(SetupDotnetStep.DotnetQuality quality)
    {
        // Act
        var step = GitHubWorkflowFileWriter.BuildSetupDotnetStep(new("11.0.x", quality));

        // Assert
        step.With.ShouldNotBeNull().ShouldContainKey("dotnet-quality");
    }

    [TestCase(SetupDotnetStep.DotnetQuality.Signed)]
    [TestCase(SetupDotnetStep.DotnetQuality.Validated)]
    public void BuildSetupDotnetStep_WithQualitySetupDotnetRejects_Throws(SetupDotnetStep.DotnetQuality quality)
    {
        // Act / Assert
        var exception = Should.Throw<NotSupportedException>(() =>
            GitHubWorkflowFileWriter.BuildSetupDotnetStep(new("11.0.x", quality)));

        exception.Message.ShouldContain(quality.ToString());
    }
}
