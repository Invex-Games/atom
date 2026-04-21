namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents the current state and execution metrics of a target during a build run.
/// </summary>
/// <param name="Name">The name of the target this state belongs to.</param>
[PublicAPI]
public sealed record TargetState(string Name)
{
    /// <summary>
    ///     Gets or sets the current execution status of the target.
    /// </summary>
    public TargetRunState Status { get; set; } = TargetRunState.Uninitialized;

    /// <summary>
    ///     Gets or sets the duration of the target's execution.
    /// </summary>
    /// <remarks>
    ///     This value is <c>null</c> until the target has completed execution (succeeded or failed).
    /// </remarks>
    public TimeSpan? RunDuration { get; set; }
}
