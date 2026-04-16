namespace DecSm.Atom;

/// <summary>
///     Represents an exception thrown when a step within an Atom build target fails.
///     This exception provides additional reporting capabilities through custom report data.
/// </summary>
/// <remarks>
///     <para>
///     This exception is used throughout the Atom framework to signal failures in build targets,
///     such as task execution errors, failed external process calls (via <see cref="IProcessRunner" />),
///     or unmet validation criteria within a target's logic.
///     Consumers of the Atom framework can also throw <c>StepFailedException</c> to programmatically halt a build target
///     and optionally provide custom report data using the <see cref="ReportData" /> property.
///     The <see cref="ReportData" /> property allows attaching custom reporting information that can be
///     used for enhanced error reporting, logging, or debugging purposes by Atom's reporting services.
///     </para>
///     <para>
///     This exception inherits from <see cref="AtomException"/>. Use <see cref="BuildConfigurationException"/> for
///     configuration errors (duplicate targets, circular dependencies); use <see cref="StepFailedException"/> for
///     runtime target execution failures.
///     </para>
/// </remarks>
/// <example>
///     <code>
///     throw new StepFailedException("Could not validate a value");
///     </code>
///     <code>
///     try
///     {
///         ...
///     }
///     catch (Exception ex)
///     {
///         // It's not recommended to throw a new exception in a catch block without
///         // including the original exception.
///         throw new StepFailedException("An error occurred during the step execution", ex)
///         {
///             ReportData = new TextReportData(...);
///         };
///     }
///     </code>
/// </example>
/// <seealso cref="Exceptions.AtomException"/>
/// <seealso cref="Exceptions.BuildConfigurationException"/>
[PublicAPI]
public sealed class StepFailedException(string message, Exception? innerException = null)
    : AtomException(message, innerException!)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StepFailedException" /> class with an empty message.
    /// </summary>
    /// <remarks>
    ///     Using this constructor is generally not recommended. It is preferable to provide a descriptive message
    ///     and/or an inner exception to aid in diagnosing the failure.
    /// </remarks>
    public StepFailedException() : this(string.Empty) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StepFailedException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error. If null, an empty string will be used.</param>
    public StepFailedException(string? message) : this(message ?? string.Empty, null) { }

    /// <summary>
    ///     Gets custom report data associated with this step failure.
    ///     This property can be used to attach additional context, diagnostic information,
    ///     or custom reporting data that helps with error analysis and reporting within the Atom framework.
    /// </summary>
    /// <value>
    ///     An <see cref="ICustomReportData" /> instance containing custom report information,
    ///     or <c>null</c> if no custom report data is associated with this exception.
    /// </value>
    /// <remarks>
    ///     This property can only be set during object initialization (init-only property).
    ///     It's designed to provide extensible reporting capabilities for different types of step failures
    ///     that can be processed by Atom's outcome reporters.
    /// </remarks>
    /// <seealso cref="ListReportData" />
    /// <seealso cref="TableReportData" />
    /// <seealso cref="TextReportData" />
    public ICustomReportData? ReportData { get; init; }
}
