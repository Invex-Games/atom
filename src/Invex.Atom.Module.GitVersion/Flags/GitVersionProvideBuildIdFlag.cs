namespace Invex.Atom.Module.GitVersion.Flags;

/// <summary>
///     A build option that controls whether GitVersion provides the build ID.
/// </summary>
/// <remarks>
///     Typically created via <c>BuildOptions.GitVersion.ProvideBuildId</c>.
/// </remarks>
[PublicAPI]
public sealed record GitVersionProvideBuildIdFlag : ToggleBuildOption;
