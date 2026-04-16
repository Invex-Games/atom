namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Schedule interval for Dependabot updates.
/// </summary>
[UnstableAPI]
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
