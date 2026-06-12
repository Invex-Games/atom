namespace Invex.Atom.Workflows;

/// <summary>
///     Extends <see cref="IBuildDefinition" /> with workflow definitions, enabling generation of CI/CD
///     workflow files (e.g., GitHub Actions, Azure DevOps) from the build definition.
/// </summary>
/// <remarks>
///     Implementations typically derive from <see cref="WorkflowBuildDefinition" />. Inheriting this interface
///     also registers the workflow services (generation, resolution, and lifecycle hooks) with the host and
///     provides the <see cref="IGen.Gen" /> target.
/// </remarks>
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

    /// <summary>
    ///     Configures the host builder to register the workflow services (generation, resolution,
    ///     context, and lifecycle hooks).
    /// </summary>
    /// <param name="builder">The host application builder.</param>
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
