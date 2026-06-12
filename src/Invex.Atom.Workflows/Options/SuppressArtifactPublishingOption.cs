namespace Invex.Atom.Workflows.Options;

/// <summary>
///     A workflow option that suppresses publishing of artifacts produced by a target.
/// </summary>
/// <remarks>
///     Typically created via <c>BuildOptions.Target.SuppressArtifactPublishing</c> or
///     <c>BuildOptions.Target.SetSuppressedArtifactPublishing(bool)</c>.
/// </remarks>
[PublicAPI]
public sealed record SuppressArtifactPublishingOption : ToggleBuildOption;
