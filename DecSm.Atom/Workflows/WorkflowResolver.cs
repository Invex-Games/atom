namespace DecSm.Atom.Workflows;

/// <summary>
///     Resolves a <see cref="WorkflowDefinition" /> into a fully structured <see cref="WorkflowModel" />.
/// </summary>
/// <param name="buildDefinition">The build definition containing global options and target information.</param>
/// <param name="buildModel">The resolved build model.</param>
/// <param name="workflowOptionProviders">A collection of workflow option providers.</param>
internal sealed class WorkflowResolver(
    IBuildDefinition buildDefinition,
    BuildModel buildModel,
    IEnumerable<IWorkflowOptionProvider> workflowOptionProviders
)
{
    private readonly IReadOnlyList<IWorkflowOptionProvider> _workflowOptionProviders = workflowOptionProviders.ToList();

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
        // Get all default options from BuildDefinition, WorkflowOptionProviders and WorkflowDefinition
        var workflowOptions = IWorkflowOption
            .Merge(buildDefinition
                .GlobalWorkflowOptions
                .Concat(_workflowOptionProviders.SelectMany(provider => provider.WorkflowOptions))
                .Concat(definition.Options))
            .ToList();

        // If there are no steps, we can return a simple workflow
        if (definition.Targets.Count is 0)
            return new(definition.Name)
            {
                Triggers = definition.Triggers,
                Options = workflowOptions,
                Jobs = [],
            };

        // Transform all step definitions into steps
        var definedSteps = definition
            .Targets
            .Select(targetDefinition => targetDefinition.CreateModel())
            .ToList();

        // Turn command steps into jobs
        var definedCommandJobs = definedSteps.ConvertAll(step => new WorkflowJobModel(step.Name, [step])
        {
            Options = step.Options,
            MatrixDimensions = step.MatrixDimensions,
            JobDependencies = buildModel
                .GetTarget(step.Name)
                .Dependencies
                .Select(x => x.Name)
                .ToList(),
        });

        // If this workflow uses a custom artifact provider, we need to ensure that steps that
        // consume or produce artifacts are dependent on the Setup step.
        // It will be up to the WorkflowWriter to implement the download/upload steps.
        if (workflowOptions.HasEnabledToggle<UseCustomArtifactProvider>())
            definedCommandJobs = definedCommandJobs.ConvertAll(job => job
                .Steps
                .Where(step => step is { SuppressArtifactPublishing: false })
                .Select(step => buildModel.GetTarget(step.Name))
                .Any(target => target.ConsumedArtifacts.Count > 0 || target.ProducedArtifacts.Count > 0)
                ? job with
                {
                    JobDependencies = job
                        .JobDependencies
                        .Append(nameof(ISetupBuildInfo.SetupBuildInfo))
                        .ToList(),
                }
                : job);

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

        // Add all targets that are not already defined as jobs
        var jobs = definedCommandJobs
            .Concat(buildModel
                .Targets
                .Select(target => target.Name)
                .Where(targetName => definedCommandJobs.All(job => job.Name != targetName))
                .Select(targetName => new WorkflowJobModel(targetName, [new(targetName)])
                {
                    JobDependencies = [],
                    Options = [],
                    MatrixDimensions = [],
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

        return new(definition.Name)
        {
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
}
