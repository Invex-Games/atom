namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public interface IWorkflowExpressionFormatter
{
    bool Enabled { get; }

    int Priority { get; }

    WorkflowExpression? Write(IWorkflowExpressionResolver resolver, WorkflowExpression expression);
}
