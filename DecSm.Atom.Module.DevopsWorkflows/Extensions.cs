namespace DecSm.Atom.Module.DevopsWorkflows;

/// <summary>
///     Provides extension methods for <see cref="WorkflowTargetDefinition" /> to simplify Azure DevOps workflow
///     configuration.
/// </summary>
[PublicAPI]
public static class Extensions
{
    extension(WorkflowTargetDefinition workflowTargetDefinition)
    {
        /// <summary>
        ///     Configures the workflow target to run on a matrix of Azure DevOps agent pool labels.
        /// </summary>
        /// <param name="labels">An array of agent pool labels (e.g., "ubuntu-latest", "windows-latest").</param>
        /// <returns>The modified <see cref="WorkflowTargetDefinition" /> for chaining.</returns>
        /// <remarks>
        ///     This method sets up a matrix dimension for the `JobRunsOn` parameter, allowing the job
        ///     to execute on multiple agent pool environments. It also adds the SetByMatrix options ( "$(job-runs-on)" )
        ///     option to indicate that the agent pool is determined by the matrix.
        /// </remarks>
        public WorkflowTargetDefinition WithDevopsPoolMatrix(string[] labels) =>
            workflowTargetDefinition
                .WithMatrixDimensions(new MatrixDimension(nameof(IJobRunsOn.JobRunsOn))
                {
                    Values = labels,
                })
                .WithOptions(WorkflowOptions.Devops.DevopsPool.SetByMatrix);
    }
}
