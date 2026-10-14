namespace Invex.Atom.Workflows;

/// <summary>
///     Defines a contract for a workflow type, representing a specific CI/CD platform or execution environment.
/// </summary>
/// <remarks>
///     This interface is implemented by classes representing different workflow platforms (e.g., GitHub Actions, Azure
///     DevOps).
///     Each workflow type is matched with an <see cref="Writer.IWorkflowWriter{T}" /> that writes its workflow files.
///     Use <see cref="IWorkflowContextProvider" /> to detect which platform the build is currently running on.
/// </remarks>
[PublicAPI]
[SuppressMessage("Design", "CA1040:Avoid empty interfaces")]
public interface IWorkflowType;
