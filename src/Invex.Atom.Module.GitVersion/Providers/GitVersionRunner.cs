namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Runs GitVersion once per configuration file and shares its output between the GitVersion providers.
/// </summary>
/// <remarks>
///     <para>
///         The configuration file is read from the <see cref="IGitVersion.GitVersionConfig" /> parameter each time
///         the output is requested. All work happens behind a single lock, so the build ID and build version
///         providers never start GitVersion twice for the same configuration. Results are kept in memory for the
///         lifetime of the process and persisted with <see cref="GitVersionCache" />.
///     </para>
///     <para>
///         <c>GitVersion.Tool</c> is pinned to <see cref="ToolVersion" /> with a minimum of
///         <see cref="MinimumToolVersion" />, because the configuration schema (for example <c>ignore.paths</c>)
///         differs between GitVersion releases.
///     </para>
/// </remarks>
internal sealed class GitVersionRunner(
    IDotnetToolInstallHelper dotnetToolInstallHelper,
    IProcessRunner processRunner,
    IParamService paramService,
    IRootedFileSystem fileSystem,
    ILogger<GitVersionRunner> logger
)
{
    /// <summary>
    ///     The command-line argument name of the <see cref="IGitVersion.GitVersionConfig" /> parameter.
    /// </summary>
    internal const string ConfigParamArgName = "gitversion-config";

    /// <summary>
    ///     The package ID of the GitVersion .NET tool.
    /// </summary>
    internal const string ToolName = "GitVersion.Tool";

    /// <summary>
    ///     The floating version that GitVersion is installed or updated to.
    /// </summary>
    internal const string ToolVersion = "6.*";

    /// <summary>
    ///     The lowest supported GitVersion version: 6.8.0 matches <c>ignore.paths</c> case-insensitively.
    /// </summary>
    internal static readonly SemVer MinimumToolVersion = SemVer.Parse("6.8.0");

    /// <summary>
    ///     The file names GitVersion looks for, in order, when no configuration file is given.
    /// </summary>
    internal static readonly string[] DefaultConfigFileNames =
    [
        "GitVersion.yml", "GitVersion.yaml", ".GitVersion.yml", ".GitVersion.yaml",
    ];

    private readonly Lock _lock = new();
    private readonly Dictionary<string, JsonElement> _outputs = new(StringComparer.Ordinal);
    private bool _toolVerified;

    /// <summary>
    ///     Gets GitVersion's JSON output (its version variables) for the configured configuration file.
    /// </summary>
    /// <returns>The root JSON element of GitVersion's <c>/output json</c> result.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if <see cref="IGitVersion.GitVersionConfig" /> is set to a file that does not exist, or if no
    ///     supported GitVersion version is available (see <see cref="IsSupportedToolVersion" />).
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Returns the in-memory result when GitVersion already ran for the same configuration in this process,
    ///         then a matching on-disk cache entry, and only otherwise installs the tool (if needed) and runs
    ///         GitVersion.
    ///     </para>
    ///     <para>
    ///         The configuration parameter is read on every call, so a value requested before the parameter is
    ///         visible (for example while the build model is resolved, when only default values are returned) does
    ///         not hide the configured file later.
    ///     </para>
    /// </remarks>
    internal JsonElement GetOutput()
    {
        lock (_lock)
        {
            var config = ResolveConfig();
            var outputKey = config?.RelativePath ?? string.Empty;

            if (_outputs.TryGetValue(outputKey, out var output))
                return output;

            var currentGitHash = processRunner
                .Run(new("git", "rev-parse HEAD")
                {
                    InvocationLogLevel = LogLevel.Debug,
                })
                .Output
                .Trim();

            var cacheFileName = GitVersionCache.GetCacheFileName(config?.RelativePath);

            var cacheKey = GitVersionCache.CreateKey(currentGitHash,
                config?.RelativePath,
                config?.ContentHash ?? GetDefaultConfigContentHash());

            var jsonOutput = GitVersionCache.TryRead(fileSystem, cacheFileName, cacheKey, logger);

            if (jsonOutput is null)
            {
                EnsureToolInstalled();

                string[] args = config is null
                    ? ["gitversion", "/output", "json"]
                    : ["gitversion", "/config", $"\"{config.FullPath}\"", "/output", "json"];

                var gitVersionResult = processRunner.Run(new("dotnet", args)
                {
                    InvocationLogLevel = LogLevel.Debug,
                });

                var runOutput = JsonSerializer.Deserialize(gitVersionResult.Output,
                    JsonElementContext.Default.JsonElement);

                GitVersionCache.Write(fileSystem, cacheFileName, cacheKey, runOutput);
                jsonOutput = runOutput;
            }

            _outputs[outputKey] = jsonOutput.Value;

            return jsonOutput.Value;
        }
    }

    /// <summary>
    ///     Determines whether an installed GitVersion version is supported: 6.x at or above
    ///     <see cref="MinimumToolVersion" />.
    /// </summary>
    /// <param name="installedVersion">The installed version, or <c>null</c> if GitVersion is not installed.</param>
    /// <returns><c>true</c> if <paramref name="installedVersion" /> is supported; otherwise, <c>false</c>.</returns>
    internal static bool IsSupportedToolVersion(string? installedVersion) =>
        installedVersion is not null &&
        SemVer.TryParse(installedVersion, out var version) &&
        version.Major == MinimumToolVersion.Major &&
        version >= MinimumToolVersion;

    /// <summary>
    ///     Resolves the <see cref="IGitVersion.GitVersionConfig" /> parameter to a configuration file under the
    ///     Atom root directory.
    /// </summary>
    /// <returns>The resolved configuration file, or <c>null</c> when the parameter is not set.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the resolved file does not exist.</exception>
    internal ResolvedConfig? ResolveConfig()
    {
        // A non-null default lets an interactive run accept an empty answer at the prompt for this optional parameter.
        var value = paramService.GetParam(nameof(IGitVersion.GitVersionConfig), string.Empty);

        if (string.IsNullOrWhiteSpace(value))
            return null;

        var rootDirectory = fileSystem.AtomRootDirectory;

        var fullPath = fileSystem.Path.GetFullPath(fileSystem.Path.Combine(rootDirectory, value.Trim()));

        if (!fileSystem.File.Exists(fullPath))
            throw new InvalidOperationException(
                $"The GitVersion config file '{fullPath}' does not exist. The '{ConfigParamArgName}' parameter " +
                $"is set to '{value}', which must be a path relative to the repository root '{rootDirectory}'.");

        var relativePath = fileSystem
            .Path
            .GetRelativePath(rootDirectory, fullPath)
            .Replace('\\', '/');

        var contentHash = Convert.ToHexStringLower(SHA256.HashData(fileSystem.File.ReadAllBytes(fullPath)));

        return new(relativePath, fullPath, contentHash);
    }

    /// <summary>
    ///     Gets the content hash of the configuration file GitVersion finds by default in the Atom root directory.
    /// </summary>
    /// <returns>
    ///     The lowercase hex SHA-256 hash of the first of <see cref="DefaultConfigFileNames" /> that exists in the
    ///     Atom root directory, or <c>null</c> if none exists.
    /// </returns>
    private string? GetDefaultConfigContentHash()
    {
        foreach (var path in DefaultConfigFileNames.Select(fileName => fileSystem.AtomRootDirectory / fileName))
        {
            if (fileSystem.File.Exists(path))
                return Convert.ToHexStringLower(SHA256.HashData(fileSystem.File.ReadAllBytes(path)));
        }

        return null;
    }

    private void EnsureToolInstalled()
    {
        if (_toolVerified)
            return;

        // 'dotnet gitversion' runs a version pinned in a local tool manifest before a global one.
        var localVersion = dotnetToolInstallHelper.GetInstalledToolVersion(ToolName, global: false);

        if (localVersion is not null)
        {
            if (!IsSupportedToolVersion(localVersion))
                throw new InvalidOperationException(
                    $"{ToolName} {localVersion} is pinned in the local tool manifest, but {MinimumToolVersion} or a " +
                    $"later {MinimumToolVersion.Major}.x version is required. Update the manifest, for example with " +
                    $"'dotnet tool update {ToolName} --version {ToolVersion}'.");

            _toolVerified = true;

            return;
        }

        var installedVersion = dotnetToolInstallHelper.GetInstalledToolVersion(ToolName);

        if (!IsSupportedToolVersion(installedVersion))
        {
            logger.LogDebug(
                "{ToolName} {InstalledVersion} is not supported (6.x, {MinimumVersion} or later); installing {ToolVersion}",
                ToolName,
                installedVersion ?? "(not installed)",
                MinimumToolVersion,
                ToolVersion);

            dotnetToolInstallHelper.InstallTool(ToolName, ToolVersion, forceReinstall: true);

            installedVersion = dotnetToolInstallHelper.GetInstalledToolVersion(ToolName);

            if (!IsSupportedToolVersion(installedVersion))
                throw new InvalidOperationException(
                    $"{ToolName} {installedVersion ?? "(unknown)"} is installed globally after installing " +
                    $"{ToolVersion}, but {MinimumToolVersion} or a later {MinimumToolVersion.Major}.x version is " +
                    "required. Check that the configured NuGet feeds provide a supported version.");
        }

        _toolVerified = true;
    }

    /// <summary>
    ///     A GitVersion configuration file resolved from the <see cref="IGitVersion.GitVersionConfig" /> parameter.
    /// </summary>
    /// <param name="RelativePath">The path relative to the Atom root directory, with forward slashes.</param>
    /// <param name="FullPath">The absolute path passed to GitVersion's <c>/config</c> argument.</param>
    /// <param name="ContentHash">The lowercase hex SHA-256 hash of the file contents.</param>
    internal sealed record ResolvedConfig(string RelativePath, string FullPath, string ContentHash);
}
