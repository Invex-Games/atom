namespace DecSm.Atom.Module.GitVersion;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    public class Options
    {
        internal static Options Instance { get; } = new();

        public UseGitVersionForBuildId Enabled =>
            new()
            {
                Value = true,
            };

        public UseGitVersionForBuildId Disabled =>
            new()
            {
                Value = false,
            };
    }

    extension(WorkflowOptions)
    {
        [PublicAPI]
        public static Options UseGitVersionForBuildId => Options.Instance;
    }
}
