namespace Invex.Atom.Workflows;

/// <summary>
///     Resolves a <see cref="WorkflowDefinition" /> into a fully structured <see cref="WorkflowModel" />.
/// </summary>
/// <param name="buildModel">The resolved build model.</param>
/// <param name="buildOptionService">The service providing the resolved set of build options.</param>
/// <param name="logger">The logger for diagnostics.</param>
internal sealed class WorkflowResolver(
    BuildModel buildModel,
    IBuildOptionService buildOptionService,
    ILogger<WorkflowResolver> logger
)
{
    /// <summary>
    ///     Resolves a <see cref="WorkflowDefinition" /> into a <see cref="WorkflowModel" />,
    ///     including job and step ordering, and dependency resolution.
    /// </summary>
    /// <param name="definition">The workflow definition to resolve.</param>
    /// <returns>A fully resolved <see cref="WorkflowModel" />.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if a consumed variable is not produced by its declared target.
    /// </exception>
    public WorkflowModel Resolve(WorkflowDefinition definition)
    {
        // Get all default options from BuildOptionService (includes IBuildDefinition.BuildOptions + providers),
        // plus GlobalWorkflowOptions from the build definition and the workflow-level options
        var workflowOptions = buildOptionService
            .Options
            .Concat(definition.Options)
            .ToList();

        // If there are no steps, we can return a simple workflow
        if (definition.Targets.Count is 0)
            return new()
            {
                Name = definition.Name,
                Triggers = definition.Triggers,
                Options = workflowOptions,
                Jobs = [],
            };

        // Transform all step definitions into steps
        var definedSteps = definition
            .Targets
            .Select(targetDefinition => targetDefinition.CreateModel(workflowOptions))
            .ToList();

        // Turn command steps into jobs
        var definedCommandJobs = definedSteps.ConvertAll(step => new WorkflowJobModel
        {
            Name = step.Name,
            TargetStep = step,
            JobDependencies = buildModel
                .GetTarget(step.Name)
                .Dependencies
                .Select(x => x.Name)
                .ToList(),
        });

        // If this workflow uses a custom artifact provider, we need to ensure that steps that
        // consume or produce artifacts are dependent on the Setup step.
        // It will be up to the WorkflowWriter to implement the download/upload steps.
        if (UseCustomArtifactProvider.IsEnabled(workflowOptions))
            definedCommandJobs = definedCommandJobs.ConvertAll(job =>
            {
                var suppressArtifactPublishing = SuppressArtifactPublishingOption.IsEnabled(job.TargetStep.Options);

                if (suppressArtifactPublishing)
                    return job;

                if (buildModel.GetTarget(job.TargetStep.Name) is { ConsumedArtifacts.Count: > 0 }
                    or { ProducedArtifacts.Count: > 0 })
                    return job with
                    {
                        JobDependencies = job
                            .JobDependencies
                            .Append(nameof(ISetupBuildInfo.SetupBuildInfo))
                            .ToList(),
                    };

                return job;
            });

        // Check that all consumed variables are produced by the target they are consumed from to avoid errors later on
        foreach (var job in definedCommandJobs)
        {
            var target = buildModel.GetTarget(job.Name);

            foreach (var consumedVariable in target.ConsumedVariables)
            {
                var consumedTarget = buildModel.GetTarget(consumedVariable.TargetName);

                var jobOutput =
                    consumedTarget.ProducedVariables.SingleOrDefault(x => x == consumedVariable.VariableName);

                if (jobOutput is null)
                    throw new InvalidOperationException(
                        $"Target '{job.Name}' consumes variable '{consumedVariable.VariableName}' from target '{consumedVariable.TargetName}', which does not produce that variable.");
            }
        }

        if (ProductValidateOption.Get(definition.Options) is { } productValidate)
            ValidateProductWiring(definition, productValidate);

        // Check implicit dependencies and warn if they aren't actually listed as dependencies
        foreach (var job in definedCommandJobs)
        foreach (var implicitDependencyOption in job.TargetStep.Options.OfType<IImplicitTargetDependencyOption>())
        foreach (var implicitDependency in implicitDependencyOption.TargetNames)
            if (job.JobDependencies.All(x => x != implicitDependency))
                logger.LogWarning(
                    "Job '{JobName}' has an implicit dependency on target '{TargetName}' via option '{OptionType}' that is not listed as a dependency. This may lead to unexpected behavior when running the workflow.",
                    job.Name,
                    implicitDependency,
                    implicitDependencyOption.GetType()
                        .Name);

        // Add all targets that are not already defined as jobs
        var jobs = definedCommandJobs
            .Concat(buildModel
                .Targets
                .Where(target => definedCommandJobs.All(job => job.Name != target.Name))
                .Select(target => new WorkflowJobModel
                {
                    Name = target.Name,
                    TargetStep = new()
                    {
                        Name = target.Name,
                        MatrixDimensions = [],
                        Options = workflowOptions,
                    },
                    JobDependencies = [],
                }))
            .ToList();

        // Remove jobs that are not defined and not depended on by any other defined or dependent job
        while (jobs.Count > 0)
        {
            var removedJob = false;

            foreach (var job in jobs.Where(job => definedCommandJobs.All(definedJob => definedJob.Name != job.Name)))
            {
                if (jobs
                    .Where(x => x != job)
                    .SelectMany(x => x.JobDependencies)
                    .Any(x => x == job.Name))
                    continue;

                jobs.Remove(job);
                removedJob = true;

                break;
            }

            if (!removedJob)
                break;
        }

        // Order jobs based on dependencies
        return new()
        {
            Name = definition.Name,
            Triggers = definition.Triggers,
            Options = workflowOptions,
            Jobs = OrderJobs(jobs),
        };
    }

    /// <summary>
    ///     Orders a list of workflow jobs based on their dependencies.
    /// </summary>
    /// <param name="jobs">The list of jobs to order.</param>
    /// <returns>A new list of jobs, ordered by their dependencies.</returns>
    private static List<WorkflowJobModel> OrderJobs(List<WorkflowJobModel> jobs)
    {
        var orderedJobs = new List<WorkflowJobModel>();

        foreach (var job in jobs)
            AddJob(job);

        return orderedJobs;

        void AddJob(WorkflowJobModel target)
        {
            foreach (var jobDep in jobs
                         .Where(x => x.Name == target.Name)
                         .SelectMany(x => x.JobDependencies))
                AddJob(jobs.Single(x => x.Name == jobDep));

            if (orderedJobs.Contains(target))
                return;

            orderedJobs.Add(target);
        }
    }

    /// <summary>
    ///     Checks that each product target depends directly on <c>DetectAffectedProducts</c> and otherwise only on
    ///     product targets of the same product, and that the gate depends directly on each product target.
    /// </summary>
    /// <param name="definition">The workflow.</param>
    /// <param name="option">The targets to check.</param>
    /// <exception cref="InvalidOperationException">Thrown if a dependency is missing.</exception>
    /// <exception cref="ArgumentException">Thrown if a target is unknown.</exception>
    private void ValidateProductWiring(WorkflowDefinition definition, ProductValidateOption option)
    {
        const string detectTarget = nameof(IProductsDefinition.DetectAffectedProducts);

        var errors = new List<string>();
        var sharedTargets = (option.SharedTargets ?? []).ToHashSet(StringComparer.Ordinal);

        // A target that is not in the workflow would be added as a job without its options (condition and product)
        errors.AddRange(option
            .ProductTargets
            .Select(x => x.Target.Name)
            .Concat(sharedTargets)
            .Append(option.GateTarget)
            .Distinct()
            .Where(x => definition.Targets.All(t => t.Name != x))
            .Select(x =>
                $"Target '{x}' is not one of the workflow's targets, so it would be added as a job without its options. Add it to the workflow's targets."));

        // A target listed for several products takes the first; the preset rejects duplicates
        var productByTarget = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var productTarget in option.ProductTargets)
            productByTarget.TryAdd(productTarget.Target.Name, productTarget.Product);

        // Unknown targets have already failed resolution, so every target here exists
        foreach (var (targetName, productName) in productByTarget)
        {
            var dependencies = buildModel
                .GetTarget(targetName)
                .Dependencies;

            if (dependencies.All(x => x.Name != detectTarget))
                errors.Add(
                    $"Product target '{targetName}' must depend directly on '{detectTarget}' (.DependsOn(nameof(IProductsDefinition.{detectTarget}))), or its job cannot read the affected products and is always skipped.");

            foreach (var dependency in dependencies.Where(x => x.Name != detectTarget && !sharedTargets.Contains(x.Name)))
                if (!productByTarget.TryGetValue(dependency.Name, out var dependencyProduct))
                    errors.Add(
                        $"Product target '{targetName}' depends on '{dependency.Name}', which is not a product or shared target of the workflow, so it would be added as a job without options or dependencies. List it as a product target of '{productName}' if it builds the product, as a shared target if it does not depend on the product, or remove the dependency.");
                else if (!string.Equals(dependencyProduct, productName, StringComparison.OrdinalIgnoreCase))
                    errors.Add(
                        $"Product target '{targetName}' of product '{productName}' depends on product target '{dependency.Name}' of product '{dependencyProduct}', so it is skipped whenever '{productName}' is affected but '{dependencyProduct}' is not, and the gate passes. Give each product its own targets.");
        }

        var gate = buildModel.GetTarget(option.GateTarget);

        errors.AddRange(productByTarget
            .Keys
            .Where(x => gate.Dependencies.All(d => d.Name != x))
            .Select(x =>
                $"Gate target '{option.GateTarget}' must depend directly on product target '{x}', or the gate does not fail when that job fails."));

        errors.AddRange(gate
            .Dependencies
            .Where(x => x.Name != detectTarget &&
                        !productByTarget.ContainsKey(x.Name) &&
                        !sharedTargets.Contains(x.Name))
            .Select(x =>
                $"Gate target '{option.GateTarget}' depends on '{x.Name}', which is not a product or shared target of the workflow, so it would be added as a job without options or dependencies."));

        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"Workflow '{definition.Name}' is not wired correctly: {string.Join(" ", errors)}");
    }
}
