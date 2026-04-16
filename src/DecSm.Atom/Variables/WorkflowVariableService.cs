namespace DecSm.Atom.Variables;

/// <summary>
///     Defines a centralized service for managing workflow variables, coordinating operations across multiple providers.
/// </summary>
/// <remarks>
///     This service abstracts the complexity of multiple variable providers, offering a unified API for reading and
///     writing
///     variables. It delegates operations to registered <see cref="IWorkflowVariableProvider" /> implementations,
///     trying custom providers first before falling back to the base Atom provider.
/// </remarks>
[PublicAPI]
public interface IWorkflowVariableService
{
    /// <summary>
    ///     Writes a variable to the workflow context, making it available for subsequent steps and jobs.
    /// </summary>
    /// <param name="variableName">The name of the variable as defined in the build parameters.</param>
    /// <param name="variableValue">The value to store for the variable.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous write operation.</returns>
    /// <remarks>
    ///     The service resolves the variable's argument name from the build definition and attempts to write it using
    ///     custom providers first. If none succeed, it uses the base Atom provider.
    /// </remarks>
    Task WriteVariable(string variableName, string variableValue, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Reads a variable from a specific job's context, making it available in the current execution context.
    /// </summary>
    /// <param name="jobName">The name of the job context from which to read the variable.</param>
    /// <param name="variableName">The name of the variable as defined in the build parameters.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous read operation.</returns>
    /// <remarks>
    ///     This method enables cross-job variable sharing. It resolves the variable's argument name and attempts to read it
    ///     using custom providers before falling back to the base Atom provider.
    /// </remarks>
    Task ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default);
}

/// <summary>
///     Internal implementation of <see cref="IWorkflowVariableService" />.
/// </summary>
/// <param name="workflowVariableProviders">The collection of registered workflow variable providers.</param>
/// <param name="buildDefinition">The build definition for resolving parameter names.</param>
internal sealed class WorkflowVariableService(
    IEnumerable<IWorkflowVariableProvider> workflowVariableProviders,
    IBuildDefinition buildDefinition
) : IWorkflowVariableService
{
    /// <summary>
    ///     The default provider, used as a fallback.
    /// </summary>
    // ReSharper disable once PossibleMultipleEnumeration - Once-only operation
    private readonly AtomWorkflowVariableProvider _baseProvider = workflowVariableProviders
        .OfType<AtomWorkflowVariableProvider>()
        .Single();

    /// <summary>
    ///     Custom providers, which are tried before the base provider.
    /// </summary>
    // ReSharper disable once PossibleMultipleEnumeration - Once-only operation
    private readonly IWorkflowVariableProvider[] _customProviders = workflowVariableProviders
        .Where(x => x is not AtomWorkflowVariableProvider)
        .ToArray();

    /// <inheritdoc />
    public async Task WriteVariable(
        string variableName,
        string variableValue,
        CancellationToken cancellationToken = default)
    {
        var variableArgName = buildDefinition.ParamDefinitions[variableName].ArgName;

        foreach (var provider in _customProviders)
            if (await provider.WriteVariable(variableArgName, variableValue, cancellationToken))
                return;

        await _baseProvider.WriteVariable(variableArgName, variableValue, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default)
    {
        var variableArgName = buildDefinition.ParamDefinitions[variableName].ArgName;

        foreach (var provider in _customProviders)
            if (await provider.ReadVariable(jobName, variableArgName, cancellationToken))
                return;

        await _baseProvider.ReadVariable(jobName, variableArgName, cancellationToken);
    }
}
