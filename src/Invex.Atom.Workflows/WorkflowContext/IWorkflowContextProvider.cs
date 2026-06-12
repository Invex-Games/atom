namespace Invex.Atom.Workflows.WorkflowContext;

/// <summary>
///     Supplies information about the workflow context the build is currently running in.
/// </summary>
/// <remarks>
///     Platform modules register implementations to detect their environment (e.g., GitHub Actions or
///     Azure DevOps). Registered providers are aggregated by <see cref="IWorkflowContext" />, which uses
///     the first provider that returns a non-<c>null</c> value.
/// </remarks>
[PublicAPI]
public interface IWorkflowContextProvider
{
    /// <summary>
    ///     Gets the workflow type detected by this provider, or <c>null</c> if its platform is not running.
    /// </summary>
    IWorkflowType? WorkflowType { get; }

    /// <summary>
    ///     Gets the name of the currently running workflow, or <c>null</c> if not available.
    /// </summary>
    string? WorkflowName { get; }
}
