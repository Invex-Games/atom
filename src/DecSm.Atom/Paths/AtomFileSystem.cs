namespace DecSm.Atom.Paths;

/// <summary>
///     Defines a specialized file system abstraction for Atom, providing access to key build-related directories and
///     paths.
/// </summary>
[PublicAPI]
public interface IAtomFileSystem : IFileSystem
{
    /// <summary>
    ///     Gets the name of the project, typically derived from the entry assembly.
    /// </summary>
    string ProjectName { get; }

    /// <summary>
    ///     Gets a value indicating whether the application is file-based (e.g., *.cs) or project-based (e.g., *.csproj).
    /// </summary>
    bool IsFileBasedApp { get; }

    /// <summary>
    ///     Gets the underlying <see cref="IFileSystem" /> instance for general-purpose file operations.
    /// </summary>
    IFileSystem FileSystem { get; }

    /// <summary>
    ///     Gets the root directory of the Atom project, identified by markers like <c>.git</c> or <c>.sln</c> files.
    /// </summary>
    RootedPath AtomRootDirectory => GetPath(AtomPaths.Root);

    /// <summary>
    ///     Gets the default directory for storing build artifacts.
    /// </summary>
    RootedPath AtomArtifactsDirectory => GetPath(AtomPaths.Artifacts);

    /// <summary>
    ///     Gets the default directory for publishing final build outputs.
    /// </summary>
    RootedPath AtomPublishDirectory => GetPath(AtomPaths.Publish);

    /// <summary>
    ///     Gets the temporary directory for build operations.
    /// </summary>
    RootedPath AtomTempDirectory => GetPath(AtomPaths.Temp);

    /// <summary>
    ///     Gets the current working directory of the application.
    /// </summary>
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
    ///     Resolves a path by its key, using registered path providers and falling back to default logic.
    /// </summary>
    /// <param name="key">The key identifying the path (e.g., "Root", "Artifacts").</param>
    /// <returns>A <see cref="RootedPath" /> corresponding to the key.</returns>
    RootedPath GetPath(string key);

    /// <summary>
    ///     Resolves the path for a given file marker type.
    /// </summary>
    /// <typeparam name="T">The type of the file marker, which must implement <see cref="IPathLocator" />.</typeparam>
    /// <returns>A <see cref="RootedPath" /> for the specified file marker.</returns>
    RootedPath GetPath<T>()
        where T : IPathLocator =>
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

    public required IReadOnlyList<IPathProvider> PathLocators { private get; init; }

    public string ProjectName { get; init; } = Assembly.GetEntryAssembly()!.GetName()
        .Name!;

    public bool IsFileBasedApp => AppContext.GetData("EntryPointFilePath") is string s && s.EndsWith(".cs");

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

            path = PathLocators
                .Select(x => x.GetPath(key))
                .FirstOrDefault(x => x is not null);

            if (path is not null)
            {
                logger.LogDebug("Path for key '{Key}' located: {Path}", key, path);

                return _pathCache[key] = path;
            }

            var result = _pathCache[key] = key switch
            {
                AtomPaths.Root => GetRoot(),
                AtomPaths.Artifacts or AtomPaths.Publish => GetPath(AtomPaths.Root) / "atom-publish",
                AtomPaths.Temp => new(this, FileSystem.Path.GetTempPath()),
                _ => throw new InvalidOperationException($"Could not locate path for key '{key}'"),
            };

            logger.LogDebug("Path for key '{Key}' computed: {Path}", key, result);

            return result;
        }
        finally
        {
            _getPathDepth.Value--;
        }
    }

    /// <summary>
    ///     Determines the root directory by traversing up from the current directory and looking for project markers.
    /// </summary>
    private RootedPath GetRoot()
    {
        var currentDir = ((IAtomFileSystem)this).CurrentDirectory;

        while (currentDir.Parent is not null)
        {
            currentDir = currentDir.Parent;

            if (FileSystem
                    .Directory
                    .EnumerateDirectories(currentDir, "*.git", SearchOption.TopDirectoryOnly)
                    .Any() ||
                FileSystem
                    .Directory
                    .EnumerateFiles(currentDir, "*.slnx", SearchOption.TopDirectoryOnly)
                    .Any() ||
                FileSystem
                    .Directory
                    .EnumerateFiles(currentDir, "*.sln", SearchOption.TopDirectoryOnly)
                    .Any())
                return currentDir;
        }

        return ((IAtomFileSystem)this).CurrentDirectory;
    }
}
