namespace Invex.Atom.Tool;

/// <summary>
///     Provides a content-hash based cache that lets <see cref="Commands.RunCommand" /> decide whether a
///     <c>dotnet restore</c> can be skipped (by passing <c>--no-restore</c> to <c>dotnet run</c>).
/// </summary>
/// <remarks>
///     The cache is stored in the project's <c>obj</c> folder (so it is naturally invalidated by
///     <c>dotnet clean</c>) and keyed by a SHA256 hash of all restore-affecting inputs: the project/file
///     itself plus any <c>Directory.Build.props</c>, <c>Directory.Build.targets</c>,
///     <c>Directory.Packages.props</c>, <c>nuget.config</c> and <c>global.json</c> files found while
///     walking up to the project root.
/// </remarks>
internal static class RestoreCache
{
    private const string CacheFileName = ".atom-restore.hash";

    private const string RestoreMarkerFileName = "project.assets.json";

    private static readonly string[] RestoreInputFileNames =
    [
        "Directory.Build.props",
        "Directory.Build.targets",
        "Directory.Packages.props",
        "nuget.config",
        "global.json",
    ];

    /// <summary>
    ///     Represents the outcome of evaluating the restore cache for a given target.
    /// </summary>
    /// <param name="CanSkipRestore">Whether the restore can be skipped because nothing relevant changed.</param>
    /// <param name="ComputedHash">The freshly computed hash of the restore-affecting inputs.</param>
    /// <param name="CacheFilePath">The path of the cache file used to persist the hash.</param>
    internal sealed record RestoreDecision(bool CanSkipRestore, string ComputedHash, string CacheFilePath);

    /// <summary>
    ///     Computes the restore input hash and compares it against the cached value to determine whether a
    ///     restore can be skipped.
    /// </summary>
    internal static RestoreDecision Evaluate(IFileSystem fileSystem, IFileInfo target)
    {
        var projectDir = target.Directory?.FullName ?? fileSystem.Path.GetDirectoryName(target.FullName)!;
        var objDir = fileSystem.Path.Combine(projectDir, "obj");
        var cacheFilePath = fileSystem.Path.Combine(objDir, CacheFileName);

        var computedHash = HashCache.ComputeHash(fileSystem,
            projectDir,
            CollectRestoreInputs(fileSystem, target, projectDir));

        // A restore can only be skipped if a previous restore actually produced its output (project.assets.json)
        // and the cached hash matches the current inputs.
        var canSkip = fileSystem.Directory.Exists(objDir) &&
                      fileSystem.File.Exists(fileSystem.Path.Combine(objDir, RestoreMarkerFileName)) &&
                      fileSystem.File.Exists(cacheFilePath) &&
                      string.Equals(HashCache.TryRead(fileSystem, cacheFilePath),
                          computedHash,
                          StringComparison.OrdinalIgnoreCase);

        return new(canSkip, computedHash, cacheFilePath);
    }

    /// <summary>
    ///     Persists the computed hash so a subsequent run can skip the restore. Best-effort: failures are ignored.
    /// </summary>
    internal static void Save(IFileSystem fileSystem, RestoreDecision decision) =>
        HashCache.Write(fileSystem, decision.CacheFilePath, decision.ComputedHash);

    /// <summary>
    ///     Collects the restore-affecting files for the given target by walking up to the project root.
    /// </summary>
    internal static List<string> CollectRestoreInputs(IFileSystem fileSystem, IFileInfo target, string projectDir)
    {
        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (fileSystem.File.Exists(target.FullName))
            files.Add(target.FullName);

        var current = fileSystem.DirectoryInfo.New(projectDir);

        while (current is { Exists: true })
        {
            foreach (var name in RestoreInputFileNames)
            {
                var candidate = fileSystem.Path.Combine(current.FullName, name);

                if (fileSystem.File.Exists(candidate))
                    files.Add(candidate);
            }

            // Don't walk higher than the project root.
            if (FileFinder.IsRootDirectory(fileSystem, current.FullName))
                break;

            current = current.Parent;
        }

        return files.ToList();
    }
}
