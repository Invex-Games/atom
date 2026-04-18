namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public abstract partial record WorkflowExpression
{
    public IndexAccessExpression this[int index] => new(this, new NumberExpression(index));

    public PropertyAccessExpression this[string property] => new(this, new RawExpression(property));

    public EvaluateExpression Evaluate() =>
        new(this);
}

[PublicAPI]
public sealed record RawExpression(string Value) : WorkflowExpression;

[PublicAPI]
public sealed record EvaluateExpression(WorkflowExpression Expression) : WorkflowExpression;

[PublicAPI]
public sealed record IndexAccessExpression(WorkflowExpression Array, WorkflowExpression Index) : WorkflowExpression;

[PublicAPI]
public sealed record PropertyAccessExpression(WorkflowExpression Object, WorkflowExpression Property)
    : WorkflowExpression;

[PublicAPI]
public sealed record ConcatExpression(IEnumerable<WorkflowExpression> Values) : WorkflowExpression;

[PublicAPI]
public static partial class WorkflowExpressionExtensions
{
    extension(WorkflowExpressions)
    {
        public static ConcatExpression Concat(IEnumerable<WorkflowExpression> expressions) =>
            new(expressions);

        public static ConcatExpression ConcatWithSeparator(
            WorkflowExpression separator,
            IEnumerable<WorkflowExpression> expressions) =>
            new(Join(separator, expressions));
    }

    private static IEnumerable<WorkflowExpression> Join(
        WorkflowExpression separator,
        IEnumerable<WorkflowExpression> expressions)
    {
        var list = expressions.ToList();

        yield return list[0];

        foreach (var expression in list.Skip(1))
        {
            yield return separator;
            yield return expression;
        }
    }
}
