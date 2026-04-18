namespace DecSm.Atom.Module.GitVersion;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class WorkflowOptionsExtensions
{
    [PublicAPI]
    public class GitVersionOptions
    {
        internal static GitVersionOptions Instance { get; } = new();

        public UseGitVersionForBuildId UseAsBuildId =>
            new()
            {
                Value = true,
            };

        public UseGitVersionForBuildId DoNotUseAsBuildId =>
            new()
            {
                Value = false,
            };
    }

    extension(WorkflowOptions)
    {
        [PublicAPI]
        public static GitVersionOptions UseGitVersionForBuildId => GitVersionOptions.Instance;
    }
}
