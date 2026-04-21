namespace DecSm.Atom.FileSystem;

/// <summary>
///     Defines a specialized file system abstraction for Atom, providing access to key build-related directories and
///     paths.
/// </summary>
[PublicAPI]
public interface IAtomFileSystem : IFileSystem
{
    /// <summary>
    ///     Gets the underlying <see cref="IFileSystem" /> instance for general-purpose file operations.
    /// </summary>
    IFileSystem FileSystem { get; }

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
    ///     Gets the current working directory of the application.
    /// </summary>
    RootedPath CurrentDirectory => new(this, FileSystem.Directory.GetCurrentDirectory());

    /// <summary>
    ///     Resolves a path by its key, using registered path providers and falling back to default logic.
    /// </summary>
    /// <param name="key">The key identifying the path (e.g., "Root", "Artifacts").</param>
    /// <returns>A <see cref="RootedPath" /> corresponding to the key.</returns>
    RootedPath GetPath(string key);

    /// <summary>
    ///     Resolves the path for a given file marker type.
    /// </summary>
    /// <typeparam name="T">The type of the file marker, which must implement <see cref="IPathMarker" />.</typeparam>
    /// <returns>A <see cref="RootedPath" /> for the specified file marker.</returns>
    RootedPath GetPath<T>()
        where T : IPathMarker =>
        T.Path(this);

    /// <summary>
    ///     Creates a new <see cref="RootedPath" /> instance from a string path.
    /// </summary>
    /// <param name="path">The string representation of the path.</param>
    /// <returns>A new <see cref="RootedPath" /> associated with this file system instance.</returns>
    RootedPath CreateRootedPath(string path) =>
        new(this, path);
}

/// <summary>
///     Internal implementation of <see cref="IAtomFileSystem" />.
/// </summary>
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
