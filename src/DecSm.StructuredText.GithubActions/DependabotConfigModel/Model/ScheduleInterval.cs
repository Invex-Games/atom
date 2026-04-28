namespace DecSm.StructuredText.GithubActions.DependabotConfigModel.Model;

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
