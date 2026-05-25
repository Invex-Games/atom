namespace DecSm.Atom.Workflows;

[PublicAPI]
public abstract class WorkflowBuildDefinition(IServiceProvider services)
    : BuildDefinition(services), IWorkflowBuildDefinition
{
    /// <inheritdoc />
    public abstract IReadOnlyList<WorkflowDefinition> Workflows { get; }
}
