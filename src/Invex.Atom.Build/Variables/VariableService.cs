namespace Invex.Atom.Build.Variables;

/// <summary>
///     Defines a centralized service for managing workflow variables, coordinating operations across multiple providers.
/// </summary>
/// <remarks>
///     This service abstracts the complexity of multiple variable providers, offering a unified API for reading and
///     writing
///     variables. It delegates operations to registered <see cref="IVariableProvider" /> implementations,
///     trying custom providers first before falling back to the base Atom provider.
/// </remarks>
[PublicAPI]
public interface IVariableService
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
    ///     Reads a variable written by a specific job.
    /// </summary>
    /// <param name="jobName">The name of the job context from which to read the variable.</param>
    /// <param name="variableName">The name of the variable as defined in the build parameters.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The variable's value, or <c>null</c> if no provider could supply it.</returns>
    /// <remarks>
    ///     A value that is found is also exposed as an environment variable named after the parameter's argument name,
    ///     so that it resolves through <see cref="IParamService" /> like any other parameter.
    /// </remarks>
    /// <remarks>
    ///     This method enables cross-job variable sharing. It resolves the variable's argument name and attempts to read it
    ///     using custom providers before falling back to the base Atom provider.
    /// </remarks>
    Task<string?> ReadVariable(string jobName, string variableName, CancellationToken cancellationToken = default);
}

/// <summary>
///     Internal implementation of <see cref="IVariableService" />.
/// </summary>
/// <param name="workflowVariableProviders">The collection of registered workflow variable providers.</param>
/// <param name="buildDefinition">The build definition for resolving parameter names.</param>
internal sealed class VariableService(
    IEnumerable<IVariableProvider> workflowVariableProviders,
    IBuildDefinition buildDefinition
) : IVariableService
{
    /// <summary>
    ///     The default provider, used as a fallback.
    /// </summary>
    // ReSharper disable once PossibleMultipleEnumeration - Once-only operation
    private readonly AtomVariableProvider _baseProvider = workflowVariableProviders
        .OfType<AtomVariableProvider>()
        .Single();

    /// <summary>
    ///     Custom providers, which are tried before the base provider.
    /// </summary>
    // ReSharper disable once PossibleMultipleEnumeration - Once-only operation
    private readonly IVariableProvider[] _customProviders = workflowVariableProviders
        .Where(x => x is not AtomVariableProvider)
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
    public async Task<string?> ReadVariable(
        string jobName,
        string variableName,
        CancellationToken cancellationToken = default)
    {
        var variableArgName = buildDefinition.ParamDefinitions[variableName].ArgName;

        string? value = null;

        foreach (var provider in _customProviders)
            if ((value = await provider.ReadVariable(jobName, variableArgName, cancellationToken)) is not null)
                break;

        value ??= await _baseProvider.ReadVariable(jobName, variableArgName, cancellationToken);

        if (value is not null)
            Environment.SetEnvironmentVariable(variableArgName, value);

        return value;
    }
}
