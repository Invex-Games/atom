namespace DecSm.Atom.Variables;

/// <summary>
///     Provides helper methods for managing workflow variables within the Atom build system.
/// </summary>
/// <remarks>
///     This interface enables components to write variables that can be shared across different
///     build steps and targets in the workflow execution context.
/// </remarks>
/// <example>
///     <code>
/// // Writing variables in a target execution
/// await WriteVariable("BuildId", "12345");
/// await WriteVariable("BuildVersion", "1.0.0");
///     </code>
/// </example>
/// <seealso cref="IBuildAccessor" />
/// <seealso cref="IWorkflowVariableService" />
[PublicAPI]
public interface IVariablesHelper : IBuildAccessor
{
    /// <summary>
    ///     Writes a variable to the workflow context, making it available for later steps.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    /// <remarks>
    ///     Variables written through this method are persisted in the current workflow execution context.
    ///     If a variable with the same name already exists, it will be overwritten.
    /// </remarks>
    Task WriteVariable(string name, string value, CancellationToken cancellationToken = default) =>
        GetService<IWorkflowVariableService>()
            .WriteVariable(name, value, cancellationToken);
}
