namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents an artifact that is consumed by a target.
/// </summary>
/// <param name="TargetName">The name of the target that produced the artifact.</param>
/// <param name="ArtifactName">The name of the artifact being consumed.</param>
/// <param name="BuildSlice">An optional identifier for a specific build variation (e.g., in a matrix build).</param>
[PublicAPI]
public sealed record ConsumedArtifact(string TargetName, string ArtifactName, string? BuildSlice = null);
