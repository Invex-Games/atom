namespace DecSm.Atom.Exceptions;

/// <summary>
///     Thrown when workflow files are out of date in headless mode.
/// </summary>
/// <remarks>
///     <para>
///         This exception is thrown when the build is running in headless mode (typically in CI/CD environments)
///         and the workflow files are detected to be out of date. In headless mode, the build does not automatically
///         regenerate workflow files to prevent unexpected changes in automated environments.
///     </para>
///     <para>
///         Headless mode is typically enabled with the <c>--headless</c> flag and is used in CI/CD pipelines
///         to ensure reproducibility and prevent unintended modifications to workflow files.
///     </para>
///     <para>
///         To resolve this error, run the build with the <c>--gen</c> flag to regenerate the workflow files,
///         then commit the updated files to your repository.
///     </para>
/// </remarks>
/// <example>
///     <para>Typical CI/CD failure scenario and resolution:</para>
///     <code>
/// // In CI/CD pipeline, the build fails with:
/// // "One or more workflows are out of date. To regenerate workflows, run the build with the --gen flag."
/// 
/// // To fix locally:
/// // 1. Run: dotnet run --project ./Build --gen
/// // 2. Commit the regenerated workflow files
/// // 3. Push to repository
/// 
/// // Example of catching this exception:
/// try
/// {
///     await atomService.RunAsync();
/// }
/// catch (WorkflowOutdatedException ex)
/// {
///     Console.WriteLine($"Workflows need regeneration: {ex.Message}");
///     Console.WriteLine("Run the build with the --gen flag to regenerate workflows.");
///     Environment.ExitCode = 3;
/// }
/// </code>
/// </example>
/// <seealso cref="AtomException" />
[PublicAPI]
[Serializable]
public class WorkflowOutdatedException : AtomException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WorkflowOutdatedException" /> class.
    /// </summary>
    public WorkflowOutdatedException() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="WorkflowOutdatedException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public WorkflowOutdatedException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="WorkflowOutdatedException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or null if no inner exception is
    ///     specified.
    /// </param>
    public WorkflowOutdatedException(string message, Exception innerException) : base(message, innerException) { }
}
