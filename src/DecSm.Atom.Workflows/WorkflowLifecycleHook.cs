namespace DecSm.Atom.Workflows;

/// <summary>
///     A lifecycle hook that manages workflow generation and outdated workflow detection.
/// </summary>
/// <remarks>
///     <para>
///         When running in non-headless mode or when the <c>--gen</c> flag is provided, this hook
///         will automatically generate workflow files before build targets are executed.
///     </para>
///     <para>
///         When running in headless mode (CI/CD), if workflows are found to be outdated,
///         a <see cref="WorkflowOutdatedException" /> is thrown to fail the build early,
///         prompting the developer to regenerate workflows locally.
///     </para>
/// </remarks>
internal sealed class WorkflowLifecycleHook(
    CommandLineArgs args,
    WorkflowGenerator workflowGenerator,
    ILogger<WorkflowLifecycleHook> logger
) : IAtomLifecycleHook
{
    public async Task BeforeExecute(CancellationToken cancellationToken)
    {
        if (args.Commands.Any(x => x.Name is nameof(IGenerateWorkflowFiles.GenerateWorkflowFiles)) || !args.HasHeadless)
        {
            logger.LogDebug("Generating workflow files");
            await workflowGenerator.GenerateWorkflows(cancellationToken);
        }
        else if (await workflowGenerator.WorkflowsOutdated(cancellationToken))
        {
            throw new WorkflowOutdatedException(
                "One or more workflows are out of date. To regenerate workflows, run the build with the --gen flag.");
        }
    }
}
