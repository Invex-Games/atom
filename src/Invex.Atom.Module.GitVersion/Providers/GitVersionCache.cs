namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Reads and writes a cache of GitVersion output, with one file per GitVersion configuration.
/// </summary>
/// <remarks>
///     <para>
///         Each cache file lives in the build project's <c>obj</c> directory (so it is git-ignored and removed by
///         <c>dotnet clean</c>). The default configuration (GitVersion's own lookup of <c>GitVersion.yml</c>) uses
///         <c>.gitversioncache</c>; a custom configuration uses <c>.gitversioncache-&lt;hash&gt;</c>, where the hash
///         is derived from the configuration's normalized, root-relative path.
///     </para>
///     <para>
///         The first line of a file stores the cache key (see <see cref="CreateKey" />), and the remainder stores
///         the raw GitVersion JSON. When the key no longer matches, the entry is ignored and overwritten - just like
///         the tool's restore/build caches, which each track a single hash file rather than one file per key.
///     </para>
/// </remarks>
internal static class GitVersionCache
{
    /// <summary>
    ///     The cache file name for the default configuration.
    /// </summary>
    internal const string DefaultCacheFileName = ".gitversioncache";

    /// <summary>
    ///     Gets the cache file name for a configuration.
    /// </summary>
    /// <param name="configRelativePath">
    ///     The configuration file's normalized path relative to the Atom root directory, or <c>null</c> for the
    ///     default configuration.
    /// </param>
    /// <returns>
    ///     <see cref="DefaultCacheFileName" /> for the default configuration; otherwise
    ///     <c>.gitversioncache-</c> followed by the first 12 hex characters of the SHA-256 hash of the path.
    /// </returns>
    internal static string GetCacheFileName(string? configRelativePath) =>
        configRelativePath is not { Length: > 0 }
            ? DefaultCacheFileName
            : $"{DefaultCacheFileName}-{HashPath(configRelativePath)}";

    /// <summary>
    ///     Creates the key that a cache entry must match to be used.
    /// </summary>
    /// <param name="gitHash">The current Git commit hash.</param>
    /// <param name="configRelativePath">
    ///     The configuration file's normalized path relative to the Atom root directory, or <c>null</c> for the
    ///     default configuration.
    /// </param>
    /// <param name="configContentHash">
    ///     The SHA-256 hash of the configuration file's contents, or <c>null</c> for the default configuration.
    /// </param>
    /// <returns>
    ///     <c>&lt;gitHash&gt;|&lt;configRelativePath&gt;|&lt;configContentHash&gt;</c>, with empty segments for the
    ///     default configuration, so moving to another commit or editing a custom configuration invalidates the entry.
    /// </returns>
    internal static string CreateKey(string gitHash, string? configRelativePath, string? configContentHash) =>
        $"{gitHash}|{configRelativePath}|{configContentHash}";

    /// <summary>
    ///     Returns the cached GitVersion output stored in <paramref name="cacheFileName" /> if it was generated for
    ///     <paramref name="key" />; otherwise <c>null</c>.
    /// </summary>
    /// <remarks>
    ///     A file that cannot be read or parsed is logged as a warning, deleted on a best-effort basis and treated as
    ///     a miss.
    /// </remarks>
    internal static JsonElement? TryRead(IRootedFileSystem fileSystem, string cacheFileName, string key, ILogger logger)
    {
        var cacheFile = GetCacheFile(fileSystem, cacheFileName);

        if (!fileSystem.File.Exists(cacheFile))
            return null;

        try
        {
            var content = fileSystem.File.ReadAllText(cacheFile);
            var newlineIndex = content.IndexOf('\n');

            if (newlineIndex < 0)
                return null;

            var cachedKey = content[..newlineIndex]
                .Trim();

            // The cached output belongs to a different commit or configuration - treat it as a miss.
            if (!string.Equals(cachedKey, key, StringComparison.OrdinalIgnoreCase))
                return null;

            return JsonSerializer.Deserialize(content[(newlineIndex + 1)..], JsonElementContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read or parse cached GitVersion output. Will re-run GitVersion.");

            try
            {
                fileSystem.File.Delete(cacheFile);
            }
            catch (IOException)
            {
                // Best-effort cleanup; a locked file should never break the build.
            }

            return null;
        }
    }

    /// <summary>
    ///     Persists the GitVersion output for <paramref name="key" /> to <paramref name="cacheFileName" />,
    ///     overwriting any previous entry in that file.
    /// </summary>
    internal static void Write(IRootedFileSystem fileSystem, string cacheFileName, string key, JsonElement output)
    {
        fileSystem.Directory.CreateDirectory(ResolveObjDirectory(fileSystem));
        fileSystem.File.WriteAllText(GetCacheFile(fileSystem, cacheFileName), $"{key}\n{output.GetRawText()}");
    }

    /// <summary>
    ///     Gets the full path of a cache file.
    /// </summary>
    internal static RootedPath GetCacheFile(IRootedFileSystem fileSystem, string cacheFileName) =>
        ResolveObjDirectory(fileSystem) / cacheFileName;

    private static string HashPath(string configRelativePath) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(configRelativePath)))[..12];

    private static RootedPath ResolveObjDirectory(IRootedFileSystem fileSystem)
    {
        // The build runs from '<project>/bin/<config>/<tfm>/', so walk up from the running assembly's
        // directory to find the project directory (the first ancestor that contains an 'obj' folder).
        var current = fileSystem.CreateRootedPath(AppContext.BaseDirectory);

        while (current.Parent is { } parent)
        {
            var objDir = parent / "obj";

            if (objDir.DirectoryExists)
                return objDir;

            current = parent;
        }

        // Fallback: use an 'obj' directory under the atom root if the project's obj cannot be located.
        return fileSystem.AtomRootDirectory / "obj";
    }
}
