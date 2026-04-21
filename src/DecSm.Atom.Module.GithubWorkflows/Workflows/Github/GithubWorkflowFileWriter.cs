namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github;

internal sealed class GithubWorkflowFileWriter(
    IAtomFileSystem atomFileSystem,
    GithubWorkflowBuilder workflowBuilder,
    IWorkflowExpressionResolver expressionResolver,
    ILogger<WorkflowFileWriter<GithubWorkflowType>> logger
) : WorkflowFileWriter<GithubWorkflowType>(atomFileSystem, logger)
{
    private readonly IAtomFileSystem _atomFileSystem = atomFileSystem;

    protected override string FileExtension => "yml";

    protected override RootedPath FileLocation => _atomFileSystem.AtomRootDirectory / ".github" / "workflows";

    protected override void WriteWorkflow(WorkflowModel workflow)
    {
        var resolvedWorkflow = workflowBuilder.Build(workflow);
        WriteWorkflow(resolvedWorkflow);
    }

    private void WriteWorkflow(GithubWorkflow githubWorkflow)
    {
        if (githubWorkflow.Name is { Length: > 0 } name)
            WriteProperty("name", name);

        var runName = expressionResolver.Resolve(githubWorkflow.RunName);

        if (runName is { Length: > 0 })
            WriteProperty("run-name", runName);

        if (githubWorkflow.Name is { Length: > 0 } || runName is { Length: > 0 })
            Writer.WriteLine();

        WriteOn(githubWorkflow.On);
        Writer.WriteLine();

        if (githubWorkflow.Permissions is { } permissions)
        {
            WritePermissions(permissions);
            Writer.WriteLine();
        }

        if (githubWorkflow.Env is { } env)
        {
            WriteEnv(env);
            Writer.WriteLine();
        }

        if (githubWorkflow.Concurrency is { } concurrency)
        {
            WriteConcurrency(concurrency);
            Writer.WriteLine();
        }

        using (Writer.WriteSection("jobs:"))
        {
            Writer.WriteLine();

            foreach (var job in githubWorkflow.Jobs)
                WriteJob(githubWorkflow, job);
        }
    }

    private void WriteConcurrency(Concurrency? concurrency)
    {
        if (concurrency is null)
            return;

        using (Writer.WriteSection("concurrency:"))
        {
            Writer.WriteLine(Format(expressionResolver.Resolve(concurrency.Group)));

            if (concurrency.CancelInProgress is { } cancelInProgress)
                WriteProperty("cancel-in-progress", expressionResolver.Resolve(cancelInProgress));
        }
    }

    private void WriteEnv(IReadOnlyDictionary<string, WorkflowExpression>? env)
    {
        if (env is not { Count: > 0 })
            return;

        using var _ = Writer.WriteSection("env:");

        foreach (var (key, value) in env)
            WriteProperty(key, expressionResolver.Resolve(value));
    }

    private void WritePermissions(Permissions? permissions)
    {
        if (permissions is null)
            return;

        switch (permissions)
        {
            case Permissions.All { Level: PermissionsLevel.Read }:
                Writer.WriteLine("permissions: read-all");

                break;
            case Permissions.All { Level: PermissionsLevel.Write }:
                Writer.WriteLine("permissions: write-all");

                break;
            case Permissions.All { Level: PermissionsLevel.None }:
                Writer.WriteLine("permissions: { }");

                break;

            case Permissions.Exact exact:
                using (Writer.WriteSection("permissions:"))
                {
                    if (exact.Permissions.Actions is { } actionsPermission)
                        WriteProperty("actions",
                            actionsPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.Attestations is { } attestationsPermission)
                        WriteProperty("attestations",
                            attestationsPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.Checks is { } checksPermission)
                        WriteProperty("checks",
                            checksPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.Contents is { } contentsPermission)
                        WriteProperty("contents",
                            contentsPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.Deployments is { } deploymentsPermission)
                        WriteProperty("deployments",
                            deploymentsPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.IdTokens is { } idTokensPermission)
                        WriteProperty("id-token",
                            idTokensPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.Issues is { } issuesPermission)
                        WriteProperty("issues",
                            issuesPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.Packages is { } packagesPermission)
                        WriteProperty("packages",
                            packagesPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.Pages is { } pagesPermission)
                        WriteProperty("pages",
                            pagesPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.PullRequests is { } pullRequestsPermission)
                        WriteProperty("pull-requests",
                            pullRequestsPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.RepositoryProjects is { } repositoryProjectsPermission)
                        WriteProperty("repository-projects",
                            repositoryProjectsPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.SecurityEvents is { } securityEventsPermission)
                        WriteProperty("security-events",
                            securityEventsPermission
                                .ToString()
                                .ToLowerInvariant());

                    if (exact.Permissions.Statuses is { } statusesPermission)
                        WriteProperty("statuses",
                            statusesPermission
                                .ToString()
                                .ToLowerInvariant());
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(permissions));
        }
    }

    private void WriteOn(IReadOnlyList<On> workflowOn)
    {
        using var _ = Writer.WriteSection("on:");

        var orderedOn = workflowOn.OrderBy(x => x.GetType()
            .FullName);

        foreach (var on in orderedOn)
            switch (on)
            {
                case On.BranchProtectionRule branchProtectionRule:
                    using (Writer.WriteSection("branch_protection_rule:"))
                        WriteProperty("types", branchProtectionRule.Types.Select(x => x.ToString()));

                    break;
                case On.CheckRun checkRun:
                    using (Writer.WriteSection("check_run:"))
                        WriteProperty("types", checkRun.Types.Select(x => x.ToString()));

                    break;
                case On.CheckSuite checkSuite:
                    using (Writer.WriteSection("check_suite:"))
                        WriteProperty("types", checkSuite.Types.Select(x => x.ToString()));

                    break;
                case On.Create:
                    Writer.WriteLine("create");

                    break;
                case On.Delete:
                    Writer.WriteLine("delete");

                    break;
                case On.Deployment:
                    Writer.WriteLine("deployment");

                    break;
                case On.DeploymentStatus:
                    Writer.WriteLine("deployment_status");

                    break;
                case On.Discussion discussion:
                    using (Writer.WriteSection("discussion:"))
                        WriteProperty("types", discussion.Types.Select(x => x.ToString()));

                    break;
                case On.DiscussionComment discussionComment:
                    using (Writer.WriteSection("discussion_comment:"))
                        WriteProperty("types", discussionComment.Types.Select(x => x.ToString()));

                    break;
                case On.Fork:
                    Writer.WriteLine("fork");

                    break;
                case On.Gollum:
                    Writer.WriteLine("gollum");

                    break;

                case On.ImageVersion imageVersion:
                    using (Writer.WriteSection("image_version:"))
                    {
                        if (imageVersion.Names is { Count: > 0 } names)
                            WriteProperty("names", names);

                        if (imageVersion.Versions is { Count: > 0 } versions)
                            WriteProperty("versions", versions);
                    }

                    break;
                case On.IssueComment issueComment:
                    using (Writer.WriteSection("issue_comment:"))
                        WriteProperty("types", string.Join(", ", issueComment.Types.Select(x => x.ToString())));

                    break;
                case On.Issues issues:
                    using (Writer.WriteSection("issues:"))
                        WriteProperty("types", string.Join(", ", issues.Types.Select(x => x.ToString())));

                    break;
                case On.Label label:
                    using (Writer.WriteSection("label:"))
                        WriteProperty("types", string.Join(", ", label.Types.Select(x => x.ToString())));

                    break;
                case On.MergeGroup mergeGroup:
                    using (Writer.WriteSection("merge_group:"))
                        WriteProperty("types", string.Join(", ", mergeGroup.Types.Select(x => x.ToString())));

                    break;
                case On.Milestone milestone:
                    using (Writer.WriteSection("milestone:"))
                        WriteProperty("types", string.Join(", ", milestone.Types.Select(x => x.ToString())));

                    break;
                case On.PageBuild:
                    Writer.WriteLine("page_build");

                    break;
                case On.Project project:
                    using (Writer.WriteSection("project:"))
                        WriteProperty("types", project.Types.Select(x => x.ToString()));

                    break;
                case On.ProjectCard projectCard:
                    using (Writer.WriteSection("project_card:"))
                        WriteProperty("types", string.Join(", ", projectCard.Types.Select(x => x.ToString())));

                    break;
                case On.ProjectColumn projectColumn:
                    using (Writer.WriteSection("project_column:"))
                        WriteProperty("types", string.Join(", ", projectColumn.Types.Select(x => x.ToString())));

                    break;
                case On.Public:
                    Writer.WriteLine("public");

                    break;

                case On.PullRequest pullRequest:
                    using (Writer.WriteSection("pull_request:"))
                    {
                        if (pullRequest.Types.Count > 0)
                            WriteProperty("types", pullRequest.Types.Select(x => x.ToString()));

                        if (pullRequest.Branches?.Count > 0)
                            WriteProperty("branches", pullRequest.Branches);

                        if (pullRequest.BranchesIgnore?.Count > 0)
                            WriteProperty("branches-ignore", pullRequest.BranchesIgnore);

                        if (pullRequest.Tags?.Count > 0)
                            WriteProperty("tags", pullRequest.Tags);

                        if (pullRequest.TagsIgnore?.Count > 0)
                            WriteProperty("tags-ignore", pullRequest.TagsIgnore);

                        if (pullRequest.Paths?.Count > 0)
                            WriteProperty("paths", pullRequest.Paths);

                        if (pullRequest.PathsIgnore?.Count > 0)
                            WriteProperty("paths-ignore", pullRequest.PathsIgnore);
                    }

                    break;
                case On.PullRequestReview pullRequestReview:
                    using (Writer.WriteSection("pull_request_review:"))
                        WriteProperty("types", pullRequestReview.Types.Select(x => x.ToString()));

                    break;
                case On.PullRequestReviewComment pullRequestReviewComment:
                    using (Writer.WriteSection("pull_request_review_comment:"))
                        WriteProperty("types", pullRequestReviewComment.Types.Select(x => x.ToString()));

                    break;
                case On.PullRequestTarget pullRequestTarget:
                    using (Writer.WriteSection("pull_request_target:"))
                        WriteProperty("types", pullRequestTarget.Types.Select(x => x.ToString()));

                    break;

                case On.Push push:
                    using (Writer.WriteSection("push:"))
                    {
                        if (push.Branches is { Count: > 0 } branches)
                            WriteProperty("branches", branches);

                        if (push.BranchesIgnore is { Count: > 0 } branchesIgnore)
                            WriteProperty("branches-ignore", branchesIgnore);

                        if (push.Tags is { Count: > 0 } tags)
                            WriteProperty("tags", tags);

                        if (push.TagsIgnore is { Count: > 0 } tagsIgnore)
                            WriteProperty("tags-ignore", tagsIgnore);
                    }

                    break;
                case On.RegistryPackage registryPackage:
                    using (Writer.WriteSection("registry_package:"))
                        WriteProperty("types", registryPackage.Types.Select(x => x.ToString()));

                    break;
                case On.Release release:
                    using (Writer.WriteSection("release:"))
                        WriteProperty("types", release.Types.Select(x => x.ToString()));

                    break;
                case On.RepositoryDispatch repositoryDispatch:
                    using (Writer.WriteSection("repository_dispatch:"))
                        WriteProperty("types", repositoryDispatch.Types.Select(x => x.ToString()));

                    break;

                case On.Schedule schedule:
                    using (Writer.WriteSection("schedule:"))
                        WriteProperty("cron", schedule.Crons);

                    break;
                case On.Status:
                    Writer.WriteLine("status");

                    break;
                case On.Watch watch:
                    using (Writer.WriteSection("watch:"))
                        WriteProperty("types", watch.Types.Select(x => x.ToString()));

                    break;
                case On.WorkflowCall:
                    Writer.WriteLine("workflow_call");

                    break;

                case On.WorkflowDispatch workflowDispatch:
                    using (Writer.WriteSection("workflow_dispatch:"))
                        if (workflowDispatch.Inputs is { Count: > 0 } inputs)
                            using (Writer.WriteSection("inputs:"))
                                foreach (var input in inputs)
                                    using (Writer.WriteSection($"{input.Name}:"))
                                    {
                                        if (input.Description is { } description)
                                            WriteProperty("description", description);

                                        if (input.Required is { } required)
                                            WriteProperty("required",
                                                required
                                                    .ToString()
                                                    .ToLowerInvariant());

                                        if (input.Default is { } defaultValue)
                                            WriteProperty("default", defaultValue);

                                        WriteProperty("type", input.Type);

                                        switch (input)
                                        {
                                            case WorkflowDispatchInput.Choice choice:
                                                if (choice.Options is { Count: > 0 } options)
                                                    WriteProperty("options", options);

                                                break;
                                            case WorkflowDispatchInput.Boolean:
                                            case WorkflowDispatchInput.Number:
                                            case WorkflowDispatchInput.String:
                                                break;
                                        }
                                    }

                    break;

                case On.WorkflowRun workflowRun:
                    using (Writer.WriteSection("workflow_run:"))
                    {
                        if (workflowRun.Workflows is { Count: > 0 } workflows)
                            WriteProperty("workflows", workflows);

                        if (workflowRun.Branches is { Count: > 0 } branches)
                            WriteProperty("branches", branches);

                        if (workflowRun.Types is { Count: > 0 } types)
                            WriteProperty("types", types.Select(x => x.ToString()));
                    }

                    break;
            }
    }

    private void WriteJob(GithubWorkflow githubWorkflow, Job job)
    {
        using var _ = Writer.WriteSection($"{expressionResolver.Resolve(job.Name)}:");

        if (job.Permissions is { } permissions && permissions != githubWorkflow.Permissions)
            WritePermissions(permissions);

        if (job.Needs is { Count: > 0 } needs)
            WriteProperty("needs", needs.Select(x => expressionResolver.Resolve(x)));

        if (expressionResolver.Resolve(job.If) is { Length: > 0 } condition)
            WriteProperty("if", condition);

        if (job.RunsOn is { Group: null, Labels.Count: 1 })
        {
            var value = job.RunsOn.Labels[0];
            WriteProperty("runs-on", expressionResolver.Resolve(value));
        }
        else
            using (Writer.WriteSection("runs-on:"))
                if (expressionResolver.Resolve(job.RunsOn.Group) is { Length: > 0 } group)
                    WriteProperty("group", group);
                else if (job.RunsOn.Labels.Count > 0)
                    WriteProperty("labels", job.RunsOn.Labels.Select(x => expressionResolver.Resolve(x)));

        switch (job.Snapshot)
        {
            case { Version: not null }:
                using (Writer.WriteSection("snapshot:"))
                {
                    WriteProperty("image-name", expressionResolver.Resolve(job.Snapshot.ImageName));
                    WriteProperty("version", expressionResolver.Resolve(job.Snapshot.Version));
                }

                break;

            case { Version: null }:
                WriteProperty("snapshot", expressionResolver.Resolve(job.Snapshot.ImageName));

                break;
        }

        switch (job.Environment)
        {
            case { UrlValue: not null }:
                using (Writer.WriteSection("environment:"))
                {
                    WriteProperty("name", expressionResolver.Resolve(job.Environment.Name));
                    WriteProperty("url", expressionResolver.Resolve(job.Environment.UrlValue!));
                }

                break;

            case { UrlValue: null }:
                WriteProperty("environment", expressionResolver.Resolve(job.Environment.Name));

                break;
        }

        WriteConcurrency(job.Concurrency);

        if (job.Outputs is { Count: > 0 } outputs)
            using (Writer.WriteSection("outputs:"))
                foreach (var (key, value) in outputs)
                    WriteProperty(key, expressionResolver.Resolve(value));

        WriteEnv(job.Env);

        if (job.TimeoutMinutes is { } timeout)
            WriteProperty("timeout-minutes", expressionResolver.Resolve(timeout));

        WriteStrategy(job.Strategy);

        if (job.ContinueOnError is { } continueOnError)
            WriteProperty("continue-on-error", expressionResolver.Resolve(continueOnError));

        WriteContainer(job.Container);

        if (job.Services is { Count: > 0 } services)
            using (Writer.WriteSection("services:"))
                foreach (var service in services)
                    using (Writer.WriteSection($"{service.Key}:"))
                        WriteContainer(service.Value);

        if (job.Steps is { Count: > 0 } steps)
            using (Writer.WriteSection("steps:"))
            {
                Writer.WriteLine();

                foreach (var step in steps)
                    WriteStep(step);
            }
    }

    private void WriteContainer(Container? jobContainer)
    {
        if (jobContainer is null)
            return;

        using var containerSection = Writer.WriteSection("container:");

        WriteProperty("image", expressionResolver.Resolve(jobContainer.Image));

        if (jobContainer.Credentials is { } credentials)
        {
            using var credentialsSection = Writer.WriteSection("credentials:");

            if (credentials.Username is { } username)
                WriteProperty("username", expressionResolver.Resolve(username));

            if (credentials.Password is { } password)
                WriteProperty("password", expressionResolver.Resolve(password));
        }

        WriteEnv(jobContainer.Env);

        if (jobContainer.Ports is { } ports)
            WriteProperty("ports", ports.Select(x => expressionResolver.Resolve(x)));

        if (jobContainer.Volumes is { } volumes)
            WriteProperty("volumes", volumes.Select(x => expressionResolver.Resolve(x)));

        if (jobContainer.Options is { } options)
            WriteProperty("options", expressionResolver.Resolve(options));
    }

    private void WriteStrategy(Strategy? strategy)
    {
        if (strategy is null)
            return;

        using var strategySection = Writer.WriteSection("strategy:");

        if (strategy.FailFast is { } failFast)
            WriteProperty("fail-fast", expressionResolver.Resolve(failFast));

        if (strategy.MaxParallel is { } maxParallel)
            WriteProperty("max-parallel", expressionResolver.Resolve(maxParallel));

        using var matrixSection = Writer.WriteSection("matrix:");

        if (strategy.Matrix.Map is { } map)
            foreach (var (key, value) in map.Where(x => x.Value.Count > 0))
                WriteProperty(key, value.Select(x => expressionResolver.Resolve(x)));

        if (strategy.Matrix.Include is { } include)
            foreach (var includeEntry in include)
            {
                if (includeEntry.Count is 0)
                    continue;

                var pairs = includeEntry.ToList();

                using (Writer.WriteSection($"- {pairs[0].Key}: {pairs[0].Value}"))
                    foreach (var (key, value) in pairs.Skip(1))
                        WriteProperty(key, expressionResolver.Resolve(value));
            }

        if (strategy.Matrix.Exclude is { } exclude)
            foreach (var excludeEntry in exclude)
            {
                if (excludeEntry.Count is 0)
                    continue;

                var pairs = excludeEntry.ToList();

                using (Writer.WriteSection($"- {pairs[0].Key}: {pairs[0].Value}"))
                    foreach (var (key, value) in pairs.Skip(1))
                        WriteProperty(key, expressionResolver.Resolve(value));
            }
    }

    private void WriteStep(Step step)
    {
        IDisposable? section = null;

        if (expressionResolver.Resolve(step.Name) is { Length: > 0 } name)
            WriteSectionOrProperty("name", name);

        if (step.Id is { Length: > 0 } id)
            WriteSectionOrProperty("id", id);

        if (expressionResolver.Resolve(step.If) is { Length: > 0 } condition)
            WriteSectionOrProperty("if", condition);

        switch (step)
        {
            case Step.RunStep runStep:
                switch (runStep.Run)
                {
                    case { Count: 1 } single:
                        WriteSectionOrProperty("run", expressionResolver.Resolve(single[0]));

                        break;

                    default:
                        WriteSectionOrProperty("run", runStep.Run.Select(x => expressionResolver.Resolve(x)));

                        break;
                }

                if (runStep.Shell is { } shell)
                    WriteProperty("shell", expressionResolver.Resolve(shell));

                break;

            case Step.UsesStep usesStep:
                WriteSectionOrProperty("uses", expressionResolver.Resolve(usesStep.Uses));

                break;
        }

        if (step.WorkingDirectory is { } workingDirectory)
            WriteProperty("working-directory", expressionResolver.Resolve(workingDirectory));

        if (step.With is { Count: > 0 } with)
            using (Writer.WriteSection("with:"))
                foreach (var (key, value) in with)
                    switch (value)
                    {
                        case { Count: 1 } single:
                            WriteProperty(key, expressionResolver.Resolve(single[0]));

                            break;
                        default:
                            WriteProperty(key, value.Select(x => expressionResolver.Resolve(x)));

                            break;
                    }

        WriteEnv(step.Env);

        if (step.ContinueOnError is { } continueOnError)
            WriteProperty("continue-on-error", expressionResolver.Resolve(continueOnError));

        if (step.TimeoutMinutes is { } timeout)
            WriteProperty("timeout-minutes", expressionResolver.Resolve(timeout));

        section?.Dispose();

        Writer.WriteLine();

        return;

        void WriteSectionOrProperty(string valueName, params IEnumerable<string> value)
        {
            var valueArray = value.ToArray();

            if (section is not null)
            {
                switch (valueArray.Length)
                {
                    case 0:
                        WriteProperty(valueName, "''");

                        break;

                    case 1:
                        WriteProperty(valueName, valueArray[0]);

                        break;

                    default:
                        WriteProperty(valueName, valueArray);

                        break;
                }

                return;
            }

            switch (valueArray.Length)
            {
                case 0:
                    section = WriteSection($"- {valueName}", "''");

                    break;

                case 1:
                    section = WriteSection($"- {valueName}", valueArray[0]);

                    break;

                default:
                    section = WriteSection($"- {valueName}", "|");

                    using (Writer.WriteSection(string.Empty))
                        foreach (var line in valueArray)
                            Writer.WriteLine(Format(line));

                    break;
            }
        }
    }

    private IDisposable WriteSection(string key, string value) =>
        value switch
        {
            { Length: 0 } => Writer.WriteSection($"{key}: ''"),
            _ => Writer.WriteSection($"{key}: {Format(value)}"),
        };

    private void WriteProperty(string key, string value)
    {
        var lines = value.Split('\r', '\n', StringSplitOptions.RemoveEmptyEntries);

        switch (lines.Length)
        {
            case 0:
                Writer.WriteLine($"{key}: ''");

                break;
            case 1:
                Writer.WriteLine($"{key}: {Format(value)}");

                break;

            default:
            {
                using (Writer.WriteSection($"{key}: |"))
                    foreach (var line in lines)
                        Writer.WriteLine(line); // Don't format lines here

                break;
            }
        }
    }

    private void WriteProperty(string key, IEnumerable<string> values)
    {
        var valueList = values.ToList();

        var valuesTotalLength = valueList.Sum(x => x.Length);

        if (valuesTotalLength < 80)
            Writer.WriteLine($"{key}: [ {string.Join(", ", valueList.Select(Format))} ]");
        else
            using (Writer.WriteSection($"{key}:"))
                foreach (var value in valueList)
                    Writer.WriteLine($"- {Format(value)}");
    }

    private static string Format(string value) =>

        // Standalone disallowed tokens
        value.Contains(" #") ||
        value.Contains(": ") ||
        value.EndsWith(':') ||

        // Mixing expressions with literals
        (value.StartsWith("${{") && !value.EndsWith("}}")) ||
        (!value.StartsWith("${{") && value.EndsWith("}}")) ||
        value.IndexOf("${{", StringComparison.Ordinal) != value.LastIndexOf("${{", StringComparison.Ordinal) ||

        // Mixing braces
        (value.Contains('[') && value.Contains('{')) ||
        (value.Contains(']') && value.Contains('}')) ||
        value.Count(x => x is '{') != value.Count(x => x is '}') ||
        value.Count(x => x is '[') != value.Count(x => x is ']')

            // Escape single quotes
            ? $"'{value.Replace("'", "''")}'"
            : value;
}
