namespace Invex.Atom.Module.GitVersion.Flags;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class BuildFlagsExtensions
{
    [PublicAPI]
    public sealed class GitVersionFlags
    {
        public static GitVersionFlags Instance => field ??= new();

        public GitVersionProvideBuildIdFlag ProvideBuildId => field ??= new();

        public GitVersionProvideBuildVersionFlag ProvideBuildVersion => field ??= new();

        public GitVersionProvideBuildIdFlag SetProvideBuildId(bool value) =>
            new()
            {
                Enabled = value,
            };

        public GitVersionProvideBuildVersionFlag SetProvideBuildVersion(bool value) =>
            new()
            {
                Enabled = value,
            };
    }

    extension(BuildOptions)
    {
        public static GitVersionFlags GitVersion => GitVersionFlags.Instance;
    }
}
