namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Reads and writes a single-file cache of GitVersion output, keyed by the current Git commit hash and the
///     pinned <see cref="GitVersionTool.Version" />.
/// </summary>
/// <remarks>
///     The cache is a single file in the build project's <c>obj</c> directory (so it is git-ignored and removed
///     by <c>dotnet clean</c>). The first line stores the GitVersion tool version and Git hash the output was
///     generated for, and the remainder stores the raw GitVersion JSON. When either no longer matches, the cache
///     is ignored and overwritten - just like the tool's restore/build caches, which each track a single hash file
///     rather than one file per key. The tool version is part of the key because GitVersion's output variables
///     can change meaning between versions, so output cached by another version must never be reused.
/// </remarks>
internal static class GitVersionCache
{
    private const string CacheFileName = ".gitversioncache";

    /// <summary>
    ///     Returns the cached GitVersion output if it was generated for <paramref name="gitHash" /> by the pinned
    ///     <see cref="GitVersionTool.Version" />; otherwise <c>null</c>.
    /// </summary>
    internal static JsonElement? TryRead(IRootedFileSystem fileSystem, string gitHash, ILogger logger)
    {
        var cacheFile = GetCacheFile(fileSystem);

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

            // The cached output belongs to a different commit or GitVersion version - treat it as a miss.
            if (!string.Equals(cachedKey, GetCacheKey(gitHash), StringComparison.OrdinalIgnoreCase))
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
    ///     Persists the GitVersion output for <paramref name="gitHash" /> and the pinned
    ///     <see cref="GitVersionTool.Version" />, overwriting any previous entry.
    /// </summary>
    internal static void Write(IRootedFileSystem fileSystem, string gitHash, JsonElement output)
    {
        fileSystem.Directory.CreateDirectory(ResolveObjDirectory(fileSystem));
        fileSystem.File.WriteAllText(GetCacheFile(fileSystem), $"{GetCacheKey(gitHash)}\n{output.GetRawText()}");
    }

    private static string GetCacheKey(string gitHash) =>
        $"{GitVersionTool.Version}:{gitHash}";

    private static RootedPath GetCacheFile(IRootedFileSystem fileSystem) =>
        ResolveObjDirectory(fileSystem) / CacheFileName;

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
