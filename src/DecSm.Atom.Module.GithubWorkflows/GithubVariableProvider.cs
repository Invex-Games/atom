using Environment = System.Environment;

namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Provides an implementation of <see cref="IVariableProvider" /> for GitHub Actions.
/// </summary>
/// <remarks>
///     This provider enables writing output variables that can be consumed by subsequent steps or jobs
///     within a GitHub Actions workflow. It also indicates whether a variable can be read from a previous job.
/// </remarks>
internal sealed class GithubVariableProvider(IAtomFileSystem atomFileSystem, ILogger<GithubVariableProvider> logger)
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
        if (!Github.IsGithubActions)
            return false;

        var githubOutputPath = Github.Variables.Output;

        if (githubOutputPath is null)
            throw new(
                $"{Github.VariableNames.Output} environment variable not set. This should not occur when running in a GitHub workflow");

        logger.LogInformation("Writing variable {VariableName} with value {VariableValue} to {GithubOutputPath}",
            variableName,
            variableValue,
            githubOutputPath);

        await atomFileSystem.File.AppendAllTextAsync(githubOutputPath,
            $"{variableName}={variableValue}{Environment.NewLine}",
            cancellationToken);

        return true;
    }

    /// <summary>
    ///     Indicates whether a variable can be read from a previous job in GitHub Actions.
    /// </summary>
    /// <param name="jobName">The name of the job from which to read the variable.</param>
    /// <param name="variableName">The name of the variable to read.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}" /> that returns <c>true</c> if running in GitHub Actions,
    ///     indicating that variables from previous jobs can potentially be read.
    /// </returns>
    public Task<bool> ReadVariable(
        string jobName,
        string variableName,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Github.IsGithubActions);
}
