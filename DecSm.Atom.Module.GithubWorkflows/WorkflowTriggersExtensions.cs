namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public static class WorkflowTriggersExtensions
{
    [PublicAPI]
    public sealed class Triggers
    {
        internal static Triggers Instance { get; } = new();

        public GithubReleaseTrigger OnReleased =>
            field ??= new()
            {
                Types = ["released"],
            };

        public GithubReleaseTrigger OnPublished =>
            field ??= new()
            {
                Types = ["published"],
            };
    }

    extension(WorkflowTriggers)
    {
        [PublicAPI]
        public static Triggers Github => Triggers.Instance;
    }
}
