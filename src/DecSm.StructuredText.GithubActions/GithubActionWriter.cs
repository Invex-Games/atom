namespace DecSm.StructuredText.GithubActions;

[PublicAPI]
public sealed class GithubActionWriter
{
    private readonly GithubExpressionFormatter _expressionFormatter = new();

    public StructuredTextWriter TextWriter { get; init; } = new();

    public void Write(GithubAction githubAction)
    {
        if (githubAction.Name is { Length: > 0 } name)
            WriteProperty("name", name);

        var runName = _expressionFormatter.Format(githubAction.RunName);

        if (runName is { Length: > 0 })
            WriteProperty("run-name", runName);

        if (githubAction.Name is { Length: > 0 } || runName is { Length: > 0 })
            TextWriter.WriteLine();

        WriteOn(githubAction.On);
        TextWriter.WriteLine();

        if (githubAction.Permissions is { } permissions)
        {
            WritePermissions(permissions);
            TextWriter.WriteLine();
        }

        if (githubAction.Env is { } env)
        {
            WriteEnv(env);
            TextWriter.WriteLine();
        }

        if (githubAction.Concurrency is { } concurrency)
        {
            WriteConcurrency(concurrency);
            TextWriter.WriteLine();
        }

        using (TextWriter.WriteSection("jobs:"))
        {
            TextWriter.WriteLine();

            foreach (var job in githubAction.Jobs)
                WriteJob(githubAction, job);
        }
    }

    private void WriteConcurrency(Concurrency? concurrency)
    {
        if (concurrency is null)
            return;

        using (TextWriter.WriteSection("concurrency:"))
        {
            TextWriter.WriteLine(Format(_expressionFormatter.Format(concurrency.Group)));

            if (concurrency.CancelInProgress is { } cancelInProgress)
                WriteProperty("cancel-in-progress", _expressionFormatter.Format(cancelInProgress));
        }
    }

    private void WriteEnv(IReadOnlyDictionary<string, TextExpression>? env)
    {
        if (env is not { Count: > 0 })
            return;

        using var _ = TextWriter.WriteSection("env:");

        foreach (var (key, value) in env)
            WriteProperty(key, _expressionFormatter.Format(value));
    }

    private void WritePermissions(Permissions? permissions)
    {
        if (permissions is null)
            return;

        switch (permissions)
        {
            case Permissions.All { Level: PermissionsLevel.Read }:
                TextWriter.WriteLine("permissions: read-all");

                break;
            case Permissions.All { Level: PermissionsLevel.Write }:
                TextWriter.WriteLine("permissions: write-all");

                break;
            case Permissions.All { Level: PermissionsLevel.None }:
                TextWriter.WriteLine("permissions: { }");

                break;

            case Permissions.Exact exact:
                using (TextWriter.WriteSection("permissions:"))
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
        using var _ = TextWriter.WriteSection("on:");

        var orderedOn = workflowOn.OrderBy(x => x.GetType()
            .FullName);

        foreach (var on in orderedOn)
            switch (on)
            {
                case On.BranchProtectionRule branchProtectionRule:
                    using (TextWriter.WriteSection("branch_protection_rule:"))
                        WriteProperty("types", branchProtectionRule.Types.Select(x => x.ToString()));

                    break;

                case On.CheckRun checkRun:
                    using (TextWriter.WriteSection("check_run:"))
                        WriteProperty("types", checkRun.Types.Select(x => x.ToString()));

                    break;

                case On.CheckSuite checkSuite:
                    using (TextWriter.WriteSection("check_suite:"))
                        WriteProperty("types", checkSuite.Types.Select(x => x.ToString()));

                    break;

                case On.Create:
                    TextWriter.WriteLine("create");

                    break;

                case On.Delete:
                    TextWriter.WriteLine("delete");

                    break;

                case On.Deployment:
                    TextWriter.WriteLine("deployment");

                    break;

                case On.DeploymentStatus:
                    TextWriter.WriteLine("deployment_status");

                    break;

                case On.Discussion discussion:
                    using (TextWriter.WriteSection("discussion:"))
                        WriteProperty("types", discussion.Types.Select(x => x.ToString()));

                    break;

                case On.DiscussionComment discussionComment:
                    using (TextWriter.WriteSection("discussion_comment:"))
                        WriteProperty("types", discussionComment.Types.Select(x => x.ToString()));

                    break;

                case On.Fork:
                    TextWriter.WriteLine("fork");

                    break;

                case On.Gollum:
                    TextWriter.WriteLine("gollum");

                    break;

                case On.ImageVersion imageVersion:
                    using (TextWriter.WriteSection("image_version:"))
                    {
                        if (imageVersion.Names is { Count: > 0 } names)
                            WriteProperty("names", names);

                        if (imageVersion.Versions is { Count: > 0 } versions)
                            WriteProperty("versions", versions);
                    }

                    break;

                case On.IssueComment issueComment:
                    using (TextWriter.WriteSection("issue_comment:"))
                        WriteProperty("types", string.Join(", ", issueComment.Types.Select(x => x.ToString())));

                    break;

                case On.Issues issues:
                    using (TextWriter.WriteSection("issues:"))
                        WriteProperty("types", string.Join(", ", issues.Types.Select(x => x.ToString())));

                    break;

                case On.Label label:
                    using (TextWriter.WriteSection("label:"))
                        WriteProperty("types", string.Join(", ", label.Types.Select(x => x.ToString())));

                    break;

                case On.MergeGroup mergeGroup:
                    using (TextWriter.WriteSection("merge_group:"))
                        WriteProperty("types", string.Join(", ", mergeGroup.Types.Select(x => x.ToString())));

                    break;

                case On.Milestone milestone:
                    using (TextWriter.WriteSection("milestone:"))
                        WriteProperty("types", string.Join(", ", milestone.Types.Select(x => x.ToString())));

                    break;

                case On.PageBuild:
                    TextWriter.WriteLine("page_build");

                    break;

                case On.Project project:
                    using (TextWriter.WriteSection("project:"))
                        WriteProperty("types", project.Types.Select(x => x.ToString()));

                    break;

                case On.ProjectCard projectCard:
                    using (TextWriter.WriteSection("project_card:"))
                        WriteProperty("types", string.Join(", ", projectCard.Types.Select(x => x.ToString())));

                    break;

                case On.ProjectColumn projectColumn:
                    using (TextWriter.WriteSection("project_column:"))
                        WriteProperty("types", string.Join(", ", projectColumn.Types.Select(x => x.ToString())));

                    break;

                case On.Public:
                    TextWriter.WriteLine("public");

                    break;

                case On.PullRequest pullRequest:
                    using (TextWriter.WriteSection("pull_request:"))
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
                    using (TextWriter.WriteSection("pull_request_review:"))
                        WriteProperty("types", pullRequestReview.Types.Select(x => x.ToString()));

                    break;

                case On.PullRequestReviewComment pullRequestReviewComment:
                    using (TextWriter.WriteSection("pull_request_review_comment:"))
                        WriteProperty("types", pullRequestReviewComment.Types.Select(x => x.ToString()));

                    break;

                case On.PullRequestTarget pullRequestTarget:
                    using (TextWriter.WriteSection("pull_request_target:"))
                        WriteProperty("types", pullRequestTarget.Types.Select(x => x.ToString()));

                    break;

                case On.Push push:
                    using (TextWriter.WriteSection("push:"))
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
                    using (TextWriter.WriteSection("registry_package:"))
                        WriteProperty("types", registryPackage.Types.Select(x => x.ToString()));

                    break;

                case On.Release release:
                    using (TextWriter.WriteSection("release:"))
                        WriteProperty("types", release.Types.Select(x => x.ToString()));

                    break;

                case On.RepositoryDispatch repositoryDispatch:
                    using (TextWriter.WriteSection("repository_dispatch:"))
                        WriteProperty("types", repositoryDispatch.Types.Select(x => x));

                    break;

                case On.Schedule schedule:
                    using (TextWriter.WriteSection("schedule:"))
                        WriteProperty("cron", schedule.Crons);

                    break;

                case On.Status:
                    TextWriter.WriteLine("status");

                    break;

                case On.Watch watch:
                    using (TextWriter.WriteSection("watch:"))
                        WriteProperty("types", watch.Types.Select(x => x.ToString()));

                    break;

                case On.WorkflowCall:
                    TextWriter.WriteLine("workflow_call");

                    break;

                case On.WorkflowDispatch workflowDispatch:
                    using (TextWriter.WriteSection("workflow_dispatch:"))
                        if (workflowDispatch.Inputs is { Count: > 0 } inputs)
                            using (TextWriter.WriteSection("inputs:"))
                                foreach (var input in inputs)
                                    using (TextWriter.WriteSection($"{input.Name}:"))
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
                    using (TextWriter.WriteSection("workflow_run:"))
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

    private void WriteJob(GithubAction githubAction, Job job)
    {
        using var _ = TextWriter.WriteSection($"{_expressionFormatter.Format(job.Name)}:");

        if (job.Permissions is { } permissions && permissions != githubAction.Permissions)
            WritePermissions(permissions);

        if (job.Needs is { Count: > 0 } needs)
            WriteProperty("needs", needs.Select(x => _expressionFormatter.Format(x)));

        if (_expressionFormatter.Format(job.If) is { Length: > 0 } condition)
            WriteProperty("if", condition);

        if (job.RunsOn is { Group: null, Labels.Count: 1 })
        {
            var value = job.RunsOn.Labels[0];
            WriteProperty("runs-on", _expressionFormatter.Format(value));
        }
        else
        {
            using (TextWriter.WriteSection("runs-on:"))
                if (_expressionFormatter.Format(job.RunsOn.Group) is { Length: > 0 } group)
                    WriteProperty("group", group);
                else if (job.RunsOn.Labels.Count > 0)
                    WriteProperty("labels", job.RunsOn.Labels.Select(x => _expressionFormatter.Format(x)));
        }

        switch (job.Snapshot)
        {
            case { Version: not null }:
                using (TextWriter.WriteSection("snapshot:"))
                {
                    WriteProperty("image-name", _expressionFormatter.Format(job.Snapshot.ImageName));
                    WriteProperty("version", _expressionFormatter.Format(job.Snapshot.Version));
                }

                break;

            case { Version: null }:
                WriteProperty("snapshot", _expressionFormatter.Format(job.Snapshot.ImageName));

                break;
        }

        switch (job.Environment)
        {
            case { UrlValue: not null }:
                using (TextWriter.WriteSection("environment:"))
                {
                    WriteProperty("name", _expressionFormatter.Format(job.Environment.Name));
                    WriteProperty("url", _expressionFormatter.Format(job.Environment.UrlValue));
                }

                break;

            case { UrlValue: null }:
                WriteProperty("environment", _expressionFormatter.Format(job.Environment.Name));

                break;
        }

        WriteConcurrency(job.Concurrency);

        if (job.Outputs is { Count: > 0 } outputs)
            using (TextWriter.WriteSection("outputs:"))
                foreach (var (key, value) in outputs)
                    WriteProperty(key, _expressionFormatter.Format(value));

        WriteEnv(job.Env);

        if (job.TimeoutMinutes is { } timeout)
            WriteProperty("timeout-minutes", _expressionFormatter.Format(timeout));

        WriteStrategy(job.Strategy);

        if (job.ContinueOnError is { } continueOnError)
            WriteProperty("continue-on-error", _expressionFormatter.Format(continueOnError));

        WriteContainer(job.Container);

        if (job.Services is { Count: > 0 } services)
            using (TextWriter.WriteSection("services:"))
                foreach (var service in services)
                    using (TextWriter.WriteSection($"{service.Key}:"))
                        WriteContainer(service.Value);

        if (job.Steps is { Count: > 0 } steps)
            using (TextWriter.WriteSection("steps:"))
            {
                TextWriter.WriteLine();

                foreach (var step in steps)
                    WriteStep(step);
            }
    }

    private void WriteContainer(Container? jobContainer)
    {
        if (jobContainer is null)
            return;

        using var containerSection = TextWriter.WriteSection("container:");

        WriteProperty("image", _expressionFormatter.Format(jobContainer.Image));

        if (jobContainer.Credentials is { } credentials)
        {
            using var credentialsSection = TextWriter.WriteSection("credentials:");

            if (credentials.Username is { } username)
                WriteProperty("username", _expressionFormatter.Format(username));

            if (credentials.Password is { } password)
                WriteProperty("password", _expressionFormatter.Format(password));
        }

        WriteEnv(jobContainer.Env);

        if (jobContainer.Ports is { } ports)
            WriteProperty("ports", ports.Select(x => _expressionFormatter.Format(x)));

        if (jobContainer.Volumes is { } volumes)
            WriteProperty("volumes", volumes.Select(x => _expressionFormatter.Format(x)));

        if (jobContainer.Options is { } options)
            WriteProperty("options", _expressionFormatter.Format(options));
    }

    private void WriteStrategy(Strategy? strategy)
    {
        if (strategy is null)
            return;

        using var strategySection = TextWriter.WriteSection("strategy:");

        if (strategy.FailFast is { } failFast)
            WriteProperty("fail-fast", _expressionFormatter.Format(failFast));

        if (strategy.MaxParallel is { } maxParallel)
            WriteProperty("max-parallel", _expressionFormatter.Format(maxParallel));

        using var matrixSection = TextWriter.WriteSection("matrix:");

        if (strategy.Matrix.Map is { } map)
            foreach (var (key, value) in map.Where(x => x.Value.Count > 0))
                WriteProperty(key, value.Select(x => _expressionFormatter.Format(x)));

        if (strategy.Matrix.Include is { } include)
            foreach (var includeEntry in include)
            {
                if (includeEntry.Count is 0)
                    continue;

                var pairs = includeEntry.ToList();

                using (TextWriter.WriteSection($"- {pairs[0].Key}: {pairs[0].Value}"))
                    foreach (var (key, value) in pairs.Skip(1))
                        WriteProperty(key, _expressionFormatter.Format(value));
            }

        if (strategy.Matrix.Exclude is { } exclude)
            foreach (var excludeEntry in exclude)
            {
                if (excludeEntry.Count is 0)
                    continue;

                var pairs = excludeEntry.ToList();

                using (TextWriter.WriteSection($"- {pairs[0].Key}: {pairs[0].Value}"))
                    foreach (var (key, value) in pairs.Skip(1))
                        WriteProperty(key, _expressionFormatter.Format(value));
            }
    }

    private void WriteStep(Step step)
    {
        IDisposable? section = null;

        if (_expressionFormatter.Format(step.Name) is { Length: > 0 } name)
            WriteSectionOrProperty("name", name);

        if (step.Id is { Length: > 0 } id)
            WriteSectionOrProperty("id", id);

        if (_expressionFormatter.Format(step.If) is { Length: > 0 } condition)
            WriteSectionOrProperty("if", condition);

        switch (step)
        {
            case Step.RunStep runStep:
                switch (runStep.Run)
                {
                    case { Count: 1 } single:
                        WriteSectionOrProperty("run", _expressionFormatter.Format(single[0]));

                        break;

                    default:
                        WriteSectionOrProperty("run", runStep.Run.Select(x => _expressionFormatter.Format(x)));

                        break;
                }

                if (runStep.Shell is { } shell)
                    WriteProperty("shell", _expressionFormatter.Format(shell));

                break;

            case Step.UsesStep usesStep:
                WriteSectionOrProperty("uses", _expressionFormatter.Format(usesStep.Uses));

                break;
        }

        if (step.WorkingDirectory is { } workingDirectory)
            WriteProperty("working-directory", _expressionFormatter.Format(workingDirectory));

        if (step.With is { Count: > 0 } with)
            using (TextWriter.WriteSection("with:"))
                foreach (var (key, value) in with)
                    switch (value)
                    {
                        case { Count: 1 }:
                            WriteProperty(key, _expressionFormatter.Format(value[0]));

                            break;
                        default:
                            WriteProperty(key, value.Select(x => _expressionFormatter.Format(x)));

                            break;
                    }

        WriteEnv(step.Env);

        if (step.ContinueOnError is { } continueOnError)
            WriteProperty("continue-on-error", _expressionFormatter.Format(continueOnError));

        if (step.TimeoutMinutes is { } timeout)
            WriteProperty("timeout-minutes", _expressionFormatter.Format(timeout));

        section?.Dispose();

        TextWriter.WriteLine();

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

                    using (TextWriter.WriteSection(string.Empty))
                        foreach (var line in valueArray)
                            TextWriter.WriteLine(Format(line));

                    break;
            }
        }
    }

    private IDisposable WriteSection(string key, string value) =>
        value switch
        {
            { Length: 0 } => TextWriter.WriteSection($"{key}: ''"),
            _ => TextWriter.WriteSection($"{key}: {Format(value)}"),
        };

    private void WriteProperty(string key, string value)
    {
        var lines = value.Split('\r', '\n', StringSplitOptions.RemoveEmptyEntries);

        switch (lines.Length)
        {
            case 0:
                TextWriter.WriteLine($"{key}: ''");

                break;
            case 1:
                TextWriter.WriteLine($"{key}: {Format(value)}");

                break;

            default:
            {
                using (TextWriter.WriteSection($"{key}: |"))
                    foreach (var line in lines)
                        TextWriter.WriteLine(line); // Don't format lines here

                break;
            }
        }
    }

    private void WriteProperty(string key, IEnumerable<string> values)
    {
        var valueList = values.ToList();

        var valuesTotalLength = valueList.Sum(x => x.Length);

        if (valuesTotalLength < 80)
            TextWriter.WriteLine($"{key}: [ {string.Join(", ", valueList.Select(Format))} ]");
        else
            using (TextWriter.WriteSection($"{key}:"))
                foreach (var value in valueList)
                    TextWriter.WriteLine($"- {Format(value)}");
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
