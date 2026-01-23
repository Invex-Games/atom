namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Defines a provider for generating a unique build identifier.
/// </summary>
[PublicAPI]
public interface IBuildIdProvider
{
    /// <summary>
    ///     Gets the unique identifier for the current build.
    /// </summary>
    /// <remarks>
    ///     This ID should be consistent within a single build run but unique across different builds.
    ///     If a build spans multiple jobs, this ID should be passed between them to ensure consistency.
    /// </remarks>
    string BuildId { get; }

    /// <summary>
    ///     Returns an optional grouping key for a given build ID, which can be used to organize artifacts or logs.
    /// </summary>
    /// <param name="buildId">The build ID to generate a group for.</param>
    /// <returns>A string representing the group, or <c>null</c> if no grouping is implemented.</returns>
    /// <example>
    ///     A date-based build ID like "2024-01-15-..." could be grouped by month, returning "2024-01".
    /// </example>
    string? GetBuildIdGroup(string buildId) =>
        null;
}
