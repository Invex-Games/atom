namespace Invex.Atom.Module.DevopsWorkflows.Workflows.Options;

/// <summary>
///     Configures how Azure Pipelines handles runs waiting on an exclusive lock.
/// </summary>
/// <param name="LockBehavior">
///     The Azure Pipelines lock behavior. Supported values are <c>runLatest</c> and <c>sequential</c>.
/// </param>
[PublicAPI]
public sealed record DevopsConcurrencyOption(TextExpression LockBehavior) : IBuildOption;
