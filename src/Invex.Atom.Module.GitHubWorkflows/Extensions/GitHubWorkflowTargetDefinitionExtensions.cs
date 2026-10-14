namespace Invex.Atom.Module.GitHubWorkflows.Extensions;

/// <summary>
///     Provides extension methods for <see cref="WorkflowTargetDefinition" /> to simplify GitHub Actions workflow
///     configuration.
/// </summary>
[PublicAPI]
public static class GitHubWorkflowTargetDefinitionExtensions
{
    /// <summary>
    ///     Extension methods for <see cref="WorkflowTargetDefinition" />.
    /// </summary>
    extension(WorkflowTargetDefinition workflowTargetDefinition)
    {
        /// <summary>
        ///     Configures the workflow target to run on a matrix of GitHub Actions runner labels.
        /// </summary>
        /// <param name="labels">An array of runner labels (e.g., "ubuntu-latest", "windows-latest").</param>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method sets up a matrix dimension for the `JobRunsOn` parameter, allowing the job
        ///     to execute on multiple runner environments. It also adds the
        ///     <see cref="Invex.Atom.Module.GitHubWorkflows.Extensions.GitHubBuildOptionsExtensions.GitHubRunsOnOptions.SetByMatrix" />
        ///     option to indicate that the matrix determines the runner.
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGitHubRunsOnMatrix(IEnumerable<TextExpression> labels) =>
            workflowTargetDefinition
                .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                {
                    Values = labels.ToList(),
                })
                .WithOptions(BuildOptions.GitHub.RunsOn.SetByMatrix);

        /// <summary>
        ///     Configures the workflow target to run on a matrix of GitHub Actions runner labels.
        /// </summary>
        /// <param name="labels">An array of runner labels (e.g., "ubuntu-latest", "windows-latest").</param>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method sets up a matrix dimension for the `JobRunsOn` parameter, allowing the job
        ///     to execute on multiple runner environments. It also adds the
        ///     <see cref="Invex.Atom.Module.GitHubWorkflows.Extensions.GitHubBuildOptionsExtensions.GitHubRunsOnOptions.SetByMatrix" />
        ///     option to indicate that the matrix determines the runner.
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGitHubRunsOnMatrix(IEnumerable<string> labels) =>
            workflowTargetDefinition.WithGitHubRunsOnMatrix(
                labels.Select(StructuredText.Expressions.WorkflowExpressionExtensions.Raw));

        /// <summary>
        ///     Configures the workflow target to inject the GitHub token as a secret.
        /// </summary>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method adds a <see cref="WorkflowSecretInjection" /> option for the <see cref="IGitHubHelper.GitHubToken" />,
        ///     making the GitHub Actions token available as a secret within the workflow job.
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGitHubTokenInjection(Permissions? permissions = null) =>
            permissions is null
                ? workflowTargetDefinition.WithOptions(BuildOptions.Inject.Secret(nameof(IGitHubHelper.GitHubToken)))
                : workflowTargetDefinition.WithOptions(BuildOptions.Inject.Secret(nameof(IGitHubHelper.GitHubToken)),
                    new GitHubTokenPermissionsOption(permissions));
    }
}
