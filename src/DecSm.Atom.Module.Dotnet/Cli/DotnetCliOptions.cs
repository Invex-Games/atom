namespace DecSm.Atom.Module.Dotnet.Cli;

/// <summary>
///     Represents a set of options for configuring the execution of .NET CLI commands.
/// </summary>
/// <remarks>
///     This record provides properties to control various aspects of how a .NET CLI process is run,
///     including working directory, logging levels, error handling, environment variables, and output transformations.
///     It is designed to be similar to <see cref="ProcessRunOptions" /> but specifically tailored for .NET CLI operations.
/// </remarks>
[PublicAPI]
public sealed record DotnetCliOptions
{
    /// <summary>
    ///     Gets or sets the working directory for the .NET CLI command execution.
    /// </summary>
    /// <value>
    ///     The path to the directory where the command should be executed, or <c>null</c> to use
    ///     the current application's working directory. Relative paths are resolved relative to
    ///     the current working directory.
    /// </value>
    /// <remarks>
    ///     When specified, this directory becomes the current working directory for the child process,
    ///     affecting relative path resolution and file operations within the executed command.
    ///     The directory must exist when the process is started, or the execution will fail.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new DotnetCliOptions { WorkingDirectory = "./src/MyProject" };
    /// </code>
    /// </example>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    ///     Gets or sets the log level for messages indicating the invocation of the .NET CLI command.
    /// </summary>
    /// <value>
    ///     The <see cref="LogLevel" /> used when logging the command start information,
    ///     including the executable name, arguments, and working directory. Defaults to <see cref="LogLevel.Information" />.
    /// </value>
    /// <remarks>
    ///     This property controls the visibility of command invocation logging. Setting this to a higher level
    ///     (e.g., <see cref="LogLevel.Debug" />) reduces log verbosity in production scenarios, while lower levels
    ///     (e.g., <see cref="LogLevel.Trace" />) provide more detailed execution tracking.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new DotnetCliOptions { InvocationLogLevel = LogLevel.Debug };
    /// </code>
    /// </example>
    public LogLevel InvocationLogLevel { get; init; } = LogLevel.Information;

    /// <summary>
    ///     Gets or sets the log level for standard output messages from the executed .NET CLI command.
    /// </summary>
    /// <value>
    ///     The <see cref="LogLevel" /> used when logging each line of standard output from the command.
    ///     Defaults to <see cref="LogLevel.Debug" />.
    /// </value>
    /// <remarks>
    ///     This property controls how standard output from the .NET CLI command is logged. Debug level is
    ///     the default as it provides detailed execution information without overwhelming production logs.
    ///     For commands that produce important output that should always be visible, consider using
    ///     <see cref="LogLevel.Information" /> or higher.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new DotnetCliOptions { OutputLogLevel = LogLevel.Information };
    /// </code>
    /// </example>
    public LogLevel OutputLogLevel { get; init; } = LogLevel.Debug;

    /// <summary>
    ///     Gets or sets the log level for standard error messages from the executed .NET CLI command.
    /// </summary>
    /// <value>
    ///     The <see cref="LogLevel" /> used when logging each line of standard error from the command.
    ///     Defaults to <see cref="LogLevel.Warning" />.
    /// </value>
    /// <remarks>
    ///     This property controls how error output from the .NET CLI command is logged. Warning level is
    ///     the default as error output typically indicates issues that should be visible in production logs.
    ///     For commands that use stderr for non-error information (like progress updates), consider
    ///     using <see cref="LogLevel.Debug" /> or <see cref="LogLevel.Information" />.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new DotnetCliOptions { ErrorLogLevel = LogLevel.Error };
    /// </code>
    /// </example>
    public LogLevel ErrorLogLevel { get; init; } = LogLevel.Warning;

    /// <summary>
    ///     Gets or sets a value indicating whether the .NET CLI command execution should tolerate non-zero exit codes.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the command execution should return normally even when the exit code is non-zero;
    ///     <c>false</c> if a <see cref="StepFailedException" /> should be thrown for non-zero exit codes.
    ///     Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         When set to <c>false</c> (the default), any non-zero exit code from the executed command
    ///         will cause the <see cref="IProcessRunner" /> to throw a <see cref="StepFailedException" />,
    ///         which is appropriate for build automation scenarios where command failures should halt execution.
    ///     </para>
    ///     <para>
    ///         When set to <c>true</c>, the command execution completes normally regardless of the exit code,
    ///         allowing the caller to inspect the <see cref="ProcessRunResult.ExitCode" /> and handle failures
    ///         appropriately. This is useful for optional operations or when the exit code carries semantic
    ///         meaning beyond success/failure.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // Strict mode - throw on any failure (default)
    /// var strictOptions = new DotnetCliOptions();
    /// // Tolerant mode - handle failures manually
    /// var tolerantOptions = new DotnetCliOptions { AllowFailedResult = true };
    /// var result = await DotnetCli.RunAsync("test", tolerantOptions);
    /// if (result.ExitCode != 0)
    /// {
    ///     // Handle the test failure case appropriately
    ///     logger.LogWarning("dotnet test failed: {Error}", result.Error);
    /// }
    /// </code>
    /// </example>
    public bool AllowFailedResult { get; init; }

    /// <summary>
    ///     Gets or sets the environment variables to be used by the .NET CLI process during execution.
    /// </summary>
    /// <value>
    ///     A dictionary where keys represent the names of the environment variables and values represent their corresponding
    ///     values.
    ///     A value of <c>null</c> for any variable indicates that it should be removed from the environment.
    /// </value>
    /// <remarks>
    ///     These environment variables are passed to the process when it is executed, potentially overriding inherited
    ///     variables from the current environment. If not set, the process will inherit the current environment's variables by
    ///     default.
    /// </remarks>
    public Dictionary<string, string?> EnvironmentVariables { get; init; } = [];

    /// <summary>
    ///     An optional per-line transformation applied to the standard output (stdout) of the executed .NET CLI command.
    /// </summary>
    /// <value>
    ///     A delegate that receives the raw line of text read from stdout (without a trailing newline) and
    ///     returns the transformed text to be logged and captured. Returning <c>null</c> suppresses the line
    ///     completely (it will neither be logged nor stored in <see cref="ProcessRunResult.Output" />).
    ///     If not set, the original text is used as-is.
    /// </value>
    /// <remarks>
    ///     The transformation is invoked once for each line of stdout as it is produced by the command.
    ///     Use this to filter noisy lines, redact sensitive data (like tokens), or normalize output
    ///     (e.g., remove ANSI color codes). Keep the transformation fast and side-effect free; it may be
    ///     called frequently for verbose commands.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new DotnetCliOptions
    /// {
    ///     // Hide lines that start with "info :" from dotnet build output
    ///     TransformOutput = line => line.StartsWith("info :", StringComparison.OrdinalIgnoreCase) ? null : line
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="TransformError" />
    public Func<string, string?>? TransformOutput { get; init; }

    /// <summary>
    ///     An optional per-line transformation applied to the standard error (stderr) of the executed .NET CLI command.
    /// </summary>
    /// <value>
    ///     A delegate that receives the raw line of text read from stderr (without a trailing newline) and
    ///     returns the transformed text to be logged and captured. Returning <c>null</c> suppresses the line
    ///     completely (it will neither be logged nor stored in <see cref="ProcessRunResult.Error" />).
    ///     If not set, the original text is used as-is.
    /// </value>
    /// <remarks>
    ///     Some .NET CLI commands might write progress or informational messages to stderr. Use this transformation
    ///     to downsample, redact, or normalize such messages. As with <see cref="TransformOutput" />, keep the delegate
    ///     efficient to avoid affecting command throughput.
    /// </remarks>
    /// <example>
    ///     <code>
    /// var options = new DotnetCliOptions
    /// {
    ///     // Strip ANSI escape sequences from dotnet run stderr
    ///     TransformError = line => System.Text.RegularExpressions.Regex.Replace(line, "\u001B\\[[0-9;]*[A-Za-z]", string.Empty)
    /// };
    /// </code>
    /// </example>
    /// <seealso cref="TransformOutput" />
    public Func<string, string?>? TransformError { get; init; }
}
