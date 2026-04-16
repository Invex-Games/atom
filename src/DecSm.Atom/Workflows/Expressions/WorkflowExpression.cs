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
