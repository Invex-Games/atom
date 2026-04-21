namespace DecSm.Atom.Build.FileSystem;

/// <summary>
///     Provides constants for key directory paths and an extension method for path provider registration.
/// </summary>
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
