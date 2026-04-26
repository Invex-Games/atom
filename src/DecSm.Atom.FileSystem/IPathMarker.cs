namespace DecSm.Atom.FileSystem;

/// <summary>
///     Defines a contract for types that represent a specific, well-known file path within the
///     file system, resolved relative to the current <see cref="IFileSystem" /> instance.
/// </summary>
/// <remarks>
///     <para>
///         Implement this interface (typically as a nested type or a source-generated partial class)
///         to give a strongly-typed name to a path that would otherwise be identified by a magic
///         string.
///     </para>
///     <para>
///         Unlike keys resolved through <see cref="IAtomFileSystem.GetPath(string)" />,
///         <see cref="IPathMarker" /> paths are statically computed by the marker type itself via
///         the <see cref="Path" /> static abstract method.
///     </para>
/// </remarks>
[PublicAPI]
public interface IPathMarker
{
    /// <summary>
    ///     Computes the rooted path that this marker represents, evaluated against the given
    ///     <paramref name="fileSystem" />.
    /// </summary>
    /// <param name="fileSystem">
    ///     The file system instance to bind the path to.  Passing the current
    ///     <see cref="IAtomFileSystem" /> keeps the returned <see cref="RootedPath" /> consistent
    ///     with all other paths in the build.
    /// </param>
    /// <returns>A <see cref="RootedPath" /> representing the location of the file or directory.</returns>
    static abstract RootedPath Path(IFileSystem fileSystem);
}
