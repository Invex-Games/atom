namespace DecSm.Atom.FileSystem;

/// <summary>
///     Defines a specialised file system abstraction for Atom, layering build-aware path resolution
///     on top of the standard <see cref="IFileSystem" /> abstraction.
/// </summary>
/// <remarks>
///     <para>
///         Path resolution is intentionally separate from file I/O: <see cref="GetPath(string)" />
///         queries a prioritized chain of <see cref="IPathProvider" /> implementations and caches the
///         result.  Callers never need to hard-code paths or know which provider resolved a key.
///     </para>
/// </remarks>
[PublicAPI]
public interface IAtomFileSystem : IFileSystem
{
    /// <summary>
    ///     Gets the underlying <see cref="IFileSystem" /> instance used for all file I/O operations.
    /// </summary>
    /// <remarks>
    ///     All <see cref="IFileSystem" /> members on this interface delegate to this property so that
    ///     consumers of <see cref="IAtomFileSystem" /> work against a single, consistent abstraction
    ///     regardless of which concrete file-system backend is registered.
    /// </remarks>
    IFileSystem FileSystem { get; }

    /// <summary>
    ///     Gets the current working directory of the application as a <see cref="RootedPath" />.
    /// </summary>
    /// <remarks>
    ///     Evaluated eagerly each time the property is read — it is not cached because the working
    ///     directory can change during the lifetime of the process.
    /// </remarks>
    RootedPath CurrentDirectory => new(this, FileSystem.Directory.GetCurrentDirectory());

    /// <inheritdoc cref="IFileSystem.Directory" />
    IDirectory IFileSystem.Directory => FileSystem.Directory;

    /// <inheritdoc cref="IFileSystem.DirectoryInfo" />
    IDirectoryInfoFactory IFileSystem.DirectoryInfo => FileSystem.DirectoryInfo;

    /// <inheritdoc cref="IFileSystem.DriveInfo" />
    IDriveInfoFactory IFileSystem.DriveInfo => FileSystem.DriveInfo;

    /// <inheritdoc cref="IFileSystem.File" />
    IFile IFileSystem.File => FileSystem.File;

    /// <inheritdoc cref="IFileSystem.FileInfo" />
    IFileInfoFactory IFileSystem.FileInfo => FileSystem.FileInfo;

    /// <inheritdoc cref="IFileSystem.FileStream" />
    IFileStreamFactory IFileSystem.FileStream => FileSystem.FileStream;

    /// <inheritdoc cref="IFileSystem.FileSystemWatcher" />
    IFileSystemWatcherFactory IFileSystem.FileSystemWatcher => FileSystem.FileSystemWatcher;

    /// <inheritdoc cref="IFileSystem.Path" />
    IPath IFileSystem.Path => FileSystem.Path;

    /// <inheritdoc cref="IFileSystem.FileVersionInfo" />
    IFileVersionInfoFactory IFileSystem.FileVersionInfo => FileSystem.FileVersionInfo;

    /// <summary>
    ///     Resolves a well-known path by key, querying the registered <see cref="IPathProvider" />
    ///     chain and caching the result for later calls.
    /// </summary>
    /// <param name="key">
    ///     An application-defined key that identifies the path (e.g. <c>"Root"</c>, <c>"Artifacts"</c>).
    ///     Keys are compared by the individual providers and are conventionally defined as constants.
    /// </param>
    /// <returns>A <see cref="RootedPath" /> corresponding to the key.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no registered provider can resolve <paramref name="key" />, or when the
    ///     resolution depth exceeds the circular-dependency guard threshold.
    /// </exception>
    /// <remarks>
    ///     Results are cached after the first successful resolution so that repeated calls to
    ///     <c>GetPath("Root")</c> always return the same <see cref="RootedPath" /> instance without
    ///     re-querying providers.  This also means that if two providers could resolve the same key,
    ///     only the one with the highest priority (earliest in the ordered list) ever runs.
    /// </remarks>
    RootedPath GetPath(string key);

    /// <summary>
    ///     Resolves the path for a given <see cref="IPathMarker" /> type by invoking its static
    ///     <see cref="IPathMarker.Path" /> method directly.
    /// </summary>
    /// <typeparam name="T">A type that implements <see cref="IPathMarker" />.</typeparam>
    /// <returns>A <see cref="RootedPath" /> for the specified marker type.</returns>
    /// <remarks>
    ///     Unlike <see cref="GetPath(string)" />, this overload bypasses the provider chain and the
    ///     cache entirely — it calls <c>T.Path(this)</c> on every invocation.  Use it for paths that
    ///     are statically determined by the marker type itself, such as source-generated project paths.
    /// </remarks>
    RootedPath GetPath<T>()
        where T : IPathMarker =>
        T.Path(this);

    /// <summary>
    ///     Creates a new <see cref="RootedPath" /> bound to this file system instance from a raw
    ///     string path.
    /// </summary>
    /// <param name="path">The absolute string path.</param>
    /// <returns>A new <see cref="RootedPath" /> associated with this file system instance.</returns>
    /// <remarks>
    ///     Prefer this factory method over constructing <see cref="RootedPath" /> directly so that the
    ///     resulting path is automatically bound to this <see cref="IAtomFileSystem" /> rather than to
    ///     the inner <see cref="FileSystem" />.
    /// </remarks>
    RootedPath CreateRootedPath(string path) =>
        new(this, path);
}

/// <summary>
///     Internal implementation of <see cref="IAtomFileSystem" />.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="PathProviders" /> must be provided in descending priority order.
///         <see cref="GetPath" /> picks the first non-<c>null</c> result, so the caller
///         (typically the DI registration in <see cref="FileSystemHostExtensions" />) is responsible
///         for ordering — <see cref="GetPath" /> itself does not re-sort.
///     </para>
///     <para>
///         An <see cref="AsyncLocal{T}" /> depth counter guards against circular provider
///         dependencies: if resolution recurses more than 100 levels deep an
///         <see cref="InvalidOperationException" /> is thrown rather than overflowing the stack.
///     </para>
/// </remarks>
/// <param name="logger">The logger for diagnostics.</param>
internal sealed class AtomFileSystem(ILogger<AtomFileSystem> logger) : IAtomFileSystem
{
    private readonly AsyncLocal<int> _getPathDepth = new();
    private readonly Dictionary<string, RootedPath> _pathCache = [];

    public required IReadOnlyList<IPathProvider> PathProviders { private get; init; }

    public required IFileSystem FileSystem { get; init; }

    /// <inheritdoc />
    public RootedPath GetPath(string key)
    {
        if (_getPathDepth.Value > 100)
            throw new InvalidOperationException(
                "Path resolution depth exceeded. It is likely that a circular dependency exists.");

        _getPathDepth.Value++;

        try
        {
            if (_pathCache.TryGetValue(key, out var path))
            {
                logger.LogDebug("Path for key '{Key}' found in cache: {Path}", key, path);

                return path;
            }

            path = PathProviders
                .Select(x => x.GetPath(key))
                .FirstOrDefault(x => x is not null);

            if (path is null)
                throw new InvalidOperationException($"Could not locate path for key '{key}'");

            logger.LogDebug("Path for key '{Key}' located: {Path}", key, path);

            return _pathCache[key] = path;
        }
        finally
        {
            _getPathDepth.Value--;
        }
    }
}
