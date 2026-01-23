namespace DecSm.Atom.Workflows.Options;

/// <summary>
///     Represents a workflow step that sets up a specific .NET SDK version.
/// </summary>
/// <param name="DotnetVersion">
///     The .NET SDK version to install (e.g., "6.0.x"). If null, the environment's default version is used.
/// </param>
/// <param name="Quality">
///     The quality of the .NET SDK to install (e.g., Preview, GA). If null, the default quality is used.
/// </param>
/// <param name="Cache">Whether to cache the installed .NET SDK.</param>
/// <param name="LockFile">The path to the project's lock file (e.g., "**/packages.lock.json")</param>
[PublicAPI]
public sealed record SetupDotnetStep(
    string? DotnetVersion = null,
    SetupDotnetStep.DotnetQuality? Quality = null,
    bool Cache = false,
    string? LockFile = null
) : CustomStep
{
    /// <summary>
    ///     Specifies the quality of the .NET SDK version to install.
    /// </summary>
    [PublicAPI]
    public enum DotnetQuality
    {
        /// <summary>
        ///     The latest daily build.
        /// </summary>
        Daily,

        /// <summary>
        ///     The latest signed build.
        /// </summary>
        Signed,

        /// <summary>
        ///     The latest validated build.
        /// </summary>
        Validated,

        /// <summary>
        ///     The latest preview build.
        /// </summary>
        Preview,

        /// <summary>
        ///     The latest general availability (GA) build.
        /// </summary>
        Ga,
    }
}
