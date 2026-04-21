namespace DecSm.Atom.Workflows;

[PublicAPI]
public interface IWorkflowBuildDefinition : IBuildDefinition, IGen
{
    /// <summary>
    ///     Gets the collection of workflow definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Workflows define how targets are orchestrated, potentially across different CI/CD platforms.
    /// </remarks>
    IReadOnlyList<WorkflowDefinition> Workflows { get; }

    /// <summary>
    ///     Gets the collection of global workflow options that apply to all workflows.
    /// </summary>
    /// <remarks>
    ///     These options can be overridden at the individual workflow level.
    /// </remarks>
    IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions { get; }
}
