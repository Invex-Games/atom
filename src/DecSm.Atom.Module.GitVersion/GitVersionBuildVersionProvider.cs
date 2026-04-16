namespace DecSm.Atom.Module.GitVersion;

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
    IAtomFileSystem fileSystem,
    ILogger<GitVersionBuildVersionProvider> logger
) : IBuildVersionProvider
{
    #if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
    #else
    private readonly object _lock = new();
    #endif

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
            if (field is not null)
                return field;

            var currentGitHash = processRunner
                .Run(new("git", "rev-parse HEAD")
                {
                    InvocationLogLevel = LogLevel.Debug,
                })
                .Output
                .Trim();

            var hashCache = fileSystem.AtomPublishDirectory / ".gitversioncache" / currentGitHash;

            JsonElement? jsonOutput = null;

            lock (_lock)
            {
                if (fileSystem.File.Exists(hashCache))
                    try
                    {
                        var cachedContent = fileSystem.File.ReadAllText(hashCache);
                        jsonOutput = JsonSerializer.Deserialize(cachedContent, JsonElementContext.Default.JsonElement);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "Failed to read or parse cached GitVersion output. Will re-run GitVersion.");

                        jsonOutput = null;
                        fileSystem.File.Delete(hashCache);
                    }

                if (jsonOutput is null)
                {
                    dotnetToolInstallHelper.InstallTool("GitVersion.Tool");

                    var gitVersionResult = processRunner.Run(new("dotnet", "gitversion /output json")
                    {
                        InvocationLogLevel = LogLevel.Debug,
                    });

                    jsonOutput = JsonSerializer.Deserialize(gitVersionResult.Output,
                        JsonElementContext.Default.JsonElement);

                    fileSystem.Directory.CreateDirectory(fileSystem.AtomPublishDirectory / ".gitversioncache");
                    fileSystem.File.WriteAllText(hashCache, jsonOutput.Value.GetRawText());
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
