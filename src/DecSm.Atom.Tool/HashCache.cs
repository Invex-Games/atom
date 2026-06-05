namespace DecSm.Atom.Tool;

/// <summary>
///     Shared low-level helpers for the content-hash based restore/build caches used by
///     <see cref="Commands.RunCommand" />.
/// </summary>
internal static class HashCache
{
    /// <summary>
    ///     Computes a stable SHA256 hash over the given files. The hash incorporates each file's path
    ///     (relative to <paramref name="projectDir" />) and its raw content, and is order-independent.
    /// </summary>
    internal static string ComputeHash(IFileSystem fileSystem, string projectDir, IEnumerable<string> files)
    {
        var ordered = files
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static x => x, StringComparer.Ordinal);

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        foreach (var file in ordered)
        {
            var relativePath = fileSystem.Path.GetRelativePath(projectDir, file);

            hash.AppendData(Encoding.UTF8.GetBytes(relativePath));

            try
            {
                hash.AppendData(fileSystem.File.ReadAllBytes(file));
            }
            catch (IOException)
            {
                // Treat an unreadable input as empty content rather than failing the run.
            }
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>
    ///     Reads the cached hash from the given file, or <c>null</c> if it cannot be read.
    /// </summary>
    internal static string? TryRead(IFileSystem fileSystem, string cacheFilePath)
    {
        try
        {
            return fileSystem
                .File
                .ReadAllText(cacheFilePath)
                .Trim();
        }
        catch (IOException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Persists the computed hash. Best-effort: failures (e.g. concurrent runs, permissions) are ignored.
    /// </summary>
    internal static void Write(IFileSystem fileSystem, string cacheFilePath, string hash)
    {
        try
        {
            var dir = fileSystem.Path.GetDirectoryName(cacheFilePath);

            if (dir is { Length: > 0 })
                fileSystem.Directory.CreateDirectory(dir);

            fileSystem.File.WriteAllText(cacheFilePath, hash);
        }
        catch (IOException)
        {
            // Best-effort: a concurrent run or a locked file should never break the build.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort: missing permissions should never break the build.
        }
    }
}
