namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Provides the build version using GitVersion.
/// </summary>
/// <remarks>
///     This provider executes the GitVersion tool to extract major, minor, patch, and pre-release
///     tag information, which is then used to construct a semantic version.
///     It installs the pinned <see cref="GitVersionTool.Version" /> of GitVersion.Tool when needed.
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

            var cacheKey = GitVersionCacheKey.Compute(processRunner, fileSystem);

            lock (_lock)
            {
                JsonElement jsonOutput;

                if (GitVersionCache.TryRead(fileSystem, cacheKey, logger) is { } cachedOutput)
                {
                    jsonOutput = cachedOutput;
                }
                else
                {
                    dotnetToolInstallHelper.InstallTool(GitVersionTool.PackageId, GitVersionTool.Version);

                    var gitVersionResult = processRunner.Run(new("dotnet", "gitversion /output json")
                    {
                        InvocationLogLevel = LogLevel.Debug,
                    });

                    jsonOutput = JsonSerializer.Deserialize(gitVersionResult.Output,
                        JsonElementContext.Default.JsonElement);

                    GitVersionCache.Write(fileSystem, cacheKey, jsonOutput);
                }

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
}
