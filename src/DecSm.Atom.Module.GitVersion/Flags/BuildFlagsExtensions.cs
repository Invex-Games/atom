using DecSm.Atom.Build.BuildOptions;

namespace DecSm.Atom.Module.GitVersion.Flags;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class BuildFlagsExtensions
{
    [PublicAPI]
    public sealed class GitVersionFlags
    {
        public static GitVersionFlags Instance => field ??= new();

        public GitVersionProvideBuildIdFlag ProvideBuildId => field ??= new(true);

        public GitVersionProvideBuildIdFlag SetProvideBuildId(bool value) =>
            new(value);

        public GitVersionProvideBuildVersionFlag ProvideBuildVersion => field ??= new(true);

        public GitVersionProvideBuildVersionFlag SetProvideBuildVersion(bool value) =>
            new(value);
    }

    extension(BuildOptions)
    {
        public static GitVersionFlags GitVersion => GitVersionFlags.Instance;
    }
}
