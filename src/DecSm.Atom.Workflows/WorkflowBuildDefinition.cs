namespace DecSm.Atom.Workflows;

[PublicAPI]
public abstract class WorkflowBuildDefinition(IServiceProvider services)
    : BuildDefinition(services), IWorkflowBuildDefinition
{
    /// <inheritdoc />
    public abstract IReadOnlyList<WorkflowDefinition> Workflows { get; }

    public override void ConfigureDefinitionHost(IHostApplicationBuilder builder)
    {
        base.ConfigureDefinitionHost(builder);

        builder
            .Services
            .AddSingleton<IWorkflowBuildDefinition>(services =>
                (IWorkflowBuildDefinition)services.GetRequiredService<IBuildDefinition>())
            .AddSingleton<IWorkflowContext, WorkflowContext.WorkflowContext>()
            .AddSingleton<WorkflowGenerator>()
            .AddSingleton<WorkflowResolver>()
            .AddSingleton<IAtomLifecycleHook, WorkflowLifecycleHook>();
    }
}
