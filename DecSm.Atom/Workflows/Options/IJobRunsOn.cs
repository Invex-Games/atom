namespace DecSm.Atom.Workflows.Options;

/// <summary>
///     Defines the execution environment (runner or agent) for a workflow job.
/// </summary>
/// <remarks>
///     This interface provides common constants for operating system tags and a parameter for specifying the runner.
///     It is typically used in build matrices to run jobs across multiple environments.
/// </remarks>
[PublicAPI]
public interface IJobRunsOn : IBuildDefinition, IBuildAccessor
{
    /// <summary>
    ///     Gets the runner or agent tag for the job, sourced from the "job-runs-on" parameter.
    /// </summary>
    [ParamDefinition("job-runs-on", "The runner or agent to use for a job (e.g., 'windows-latest').")]
    string JobRunsOn => GetParam(() => JobRunsOn)!;
}
