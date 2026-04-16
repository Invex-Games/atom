namespace DecSm.Atom.Module.Dotnet.Cli;

/// <summary>
///     Specifies the verbosity level for .NET CLI commands.
/// </summary>
[PublicAPI]
public enum VerbosityOptions
{
    /// <summary>
    ///     Suppresses all output except for errors and warnings.
    /// </summary>
    Quiet,

    /// <summary>
    ///     Shows minimal output, typically just progress and summary information.
    /// </summary>
    Minimal,

    /// <summary>
    ///     Shows normal output, which is the default for most commands.
    /// </summary>
    Normal,

    /// <summary>
    ///     Shows detailed output, including more diagnostic information.
    /// </summary>
    Detailed,

    /// <summary>
    ///     Shows diagnostic output, which is the most verbose level, including all internal logging.
    /// </summary>
    Diagnostic,
}

/// <summary>
///     Provides extension methods for <see cref="VerbosityOptions" /> to convert them to their string representations.
/// </summary>
[PublicAPI]
public static class VerbosityOptionsExtensions
{
    /// <summary>
    ///     Extension methods for <see cref="VerbosityOptions" />.
    /// </summary>
    extension(VerbosityOptions verbosityOptions)
    {
        /// <summary>
        ///     Converts the <see cref="VerbosityOptions" /> enum value to its corresponding .NET CLI string argument.
        /// </summary>
        /// <returns>The string representation of the verbosity option (e.g., "quiet", "minimal").</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the verbosity option is not a valid value.</exception>
        [PublicAPI]
        public string ToString() =>
            verbosityOptions switch
            {
                VerbosityOptions.Quiet => "quiet",
                VerbosityOptions.Minimal => "minimal",
                VerbosityOptions.Normal => "normal",
                VerbosityOptions.Detailed => "detailed",
                VerbosityOptions.Diagnostic => "diagnostic",
                _ => throw new ArgumentOutOfRangeException(nameof(verbosityOptions), verbosityOptions, null),
            };
    }
}
