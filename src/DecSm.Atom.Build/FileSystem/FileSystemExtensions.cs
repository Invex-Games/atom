namespace DecSm.Atom.Build.FileSystem;

[PublicAPI]
public static class FileSystemExtensions
{
    extension(IAtomFileSystem atomFileSystem)
    {
        /// <summary>
        ///     Gets the root directory of the Atom project, identified by markers like <c>.git</c> or <c>.sln</c> files.
        /// </summary>
        public RootedPath AtomRootDirectory => atomFileSystem.GetPath(AtomPaths.Root);

        /// <summary>
        ///     Gets the default directory for storing build artifacts.
        /// </summary>
        public RootedPath AtomArtifactsDirectory => atomFileSystem.GetPath(AtomPaths.Artifacts);

        /// <summary>
        ///     Gets the default directory for publishing final build outputs.
        /// </summary>
        public RootedPath AtomPublishDirectory => atomFileSystem.GetPath(AtomPaths.Publish);

        /// <summary>
        ///     Gets the temporary directory for build operations.
        /// </summary>
        public RootedPath AtomTempDirectory => atomFileSystem.GetPath(AtomPaths.Temp);

        /// <summary>
        ///     Gets the current working directory of the application.
        /// </summary>
        public RootedPath CurrentDirectory => new(atomFileSystem, atomFileSystem.Directory.GetCurrentDirectory());
    }
}
