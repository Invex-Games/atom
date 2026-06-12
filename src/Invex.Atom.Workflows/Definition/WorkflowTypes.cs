namespace Invex.Atom.Workflows.Definition;

/// <summary>
///     Serves as an extension anchor for workflow type factories.
/// </summary>
/// <remarks>
///     Modules attach extension members to this class to expose their <see cref="IWorkflowType" />
///     instances under a single discoverable entry point, such as <c>WorkflowTypes.Github.Action</c>.
/// </remarks>
[PublicAPI]
public static class WorkflowTypes;
