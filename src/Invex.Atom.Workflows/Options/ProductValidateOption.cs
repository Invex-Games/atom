namespace Invex.Atom.Workflows.Options;

/// <summary>
///     Asks <c>atom gen</c> to check the wiring of a monorepo validation workflow.
///     <see cref="WorkflowPresets.ProductValidate" /> adds it, and a hand-built validation workflow can add it too.
/// </summary>
/// <param name="GateTarget">The gate target, which must depend directly on every product target.</param>
/// <param name="ProductTargets">
///     The product targets, each with its product. Only the names of the products and targets are used.
/// </param>
/// <param name="SharedTargets">
///     The names of targets that do not depend on the product, such as a solution-wide compile, which product targets
///     may depend on. Their jobs run on every pull request, without the product.
/// </param>
/// <remarks>
///     <para>
///         A job condition does not add a dependency, and a skipped job skips the jobs that depend on it. So
///         generation fails if:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///                 a product target, a shared target or the gate is not one of the workflow's targets: it would be
///                 added as a job without its options;
///             </description>
///         </item>
///         <item>
///             <description>
///                 a product target does not depend directly on <see cref="IProductsDefinition.DetectAffectedProducts" />:
///                 its job cannot read the affected products and is always skipped;
///             </description>
///         </item>
///         <item>
///             <description>
///                 a product target depends on any other target that is not a product target of the same product or a
///                 shared target: a product target of another product is skipped whenever only this product is
///                 affected (which skips this one too), and any other target would be added as a job without options
///                 or dependencies;
///             </description>
///         </item>
///         <item>
///             <description>
///                 the gate does not depend directly on every product target (it would not see that job fail), or
///                 depends on a target that is not a product target, a shared target or
///                 <c>DetectAffectedProducts</c>.
///             </description>
///         </item>
///     </list>
/// </remarks>
[PublicAPI]
public sealed record ProductValidateOption(
    string GateTarget,
    IReadOnlyList<ProductWorkflowTarget> ProductTargets,
    IReadOnlyList<string>? SharedTargets = null) : IBuildOption;
