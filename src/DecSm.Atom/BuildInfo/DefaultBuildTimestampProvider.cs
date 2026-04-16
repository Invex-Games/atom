namespace DecSm.Atom.BuildInfo;

/// <summary>
///     The default provider for determining the build timestamp, used when no custom implementation is registered.
/// </summary>
/// <remarks>
///     This provider generates a timestamp based on the current UTC time upon first access and caches it for consistency
///     within a single process. It uses the injected <see cref="TimeProvider" /> to facilitate testing.
/// </remarks>
/// <param name="timeProvider">The time provider used to get the current UTC time.</param>
internal sealed class DefaultBuildTimestampProvider(TimeProvider timeProvider) : IBuildTimestampProvider
{
    /// <summary>
    ///     Holds the cached build timestamp after the first access.
    /// </summary>
    private long? _buildTimestamp;

    /// <summary>
    ///     Gets the build timestamp as the number of seconds since the Unix epoch (UTC).
    /// </summary>
    /// <remarks>
    ///     The timestamp is generated upon first access and then cached to ensure it remains consistent within the same
    ///     process.
    ///     Note that this value is not shared across different processes or workflow jobs. For a globally consistent
    ///     timestamp,
    ///     it is recommended to use <see cref="ISetupBuildInfo" />.
    /// </remarks>
    public long Timestamp =>
        _buildTimestamp ??= timeProvider
            .GetUtcNow()
            .ToUnixTimeSeconds();
}
