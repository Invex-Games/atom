namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public interface IWorkflowExpressionResolver
{
    [return: NotNullIfNotNull("expression")]
    string? Resolve(WorkflowExpression? expression);

    string[] Resolve(IEnumerable<WorkflowExpression> expressions);
}

[PublicAPI]
internal sealed class WorkflowExpressionResolver(IEnumerable<IWorkflowExpressionFormatter> writers)
    : IWorkflowExpressionResolver
{
    private readonly IWorkflowExpressionFormatter[] _writers = writers
        .Where(x => x.Enabled)
        .OrderBy(x => x.Priority)
        .ToArray();

    [return: NotNullIfNotNull("expression")]
    public string? Resolve(WorkflowExpression? expression)
    {
        switch (expression)
        {
            case null:
                return null;

            case RawExpression raw:
                return raw.Value;

            case ConcatExpression concat:
                return string.Concat(concat.Values.Select(Resolve));
        }

        foreach (var writer in _writers)
            if (writer.Write(this, expression) is { } result)
                return Resolve(result);

        throw new InvalidOperationException($"No writer found to handle expression {expression}");
    }

    public string[] Resolve(IEnumerable<WorkflowExpression> expressions) =>
        expressions
            .Select(Resolve)
            .OfType<string>()
            .ToArray();
}
