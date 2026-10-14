namespace Invex.Atom.Module.GitVersion.Options;

/// <summary>
///     Extends the <see cref="BuildOptions" /> anchor class with fluent factories for GitVersion options.
/// </summary>
[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class GitVersionBuildOptionsExtensions
{
    /// <summary>
    ///     Provides factories for configuring which build information GitVersion supplies.
    /// </summary>
    [PublicAPI]
    public sealed class GitVersionOptions
    {
        /// <summary>
        ///     Gets the singleton instance of the flags factory.
        /// </summary>
        public static GitVersionOptions Instance => field ??= new();

        /// <summary>
        ///     Gets an option that enables GitVersion as the provider of the build ID.
        /// </summary>
        public GitVersionProvideBuildIdOption ProvideBuildId => field ??= new();

        /// <summary>
        ///     Gets an option that enables GitVersion as the provider of the build version.
        /// </summary>
        public GitVersionProvideBuildVersionOption ProvideBuildVersion => field ??= new();

        /// <summary>
        ///     Creates an option that enables or disables GitVersion as the provider of the build ID.
        /// </summary>
        /// <param name="value"><c>true</c> to provide the build ID from GitVersion; otherwise, <c>false</c>.</param>
        /// <returns>A <see cref="GitVersionProvideBuildIdOption" /> option.</returns>
        public GitVersionProvideBuildIdOption SetProvideBuildId(bool value) =>
            new()
            {
                Enabled = value,
            };

        /// <summary>
        ///     Creates an option that enables or disables GitVersion as the provider of the build version.
        /// </summary>
        /// <param name="value"><c>true</c> to provide the build version from GitVersion; otherwise, <c>false</c>.</param>
        /// <returns>A <see cref="GitVersionProvideBuildVersionOption" /> option.</returns>
        public GitVersionProvideBuildVersionOption SetProvideBuildVersion(bool value) =>
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
        public static GitVersionOptions GitVersion => GitVersionOptions.Instance;
    }
}
