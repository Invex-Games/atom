namespace DecSm.Atom.Process;

/// <summary>
///     Represents the result of a completed external-process execution.
/// </summary>
/// <remarks>
///     <para>
///         Both <see cref="Output" /> and <see cref="Error" /> are always populated with whatever
///         the process wrote, regardless of the log levels configured in <see cref="RunOptions" />.
///         This means callers can inspect the full output in error-handling logic even when debug
///         logging was disabled during the run.
///     </para>
///     <para>
///         For processes run with <see cref="ProcessRunOptions.AllowFailedResult" /> set to
///         <c>false</c> (the default), this record is only returned when <see cref="ExitCode" />
///         is zero — otherwise <see cref="IProcessRunner" /> throws before returning.
///     </para>
/// </remarks>
/// <param name="RunOptions">The options that were used to run the process.</param>
/// <param name="ExitCode">The exit code returned by the process upon termination.</param>
/// <param name="Output">The complete standard output (stdout) captured from the process, with lines joined by newlines.</param>
/// <param name="Error">The complete standard error (stderr) captured from the process, with lines joined by newlines.</param>
[PublicAPI]
public sealed record ProcessRunResult(ProcessRunOptions RunOptions, int ExitCode, string Output, string Error);
