namespace DecSm.Atom.Process;

/// <summary>
///     Represents the result of an external process execution, including its exit code and captured output.
/// </summary>
/// <param name="RunOptions">The options that were used to run the process.</param>
/// <param name="ExitCode">The exit code returned by the process.</param>
/// <param name="Output">The standard output (stdout) captured from the process.</param>
/// <param name="Error">The standard error (stderr) captured from the process.</param>
[PublicAPI]
public sealed record ProcessRunResult(ProcessRunOptions RunOptions, int ExitCode, string Output, string Error);
