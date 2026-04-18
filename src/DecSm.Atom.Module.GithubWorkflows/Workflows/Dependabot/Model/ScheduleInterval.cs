namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Schedule interval for Dependabot updates.
/// </summary>
[PublicAPI]
public enum ScheduleInterval
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Semiannually,
    Yearly,
    Cron,
}
