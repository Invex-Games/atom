namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public static class WorkflowExpressions
{
    public static LiteralExpression Literal(string value) =>
        new(value);
}
