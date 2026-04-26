namespace DecSm.Atom.Process;

/// <summary>
///     Defines a service for executing external processes with comprehensive logging, output
///     capture, and error handling.
/// </summary>
/// <remarks>
///     <para>
///         Both stdout and stderr are captured in real time via async read loops so that long-running
///         processes stream their output to the logger rather than buffering it until exit.
///     </para>
///     <para>
///         This interface is registered as a singleton by <see cref="ProcessHostExtensions" />.
///         Inject it into build targets and helpers instead of using
///         <see cref="System.Diagnostics.Process" /> directly, to benefit from consistent logging
///         and uniform error handling across all process invocations.
///     </para>
/// </remarks>
[PublicAPI]
public interface IProcessRunner
{
    /// <summary>
    ///     Executes an external process synchronously and returns its result.
    /// </summary>
    /// <param name="options">The configuration for the process execution.</param>
    /// <returns>
    ///     A <see cref="ProcessRunResult" /> containing the exit code and captured output.
    ///     Only returned when the exit code is zero, or when
    ///     <see cref="ProcessRunOptions.AllowFailedResult" /> is <c>true</c>.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown when the process exits with a non-zero code and
    ///     <see cref="ProcessRunOptions.AllowFailedResult" /> is <c>false</c>.  The message
    ///     includes the command name, exit code, and any captured stderr.
    /// </exception>
    ProcessRunResult Run(ProcessRunOptions options);

    /// <summary>
    ///     Executes an external process asynchronously and returns its result.
    /// </summary>
    /// <param name="options">The configuration for the process execution.</param>
    /// <param name="cancellationToken">
    ///     A token that cancels the wait for the process to exit.  Note: cancelling the token
    ///     aborts the wait but does <em>not</em> kill the underlying OS process.
    /// </param>
    /// <returns>
    ///     A task that resolves to a <see cref="ProcessRunResult" /> when the process exits.
    ///     Only completes successfully when the exit code is zero, or when
    ///     <see cref="ProcessRunOptions.AllowFailedResult" /> is <c>true</c>.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown (on the returned task) when the process exits with a non-zero code and
    ///     <see cref="ProcessRunOptions.AllowFailedResult" /> is <c>false</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown when <paramref name="cancellationToken" /> is cancelled before the process exits.
    /// </exception>
    Task<ProcessRunResult> RunAsync(ProcessRunOptions options, CancellationToken cancellationToken = default);
}

/// <summary>
///     Default implementation of <see cref="IProcessRunner" /> that wraps
///     <see cref="System.Diagnostics.Process" />.
/// </summary>
/// <remarks>
///     <para>
///         stdout and stderr are both redirected and read asynchronously via
///         <see cref="System.Diagnostics.Process.BeginOutputReadLine" /> /
///         <see cref="System.Diagnostics.Process.BeginErrorReadLine" /> so that output is streamed
///         to the logger in real time rather than buffered.
///     </para>
///     <para>
///         Each received line is passed through <see cref="ProcessRunOptions.TransformOutput" /> or
///         <see cref="ProcessRunOptions.TransformError" /> when set: a <c>null</c> return from the
///         transform suppresses the line from both the log and the captured result.
///     </para>
///     <para>
///         On a non-zero exit code without <see cref="ProcessRunOptions.AllowFailedResult" />,
///         stdout and stderr are promoted to at least <see cref="LogLevel.Information" /> /
///         <see cref="LogLevel.Warning" /> respectively (if they were configured below those
///         thresholds) before a descriptive exception is thrown.
///     </para>
/// </remarks>
/// <param name="logger">The logger for capturing process execution diagnostics.</param>
[PublicAPI]
internal sealed class ProcessRunner(ILogger<ProcessRunner> logger) : IProcessRunner
{
    /// <inheritdoc />
    public ProcessRunResult Run(ProcessRunOptions options)
    {
        switch (options)
        {
            case { WorkingDirectory.Length: > 0, EnvironmentVariables.Count: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} in {WorkingDirectory} with env {EnvironmentVariables}",
                    options.Name,
                    options.Args,
                    options.WorkingDirectory,
                    string.Join(", ", options.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}")));

                break;
            case { WorkingDirectory.Length: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} in {WorkingDirectory}",
                    options.Name,
                    options.Args,
                    options.WorkingDirectory);

                break;
            case { EnvironmentVariables.Count: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} with env {EnvironmentVariables}",
                    options.Name,
                    options.Args,
                    string.Join(", ", options.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}")));

                break;
            default:
                logger.Log(options.InvocationLogLevel, "Run: {Name} {Args}", options.Name, options.Args);

                break;
        }

        using var process = new System.Diagnostics.Process();

        process.StartInfo = new()
        {
            FileName = options.Name,
            Arguments = options.Args,
            WorkingDirectory = options.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var environmentVariable in options.EnvironmentVariables)
            process.StartInfo.Environment[environmentVariable.Key] = environmentVariable.Value;

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += OnProcessOnOutputDataReceived;
        process.ErrorDataReceived += OnProcessOnErrorDataReceived;

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
        finally
        {
            process.OutputDataReceived -= OnProcessOnOutputDataReceived;
            process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
        }

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        var result = new ProcessRunResult(options, process.ExitCode, output, error);

        if (result.ExitCode is 0)
            return result;

        if (options.OutputLogLevel < LogLevel.Information)
            logger.LogInformation("{Output}", output);

        if (options.ErrorLogLevel < LogLevel.Information)
            logger.LogWarning("{Error}", error);

        if (options.AllowFailedResult)
        {
            logger.Log(options.InvocationLogLevel, "Process finished with exit code {ExitCode}", result.ExitCode);

            return result;
        }

        throw new(BuildProcessErrorMessage(options, result.ExitCode, error));

        void OnProcessOnErrorDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            var text = options.TransformError is not null
                ? options.TransformError(e.Data)
                : e.Data;

            if (text is null)
                return;

            errorBuilder.AppendLine(text);
            logger.Log(options.ErrorLogLevel, "{Error}", text);
        }

        void OnProcessOnOutputDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            var text = options.TransformOutput is not null
                ? options.TransformOutput(e.Data)
                : e.Data;

            if (text is null)
                return;

            outputBuilder.AppendLine(text);
            logger.Log(options.OutputLogLevel, "{Output}", text);
        }
    }

    /// <inheritdoc />
    public async Task<ProcessRunResult> RunAsync(
        ProcessRunOptions options,
        CancellationToken cancellationToken = default)
    {
        switch (options)
        {
            case { WorkingDirectory.Length: > 0, EnvironmentVariables.Count: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} in {WorkingDirectory} with env {EnvironmentVariables}",
                    options.Name,
                    options.Args,
                    options.WorkingDirectory,
                    string.Join(", ", options.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}")));

                break;
            case { WorkingDirectory.Length: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} in {WorkingDirectory}",
                    options.Name,
                    options.Args,
                    options.WorkingDirectory);

                break;
            case { EnvironmentVariables.Count: > 0 }:
                logger.Log(options.InvocationLogLevel,
                    "Run: {Name} {Args} with env {EnvironmentVariables}",
                    options.Name,
                    options.Args,
                    string.Join(", ", options.EnvironmentVariables.Select(kv => $"{kv.Key}={kv.Value}")));

                break;
            default:
                logger.Log(options.InvocationLogLevel, "Run: {Name} {Args}", options.Name, options.Args);

                break;
        }

        using var process = new System.Diagnostics.Process();

        process.StartInfo = new()
        {
            FileName = options.Name,
            Arguments = options.Args,
            WorkingDirectory = options.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var environmentVariable in options.EnvironmentVariables)
            process.StartInfo.Environment[environmentVariable.Key] = environmentVariable.Value;

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += OnProcessOnOutputDataReceived;
        process.ErrorDataReceived += OnProcessOnErrorDataReceived;

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cancellationToken);
        }
        finally
        {
            process.OutputDataReceived -= OnProcessOnOutputDataReceived;
            process.ErrorDataReceived -= OnProcessOnErrorDataReceived;
        }

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        var result = new ProcessRunResult(options, process.ExitCode, output, error);

        if (result.ExitCode is 0)
            return result;

        if (options.OutputLogLevel < LogLevel.Information)
            logger.LogInformation("{Output}", output);

        if (options.ErrorLogLevel < LogLevel.Information)
            logger.LogWarning("{Error}", error);

        if (options.AllowFailedResult)
        {
            logger.Log(options.InvocationLogLevel, "Process finished with exit code {ExitCode}", result.ExitCode);

            return result;
        }

        throw new(BuildProcessErrorMessage(options, result.ExitCode, error));

        void OnProcessOnErrorDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            var text = options.TransformError is not null
                ? options.TransformError(e.Data)
                : e.Data;

            if (text is null)
                return;

            errorBuilder.AppendLine(text);
            logger.Log(options.ErrorLogLevel, "{Error}", text);
        }

        void OnProcessOnOutputDataReceived(object _, DataReceivedEventArgs e)
        {
            if (e.Data is null)
                return;

            var text = options.TransformOutput is not null
                ? options.TransformOutput(e.Data)
                : e.Data;

            if (text is null)
                return;

            outputBuilder.AppendLine(text);
            logger.Log(options.OutputLogLevel, "{Output}", text);
        }
    }

    private static string BuildProcessErrorMessage(ProcessRunOptions options, int exitCode, string error)
    {
        var errorMessage = new StringBuilder();
        errorMessage.AppendLine($"Process '{options.Name}' failed with exit code {exitCode}");
        errorMessage.AppendLine($"  Command: {options.Name} {options.Args}");

        if (options.WorkingDirectory is { Length: > 0 })
            errorMessage.AppendLine($"  Working Directory: {options.WorkingDirectory}");

        if (error is { Length: > 0 })
        {
            errorMessage.AppendLine("  Error Output:");
            errorMessage.AppendLine($"    {error}");
        }

        return errorMessage
            .ToString()
            .TrimEnd();
    }
}
