namespace DecSm.Atom.BuildInfo;

/// <summary>
///     The default provider for generating a build ID, used when no custom implementation is registered.
/// </summary>
/// <param name="buildVersionProvider">The provider for retrieving the build version.</param>
/// <param name="buildTimestampProvider">The provider for retrieving the build timestamp.</param>
internal sealed class DefaultBuildIdProvider(
    IBuildVersionProvider buildVersionProvider,
    IBuildTimestampProvider buildTimestampProvider
) : IBuildIdProvider
{
    /// <summary>
    ///     Gets the build ID by combining the build version and timestamp.
    /// </summary>
    /// <example>A typical build ID might look like: "1.2.3-1672531200".</example>
    public string BuildId => $"{buildVersionProvider.Version}-{buildTimestampProvider.Timestamp}";

    /// <summary>
    ///     Returns null, as the default provider does not implement build ID grouping.
    /// </summary>
    /// <param name="buildId">The build ID (ignored).</param>
    /// <returns><c>null</c>.</returns>
    public string? GetBuildIdGroup(string buildId) =>
        null;
}
