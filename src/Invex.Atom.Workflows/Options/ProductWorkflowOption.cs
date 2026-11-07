namespace Invex.Atom.Workflows.Options;

/// <summary>
///     Marks a workflow as a monorepo product's own workflow. <see cref="WorkflowDefinition.ForProduct" /> adds it.
/// </summary>
/// <param name="Product">
///     The product, as <see cref="IProductsDefinition.GetProduct" /> returns it. Its <see cref="Product.TagPrefix" />,
///     such as <c>appa/v</c>, is the prefix the release guard checks.
/// </param>
/// <remarks>
///     On GitHub Actions, when the workflow also has a release trigger, every job gets the condition
///     <c>github.event_name != 'release' || startsWith(github.event.release.tag_name, '&lt;prefix&gt;')</c>, so that a
///     release of another product does not run this product's jobs. Other platforms ignore the option.
/// </remarks>
[PublicAPI]
public sealed record ProductWorkflowOption(Product Product) : IBuildOption;
