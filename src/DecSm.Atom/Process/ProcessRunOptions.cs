namespace DecSm.Atom.Process;

/// <summary>
///     Configuration options for executing external processes through the <see cref="IProcessRunner" /> service.
/// </summary>
/// <remarks>
///     This record provides a comprehensive set of options for controlling how external processes are executed,
///     including logging behavior, error handling, and execution context. It supports both string-based and
///     array-based argument specification.
///     <para>
///         Default values are configured for common build automation scenarios:
///         <list type="bullet">
///             <item>Process invocation logged at <see cref="LogLevel.Information" />.</item>
///             <item>Standard output logged at <see cref="LogLevel.Debug" />.</item>
///             <item>Error output logged at <see cref="LogLevel.Warning" />.</item>
///             <item>Failed processes throw <see cref="StepFailedException" /> by default.</item>
///         </list>
///     </para>
/// </remarks>
/// <param name="Name">The name or path of the executable to run (e.g., "dotnet", "git").</param>
/// <param name="Args">The command-line arguments to pass to the executable as a single string.</param>
[PublicAPI]
public sealed record ProcessRunOptions(string Name, string Args)
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ProcessRunOptions" /> with the specified executable name and an array of
    ///     arguments.
    /// </summary>
    /// <param name="Name">The name or path of the executable to run.</param>
    /// <param name="Args">An array of command-line arguments, which will be joined into a single string with spaces.</param>
    public ProcessRunOptions(string Name, string[] Args) : this(Name,
        string.Join(" ", Args.Where(a => !string.IsNullOrWhiteSpace(a)))) { }

    /// <summary>
    ///     Gets or sets the working directory for the process execution.
    /// </summary>
    /// <remarks>
    ///     If <c>null</c>, the current application's working directory is used. Relative paths are resolved
    ///     relative to the current working directory. The directory must exist when the process starts.
    /// </remarks>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    ///     Gets or sets the log level for messages indicating the process invocation.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="LogLevel.Information" />.
    /// </remarks>
    public LogLevel InvocationLogLevel { get; init; } = LogLevel.Information;

    /// <summary>
    ///     Gets or sets the log level for standard output messages from the executed process.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="LogLevel.Debug" />.
    /// </remarks>
    public LogLevel OutputLogLevel { get; init; } = LogLevel.Debug;

    /// <summary>
    ///     Gets or sets the log level for standard error messages from the executed process.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="LogLevel.Warning" />.
    /// </remarks>
    public LogLevel ErrorLogLevel { get; init; } = LogLevel.Warning;

    /// <summary>
    ///     Gets or sets a value indicating whether the process execution should tolerate non-zero exit codes.
    /// </summary>
    /// <remarks>
    ///     If <c>false</c> (default), a non-zero exit code will cause a <see cref="StepFailedException" /> to be thrown.
    ///     If <c>true</c>, the process completes normally, and the caller must check <see cref="ProcessRunResult.ExitCode" />.
    /// </remarks>
    public bool AllowFailedResult { get; init; }

    /// <summary>
    ///     Gets or sets a dictionary of environment variables to be used by the process.
    /// </summary>
    /// <remarks>
    ///     Keys are variable names, and values are their corresponding values. A <c>null</c> value removes the variable.
    ///     If not set, the process inherits the current environment's variables.
    /// </remarks>
    public Dictionary<string, string?> EnvironmentVariables { get; init; } = [];

    /// <summary>
    ///     An optional per-line transformation applied to the standard output (stdout) of the executed process.
    /// </summary>
    /// <remarks>
    ///     The delegate receives a raw line of text. Returning <c>null</c> suppresses the line from being logged or captured.
    ///     This is useful for filtering, redacting, or normalizing output.
    /// </remarks>
    public Func<string, string?>? TransformOutput { get; init; }

    /// <summary>
    ///     An optional per-line transformation applied to the standard error (stderr) of the executed process.
    /// </summary>
    /// <remarks>
    ///     Similar to <see cref="TransformOutput" />, this can be used to filter, redact, or normalize error messages.
    /// </remarks>
    public Func<string, string?>? TransformError { get; init; }
}
