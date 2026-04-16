namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents an artifact that is produced by a target.
/// </summary>
/// <param name="ArtifactName">The name of the artifact being produced.</param>
/// <param name="BuildSlice">An optional identifier for a specific build variation (e.g., in a matrix build).</param>
[PublicAPI]
public sealed record ProducedArtifact(string ArtifactName, string? BuildSlice = null);
