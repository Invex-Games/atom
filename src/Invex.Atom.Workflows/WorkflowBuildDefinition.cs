namespace Invex.Atom.Workflows;

/// <summary>
///     The standard abstract base class for build definitions that generate CI/CD workflow files,
///     providing default implementations for <see cref="IWorkflowBuildDefinition" />.
/// </summary>
/// <remarks>
///     Derive from this class instead of <see cref="BuildDefinition" /> when the build should produce
///     workflow files. Override <see cref="Workflows" /> to declare the workflows to generate.
/// </remarks>
/// <param name="services">The service provider for dependency injection.</param>
[PublicAPI]
public abstract class WorkflowBuildDefinition(IServiceProvider services)
    : BuildDefinition(services), IWorkflowBuildDefinition
{
    /// <inheritdoc />
    public abstract IReadOnlyList<WorkflowDefinition> Workflows { get; }
}
