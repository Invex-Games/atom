namespace Invex.Atom.Workflows.Definition;

/// <summary>
///     Ready-made workflow definitions. Modules add platform-specific presets, such as <c>WorkflowPresets.GitHub</c>.
/// </summary>
[PublicAPI]
public static class WorkflowPresets
{
    /// <summary>
    ///     The default name of the workflow that <see cref="ProductValidate" /> creates.
    /// </summary>
    public const string ProductValidateName = "Validate";

    /// <summary>
    ///     Creates a monorepo validation workflow: it detects the products a pull request affects, runs only the
    ///     affected products' targets, and has a single gate job to require in branch protection.
    /// </summary>
    /// <param name="build">The build, which declares the products.</param>
    /// <param name="gateTarget">
    ///     The name of the gate target. It must depend directly on every product target, and should fail when it runs
    ///     in CI (it only runs there when a product job failed or the run was cancelled).
    /// </param>
    /// <param name="targets">The product targets. Each one must depend directly on <c>DetectAffectedProducts</c>.</param>
    /// <param name="name">The workflow's name.</param>
    /// <param name="sharedTargets">
    ///     Targets that do not depend on the product, such as a solution-wide compile, that product targets may depend
    ///     on. Their jobs run on every pull request, without a condition or the product. A target that reads the build
    ///     version or name belongs to each product instead, because it would get the repository's. A shared target may
    ///     depend only on other shared targets and <c>DetectAffectedProducts</c>, and the gate must depend on it,
    ///     directly or through a product target.
    /// </param>
    /// <returns>
    ///     A workflow definition triggered by pull requests into <c>main</c> and manual runs. Set its
    ///     <see cref="WorkflowDefinition.Types" /> (and change its triggers if needed) with a <c>with</c> expression, and
    ///     add options with <see cref="WorkflowDefinition.WithOptions" />: replacing <c>Options</c> in a <c>with</c>
    ///     expression would remove the <see cref="ProductValidateOption" /> and its checks.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if an argument is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="targets" /> is empty, or the product and shared targets list a target twice (each
    ///     product needs its own targets), the gate or <c>DetectAffectedProducts</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown if a target names an unknown product.</exception>
    /// <remarks>
    ///     <para>
    ///         The workflow runs <c>DetectAffectedProducts</c> first. Each product target runs only when that target
    ///         succeeded and its product is affected (<c>Tex.Success() &amp;&amp; Tex.ProductAffected(product)</c>; the
    ///         success check keeps Azure DevOps from running it after a failure), and gets the <c>product</c>
    ///         parameter, so it builds with the product's name, version and GitVersion configuration. The gate runs
    ///         only if a product job failed or the run was cancelled. On GitHub, a skipped required check counts as
    ///         passed, so require only the gate.
    ///     </para>
    ///     <para>
    ///         <c>atom gen</c> fails if a product target does not depend directly on <c>DetectAffectedProducts</c>,
    ///         depends on a target that is not a product target of the same product or a shared target, a shared target
    ///         depends on a product target or an unlisted target, the gate does not depend directly on every product
    ///         target, or a shared target is not among the gate's dependencies (see
    ///         <see cref="ProductValidateOption" />).
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// WorkflowPresets.ProductValidate(this,
    ///     nameof(ValidateGate),
    ///     [new("AppA", nameof(TestAppA)), new("AppB", nameof(TestAppB))]) with
    /// {
    ///     Types = [WorkflowTypes.GitHub.Action],
    /// },
    ///     </code>
    /// </example>
    public static WorkflowDefinition ProductValidate(
        IProductsDefinition build,
        string gateTarget,
        IReadOnlyList<ProductWorkflowTarget> targets,
        string name = ProductValidateName,
        IReadOnlyList<WorkflowTargetDefinition>? sharedTargets = null)
    {
        ArgumentNullException.ThrowIfNull(build);
        ArgumentException.ThrowIfNullOrWhiteSpace(gateTarget);
        ArgumentNullException.ThrowIfNull(targets);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (targets.Count is 0)
            throw new ArgumentException("At least one product target is needed.", nameof(targets));

        const string detectTarget = nameof(IProductsDefinition.DetectAffectedProducts);

        sharedTargets ??= [];

        var allTargetNames = targets
            .Select(x => x.Target.Name)
            .Concat(sharedTargets.Select(x => x.Name))
            .ToList();

        var reserved = allTargetNames
            .Where(x => x == gateTarget || x == detectTarget)
            .Distinct()
            .ToList();

        if (reserved.Count > 0)
            throw new ArgumentException(
                $"'{string.Join("', '", reserved)}' cannot be a product or shared target: the workflow adds it itself.",
                nameof(targets));

        var duplicates = allTargetNames
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        if (duplicates.Count > 0)
            throw new ArgumentException(
                $"Target '{string.Join("', '", duplicates)}' is listed more than once. Each product needs its own targets, because a job runs with one product; a target that does not depend on the product can be a shared target.",
                nameof(targets));

        var productTargets = targets
            .Select(x => x with
            {
                Product = build.GetProduct(x.Product)
                    .Name,
            })
            .ToList();

        return new(name)
        {
            Triggers = [WorkflowTriggers.PullIntoMain, WorkflowTriggers.Manual],
            Options =
            [
                new ProductValidateOption(gateTarget,
                    productTargets,
                    sharedTargets
                        .Select(x => x.Name)
                        .ToList()),
            ],
            Targets =
            [
                new(detectTarget),
                .. sharedTargets,
                .. productTargets.Select(x => x.Target.WithOptions(
                    BuildOptions.Target.RunIfWorkflowCondition(Tex.Success() && Tex.ProductAffected(x.Product)),
                    BuildOptions.Inject.Param(nameof(IProductsDefinition.ProductName), x.Product))),
                new(gateTarget)
                {
                    Options = [BuildOptions.Target.RunIfAnyDependencyFailed],
                },
            ],
        };
    }
}
