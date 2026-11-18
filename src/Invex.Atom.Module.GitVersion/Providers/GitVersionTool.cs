namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Identifies the GitVersion .NET tool package, and the version of it, that the GitVersion providers install
///     and run.
/// </summary>
/// <remarks>
///     The version is pinned because GitVersion's command-line arguments and JSON output change between major
///     versions; running whatever version happens to be latest can break every build that uses this module.
/// </remarks>
internal static class GitVersionTool
{
    /// <summary>
    ///     The NuGet package ID of the GitVersion .NET tool.
    /// </summary>
    internal const string PackageId = "GitVersion.Tool";

    /// <summary>
    ///     The exact version of <see cref="PackageId" /> that the providers install and run.
    /// </summary>
    internal const string Version = "6.8.2";
}
