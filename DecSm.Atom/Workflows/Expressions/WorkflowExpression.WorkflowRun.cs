namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public sealed record TargetOutputExpression : WorkflowExpression
{
    public required string TargetName { get; init; }

    public required string? OutputName { get; init; }
}

[PublicAPI]
public sealed record TargetOutcomeExpression : WorkflowExpression
{
    public required string Target { get; init; }
}

[PublicAPI]
public sealed record StepOutputExpression : WorkflowExpression
{
    public required string StepName { get; init; }

    public required string OutputName { get; init; }
}

[PublicAPI]
public sealed record StepOutcomeExpression : WorkflowExpression
{
    public required string StepName { get; init; }
}

[PublicAPI]
public sealed record TargetOutcomeTypeExpression : WorkflowExpression
{
    public enum OutcomeType
    {
        Success,
        Failure,
        Cancelled,
    }

    public required OutcomeType Type { get; init; }
}

[PublicAPI]
public sealed record StepOutcomeTypeExpression : WorkflowExpression
{
    public enum OutcomeType
    {
        Success,
        Failure,
        Cancelled,
        Skipped,
    }

    public required OutcomeType Type { get; init; }
}
