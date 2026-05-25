namespace DecSm.Atom.Workflows.Tests;

[TestFixture]
internal sealed class WorkflowBuildTests
{
    [Test]
    public async Task EmptyWorkflowBuild_NoWorkflows_DoesNotCreateWorkflow()
    {
        // Arrange
        var writer = new TestWorkflowWriter();
        using var host = CreateWorkflowTestHost<EmptyWorkflowBuild>(workflowWriter: writer);

        // Act
        await host.RunAsync();

        // Assert
        writer.GeneratedWorkflows.ShouldBeEmpty();
        await Verify(writer);
    }

    [Test]
    public async Task DependentTargetsBuild_WorkflowWithDependentTargets_CreatesWorkflow()
    {
        // Arrange
        var writer = new TestWorkflowWriter();
        using var host = CreateWorkflowTestHost<DependentTargetsBuild>(workflowWriter: writer);

        // Act
        await host.RunAsync();

        // Assert
        writer
            .GeneratedWorkflows
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(workflow =>
            {
                workflow.Name.ShouldBe("dependent-workflow");

                workflow
                    .Triggers
                    .ShouldHaveSingleItem()
                    .ShouldBeOfType<TestWorkflowTrigger>();

                workflow.Jobs.Count.ShouldBe(2);

                workflow
                    .Jobs[0]
                    .ShouldSatisfyAllConditions(job => job.Name.ShouldBe("FirstTarget"));

                workflow
                    .Jobs[1]
                    .ShouldSatisfyAllConditions(job =>
                    {
                        job.Name.ShouldBe("SecondTarget");

                        job
                            .JobDependencies
                            .ShouldHaveSingleItem()
                            .ShouldBe("FirstTarget");
                    });
            });

        await Verify(writer);
    }

    [Test]
    public async Task EmptyTargetsWorkflowBuild_WorkflowWithEmptyTargets_CreatesWorkflow()
    {
        // Arrange
        var writer = new TestWorkflowWriter();
        using var host = CreateWorkflowTestHost<EmptyTargetsWorkflowBuild>(workflowWriter: writer);

        // Act
        await host.RunAsync();

        // Assert
        writer
            .GeneratedWorkflows
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(workflow =>
            {
                workflow.Name.ShouldBe("empty-targets-workflow");

                workflow
                    .Triggers
                    .ShouldHaveSingleItem()
                    .ShouldBeOfType<TestWorkflowTrigger>();

                workflow.Jobs.ShouldBeEmpty();
            });

        await Verify(writer);
    }

    [Test]
    public async Task WriterError_SetsExitCode()
    {
        // Arrange
        var writer = new TestWorkflowWriter
        {
            ThrowOnWrite = true,
        };

        using var host = CreateWorkflowTestHost<SingleTargetBuild>(workflowWriter: writer);

        // Act
        await host.RunAsync();

        // Assert
        Environment.ExitCode.ShouldBe(1);

        Environment.ExitCode = 0; // Reset exit code for other tests
    }

    [Test]
    public async Task MatrixWorkflowBuild_WorkflowWithMatrix_CreatesWorkflow()
    {
        // Arrange
        var writer = new TestWorkflowWriter();
        using var host = CreateWorkflowTestHost<MatrixWorkflowBuild>(workflowWriter: writer);

        // Act
        await host.RunAsync();

        // Assert
        writer
            .GeneratedWorkflows
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(workflow =>
            {
                workflow.Name.ShouldBe("matrix-workflow");

                workflow
                    .Triggers
                    .ShouldHaveSingleItem()
                    .ShouldBeOfType<TestWorkflowTrigger>();

                workflow.Jobs.Count.ShouldBe(1);

                workflow
                    .Jobs[0]
                    .ShouldSatisfyAllConditions(job =>
                    {
                        job.Name.ShouldBe("SingleTarget");

                        job.TargetStep.ShouldSatisfyAllConditions(step =>
                        {
                            step.Name.ShouldBe("SingleTarget");

                            step
                                .MatrixDimensions
                                .ShouldHaveSingleItem()
                                .ShouldSatisfyAllConditions(dimension =>
                                {
                                    dimension.Name.ShouldBe("os");
                                    dimension.Values.Count.ShouldBe(2);

                                    dimension
                                        .Values[0]
                                        .ShouldBe("ubuntu-latest");

                                    dimension
                                        .Values[1]
                                        .ShouldBe("windows-latest");
                                });
                        });
                    });
            });

        await Verify(writer);
    }

    [Test]
    public async Task Workflow_Generates_ForEach_Type()
    {
        // Arrange
        var writer = new TestWorkflowWriter();
        using var host = CreateWorkflowTestHost<MultiTypeBuild>(workflowWriter: writer);

        // Act
        await host.RunAsync();

        // Assert
        writer.GeneratedWorkflows.Count.ShouldBe(2);

        await Verify(writer);
    }

    [Test]
    public async Task Workflow_WithOptions_Generates_WithOptions()
    {
        // Arrange
        var writer = new TestWorkflowWriter();
        using var host = CreateWorkflowTestHost<WorkflowWithOptionsBuild>(workflowWriter: writer);

        // Act
        await host.RunAsync();

        // Assert
        writer
            .GeneratedWorkflows
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(workflow =>
            {
                workflow.Name.ShouldBe("options-workflow");

                workflow
                    .Triggers
                    .ShouldHaveSingleItem()
                    .ShouldBeOfType<TestWorkflowTrigger>();

                workflow.Options.Count.ShouldBe(1);

                workflow
                    .Options[0]
                    .ShouldBeOfType<TestWorkflowOption>()
                    .ShouldSatisfyAllConditions(option =>
                    {
                        option.ShouldBeOfType<TestWorkflowOption>();
                        option.Value.ShouldBe("workflow-level");
                    });
            });

        await Verify(writer);
    }

    [Test]
    public async Task InterfaceBuild_CreatesWorkflow()
    {
        // Arrange
        var writer = new TestWorkflowWriter();
        using var host = CreateWorkflowTestHost<InterfaceBuild>(workflowWriter: writer);

        // Act
        await host.RunAsync();

        // Assert
        writer
            .GeneratedWorkflows
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(workflow =>
            {
                workflow.Name.ShouldBe("single-workflow");

                workflow
                    .Triggers
                    .ShouldHaveSingleItem()
                    .ShouldBeOfType<TestWorkflowTrigger>();

                workflow.Jobs.Count.ShouldBe(1);

                workflow
                    .Jobs[0]
                    .ShouldSatisfyAllConditions(job =>
                    {
                        job.Name.ShouldBe("InterfaceTarget");
                        job.TargetStep.Name.ShouldBe("InterfaceTarget");
                    });
            });

        await Verify(writer);
    }
}
