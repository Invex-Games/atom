namespace DecSm.Atom.Build.Exceptions;

/// <summary>
///     Thrown when the build configuration contains errors such as duplicate targets, missing dependencies, or circular
///     dependencies.
/// </summary>
/// <remarks>
///     <para>
///         This exception is thrown during build model resolution when the <see cref="Build.BuildResolver" /> detects
///         configuration errors that prevent the build from proceeding. Common scenarios include:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>Duplicate target names defined in the build</description>
///         </item>
///         <item>
///             <description>A target depends on a non-existent target</description>
///         </item>
///         <item>
///             <description>Circular dependencies between targets</description>
///         </item>
///     </list>
///     <para>
///         The exception may include a <see cref="ReportData" /> property with enhanced error details
///         that can be rendered for better visualization of the configuration problem.
///     </para>
/// </remarks>
/// <example>
///     <para>Example 1: Duplicate target error with ListReportData</para>
///     <code>
/// throw new BuildConfigurationException("One or more targets are defined multiple times.")
/// {
///     ReportData = new ListReportData(new[] { "Target 'Build' is defined multiple times" })
///     {
///         Title = "Duplicate Targets"
///     }
/// };
/// </code>
///     <para>Example 2: Circular dependency error with TextReportData</para>
///     <code>
/// throw new BuildConfigurationException("Circular dependency detected: Build -> Test -> Build")
/// {
///     ReportData = new TextReportData("Dependency cycle:\n  Build -> Test -> Build")
///     {
///         Title = "Circular Dependency Detected"
///     }
/// };
/// </code>
///     <para>Example 3: Missing dependency error</para>
///     <code>
/// throw new BuildConfigurationException("Target 'Build' depends on target 'Compile' which does not exist.");
/// </code>
///     <para>Example 4: How to catch and handle</para>
///     <code>
/// try
/// {
///     var buildModel = buildResolver.Resolve();
/// }
/// catch (BuildConfigurationException ex)
/// {
///     Console.WriteLine($"Configuration error: {ex.Message}");
///     if (ex.ReportData is not null)
///     {
///         reportService.AddReportData(ex.ReportData);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="AtomException" />
/// <seealso cref="Build.BuildResolver" />
[PublicAPI]
[Serializable]
public class BuildConfigurationException : AtomException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BuildConfigurationException" /> class.
    /// </summary>
    public BuildConfigurationException() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BuildConfigurationException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public BuildConfigurationException(string message) : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BuildConfigurationException" /> class with a specified error message
    ///     and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or null if no inner exception is
    ///     specified.
    /// </param>
    public BuildConfigurationException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    ///     Gets optional enhanced error details that can be rendered to provide better visualization of the configuration
    ///     problem.
    /// </summary>
    /// <remarks>
    ///     This property can contain <see cref="ListReportData" /> for listing multiple issues,
    ///     or <see cref="TextReportData" /> for formatted text output, or any other <see cref="ICustomReportData" />
    ///     implementation.
    /// </remarks>
    public ICustomReportData? ReportData { get; init; }
}
