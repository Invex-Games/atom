using DecSm.Atom.StructuredText.Expressions;
using DecSm.Atom.Workflows;

namespace DecSm.Atom.Module.GithubWorkflows.Extensions;

/// <summary>
///     Provides extension methods for <see cref="WorkflowTargetDefinition" /> to simplify GitHub Actions workflow
///     configuration.
/// </summary>
[PublicAPI]
public static class WorkflowTargetDefinitionExtensions
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
        ///     <see cref="DecSm.Atom.Module.GithubWorkflows.Extensions.WorkflowOptionsExtensions.GithubRunsOnOptions.SetByMatrix" />
        ///     option to indicate that the matrix determines the runner.
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGithubRunsOnMatrix(IEnumerable<TextExpression> labels) =>
            workflowTargetDefinition
                .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                {
                    Values = labels.ToList(),
                })
                .WithOptions(WorkflowOptions.Github.RunsOn.SetByMatrix);

        /// <summary>
        ///     Configures the workflow target to run on a matrix of GitHub Actions runner labels.
        /// </summary>
        /// <param name="labels">An array of runner labels (e.g., "ubuntu-latest", "windows-latest").</param>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method sets up a matrix dimension for the `JobRunsOn` parameter, allowing the job
        ///     to execute on multiple runner environments. It also adds the
        ///     <see cref="DecSm.Atom.Module.GithubWorkflows.Extensions.WorkflowOptionsExtensions.GithubRunsOnOptions.SetByMatrix" />
        ///     option to indicate that the matrix determines the runner.
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGithubRunsOnMatrix(IEnumerable<string> labels) =>
            workflowTargetDefinition.WithGithubRunsOnMatrix(
                labels.Select(StructuredText.Expressions.WorkflowExpressionExtensions.Raw));

        /// <summary>
        ///     Configures the workflow target to inject the GitHub token as a secret.
        /// </summary>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method adds a <see cref="WorkflowSecretInjection" /> option for the <see cref="IGithubHelper.GithubToken" />,
        ///     making the GitHub Actions token available as a secret within the workflow job.
        /// </remarks>
        [PublicAPI]
        public WorkflowTargetDefinition WithGithubTokenInjection(Permissions? permissions = null) =>
            permissions is null
                ? workflowTargetDefinition.WithOptions(WorkflowOptions.Inject.Secret(nameof(IGithubHelper.GithubToken)))
                : workflowTargetDefinition.WithOptions(WorkflowOptions.Inject.Secret(nameof(IGithubHelper.GithubToken)),
                    new GithubTokenPermissionsOption(permissions));
    }
}
