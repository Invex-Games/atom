namespace Invex.Atom.Module.GitHubWorkflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class GitHubWorkflowTriggersExtensions
{
    [PublicAPI]
    public sealed class Triggers
    {
        internal static Triggers Instance { get; } = new();

        public GitHubTrigger OnRelease(On.Release.ReleaseType type = On.Release.ReleaseType.released) =>
            new(new On.Release([type]));

        public GitHubTrigger OnSchedule(params IReadOnlyList<string> crons) =>
            new(new On.Schedule(crons));
    }

    extension(WorkflowTriggers)
    {
        [PublicAPI]
        public static Triggers GitHub => Triggers.Instance;
    }
}
