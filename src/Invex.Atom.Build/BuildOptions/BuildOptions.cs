namespace Invex.Atom.Build.BuildOptions;

/// <summary>
///     Serves as an extension anchor for fluent build option factories.
/// </summary>
/// <remarks>
///     Modules attach extension members to this class to expose option factories under a single
///     discoverable entry point (e.g., <c>BuildOptions.Inject</c>, <c>BuildOptions.Artifacts</c>,
///     and <c>BuildOptions.Steps</c> from the workflows package).
/// </remarks>
[PublicAPI]
public static class BuildOptions;
