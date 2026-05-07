namespace DecSm.Atom.Workflows.Options;

[PublicAPI]
public sealed record TargetCondition(TextExpression Condition) : IBuildOption, IImplicitTargetDependencyOption
{
    public IEnumerable<string> TargetNames =>
        TextExpressionUtils
            .Flatten(Condition)
            .OfType<TargetOutputExpression>()
            .Select(x => x.TargetName);
}
