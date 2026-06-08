namespace Invex.Atom.Tool;

/// <summary>
///     Content-hash based cache that lets <see cref="Commands.RunCommand" /> decide whether a
///     <c>dotnet build</c> can be skipped (by passing <c>--no-build</c> to <c>dotnet run</c>).
/// </summary>
/// <remarks>
///     The cache hashes the project's restore inputs plus every <c>.cs</c> source file under the project
///     directory (excluding <c>bin</c>/<c>obj</c>), and is stored in <c>obj/.atom-build.hash</c>. A build is
///     only skippable when a previous build output (<c>bin/&lt;project&gt;.dll</c>) exists and the cached hash
///     matches.
///     <para>
///         Note: source changes in projects referenced via <c>&lt;ProjectReference&gt;</c> are not tracked.
///         Use <c>--no-restore-cache</c> (or the <c>ATOM_NO_RESTORE_CACHE</c> environment variable) to force a
///         full restore and build.
///     </para>
/// </remarks>
internal static class BuildCache
{
    private const string CacheFileName = ".atom-build.hash";

    private static readonly string[] ExcludedFolderNames = ["bin", "obj"];

    /// <summary>
    ///     Represents the outcome of evaluating the build cache for a given target.
    /// </summary>
    /// <param name="CanSkipBuild">Whether the build can be skipped because nothing relevant changed.</param>
    /// <param name="ComputedHash">The freshly computed hash of the build-affecting inputs.</param>
    /// <param name="CacheFilePath">The path of the cache file used to persist the hash.</param>
    internal sealed record BuildDecision(bool CanSkipBuild, string ComputedHash, string CacheFilePath);

    /// <summary>
    ///     Computes the build input hash and compares it against the cached value to determine whether a
    ///     build can be skipped.
    /// </summary>
    internal static BuildDecision Evaluate(IFileSystem fileSystem, IFileInfo target)
    {
        var projectDir = target.Directory?.FullName ?? fileSystem.Path.GetDirectoryName(target.FullName)!;
        var objDir = fileSystem.Path.Combine(projectDir, "obj");
        var cacheFilePath = fileSystem.Path.Combine(objDir, CacheFileName);

        var inputs = RestoreCache.CollectRestoreInputs(fileSystem, target, projectDir);
        var sources = CollectSourceFiles(fileSystem, projectDir);

        var computedHash = HashCache.ComputeHash(fileSystem, projectDir, inputs.Concat(sources));

        // A build can only be skipped if a previous build actually produced an assembly and the cached hash
        // matches the current inputs.
        var canSkip = HasBuildOutput(fileSystem, projectDir, target) &&
                      fileSystem.File.Exists(cacheFilePath) &&
                      string.Equals(HashCache.TryRead(fileSystem, cacheFilePath),
                          computedHash,
                          StringComparison.OrdinalIgnoreCase);

        return new(canSkip, computedHash, cacheFilePath);
    }

    /// <summary>
    ///     Persists the computed hash so a subsequent run can skip the build. Best-effort: failures are ignored.
    /// </summary>
    internal static void Save(IFileSystem fileSystem, BuildDecision decision) =>
        HashCache.Write(fileSystem, decision.CacheFilePath, decision.ComputedHash);

    private static bool HasBuildOutput(IFileSystem fileSystem, string projectDir, IFileInfo target)
    {
        var binDir = fileSystem.Path.Combine(projectDir, "bin");

        if (!fileSystem.Directory.Exists(binDir))
            return false;

        var assemblyFileName = $"{fileSystem.Path.GetFileNameWithoutExtension(target.Name)}.dll";

        try
        {
            return fileSystem
                .Directory
                .EnumerateFiles(binDir, assemblyFileName, SearchOption.AllDirectories)
                .Any();
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static List<string> CollectSourceFiles(IFileSystem fileSystem, string projectDir)
    {
        var result = new List<string>();
        var queue = new Queue<string>();
        queue.Enqueue(projectDir);

        while (queue.Count > 0)
        {
            var dir = queue.Dequeue();

            try
            {
                result.AddRange(fileSystem.Directory.EnumerateFiles(dir, "*.cs"));
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            try
            {
                foreach (var sub in fileSystem.Directory.EnumerateDirectories(dir))
                {
                    var name = fileSystem.Path.GetFileName(sub);

                    if (!ExcludedFolderNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                        queue.Enqueue(sub);
                }
            }
            catch (IOException)
            {
                // Ignore directories we cannot enumerate.
            }
            catch (UnauthorizedAccessException)
            {
                // Ignore directories we cannot access.
            }
        }

        return result;
    }
}
