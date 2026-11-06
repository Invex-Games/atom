namespace Invex.Atom.Workflows.Definition;

/// <summary>
///     A target that builds, tests or checks one monorepo product, for <see cref="WorkflowPresets.ProductValidate" />.
/// </summary>
/// <param name="Product">The name of the product, as declared in <see cref="IProductsDefinition.Products" />.</param>
/// <param name="Target">The workflow target, which can carry its own options.</param>
[PublicAPI]
public sealed record ProductWorkflowTarget(string Product, WorkflowTargetDefinition Target)
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProductWorkflowTarget" /> class for a target with no options.
    /// </summary>
    /// <param name="product">The name of the product.</param>
    /// <param name="target">The name of the target.</param>
    public ProductWorkflowTarget(string product, string target) : this(product, new WorkflowTargetDefinition(target)) { }
}
