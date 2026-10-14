namespace Invex.Atom.Module.DevopsWorkflows;

/// <summary>
///     Provides an implementation of <see cref="IVariableProvider" /> for Azure DevOps Pipelines.
/// </summary>
/// <remarks>
///     This provider enables writing output variables that can be consumed by subsequent steps or jobs
///     within an Azure DevOps Pipeline. It also supports reading variables from previous jobs.
/// </remarks>
internal sealed class DevopsVariableProvider(ILogger<DevopsVariableProvider> logger) : IVariableProvider
{
    /// <summary>
    ///     Writes a variable to the Azure DevOps Pipeline output, making it available to subsequent steps or jobs.
    /// </summary>
    /// <param name="variableName">The name of the variable to write.</param>
    /// <param name="variableValue">The value of the variable.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that returns <c>true</c> if the variable was written (i.e., running in Azure
    ///     DevOps),
    ///     otherwise <c>false</c>.
    /// </returns>
    public Task<bool> WriteVariable(
        string variableName,
        string variableValue,
        CancellationToken cancellationToken = default)
    {
        if (!Devops.IsDevopsPipelines)
            return Task.FromResult(false);

        logger.LogInformation("Writing variable {VariableName} with value '{VariableValue}' to Azure DevOps pipeline.",
            variableName,
            variableValue);

        // Azure DevOps command to set an output variable
        Console.WriteLine($"##vso[task.setvariable variable={variableName};isoutput=true;]{variableValue}");

        return Task.FromResult(true);
    }

    /// <summary>
    ///     Reads a variable written by a previous job in Azure DevOps Pipelines.
    /// </summary>
    /// <param name="jobName">
    ///     The name of the job from which to read the variable (not directly used for reading in Azure
    ///     DevOps, but part of the interface).
    /// </param>
    /// <param name="variableName">The name of the variable to read.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     The variable's value, which the pipeline exposes as an environment variable, or <c>null</c> if not running in
    ///     Azure DevOps or the variable is not set.
    /// </returns>
    /// <remarks>
    ///     Azure DevOps exposes a variable named <c>my-var</c> as the environment variable <c>MY_VAR</c>, so both forms are
    ///     checked.
    /// </remarks>
    public Task<string?> ReadVariable(
        string jobName,
        string variableName,
        CancellationToken cancellationToken = default)
    {
        if (!Devops.IsDevopsPipelines)
            return Task.FromResult<string?>(null);

        var value = Environment.GetEnvironmentVariable(variableName) ??
                    Environment.GetEnvironmentVariable(variableName
                        .ToUpperInvariant()
                        .Replace('-', '_')
                        .Replace('.', '_'));

        if (value == null)
        {
            logger.LogWarning("Variable '{VariableName}' not found in Azure DevOps pipeline environment variables.",
                variableName);

            return Task.FromResult<string?>(null);
        }

        logger.LogInformation("Read variable '{VariableName}' with value '{VariableValue}' from Azure DevOps pipeline.",
            variableName,
            value.SanitizeForLogging());

        return Task.FromResult<string?>(value);
    }
}
