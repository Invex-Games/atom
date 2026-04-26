namespace DecSm.Atom.Process;

/// <summary>
///     Immutable configuration for a single external-process execution through
///     <see cref="IProcessRunner" />.
/// </summary>
/// <remarks>
///     <para>
///         All properties have defaults suited to typical build-automation use: invocation is
///         logged at <see cref="LogLevel.Information" />, stdout at <see cref="LogLevel.Debug" />,
///         and stderr at <see cref="LogLevel.Warning" />.  A non-zero exit code throws by default
///         so build steps fail fast without requiring callers to check exit codes manually.
///     </para>
///     <para>
///         Both stdout and stderr are fully captured into <see cref="ProcessRunResult" /> regardless
///         of log level, so callers can inspect the full output even when logs are suppressed.
///     </para>
/// </remarks>
/// <param name="Name">The name or path of the executable to run (e.g. <c>"dotnet"</c>, <c>"git"</c>).</param>
/// <param name="Args">The command-line arguments to pass to the executable as a pre-joined string.</param>
[PublicAPI]
public sealed record ProcessRunOptions(string Name, string Args)
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ProcessRunOptions" /> from an array of argument
    ///     tokens, joining them with a single space.
    /// </summary>
    /// <param name="Name">The name or path of the executable to run.</param>
    /// <param name="Args">
    ///     An array of argument tokens.  Empty and whitespace-only entries are removed before
    ///     joining so that optional arguments can be passed as <see cref="string.Empty" /> without
    ///     creating spurious spaces in the final argument string.
    /// </param>
    public ProcessRunOptions(string Name, string[] Args) : this(Name,
        string.Join(" ", Args.Where(a => !string.IsNullOrWhiteSpace(a)))) { }

    /// <summary>
    ///     Gets the working directory for the process execution.
    /// </summary>
    /// <remarks>
    ///     When <c>null</c> (the default), the process inherits the current application's working
    ///     directory.  Set this when a tool must be run from a specific project or repository root.
    /// </remarks>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    ///     Gets the log level used when logging the process invocation line
    ///     (e.g. <c>Run: dotnet build --configuration Release</c>).
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="LogLevel.Information" />.  Lower this to
    ///     <see cref="LogLevel.Debug" /> for noisy or frequently-called sub-processes.
    /// </remarks>
    public LogLevel InvocationLogLevel { get; init; } = LogLevel.Information;

    /// <summary>
    ///     Gets the log level used when streaming each line of the process's standard output.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="LogLevel.Debug" />.  The full stdout is still captured into
    ///     <see cref="ProcessRunResult.Output" /> regardless of this setting, so callers can
    ///     inspect it even when debug logging is disabled.
    ///     Set to <see cref="LogLevel.None" /> to suppress stdout entirely from the log.
    /// </remarks>
    public LogLevel OutputLogLevel { get; init; } = LogLevel.Debug;

    /// <summary>
    ///     Gets the log level used when streaming each line of the process's standard error.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="LogLevel.Warning" />.  Like <see cref="OutputLogLevel" />, the
    ///     full stderr is captured into <see cref="ProcessRunResult.Error" /> irrespective of this
    ///     value.  On a non-zero exit, stderr is additionally promoted to
    ///     <see cref="LogLevel.Information" /> or higher when <see cref="ErrorLogLevel" /> is
    ///     below that threshold.
    /// </remarks>
    public LogLevel ErrorLogLevel { get; init; } = LogLevel.Warning;

    /// <summary>
    ///     Gets a value indicating whether a non-zero exit code should be tolerated rather than
    ///     throwing an exception.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When <c>false</c> (the default), <see cref="IProcessRunner.Run" /> and
    ///         <see cref="IProcessRunner.RunAsync" /> throw if the exit code is non-zero, which
    ///         causes the enclosing build target to fail immediately.
    ///     </para>
    ///     <para>
    ///         Set to <c>true</c> when the caller needs to inspect the result regardless of success
    ///         — for example, running a linter and deciding separately whether to fail the build.
    ///         The caller must then check <see cref="ProcessRunResult.ExitCode" /> explicitly.
    ///     </para>
    /// </remarks>
    public bool AllowFailedResult { get; init; }

    /// <summary>
    ///     Gets the environment variables to inject into the process's environment.
    /// </summary>
    /// <remarks>
    ///     Keys are variable names and values are their string values.  A <c>null</c> value removes
    ///     the inherited variable from the child process's environment.  Variables not listed here
    ///     are inherited from the current process unchanged.
    /// </remarks>
    public Dictionary<string, string?> EnvironmentVariables { get; init; } = [];

    /// <summary>
    ///     Gets an optional per-line transformation applied to each line of the process's standard
    ///     output before it is logged and captured.
    /// </summary>
    /// <remarks>
    ///     The delegate receives a raw non-null line (null data events are filtered before the
    ///     delegate is called).  Returning <c>null</c> suppresses the line entirely — it is neither
    ///     logged nor included in <see cref="ProcessRunResult.Output" />.  This is useful for
    ///     filtering noisy progress lines or redacting secrets from captured output.
    /// </remarks>
    public Func<string, string?>? TransformOutput { get; init; }

    /// <summary>
    ///     Gets an optional per-line transformation applied to each line of the process's standard
    ///     error before it is logged and captured.
    /// </summary>
    /// <remarks>
    ///     Behaves identically to <see cref="TransformOutput" /> but operates on stderr.  Returning
    ///     <c>null</c> suppresses the line from both the log and <see cref="ProcessRunResult.Error" />.
    /// </remarks>
    public Func<string, string?>? TransformError { get; init; }
}
