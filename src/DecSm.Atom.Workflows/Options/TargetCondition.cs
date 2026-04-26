namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public sealed record TargetCondition(TextExpression Value) : IBuildOption;
