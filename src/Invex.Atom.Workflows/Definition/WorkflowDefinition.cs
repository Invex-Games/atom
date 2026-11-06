namespace Invex.Atom.Workflows.Definition;

/// <summary>
///     Represents the definition of a workflow, including its name, triggers, options, targets, and types.
/// </summary>
/// <param name="Name">The name of the workflow.</param>
[PublicAPI]
public sealed record WorkflowDefinition(string Name)
{
    /// <summary>
    ///     Gets the collection of triggers that define when and how the workflow is initiated.
    /// </summary>
    public IReadOnlyList<IWorkflowTrigger> Triggers { get; init; } = [];

    /// <summary>
    ///     Gets the collection of options or parameters that can be configured for the workflow.
    /// </summary>
    public IReadOnlyList<IBuildOption> Options { get; init; } = [];

    /// <summary>
    ///     Gets the collection of targets that define the sequence of tasks to be executed by the workflow.
    /// </summary>
    public IReadOnlyList<WorkflowTargetDefinition> Targets { get; init; } = [];

    /// <summary>
    ///     Gets the collection of workflow types that this definition applies to (e.g., GitHub Actions, Azure DevOps).
    /// </summary>
    public IReadOnlyList<IWorkflowType> Types { get; init; } = [];

    /// <summary>
    ///     Returns a copy of this definition with more triggers.
    /// </summary>
    /// <param name="triggers">The triggers to add after the existing ones.</param>
    /// <returns>The new definition.</returns>
    public WorkflowDefinition WithTriggers(params IEnumerable<IWorkflowTrigger> triggers) =>
        this with
        {
            Triggers = Triggers
                .Concat(triggers)
                .ToList(),
        };

    /// <summary>
    ///     Returns a copy of this definition with more options.
    /// </summary>
    /// <param name="options">The options to add after the existing ones.</param>
    /// <returns>The new definition.</returns>
    public WorkflowDefinition WithOptions(params IEnumerable<IBuildOption> options) =>
        this with
        {
            Options = Options
                .Concat(options)
                .ToList(),
        };

    /// <summary>
    ///     Creates a workflow for one monorepo product, such as its build or release pipeline.
    /// </summary>
    /// <param name="build">The build, which declares the products.</param>
    /// <param name="productName">The product's name, as declared in <see cref="IProductsDefinition.Products" />.</param>
    /// <param name="name">The workflow's name, such as <c>Build</c>. The workflow is named <c>AppA - Build</c>.</param>
    /// <param name="branches">The branches whose pushes run the workflow; <c>main</c> if not given.</param>
    /// <param name="releaseTagTrigger">
    ///     Whether pushing one of the product's release tags (such as <c>appa/v1.2.0</c>) also runs the workflow. Use it
    ///     on Azure DevOps, which has no release trigger. The tags share the push trigger with the path filters, so
    ///     check that your Azure DevOps organisation does not apply the path filters to tag pushes, or a release tag
    ///     would not run the workflow. On GitHub Actions, add <c>WorkflowTriggers.GitHub.OnRelease()</c> instead:
    ///     creating a release also pushes its tag, so both triggers would run the workflow twice.
    /// </param>
    /// <returns>
    ///     A workflow definition with a push trigger and the product's parameter. Add triggers with
    ///     <see cref="WithTriggers" /> and options with <see cref="WithOptions" />, and set its targets and types with
    ///     a <c>with</c> expression. Do not replace <c>Options</c> in a <c>with</c> expression: that would remove the
    ///     product parameter and the release guard, so the jobs would build without the product.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="build" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="productName" /> or <paramref name="name" /> is empty.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the product is unknown, or the product dependencies are invalid.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         The workflow has a single push trigger for <paramref name="branches" />, filtered to the product's
    ///         paths, the paths of the products it depends on and the global paths
    ///         (<see cref="IProductsDefinition.GetProductPaths" />). Every job gets the <c>product</c> parameter, so
    ///         the build name, version, GitVersion configuration and release tags are the product's.
    ///     </para>
    ///     <para>
    ///         On GitHub Actions, if the workflow has a release trigger, every job only runs for releases whose tag
    ///         starts with the product's tag prefix (see <see cref="ProductWorkflowOption" />).
    ///     </para>
    ///     <para>
    ///         Use this for a product's own pipelines only. Pull request validation for all products belongs in one
    ///         workflow, see <see cref="WorkflowPresets.ProductValidate" />.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// WorkflowDefinition
    ///     .ForProduct(this, "AppA", "Build")
    ///     .WithTriggers(WorkflowTriggers.Manual, WorkflowTriggers.GitHub.OnRelease()) with
    ///     {
    ///         Targets = [new(nameof(PackAppA))],
    ///         Types = [WorkflowTypes.GitHub.Action],
    ///     },
    ///     </code>
    /// </example>
    public static WorkflowDefinition ForProduct(
        IProductsDefinition build,
        string productName,
        string name,
        IReadOnlyList<string>? branches = null,
        bool releaseTagTrigger = false)
    {
        ArgumentNullException.ThrowIfNull(build);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var product = build.GetProduct(productName.Trim());

        return new($"{product.Name} - {name}")
        {
            Triggers =
            [
                new GitPushTrigger
                {
                    IncludedBranches = branches ?? ["main"],
                    IncludedPaths = build.GetProductPaths(product),
                    IncludedTags = releaseTagTrigger
                        ? [ReleaseTag.Glob(product.TagPrefix)]
                        : [],
                },
            ],
            Options =
            [
                BuildOptions.Inject.Param(nameof(IProductsDefinition.ProductName), product.Name),
                new ProductWorkflowOption(product.Name, product.TagPrefix),
            ],
        };
    }
}
