namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
[Union]
public partial record Step
{
    public string? Id { get; init; }

    public WorkflowExpression? If { get; init; }

    public WorkflowExpression? Name { get; init; }

    public WorkflowExpression? WorkingDirectory { get; init; }

    public IReadOnlyDictionary<string, WorkflowExpressionOrCollection>? With { get; init; }

    public IReadOnlyDictionary<string, WorkflowExpression>? Env { get; init; }

    public WorkflowExpression? ContinueOnError { get; init; }

    public WorkflowExpression? TimeoutMinutes { get; init; }

    public partial record UsesStep
    {
        public required WorkflowExpression Uses { get; init; }
    }

    public partial record RunStep
    {
        public required WorkflowExpressionOrCollection Run { get; init; }

        public WorkflowExpression? Shell { get; init; }
    }
}
