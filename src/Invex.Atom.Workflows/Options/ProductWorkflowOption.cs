namespace Invex.Atom.Workflows.Options;

/// <summary>
///     Marks a workflow as a monorepo product's own workflow. <see cref="WorkflowDefinition.ForProduct" /> adds it.
/// </summary>
/// <param name="ProductName">The product's name.</param>
/// <param name="TagPrefix">The product's release tag prefix, such as <c>appa/v</c>.</param>
/// <remarks>
///     On GitHub Actions, when the workflow also has a release trigger, every job gets the condition
///     <c>github.event_name != 'release' || startsWith(github.event.release.tag_name, '&lt;prefix&gt;')</c>, so that a
///     release of another product does not run this product's jobs. Other platforms ignore the option.
/// </remarks>
[PublicAPI]
public sealed record ProductWorkflowOption(string ProductName, string TagPrefix) : IBuildOption;
