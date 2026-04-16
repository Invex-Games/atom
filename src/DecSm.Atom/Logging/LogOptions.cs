namespace DecSm.Atom.Logging;

/// <summary>
///     Provides static options for controlling logging behavior across the application.
/// </summary>
/// <remarks>
///     This class allows for dynamic adjustment of logging settings, such as enabling verbose output.
///     Changes to these options take effect immediately.
/// </remarks>
[PublicAPI]
public static class LogOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether verbose logging is enabled.
    /// </summary>
    /// <remarks>
    ///     When set to <c>true</c>, the application should produce more detailed diagnostic information.
    ///     This is typically controlled by the <c>--verbose</c> command-line flag.
    /// </remarks>
    public static bool IsVerboseEnabled { get; set; }
}
