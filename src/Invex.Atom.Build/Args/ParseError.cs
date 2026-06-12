namespace Invex.Atom.Build.Args;

/// <summary>
///     Represents a single error encountered while parsing command-line arguments.
/// </summary>
/// <param name="Message">A human-readable description of the parse error.</param>
/// <remarks>
///     Instances of this record are created by the <see cref="CommandLineArgsParser" /> and stored on
///     <see cref="CommandLineArgs.Errors" />. Parsing never throws or logs; errors are reported after the
///     application host is fully constructed, so that all output can be masked for secrets.
/// </remarks>
/// <example>
///     For the command <c>atom Bild</c> (a typo of a target named <c>Build</c>), a <see cref="ParseError" /> is
///     created with <c>Message = "Unknown argument 'Bild'"</c> and <c>SimilarCommands</c> containing "Build".
/// </example>
[PublicAPI]
public sealed record ParseError(string Message)
{
    /// <summary>
    ///     Gets the name of the command-line argument that caused the error, if applicable.
    /// </summary>
    /// <remarks>
    ///     For example, if the <c>--project</c> option is missing its value, this property is set to "project".
    ///     For unknown arguments, this is the raw argument as provided on the command line.
    /// </remarks>
    public string? ArgumentName { get; init; }

    /// <summary>
    ///     Gets a list of defined command (target) names that closely match the unrecognized argument.
    /// </summary>
    /// <remarks>
    ///     Populated for unknown-argument errors to help users correct typos; empty for other error kinds.
    /// </remarks>
    public IReadOnlyList<string> SimilarCommands { get; init; } = [];

    /// <summary>
    ///     Gets a list of defined parameter argument names that closely match the unrecognized argument.
    /// </summary>
    /// <remarks>
    ///     Populated for unknown-argument errors to help users correct typos; empty for other error kinds.
    ///     The names are listed without the leading <c>--</c> prefix.
    /// </remarks>
    public IReadOnlyList<string> SimilarParams { get; init; } = [];
}
