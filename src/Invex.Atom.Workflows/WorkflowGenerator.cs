namespace Invex.Atom.Workflows;

/// <summary>
///     Generates workflow files based on the defined <see cref="WorkflowDefinition" />s.
/// </summary>
/// <param name="buildDefinition">The build definition containing workflow configurations.</param>
/// <param name="writers">A collection of available workflow writers.</param>
/// <param name="workflowResolver">The resolver for transforming workflow definitions into models.</param>
/// <param name="logger">The logger for diagnostics.</param>
internal sealed class WorkflowGenerator(
    IWorkflowBuildDefinition buildDefinition,
    IEnumerable<IWorkflowWriter> writers,
    WorkflowResolver workflowResolver,
    ILogger<WorkflowGenerator> logger
)
{
    private readonly List<IWorkflowWriter> _writers = writers.ToList();

    /// <summary>
    ///     Generates all defined workflows using the appropriate workflow writers.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task GenerateWorkflows(CancellationToken cancellationToken = default)
    {
        var workflowDefinitions = buildDefinition.Workflows;

        var generationTasks = new List<Task>();

        // ReSharper disable LoopCanBeConvertedToQuery
        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.Types)
        {
            // ReSharper restore LoopCanBeConvertedToQuery
            var writer = _writers.FirstOrDefault(w => w.WorkflowType == workflowType.GetType());

            if (writer is null)
            {
                logger.LogWarning(
                    "No workflow writer found for workflow type {WorkflowType} in workflow {WorkflowName}. " +
                    "The workflow will not be generated. Ensure the appropriate module (e.g., Invex.Atom.Module.GithubWorkflows) is referenced.",
                    workflowType.GetType()
                        .Name,
                    workflowDefinition.Name);

                continue;
            }

            var workflow = workflowResolver.Resolve(workflowDefinition);
            var generateTask = writer.Generate(workflow, cancellationToken);

            generationTasks.Add(generateTask);
        }

        await Task.WhenAll(generationTasks);
    }

    /// <summary>
    ///     Checks if any of the defined workflow files are outdated (i.e., need to be regenerated).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><c>true</c> if any workflow file is outdated; otherwise, <c>false</c>.</returns>
    public async Task<bool> WorkflowsOutdated(CancellationToken cancellationToken = default)
    {
        var workflowDefinitions = buildDefinition.Workflows;

        var checkTasks = new List<Task<bool>>();

        // ReSharper disable LoopCanBeConvertedToQuery
        foreach (var workflowDefinition in workflowDefinitions)
        foreach (var workflowType in workflowDefinition.Types)
        {
            // ReSharper restore LoopCanBeConvertedToQuery
            var writer = _writers.FirstOrDefault(w => w.WorkflowType == workflowType.GetType());

            if (writer is null)
                continue;

            var workflow = workflowResolver.Resolve(workflowDefinition);
            var checkTask = writer.CheckForOutdatedWorkflow(workflow, cancellationToken);

            checkTasks.Add(checkTask);
        }

        var result = await Task.WhenAll(checkTasks);

        return result.Any(x => x);
    }
}
