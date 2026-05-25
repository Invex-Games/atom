namespace DecSm.Atom.Workflows;

[PublicAPI]
[ConfigureHostBuilder]
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public partial interface IWorkflowBuildDefinition : IBuildDefinition, IGen
{
    /// <summary>
    ///     Gets the collection of workflow definitions for the build.
    /// </summary>
    /// <remarks>
    ///     Workflows define how targets are orchestrated, potentially across different CI/CD platforms.
    /// </remarks>
    IReadOnlyList<WorkflowDefinition> Workflows { get; }

    protected static partial void ConfigureBuilderFromIWorkflowBuildDefinition(IHostApplicationBuilder builder) =>
        builder
            .Services
            .AddSingleton<IWorkflowBuildDefinition>(services =>
                (IWorkflowBuildDefinition)services.GetRequiredService<IBuildDefinition>())
            .AddSingleton<IWorkflowContext, WorkflowContext.WorkflowContext>()
            .AddSingleton<WorkflowGenerator>()
            .AddSingleton<WorkflowResolver>()
            .AddSingleton<IAtomLifecycleHook, WorkflowLifecycleHook>();
}
