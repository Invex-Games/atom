namespace DecSm.Atom.Module.DevopsWorkflows.Generation.Options;

/// <summary>
///     Represents a workflow option for configuring the checkout step in Azure DevOps Pipelines.
/// </summary>
/// <param name="Lfs">Whether to enable Git LFS support.</param>
/// <param name="Submodules">How to handle submodules (e.g., "true", "recursive", "false").</param>
/// <remarks>
///     This option allows customization of the checkout action, such as enabling LFS and handling submodules.
/// </remarks>
[PublicAPI]
public sealed record DevopsCheckoutOption(bool Lfs = false, string? Submodules = null) : IWorkflowOption;
