namespace DecSm.Atom.Workflows;

[PublicAPI]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IWorkflowBuildDefinition : IBuildDefinition, IGen
{
    /// <summary>
    ///     Gets the collection of workflow definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Workflows define how targets are orchestrated, potentially across different CI/CD platforms.
    /// </remarks>
    IReadOnlyList<WorkflowDefinition> Workflows => [];
}
