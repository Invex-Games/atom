namespace DecSm.Atom.Workflows.TestUtils;

[PublicAPI]
public class TestWorkflowWriter : IWorkflowWriter<TestWorkflowType>
{
    public bool IsOutdated { get; init; }

    public List<WorkflowModel> GeneratedWorkflows { get; } = [];

    public Task Generate(WorkflowModel workflow, CancellationToken cancellationToken = default)
    {
        GeneratedWorkflows.Add(workflow);

        return Task.CompletedTask;
    }

    public Task<bool> CheckForOutdatedWorkflow(WorkflowModel workflow, CancellationToken cancellationToken = default) =>
        Task.FromResult(IsOutdated);
}
