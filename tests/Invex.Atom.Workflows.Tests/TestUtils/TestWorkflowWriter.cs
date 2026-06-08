namespace Invex.Atom.Workflows.Tests.TestUtils;

[PublicAPI]
internal sealed class TestWorkflowWriter : IWorkflowWriter<TestWorkflowType>
{
    public bool IsOutdated { get; init; }

    public bool ThrowOnWrite { get; init; }

    public List<WorkflowModel> GeneratedWorkflows { get; } = [];

    public Task Generate(WorkflowModel workflow, CancellationToken cancellationToken = default)
    {
        if (ThrowOnWrite)
            throw new StepFailedException("TestWorkflowWriter is configured to throw on write.");

        GeneratedWorkflows.Add(workflow);

        return Task.CompletedTask;
    }

    public Task<bool> CheckForOutdatedWorkflow(WorkflowModel workflow, CancellationToken cancellationToken = default) =>
        Task.FromResult(IsOutdated);
}
