namespace DecSm.Atom.Workflows.Expressions;

public interface IWorkflowExpressionGenerator
{
    string Write(WorkflowExpression? expression);
}

public sealed class WorkflowExpressionGenerator(IEnumerable<IWorkflowExpressionWriter> writers)
    : IWorkflowExpressionGenerator
{
    private readonly IWorkflowExpressionWriter[] _writers = writers
        .Where(x => x.Enabled)
        .OrderBy(x => x.Priority)
        .ToArray();

    public string Write(WorkflowExpression? expression)
    {
        switch (expression)
        {
            case null:
                return string.Empty;

            case LiteralExpression literal:
                return literal.Value;
        }

        foreach (var writer in _writers)
            if (writer.Write(this, expression) is { } result)
                return Write(result);

        throw new InvalidOperationException($"No writer found to handle expression {expression}");
    }
}
