namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class DuplicateDependencyBuild : MinimalBuildDefinition, IDevopsWorkflows, IDuplicateDependencyTarget
{
    public override IReadOnlyList<IWorkflowOption> GlobalWorkflowOptions =>
    [
        WorkflowOptions.Artifacts.UseCustomProvider,
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("duplicatedependency-workflow")
        {
            Triggers = [WorkflowTriggers.Manual],
            Targets = [WorkflowTargets.DuplicateDependencyTarget1],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

[ConfigureHostBuilder]
public partial interface IDuplicateDependencyTarget : IStoreArtifact, IRetrieveArtifact
{
    Target DuplicateDependencyTarget1 =>
        t => t
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildName))
            .ConsumesVariable(nameof(SetupBuildInfo), nameof(BuildId))
            .ProducesArtifact("artifact-name");

    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IArtifactProvider, TestArtifactProvider>();
}

internal sealed class TestArtifactProvider : IArtifactProvider
{
    public Task StoreArtifacts(
        IReadOnlyList<string> artifactNames,
        string? buildId = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default) =>
        throw new();

    public Task RetrieveArtifacts(
        IReadOnlyList<string> artifactNames,
        string? buildId = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default) =>
        throw new();

    public Task Cleanup(IReadOnlyList<string> runIdentifiers, CancellationToken cancellationToken = default) =>
        throw new();

    public Task<IReadOnlyList<string>> GetStoredRunIdentifiers(
        string? artifactName = null,
        string? buildSlice = null,
        CancellationToken cancellationToken = default) =>
        throw new();
}
