using Environment = System.Environment;

namespace Invex.Atom.Module.GitHubWorkflows;

/// <summary>
///     Provides an implementation of <see cref="IVariableProvider" /> for GitHub Actions.
/// </summary>
/// <remarks>
///     This provider enables writing output variables that can be consumed by subsequent steps or jobs
///     within a GitHub Actions workflow. It also indicates whether a variable can be read from a previous job.
/// </remarks>
internal sealed class GitHubVariableProvider(IRootedFileSystem fileSystem, ILogger<GitHubVariableProvider> logger)
    : IVariableProvider
{
    /// <summary>
    ///     Writes a variable to the GitHub Actions output, making it available to subsequent steps or jobs.
    /// </summary>
    /// <param name="variableName">The name of the variable to write.</param>
    /// <param name="variableValue">The value of the variable.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that returns <c>true</c> if the variable was written (i.e., running in GitHub
    ///     Actions),
    ///     otherwise <c>false</c>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the <c>GITHUB_OUTPUT</c> environment variable is not set when running in GitHub Actions.
    /// </exception>
    public async Task<bool> WriteVariable(
        string variableName,
        string variableValue,
        CancellationToken cancellationToken = default)
    {
        if (!GitHub.IsGitHubActions)
            return false;

        var githubOutputPath = GitHub.Variables.Output;

        if (githubOutputPath is null)
            throw new(
                $"{GitHub.VariableNames.Output} environment variable not set. This should not occur when running in a GitHub workflow");

        logger.LogInformation("Writing variable {VariableName} with value {VariableValue} to {GitHubOutputPath}",
            variableName,
            variableValue,
            githubOutputPath);

        await fileSystem.File.AppendAllTextAsync(githubOutputPath,
            $"{variableName}={variableValue}{Environment.NewLine}",
            cancellationToken);

        return true;
    }

    /// <summary>
    ///     Reads a variable written by a previous job in GitHub Actions.
    /// </summary>
    /// <param name="jobName">The name of the job from which to read the variable.</param>
    /// <param name="variableName">The name of the variable to read.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     The variable's value, which the generated workflow injects as an environment variable from the job's
    ///     outputs, or <c>null</c> if not running in GitHub Actions or the variable is not set.
    /// </returns>
    public Task<string?> ReadVariable(
        string jobName,
        string variableName,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(GitHub.IsGitHubActions
            ? Environment.GetEnvironmentVariable(variableName)
            : null);
}
