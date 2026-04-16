namespace DecSm.Atom.Variables;

/// <summary>
///     Defines a provider for reading and writing workflow variables to a specific storage backend.
/// </summary>
/// <remarks>
///     <para>
///         This interface is part of Atom's extensible variable management system. Multiple providers can be registered,
///         and the <see cref="IWorkflowVariableService" /> will delegate operations to them in a chain of responsibility.
///     </para>
///     <para>
///         Implementations should return <c>true</c> if they successfully handle an operation, or <c>false</c> to allow
///         the next provider in the chain to attempt it.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// public class CustomVariableProvider : IWorkflowVariableProvider
/// {
///     public async Task&lt;bool&gt; WriteVariable(string variableName, string variableValue, CancellationToken cancellationToken)
///     {
///         // Custom logic to write to an external system
///         return await WriteToExternalSystemAsync(variableName, variableValue);
///     }
///     public async Task&lt;bool&gt; ReadVariable(string jobName, string variableName, CancellationToken cancellationToken)
///     {
///         // Custom logic to read from an external system
///         var value = await ReadFromExternalSystemAsync(jobName, variableName);
///         return value != null;
///     }
/// }
///     </code>
/// </example>
/// <seealso cref="IWorkflowVariableService" />
[PublicAPI]
public interface IWorkflowVariableProvider
{
    /// <summary>
    ///     Writes a variable to the provider's storage system.
    /// </summary>
    /// <param name="variableName">The name of the variable to write.</param>
    /// <param name="variableValue">The value to store for the variable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     A task that resolves to <c>true</c> if the provider successfully handled the write operation;
    ///     otherwise, <c>false</c> to delegate to the next provider.
    /// </returns>
    Task<bool> WriteVariable(string variableName, string variableValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads a variable from the provider's storage system for a specific job context.
    /// </summary>
    /// <param name="jobName">The name of the job context from which to read the variable.</param>
    /// <param name="variableName">The name of the variable to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     A task that resolves to <c>true</c> if the provider successfully found and retrieved the variable;
    ///     otherwise, <c>false</c>.
    /// </returns>
    Task<bool> ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default);
}
