namespace DecSm.Atom.FileSystem;

/// <summary>
///     Represents a file system path that is rooted within a specific <see cref="IFileSystem" /> instance.
/// </summary>
/// <remarks>
///     <para>
///         Properties such as <see cref="FileExists" />, <see cref="FileName" />, and
///         <see cref="DirectoryName" /> intentionally return <c>null</c> when the path does not match
///         the expected entry type (file vs. directory), rather than throwing, so callers can use
///         null-coalescing patterns naturally.
///     </para>
/// </remarks>
/// <param name="FileSystem">The file system instance this path belongs to.</param>
/// <param name="Path">The absolute path string.</param>
[PublicAPI]
public record RootedPath(IFileSystem FileSystem, string Path)
{
    /// <summary>
    ///     Gets the parent directory of the current path.
    /// </summary>
    /// <returns>
    ///     A new <see cref="RootedPath" /> representing the parent directory, or <c>null</c> if the
    ///     current path is already a root.
    /// </returns>
    /// <remarks>
    ///     A trailing directory separator on the input path is stripped before the parent is computed
    ///     so that <c>/foo/</c> and <c>/foo</c> both yield the same parent (<c>/</c>).
    ///     The returned path always ends with <see cref="IPath.DirectorySeparatorChar" /> so it can be
    ///     combined further with the <c>/</c> operator without producing double separators.
    /// </remarks>
    public RootedPath? Parent
    {
        get
        {
            if (FileSystem.Path.GetPathRoot(Path) == Path)
                return null;

            var path = Path switch
            {
                [.., '/'] or [.., '\\'] => Path[..^1],
                _ => Path,
            };

            var lastForwardSlash = path.LastIndexOf('/');
            var lastBackSlash = path.LastIndexOf('\\');

            var lastSlash = Math.Max(lastForwardSlash, lastBackSlash);

            if (lastSlash == -1)
                return null;

            return this with
            {
                Path = $"{path[..lastSlash]}{FileSystem.Path.DirectorySeparatorChar}",
            };
        }
    }

    /// <summary>
    ///     Indicates whether the path exists in the file system as either a file or a directory.
    /// </summary>
    /// <remarks>Equivalent to <c><see cref="FileExists" /> || <see cref="DirectoryExists" /></c>.</remarks>
    public bool PathExists => FileExists || DirectoryExists;

    /// <summary>
    ///     Indicates whether the path exists in the file system as a file.
    /// </summary>
    public bool FileExists => FileSystem.File.Exists(Path);

    /// <summary>
    ///     Indicates whether the path exists in the file system as a directory.
    /// </summary>
    public bool DirectoryExists => FileSystem.Directory.Exists(Path);

    /// <summary>
    ///     Gets the file name (including extension) from the current path.
    /// </summary>
    /// <returns>
    ///     The file name if the path resolves to an existing file; otherwise <c>null</c>.
    ///     Returns <c>null</c> (rather than throwing) when the path points to a directory or does not
    ///     exist, so callers can safely use null-coalescing without a prior existence check.
    /// </returns>
    public string? FileName =>
        FileExists
            ? FileSystem.Path.GetFileName(Path)
            : null;

    /// <summary>
    ///     Gets the file name of the current path without its extension.
    /// </summary>
    /// <remarks>
    ///     Unlike <see cref="FileName" />, this property does not check whether the file exists —
    ///     it is a pure path-string operation and always returns a value.
    /// </remarks>
    public string FileNameWithoutExtension => FileSystem.Path.GetFileNameWithoutExtension(Path);

    /// <summary>
    ///     Gets the parent directory path of the current path as reported by the file system.
    /// </summary>
    /// <returns>
    ///     The directory component of the path if the path resolves to an existing directory;
    ///     otherwise <c>null</c>.
    ///     Returns <c>null</c> (rather than throwing) when the path points to a file or does not exist.
    /// </returns>
    /// <remarks>
    ///     This delegates to <see cref="IPath.GetDirectoryName(string)" />, which strips the last path
    ///     segment and trailing separator — it does <em>not</em> return the directory's own name.
    /// </remarks>
    public string? DirectoryName =>
        DirectoryExists
            ? FileSystem.Path.GetDirectoryName(Path)
            : null;

    /// <summary>
    ///     Combines the current <see cref="RootedPath" /> with a relative string segment using
    ///     <see cref="IPath.Combine(string, string)" />.
    /// </summary>
    /// <param name="left">The base <see cref="RootedPath" />.</param>
    /// <param name="right">The relative segment to append.</param>
    /// <returns>A new <see cref="RootedPath" /> representing the combined path.</returns>
    /// <remarks>
    ///     Using an operator instead of a method keeps path-construction code concise and readable,
    ///     e.g. <c>atomFileSystem.GetPath("Root") / "src" / "MyProject"</c>.
    ///     The operator preserves the original <see cref="FileSystem" /> reference so the resulting
    ///     path remains bound to the same file-system abstraction.
    /// </remarks>
    public static RootedPath operator /(RootedPath left, string right) =>
        left with
        {
            Path = left.FileSystem.Path.Combine(left.Path, right),
        };

    /// <summary>
    ///     Implicitly converts a <see cref="RootedPath" /> to its string representation.
    /// </summary>
    /// <param name="path">The <see cref="RootedPath" /> to convert.</param>
    /// <returns>The string representation of the path.</returns>
    /// <remarks>
    ///     Allows a <see cref="RootedPath" /> to be passed directly to any API that accepts a
    ///     <see cref="string" /> path without an explicit cast or <c>.ToString()</c> call.
    /// </remarks>
    public static implicit operator string(RootedPath path) =>
        path.Path;

    /// <inheritdoc />
    public override string ToString() =>
        Path;
}
