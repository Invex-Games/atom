namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Provides the build version using GitVersion.
/// </summary>
/// <remarks>
///     This provider executes the GitVersion tool to extract major, minor, patch, and pre-release
///     tag information, which is then used to construct a semantic version.
///     GitVersion is run (and the tool installed if needed) by the shared <see cref="GitVersionRunner" />, so the
///     build ID and build version use a single GitVersion run.
/// </remarks>
[PublicAPI]
internal sealed class GitVersionBuildVersionProvider(
    GitVersionRunner gitVersionRunner,
    IBuildOptionService buildOptionService
) : IBuildVersionProvider
{
    /// <summary>
    ///     Gets the semantic version of the build, derived from GitVersion's output.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the version information cannot be determined from GitVersion's output.
    /// </exception>
    public SemVer Version
    {
        get
        {
            if (!GitVersionProvideBuildVersionOption.IsEnabled(buildOptionService.Options))
                throw new InvalidOperationException(
                    "GitVersion is not enabled for build version generation. Ensure GitVersionProvideBuildVersionOption is enabled.");

            if (field is not null)
                return field;

            var jsonOutput = gitVersionRunner.GetOutput();

            var majorProp = jsonOutput
                .GetProperty("Major")
                .GetUInt32();

            var minorProp = jsonOutput
                .GetProperty("Minor")
                .GetUInt32();

            var patchProp = jsonOutput
                .GetProperty("Patch")
                .GetUInt32();

            var preReleaseTagProp = jsonOutput
                .GetProperty("PreReleaseTag")
                .GetString()!;

            return field = preReleaseTagProp is { Length: > 0 }
                ? SemVer.Parse($"{majorProp}.{minorProp}.{patchProp}-{preReleaseTagProp}")
                : SemVer.Parse($"{majorProp}.{minorProp}.{patchProp}");
        }
    }
}
