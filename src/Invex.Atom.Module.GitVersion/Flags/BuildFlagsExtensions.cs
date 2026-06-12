namespace Invex.Atom.Module.GitVersion.Flags;

/// <summary>
///     Extends the <see cref="BuildOptions" /> anchor class with fluent factories for GitVersion options.
/// </summary>
[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class BuildFlagsExtensions
{
    /// <summary>
    ///     Provides factories for configuring which build information GitVersion supplies.
    /// </summary>
    [PublicAPI]
    public sealed class GitVersionFlags
    {
        /// <summary>
        ///     Gets the singleton instance of the flags factory.
        /// </summary>
        public static GitVersionFlags Instance => field ??= new();

        /// <summary>
        ///     Gets an option that enables GitVersion as the provider of the build ID.
        /// </summary>
        public GitVersionProvideBuildIdFlag ProvideBuildId => field ??= new();

        /// <summary>
        ///     Gets an option that enables GitVersion as the provider of the build version.
        /// </summary>
        public GitVersionProvideBuildVersionFlag ProvideBuildVersion => field ??= new();

        /// <summary>
        ///     Creates an option that enables or disables GitVersion as the provider of the build ID.
        /// </summary>
        /// <param name="value"><c>true</c> to provide the build ID from GitVersion; otherwise, <c>false</c>.</param>
        /// <returns>A <see cref="GitVersionProvideBuildIdFlag" /> option.</returns>
        public GitVersionProvideBuildIdFlag SetProvideBuildId(bool value) =>
            new()
            {
                Enabled = value,
            };

        /// <summary>
        ///     Creates an option that enables or disables GitVersion as the provider of the build version.
        /// </summary>
        /// <param name="value"><c>true</c> to provide the build version from GitVersion; otherwise, <c>false</c>.</param>
        /// <returns>A <see cref="GitVersionProvideBuildVersionFlag" /> option.</returns>
        public GitVersionProvideBuildVersionFlag SetProvideBuildVersion(bool value) =>
            new()
            {
                Enabled = value,
            };
    }

    extension(BuildOptions)
    {
        /// <summary>
        ///     Gets factories for configuring GitVersion build options.
        /// </summary>
        public static GitVersionFlags GitVersion => GitVersionFlags.Instance;
    }
}
