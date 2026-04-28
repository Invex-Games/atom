namespace DecSm.StructuredText.Expressions;

[PublicAPI]
public sealed record TargetOutputExpression : TextExpression, ITextExpression<string>
{
    public required string TargetName { get; init; }

    public required string? OutputName { get; init; }
}

[PublicAPI]
public sealed record TargetOutcomeExpression : TextExpression, ITextExpression<string>
{
    public required string Target { get; init; }
}

[PublicAPI]
public sealed record StepOutputExpression : TextExpression, ITextExpression<string>
{
    public required string StepName { get; init; }

    public required string OutputName { get; init; }
}

[PublicAPI]
public sealed record StepOutcomeExpression : TextExpression, ITextExpression<string>
{
    public required string StepName { get; init; }
}

[PublicAPI]
public sealed record TargetOutcomeTypeExpression : TextExpression, ITextExpression<string>
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
public sealed record StepOutcomeTypeExpression : TextExpression, ITextExpression<string>
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
