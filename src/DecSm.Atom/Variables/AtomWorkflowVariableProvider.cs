namespace DecSm.Atom.Variables;

/// <summary>
///     The default implementation of <see cref="IWorkflowVariableProvider" />, which uses a local JSON file for storage.
/// </summary>
/// <remarks>
///     This provider serves as the fallback for variable management, persisting variables in a `variables` file
///     within the Atom temporary directory. Variables are stored in a job-scoped manner.
/// </remarks>
/// <param name="fileSystem">The file system service for accessing the variables file.</param>
/// <param name="buildModel">The build model for accessing the current target context.</param>
internal sealed partial class AtomWorkflowVariableProvider(IAtomFileSystem fileSystem, BuildModel buildModel)
    : IWorkflowVariableProvider
{
    /// <summary>
    ///     Writes a variable to the local `variables` JSON file, scoped to the current job.
    /// </summary>
    /// <param name="variableName">The name of the variable to write.</param>
    /// <param name="variableValue">The value to store.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> upon successful write.</returns>
    public async Task<bool> WriteVariable(
        string variableName,
        string variableValue,
        CancellationToken cancellationToken = default)
    {
        var variablesPath = fileSystem.AtomTempDirectory / "variables";

        Dictionary<string, string>? variables;

        if (variablesPath.FileExists)
        {
            var text = await fileSystem.File.ReadAllTextAsync(variablesPath, cancellationToken);
            variables = JsonSerializer.Deserialize(text, AppJsonContext.Default.DictionaryStringString) ?? [];
        }
        else
        {
            variables = new();
        }

        var jobScopedVariableName = $"{buildModel.CurrentTarget!.Name}:{variableName}";
        variables[jobScopedVariableName] = variableValue;
        var json = JsonSerializer.Serialize(variables, AppJsonContext.Default.DictionaryStringString);

        await fileSystem.File.WriteAllTextAsync(variablesPath, json, cancellationToken);

        return true;
    }

    /// <summary>
    ///     Reads a variable from the local `variables` JSON file for a specific job and sets it as an environment variable.
    /// </summary>
    /// <param name="jobName">The name of the job context from which to read.</param>
    /// <param name="variableName">The name of the variable to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if the variable was found and set; otherwise, <c>false</c>.</returns>
    public async Task<bool> ReadVariable(
        string jobName,
        string variableName,
        CancellationToken cancellationToken = default)
    {
        var variablesPath = fileSystem.AtomTempDirectory / "variables";

        if (!variablesPath.FileExists)
            return false;

        var text = await fileSystem.File.ReadAllTextAsync(variablesPath, cancellationToken);

        var variables = JsonSerializer.Deserialize(text, AppJsonContext.Default.DictionaryStringString) ?? [];

        var jobScopedVariableName = $"{jobName}:{variableName}";

        if (!variables.TryGetValue(jobScopedVariableName, out var variable))
            return true;

        Environment.SetEnvironmentVariable(variableName, variable);

        return true;
    }

    /// <summary>
    ///     Provides source-generated JSON serialization metadata for the variable dictionary.
    /// </summary>
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class AppJsonContext : JsonSerializerContext;
}
