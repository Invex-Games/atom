namespace DecSm.Atom.Workflows;

[PublicAPI]
public abstract class WorkflowBuildDefinition(IServiceProvider services)
    : BuildDefinition(services), IWorkflowBuildDefinition
{
    /// <inheritdoc />
    public virtual IReadOnlyList<WorkflowDefinition> Workflows { get; } = [];

    public virtual IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions { get; } = [];

    public override void ConfigureDefinitionHost(IHostApplicationBuilder builder)
    {
        base.ConfigureDefinitionHost(builder);

        builder
            .Services
            .AddSingleton<IWorkflowBuildDefinition>(services =>
                (IWorkflowBuildDefinition)services.GetRequiredService<IBuildDefinition>())
            .AddSingleton<WorkflowGenerator>()
            .AddSingleton<WorkflowResolver>();
    }
}
