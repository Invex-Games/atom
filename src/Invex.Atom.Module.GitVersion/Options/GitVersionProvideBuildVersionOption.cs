namespace Invex.Atom.Module.GitVersion.Options;

/// <summary>
///     A build option that controls whether GitVersion provides the build version.
/// </summary>
/// <remarks>
///     Typically created via <c>BuildOptions.GitVersion.ProvideBuildVersion</c>.
/// </remarks>
[PublicAPI]
public sealed record GitVersionProvideBuildVersionOption : ToggleBuildOption;
