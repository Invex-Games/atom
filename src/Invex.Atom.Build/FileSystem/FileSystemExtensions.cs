namespace Invex.Atom.Build.FileSystem;

/// <summary>
///     Provides extension members on <see cref="IRootedFileSystem" /> for accessing well-known
///     Atom directories.
/// </summary>
[PublicAPI]
public static class FileSystemExtensions
{
    extension(IRootedFileSystem fileSystem)
    {
        /// <summary>
        ///     Gets the root directory of the Atom project, identified by markers like <c>.git</c> or <c>.sln</c> files.
        /// </summary>
        public RootedPath AtomRootDirectory => fileSystem.GetPath(AtomPaths.Root);

        /// <summary>
        ///     Gets the default directory for storing build artifacts.
        /// </summary>
        public RootedPath AtomArtifactsDirectory => fileSystem.GetPath(AtomPaths.Artifacts);

        /// <summary>
        ///     Gets the default directory for publishing final build outputs.
        /// </summary>
        public RootedPath AtomPublishDirectory => fileSystem.GetPath(AtomPaths.Publish);

        /// <summary>
        ///     Gets the temporary directory for build operations.
        /// </summary>
        public RootedPath AtomTempDirectory => fileSystem.GetPath(AtomPaths.Temp);

        /// <summary>
        ///     Gets the current working directory of the application.
        /// </summary>
        public RootedPath CurrentDirectory => new(fileSystem, fileSystem.Directory.GetCurrentDirectory());
    }
}
