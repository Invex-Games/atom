namespace DecSm.Atom.Workflows.Definition;

/// <summary>
///     Defines a contract for a workflow type, representing a specific CI/CD platform or execution environment.
/// </summary>
/// <remarks>
///     This interface is implemented by classes representing different workflow platforms (e.g., GitHub Actions, Azure
///     DevOps).
///     It allows the Atom framework to determine if a particular workflow type is currently active.
/// </remarks>
[PublicAPI]
public interface IWorkflowType
{
    /// <summary>
    ///     Gets a value indicating whether this workflow type is currently running.
    /// </summary>
    bool IsRunning { get; }
}
