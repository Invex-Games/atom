namespace Invex.Atom.Build.Exceptions;

/// <summary>
///     Thrown when command-line arguments are invalid or malformed.
/// </summary>
/// <remarks>
///     <para>
///         This exception signals that build execution cannot proceed because of a problem with the
///         command-line input. Note that <see cref="Args.CommandLineArgsParser" /> itself does not throw;
///         it collects parse problems as <see cref="Args.ParseError" /> objects on
///         <see cref="Args.CommandLineArgs.Errors" />, which are reported once the application host is
///         fully constructed. This exception is reserved for argument-related failures detected later,
///         such as during parameter resolution.
///     </para>
///     <para>
///         The <see cref="ArgumentName" /> property, when set, identifies the specific command-line argument
///         that caused the error, making it easier to diagnose and fix the issue.
///     </para>
///     <para>
///         For help with command-line usage, run the build with the <c>--help</c> flag.
///     </para>
/// </remarks>
/// <example>
///     <para>Example 1: Throwing for an argument-related failure</para>
///     <code>
/// throw new CommandLineException("Parameter 'output' could not be resolved to a valid path.")
/// {
///     ArgumentName = "output"
/// };
/// </code>
///     <para>Example 2: How to catch and provide user guidance</para>
///     <code>
/// try
/// {
///     await executor.Execute(cancellationToken);
/// }
/// catch (CommandLineException ex)
/// {
///     Console.WriteLine($"Invalid arguments: {ex.Message}");
///     if (ex.ArgumentName is not null)
///     {
///         Console.WriteLine($"Problematic argument: {ex.ArgumentName}");
///     }
///     Console.WriteLine("Run with --help for usage information.");
///     Environment.ExitCode = 2;
/// }
/// </code>
/// </example>
/// <seealso cref="AtomException" />
/// <seealso cref="Args.CommandLineArgs" />
/// <seealso cref="Args.ParseError" />
[PublicAPI]
[Serializable]
public class CommandLineException : AtomException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLineException" /> class.
    /// </summary>
    public CommandLineException() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLineException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CommandLineException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandLineException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or null if no inner exception is
    ///     specified.
    /// </param>
    public CommandLineException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    ///     Gets the name of the command-line argument that caused the error, if applicable.
    /// </summary>
    /// <remarks>
    ///     This property helps identify which specific argument was problematic, making it easier to diagnose
    ///     and fix command-line issues. For example, if the <c>--project</c> option is missing its value,
    ///     this property would be set to "project".
    /// </remarks>
    public string? ArgumentName { get; init; }
}
