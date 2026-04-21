namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents the execution state of a target during a build run.
/// </summary>
[PublicAPI]
public enum TargetRunState
{
    /// <summary>
    ///     The target has not yet been initialized or processed by the build executor.
    /// </summary>
    Uninitialized,

    /// <summary>
    ///     The target is scheduled to run but is waiting for its dependencies to complete.
    /// </summary>
    PendingRun,

    /// <summary>
    ///     The target is currently executing.
    /// </summary>
    Running,

    /// <summary>
    ///     The target completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    ///     The target failed during execution.
    /// </summary>
    Failed,

    /// <summary>
    ///     The target was not scheduled to run.
    /// </summary>
    NotRun,

    /// <summary>
    ///     The target was scheduled to run but was skipped, typically due to a build failure or user option.
    /// </summary>
    Skipped,
}
