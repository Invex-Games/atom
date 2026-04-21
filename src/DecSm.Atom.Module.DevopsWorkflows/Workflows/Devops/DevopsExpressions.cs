namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops;

/// <summary>
///     Wraps an expression to be evaluated at runtime using Azure DevOps runtime expression syntax: $[ expression ]
/// </summary>
[PublicAPI]
public sealed record DevopsRuntimeExpression(WorkflowExpression Expression) : WorkflowExpression;

/// <summary>
///     Wraps a variable name to be expanded using Azure DevOps macro syntax: $(variableName)
/// </summary>
[PublicAPI]
public sealed record DevopsMacroExpression(WorkflowExpression Variable) : WorkflowExpression;
