namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Provides the build ID using GitVersion.
/// </summary>
/// <remarks>
///     This provider executes the GitVersion tool to extract the full semantic version,
///     which is then used as the build ID. GitVersion is run (and the tool installed if needed) by the shared
///     <see cref="GitVersionRunner" />, so the build ID and build version use a single GitVersion run.
/// </remarks>
internal sealed class GitVersionBuildIdProvider(
    GitVersionRunner gitVersionRunner,
    IBuildOptionService buildOptionService
) : IBuildIdProvider
{
    /// <summary>
    ///     Gets the build ID, derived from GitVersion's FullSemVer.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if GitVersion is not enabled via <see cref="GitVersionProvideBuildIdOption" />
    ///     or if the build ID cannot be determined from GitVersion's output.
    /// </exception>
    /// <remarks>
    ///     The value is not cached here: <see cref="GitVersionRunner" /> caches GitVersion's output per configuration
    ///     file, so a value first read while the configuration parameter is not yet visible (for example while the
    ///     build model is resolved) does not hide the configured one.
    /// </remarks>
    public string BuildId
    {
        get
        {
            if (!GitVersionProvideBuildIdOption.IsEnabled(buildOptionService.Options))
                throw new InvalidOperationException(
                    "GitVersion is not enabled for build ID generation. Ensure GitVersionProvideBuildIdOption is enabled.");

            var buildId = gitVersionRunner
                .GetOutput()
                .GetProperty("FullSemVer")
                .GetString();

            return buildId ?? throw new InvalidOperationException("Failed to determine build ID");
        }
    }

    /// <summary>
    ///     Gets a group identifier for the given build ID, typically representing the major.minor.patch version.
    /// </summary>
    /// <param name="buildId">The build ID (expected to be a semantic version string).</param>
    /// <returns>A string representing the major.minor.patch group.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the build ID cannot be parsed as a semantic version.</exception>
    public string GetBuildIdGroup(string buildId) =>
        !SemVer.TryParse(buildId, out var version)
            ? throw new InvalidOperationException($"Failed to parse build ID '{buildId}' as a semantic version.")
            : $"{version.Major}.{version.Minor}.{version.Patch}";
}
