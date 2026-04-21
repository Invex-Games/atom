namespace DecSm.Atom.FileSystem;

/// <summary>
///     Defines a contract for types that represent a specific, well-known file path within the file system.
/// </summary>
[PublicAPI]
public interface IPathMarker
{
    /// <summary>
    ///     Gets the rooted path of the file.
    /// </summary>
    /// <param name="fileSystem">The file system service to resolve the path against.</param>
    /// <returns>A <see cref="RootedPath" /> representing the location of the file.</returns>
    static abstract RootedPath Path(IFileSystem fileSystem);
}
