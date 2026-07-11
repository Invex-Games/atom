namespace Atom.Tests;

[TestFixture]
internal sealed class WorkflowSecurityTests
{
    private static string RepositoryRoot =>
        Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".."));

    [Test]
    public void BuildWorkflow_DoesNotReceiveProductionCredentialsOrWritePermissions()
    {
        var workflow = ReadWorkflow("Build.yml");

        workflow.ShouldNotContain("NUGET_PUSH_API_KEY");
        workflow.ShouldNotContain("contents: write");
        workflow.ShouldNotContain("id-token:");
        workflow.ShouldNotContain("DeployRelease");
        workflow.ShouldNotContain("release:");
    }

    [Test]
    public void ValidateWorkflow_RunsPrCodeReadOnlyAndGuardsReviewJobs()
    {
        var workflow = ReadWorkflow("Validate.yml");

        workflow.ShouldNotContain("id-token:");
        workflow.ShouldNotContain("checks: write");
        workflow.ShouldContain("contents: read");
        workflow.ShouldContain("pull-requests: read");

        Count(workflow, "if: github.event_name == 'pull_request'")
            .ShouldBe(2);
    }

    [Test]
    public void CreateReleaseWorkflow_ProtectsTheOnlyProductionCredentialJob()
    {
        var workflow = ReadWorkflow("CreateRelease.yml");

        workflow.ShouldContain("DeployRelease:");
        workflow.ShouldContain("needs: [ PackProjects, PackTool, TestProjects, SetupBuildInfo ]");
        workflow.ShouldContain("environment: production");
        workflow.ShouldContain("if: github.event_name == 'workflow_dispatch' && github.ref == 'refs/heads/main'");

        Count(workflow, "nuget-push-api-key: ${{ secrets.NUGET_PUSH_API_KEY }}")
            .ShouldBe(1);

        Count(workflow, "environment: production")
            .ShouldBe(2);
    }

    private static string ReadWorkflow(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot, ".github", "workflows", name));

    private static int Count(string value, string fragment) =>
        value.Split(fragment)
            .Length -
        1;
}
