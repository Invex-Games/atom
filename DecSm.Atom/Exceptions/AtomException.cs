namespace DecSm.Atom.Exceptions;

/// <summary>
///     Represents the base exception class for all Atom framework-specific exceptions.
/// </summary>
/// <remarks>
///     <para>
///         This is the base class for all exceptions thrown by the Atom build framework.
///         Catching this exception type allows you to handle all Atom-specific errors uniformly,
///         while catching specific derived types allows for more targeted error handling.
///     </para>
///     <para>
///         Exception Hierarchy:
///         <list type="bullet">
///             <item>
///                 <description><see cref="AtomException" /> - Base class for all Atom exceptions</description>
///             </item>
///             <item>
///                 <description>
///                     <see cref="BuildConfigurationException" /> - Build configuration errors (duplicate
///                     targets, circular dependencies, etc.)
///                 </description>
///             </item>
///             <item>
///                 <description><see cref="CommandLineException" /> - Invalid command-line arguments</description>
///             </item>
///             <item>
///                 <description><see cref="WorkflowOutdatedException" /> - Workflow files need regeneration</description>
///             </item>
///             <item>
///                 <description><see cref="StepFailedException" /> - Target execution failures</description>
///             </item>
///         </list>
///     </para>
/// </remarks>
/// <example>
///     <para>Catching all Atom exceptions:</para>
///     <code>
/// try
/// {
///     await atomService.RunAsync();
/// }
/// catch (AtomException ex)
/// {
///     Console.WriteLine($"Atom error: {ex.Message}");
/// }
/// </code>
///     <para>Catching specific exception types:</para>
///     <code>
/// try
/// {
///     await atomService.RunAsync();
/// }
/// catch (BuildConfigurationException ex)
/// {
///     Console.WriteLine($"Configuration error: {ex.Message}");
/// }
/// catch (CommandLineException ex)
/// {
///     Console.WriteLine($"Invalid arguments: {ex.Message}");
/// }
/// catch (AtomException ex)
/// {
///     Console.WriteLine($"Other Atom error: {ex.Message}");
/// }
/// </code>
/// </example>
/// <seealso cref="BuildConfigurationException" />
/// <seealso cref="CommandLineException" />
/// <seealso cref="WorkflowOutdatedException" />
/// <seealso cref="StepFailedException" />
[PublicAPI]
[Serializable]
public class AtomException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AtomException" /> class.
    /// </summary>
    public AtomException() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AtomException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AtomException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AtomException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or null if no inner exception is
    ///     specified.
    /// </param>
    public AtomException(string message, Exception innerException) : base(message, innerException) { }
}
