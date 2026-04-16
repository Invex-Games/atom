namespace DecSm.Atom.Module.DevopsWorkflows;

/// <summary>
///     Represents a workflow option to use the Azure DevOps run ID as the workflow ID.
/// </summary>
/// <remarks>
///     When this option is enabled, the `Build.BuildId` variable from Azure DevOps
///     will be used as the unique identifier for the workflow run.
/// </remarks>
[PublicAPI]
public sealed record ProvideDevopsRunIdAsWorkflowId : ToggleWorkflowOption<ProvideDevopsRunIdAsWorkflowId>;
