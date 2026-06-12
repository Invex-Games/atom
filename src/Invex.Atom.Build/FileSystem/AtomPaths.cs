namespace Invex.Atom.Build.FileSystem;

/// <summary>
///     Provides well-known path keys used to identify key directories in the Atom build system.
/// </summary>
/// <remarks>
///     These keys are used by <see cref="IPathProvider" /> implementations to identify which
///     directory path is being resolved.
/// </remarks>
[PublicAPI]
public static class AtomPaths
{
    /// <summary>
    ///     Represents the key for the root directory of the Atom project.
    /// </summary>
    public const string Root = "Root";

    /// <summary>
    ///     Represents the key for the directory where build artifacts are stored.
    /// </summary>
    public const string Artifacts = "Artifacts";

    /// <summary>
    ///     Represents the key for the directory where build outputs are published.
    /// </summary>
    public const string Publish = "Publish";

    /// <summary>
    ///     Represents the key for the temporary directory used during builds.
    /// </summary>
    public const string Temp = "Temp";
}
