namespace Invex.Atom.Workflows.Definition;

/// <summary>
///     Serves as an extension anchor for well-known workflow label factories.
/// </summary>
/// <remarks>
///     Modules attach extension members to this class to expose common labels (e.g., runner images or
///     target frameworks) under a single discoverable entry point, such as
///     <c>WorkflowLabels.Github.RunsOn</c> or <c>WorkflowLabels.Dotnet.Framework</c>.
/// </remarks>
[PublicAPI]
public static class WorkflowLabels;
