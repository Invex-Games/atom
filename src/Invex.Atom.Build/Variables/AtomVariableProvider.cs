namespace Invex.Atom.Build.Variables;

/// <summary>
///     The default implementation of <see cref="IVariableProvider" />, which uses a local JSON file for storage.
/// </summary>
/// <remarks>
///     This provider serves as the fallback for variable management, persisting variables in a `variables` file
///     within the Atom temporary directory. Variables are stored in a job-scoped manner.
/// </remarks>
/// <param name="fileSystem">The file system service for accessing the variables file.</param>
/// <param name="buildModel">The build model for accessing the current target context.</param>
internal sealed partial class AtomVariableProvider(IRootedFileSystem fileSystem, BuildModel buildModel)
    : IVariableProvider
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
    ///     Reads a variable from the local `variables` JSON file for a specific job.
    /// </summary>
    /// <param name="jobName">The name of the job context from which to read.</param>
    /// <param name="variableName">The name of the variable to read.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The variable's value, or <c>null</c> if it was not found.</returns>
    public async Task<string?> ReadVariable(
        string jobName,
        string variableName,
        CancellationToken cancellationToken = default)
    {
        var variablesPath = fileSystem.AtomTempDirectory / "variables";

        if (!variablesPath.FileExists)
            return null;

        var text = await fileSystem.File.ReadAllTextAsync(variablesPath, cancellationToken);

        var variables = JsonSerializer.Deserialize(text, AppJsonContext.Default.DictionaryStringString) ?? [];

        var jobScopedVariableName = $"{jobName}:{variableName}";

        return variables.GetValueOrDefault(jobScopedVariableName);
    }

    /// <summary>
    ///     Provides source-generated JSON serialization metadata for the variable dictionary.
    /// </summary>
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class AppJsonContext : JsonSerializerContext;
}
