namespace DecSm.Atom.BuildInfo;

/// <summary>
///     Defines a provider for determining the build timestamp.
/// </summary>
[PublicAPI]
public interface IBuildTimestampProvider
{
    /// <summary>
    ///     Gets the build timestamp as the number of seconds since the Unix epoch (UTC).
    /// </summary>
    /// <remarks>
    ///     This value should remain consistent throughout the build lifecycle.
    /// </remarks>
    long Timestamp { get; }
}
