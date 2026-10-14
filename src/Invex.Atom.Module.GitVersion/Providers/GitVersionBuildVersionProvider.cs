namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Provides the build version using GitVersion.
/// </summary>
/// <remarks>
///     This provider executes the GitVersion tool to extract major, minor, patch, and pre-release
///     tag information, which is then used to construct a semantic version.
///     It requires the GitVersion.Tool to be installed.
/// </remarks>
[PublicAPI]
internal sealed class GitVersionBuildVersionProvider(
    IDotnetToolInstallHelper dotnetToolInstallHelper,
    IProcessRunner processRunner,
    IBuildOptionService buildOptionService,
    IRootedFileSystem fileSystem,
    ILogger<GitVersionBuildVersionProvider> logger
) : IBuildVersionProvider
{
    private readonly Lock _lock = new();

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

            var currentGitHash = processRunner
                .Run(new("git", "rev-parse HEAD")
                {
                    InvocationLogLevel = LogLevel.Debug,
                })
                .Output
                .Trim();

            lock (_lock)
            {
                var jsonOutput = GitVersionCache.TryRead(fileSystem, currentGitHash, logger);

                if (jsonOutput is null)
                {
                    dotnetToolInstallHelper.InstallTool("GitVersion.Tool");

                    var gitVersionResult = processRunner.Run(new("dotnet", "gitversion /output json")
                    {
                        InvocationLogLevel = LogLevel.Debug,
                    });

                    jsonOutput = JsonSerializer.Deserialize(gitVersionResult.Output,
                        JsonElementContext.Default.JsonElement);

                    GitVersionCache.Write(fileSystem, currentGitHash, jsonOutput.Value);
                }

                var majorProp = jsonOutput
                    .Value
                    .GetProperty("Major")
                    .GetUInt32();

                var minorProp = jsonOutput
                    .Value
                    .GetProperty("Minor")
                    .GetUInt32();

                var patchProp = jsonOutput
                    .Value
                    .GetProperty("Patch")
                    .GetUInt32();

                var preReleaseTagProp = jsonOutput
                    .Value
                    .GetProperty("PreReleaseTag")
                    .GetString()!;

                return field = preReleaseTagProp is { Length: > 0 }
                    ? SemVer.Parse($"{majorProp}.{minorProp}.{patchProp}-{preReleaseTagProp}")
                    : SemVer.Parse($"{majorProp}.{minorProp}.{patchProp}");
            }
        }
    }
}
