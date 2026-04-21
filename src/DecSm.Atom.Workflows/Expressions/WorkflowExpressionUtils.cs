namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public static class WorkflowExpressionUtils
{
    extension(IEnumerable<WorkflowExpression> expressions)
    {
        public IEnumerable<WorkflowExpression> Join(WorkflowExpression separator)
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
}
