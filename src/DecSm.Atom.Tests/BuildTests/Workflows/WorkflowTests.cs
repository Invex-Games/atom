namespace DecSm.Atom.Tests.BuildTests.Workflows;

[TestFixture]
public class WorkflowTests
{
    [Test]
    public async Task WorkflowSingleTargetBuild_GeneratesWorkflow()
    {
        // Arrange
        var workflowWriter = new TestWorkflowWriter();

        var host = CreateTestHost<WorkflowSingleTargetBuild>(configure: builder =>
            builder.Services.AddSingleton<IWorkflowWriter>(workflowWriter));

        // Act
        await host.RunAsync();

        // Assert
        await Verify(workflowWriter.GeneratedWorkflows);
    }

    [Test]
    public async Task WorkflowDependentTargetBuild_GeneratesWorkflow()
    {
        // Arrange
        var workflowWriter = new TestWorkflowWriter();

        var host = CreateTestHost<WorkflowDependentTargetBuild>(configure: builder =>
            builder.Services.AddSingleton<IWorkflowWriter>(workflowWriter));

        // Act
        await host.RunAsync();

        // Assert
        await Verify(workflowWriter.GeneratedWorkflows);
    }

    [Test]
    public async Task Workflows_WhenDirtyAndHeadless_RegeneratesWorkflows()
    {
        // Arrange
        var console = new TestConsole();

        var workflowWriter = new TestWorkflowWriter
        {
            IsDirty = true,
        };

        var host = CreateTestHost<WorkflowDependentTargetBuild>(console,
            commandLineArgs: new(true, [new HeadlessArg()]),
            configure: builder => builder.Services.AddSingleton<IWorkflowWriter>(workflowWriter));

        // Act
        await host.RunAsync();

        // Assert
        await Verify(ConsoleOutputUtils.SanitizeLogDateTime(console.Output));
    }
}
