namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public abstract partial record WorkflowExpression
{
    public IndexAccessExpression this[int index] => new(this, new NumberExpression(index));

    public PropertyAccessExpression this[string property] => new(this, property);

    public static implicit operator WorkflowExpression(string value) =>
        new LiteralExpression(value);

    public EvaluateExpression Evaluate() =>
        new(this);
}

[PublicAPI]
public sealed record LiteralExpression(string Value) : WorkflowExpression;

[PublicAPI]
public sealed record EvaluateExpression(WorkflowExpression Expression) : WorkflowExpression;

[PublicAPI]
public sealed record IndexAccessExpression(WorkflowExpression Array, WorkflowExpression Index) : WorkflowExpression;

[PublicAPI]
public sealed record PropertyAccessExpression(WorkflowExpression Object, WorkflowExpression Property)
    : WorkflowExpression;
