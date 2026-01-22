namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public interface IWorkflowExpressionWriter
{
    bool Enabled { get; }

    int Priority { get; }

    WorkflowExpression? Write(IWorkflowExpressionGenerator workflowExpressionGenerator, WorkflowExpression expression);
}
