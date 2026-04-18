namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Schedule preferences for Dependabot updates.
/// </summary>
[PublicAPI]
public sealed record DependabotSchedule
{
    /// <summary>
    ///     How often to check for updates.
    /// </summary>
    public required ScheduleInterval Interval { get; init; }

    /// <summary>
    ///     Specify an alternative day to check for updates.
    /// </summary>
    public ScheduleDay? Day { get; init; }

    /// <summary>
    ///     Specify an alternative time of day to check for updates (format: hh:mm).
    /// </summary>
    public string? Time { get; init; }

    /// <summary>
    ///     The time zone identifier must be from the Time Zone database maintained by IANA.
    /// </summary>
    public string? Timezone { get; init; }

    /// <summary>
    ///     Specify a valid cron expression for updates.
    /// </summary>
    public string? Cronjob { get; init; }
}
