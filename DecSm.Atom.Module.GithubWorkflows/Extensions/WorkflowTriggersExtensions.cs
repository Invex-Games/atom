namespace DecSm.Atom.Module.GithubWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowTriggersExtensions
{
    [PublicAPI]
    public sealed class Triggers
    {
        internal static Triggers Instance { get; } = new();

        public GithubTrigger OnRelease(On.Release.ReleaseType type = On.Release.ReleaseType.released) =>
            new(new On.Release([type]));

        public GithubTrigger OnSchedule(params IReadOnlyList<string> crons) =>
            new(new On.Schedule(crons));
    }

    extension(WorkflowTriggers)
    {
        [PublicAPI]
        public static Triggers Github => Triggers.Instance;
    }
}
