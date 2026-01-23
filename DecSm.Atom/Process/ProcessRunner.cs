namespace DecSm.Atom.Process;

/// <summary>
///     Defines a service for executing external processes with comprehensive logging and error handling.
/// </summary>
[PublicAPI]
public interface IProcessRunner
{
    /// <summary>
    ///     Executes an external process synchronously.
    /// </summary>
    /// <param name="options">The configuration options for the process execution.</param>
    /// <returns>A <see cref="ProcessRunResult" /> containing the exit code and captured output.</returns>
    /// <exception cref="StepFailedException">
    ///     Thrown if the process returns a non-zero exit code and <see cref="ProcessRunOptions.AllowFailedResult" /> is
    ///     <c>false</c>.
    /// </exception>
    ProcessRunResult Run(ProcessRunOptions options);

    /// <summary>
    ///     Executes an external process asynchronously.
    /// </summary>
    /// <param name="options">The configuration options for the process execution.</param>
    /// <param name="cancellationToken">A token to cancel the process.</param>
    /// <returns>A task that resolves to a <see cref="ProcessRunResult" /> containing the exit code and captured output.</returns>
    /// <exception cref="StepFailedException">
    ///     Thrown if the process returns a non-zero exit code and <see cref="ProcessRunOptions.AllowFailedResult" /> is
    ///     <c>false</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled.</exception>
    Task<ProcessRunResult> RunAsync(ProcessRunOptions options, CancellationToken cancellationToken = default);
}

/// <summary>
///     Provides a standardized service for executing external processes with comprehensive logging, error handling, and
///     result capture.
/// </summary>
/// <remarks>
///     This class wraps <see cref="System.Diagnostics.Process" /> to provide a more robust and configurable execution
///     model
///     for build automation, including real-time stream capture, flexible error handling, and cancellation support.
/// </remarks>
/// <param name="logger">The logger for capturing process execution information.</param>
[PublicAPI]
public sealed class ProcessRunner(ILogger<ProcessRunner> logger) : IProcessRunner
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

        throw new StepFailedException($"Process {options.Name} {options.Args} failed with exit code {result.ExitCode}");

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

        throw new StepFailedException($"Process {options.Name} {options.Args} failed with exit code {result.ExitCode}");

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
}
