namespace DecSm.StructuredText.AzureDevopsPipelines;

[PublicAPI]
public sealed class DevopsPipelineWriter
{
    private readonly DevopsExpressionFormatter _expressionFormatter = new();

    public StructuredTextWriter TextWriter { get; init; } = new();

    public void Write(DevopsPipeline devopsPipeline) =>
        devopsPipeline.Match(WritePipelineWithStages,
            WritePipelineWithExtends,
            WritePipelineWithJobs,
            WritePipelineWithSteps);

    private void WritePipelineWithStages(DevopsPipeline.DevopsPipelineWithStages devopsPipeline)
    {
        WritePipelineHeader(devopsPipeline.Name, devopsPipeline.AppendCommitMessageToRunName);
        WriteTrigger(devopsPipeline.Trigger);
        WritePr(devopsPipeline.Pr);
        WriteParameters(devopsPipeline.Parameters);
        WriteSchedules(devopsPipeline.Schedules);
        WriteResources(devopsPipeline.PipelineResources);
        WriteVariables(devopsPipeline.Variables);
        WritePool(devopsPipeline.Pool);
        WriteLockBehavior(devopsPipeline.LockBehavior);

        using (TextWriter.WriteSection("stages:"))
            foreach (var stage in devopsPipeline.Stages)
                WriteStage(stage);
    }

    private void WritePipelineWithExtends(DevopsPipeline.DevopsPipelineWithExtends devopsPipeline)
    {
        WritePipelineHeader(devopsPipeline.Name, devopsPipeline.AppendCommitMessageToRunName);
        WriteTrigger(devopsPipeline.Trigger);
        WritePr(devopsPipeline.Pr);
        WriteParameters(devopsPipeline.Parameters);
        WriteSchedules(devopsPipeline.Schedules);
        WriteResources(devopsPipeline.PipelineResources);
        WriteVariables(devopsPipeline.Variables);
        WritePool(devopsPipeline.Pool);
        WriteLockBehavior(devopsPipeline.LockBehavior);

        using (TextWriter.WriteSection("extends:"))
        {
            WriteProperty("template", devopsPipeline.Extends.Template);

            if (devopsPipeline.Extends.Parameters is { Count: > 0 } parameters)
                using (TextWriter.WriteSection("parameters:"))
                    foreach (var (key, value) in parameters)
                        WriteProperty(key, value);
        }
    }

    private void WritePipelineWithJobs(DevopsPipeline.DevopsPipelineWithJobs devopsPipeline)
    {
        WritePipelineHeader(devopsPipeline.Name, devopsPipeline.AppendCommitMessageToRunName);
        WriteTrigger(devopsPipeline.Trigger);
        WritePr(devopsPipeline.Pr);
        WriteParameters(devopsPipeline.Parameters);
        WriteSchedules(devopsPipeline.Schedules);
        WriteVariables(devopsPipeline.Variables);
        WritePool(devopsPipeline.Pool);
        WriteLockBehavior(devopsPipeline.LockBehavior);

        TextWriter.WriteLine();

        using (TextWriter.WriteSection("jobs:"))
            foreach (var job in devopsPipeline.Jobs)
            {
                TextWriter.WriteLine();
                WriteJob(job);
            }
    }

    private void WritePipelineWithSteps(DevopsPipeline.DevopsPipelineWithSteps devopsPipeline)
    {
        WritePipelineHeader(devopsPipeline.Name, devopsPipeline.AppendCommitMessageToRunName);
        WriteTrigger(devopsPipeline.Trigger);
        WritePr(devopsPipeline.Pr);
        WriteParameters(devopsPipeline.Parameters);
        WriteSchedules(devopsPipeline.Schedules);
        WriteVariables(devopsPipeline.Variables);
        WritePool(devopsPipeline.Pool);

        if (devopsPipeline.Strategy is { } strategy)
            WriteJobStrategy(strategy);

        if (devopsPipeline.ContinueOnError is { } continueOnError)
            WriteProperty("continueOnError", continueOnError);

        WriteJobContainer(devopsPipeline.Container);

        if (devopsPipeline.Services is { Count: > 0 } services)
            using (TextWriter.WriteSection("services:"))
                foreach (var (key, value) in services)
                    WriteProperty(key, value);

        if (devopsPipeline.Workspace is { } workspace)
            WriteWorkspace(workspace);

        using (TextWriter.WriteSection("steps:"))
            foreach (var step in devopsPipeline.Steps)
                WriteStep(step);
    }

    private void WritePipelineHeader(TextExpression? name, TextExpression? appendCommitMessage)
    {
        if (name is not null)
        {
            WriteProperty("name", name);
            TextWriter.WriteLine();
        }

        if (appendCommitMessage is not null)
            WriteProperty("appendCommitMessageToRunName", appendCommitMessage);
    }

    private void WriteTrigger(Trigger? trigger)
    {
        if (trigger is null)
            return;

        trigger.Match(_ => TextWriter.WriteLine("trigger: none"),
            bl =>
            {
                using (TextWriter.WriteSection("trigger:"))
                    WriteExpressionList("branches", bl.Branches);
            },
            full =>
            {
                using (TextWriter.WriteSection("trigger:"))
                {
                    if (full.Batch is { } batch)
                        WriteProperty("batch", batch);

                    WriteIncludeExcludeFilters("branches", full.Branches);
                    WriteIncludeExcludeFilters("paths", full.Paths);
                    WriteIncludeExcludeFilters("tags", full.Tags);
                }
            });
    }

    private void WritePr(Pr? pr)
    {
        if (pr is null)
            return;

        pr.Match(_ => TextWriter.WriteLine("pr: none"),
            bl =>
            {
                using (TextWriter.WriteSection("pr:"))
                    WriteExpressionList("branches", bl.Branches);
            },
            full =>
            {
                using (TextWriter.WriteSection("pr:"))
                {
                    if (full.AutoCancel is { } autoCancel)
                        WriteProperty("autoCancel", autoCancel);

                    WriteIncludeExcludeFilters("branches", full.Branches);
                    WriteIncludeExcludeFilters("paths", full.Paths);

                    if (full.Drafts is { } drafts)
                        WriteProperty("drafts", drafts);
                }
            });
    }

    private void WriteParameters(IReadOnlyList<Parameter>? parameters)
    {
        if (parameters is not { Count: > 0 })
            return;

        using var _ = TextWriter.WriteSection("parameters:");

        foreach (var param in parameters)
            using (TextWriter.WriteSection($"- name: {Resolve(param.Name)}"))
            {
                if (param.DisplayName is { } displayName)
                    WriteProperty("displayName", displayName);

                if (param.Type is { } type)
                    WriteProperty("type", type);

                if (param.Default is { } defaultValue)
                    WriteProperty("default", defaultValue);

                if (param.Values is { Count: > 0 } values)
                    WriteExpressionList("values", values);
            }
    }

    private void WriteSchedules(IReadOnlyList<Schedule>? schedules)
    {
        if (schedules is not { Count: > 0 })
            return;

        using var _ = TextWriter.WriteSection("schedules:");

        foreach (var schedule in schedules)
            using (TextWriter.WriteSection($"- cron: {Resolve(schedule.Cron)}"))
            {
                if (schedule.DisplayName is { } displayName)
                    WriteProperty("displayName", displayName);

                WriteIncludeExcludeFilters("branches", schedule.Branches);

                if (schedule.Always is { } always)
                    WriteProperty("always", always);
            }
    }

    private void WriteResources(Resources? resources)
    {
        if (resources is null)
            return;

        using var _ = TextWriter.WriteSection("resources:");

        if (resources.Builds is { Count: > 0 } builds)
            using (TextWriter.WriteSection("builds:"))
                foreach (var build in builds)
                    using (TextWriter.WriteSection($"- build: {Resolve(build.Build)}"))
                    {
                        if (build.Type is { } type)
                            WriteProperty("type", type);

                        if (build.Connection is { } conn)
                            WriteProperty("connection", conn);

                        if (build.Source is { } source)
                            WriteProperty("source", source);

                        if (build.Version is { } version)
                            WriteProperty("version", version);

                        if (build.Branch is { } branch)
                            WriteProperty("branch", branch);

                        if (build.Trigger is { } trigger)
                            WriteProperty("trigger", trigger);
                    }

        if (resources.Containers is { Count: > 0 } containers)
            using (TextWriter.WriteSection("containers:"))
                foreach (var container in containers)
                    using (TextWriter.WriteSection($"- container: {Resolve(container.Container)}"))
                    {
                        WriteProperty("image", container.Image);

                        if (container.Endpoint is { } endpoint)
                            WriteProperty("endpoint", endpoint);

                        if (container.Env is { Count: > 0 } env)
                            using (TextWriter.WriteSection("env:"))
                                foreach (var (key, value) in env)
                                    WriteProperty(key, value);

                        if (container.Options is { } options)
                            WriteProperty("options", options);

                        if (container.Ports is { Count: > 0 } ports)
                            WriteExpressionList("ports", ports);

                        if (container.Volumes is { Count: > 0 } volumes)
                            WriteExpressionList("volumes", volumes);

                        if (container.Trigger is { } trigger)
                            using (TextWriter.WriteSection("trigger:"))
                            {
                                if (trigger.Enabled is { } enabled)
                                    WriteProperty("enabled", enabled);

                                WriteIncludeExcludeFilters("tags", trigger.Tags);
                            }
                    }

        if (resources.Pipelines is { Count: > 0 } pipelines)
            using (TextWriter.WriteSection("pipelines:"))
                foreach (var pipeline in pipelines)
                    using (TextWriter.WriteSection($"- pipeline: {Resolve(pipeline.Pipeline)}"))
                    {
                        WriteProperty("source", pipeline.Source);

                        if (pipeline.Project is { } project)
                            WriteProperty("project", project);

                        if (pipeline.Version is { } version)
                            WriteProperty("version", version);

                        if (pipeline.Branch is { } branch)
                            WriteProperty("branch", branch);

                        if (pipeline.Trigger is { } trigger)
                            using (TextWriter.WriteSection("trigger:"))
                            {
                                if (trigger.Enabled is { } enabled)
                                    WriteProperty("enabled", enabled);

                                WriteIncludeExcludeFilters("branches", trigger.Branches);
                                WriteIncludeExcludeFilters("tags", trigger.Tags);

                                if (trigger.Stages is { Count: > 0 } stages)
                                    WriteExpressionList("stages", stages);
                            }
                    }

        if (resources.Repositories is { Count: > 0 } repositories)
            using (TextWriter.WriteSection("repositories:"))
                foreach (var repo in repositories)
                    using (TextWriter.WriteSection($"- repository: {Resolve(repo.Repository)}"))
                    {
                        WriteProperty("type", repo.Type);

                        if (repo.Endpoint is { } endpoint)
                            WriteProperty("endpoint", endpoint);

                        if (repo.Name is { } name)
                            WriteProperty("name", name);

                        if (repo.Ref is { } @ref)
                            WriteProperty("ref", @ref);
                    }

        if (resources.Webhooks is { Count: > 0 } webhooks)
            using (TextWriter.WriteSection("webhooks:"))
                foreach (var webhook in webhooks)
                    using (TextWriter.WriteSection($"- webhook: {Resolve(webhook.Webhook)}"))
                    {
                        WriteProperty("connection", webhook.Connection);

                        if (webhook.Type is { } type)
                            WriteProperty("type", type);

                        if (webhook.Filters is { Count: > 0 } filters)
                            using (TextWriter.WriteSection("filters:"))
                                foreach (var filter in filters)
                                    using (TextWriter.WriteSection($"- path: {Resolve(filter.Path)}"))
                                        WriteProperty("value", filter.Value);
                    }

        if (resources.Packages is { Count: > 0 } packages)
            using (TextWriter.WriteSection("packages:"))
                foreach (var package in packages)
                    using (TextWriter.WriteSection($"- package: {Resolve(package.Package)}"))
                    {
                        WriteProperty("type", package.Type);

                        if (package.Connection is { } conn)
                            WriteProperty("connection", conn);

                        if (package.Name is { } name)
                            WriteProperty("name", name);

                        if (package.Version is { } version)
                            WriteProperty("version", version);

                        if (package.Tag is { } tag)
                            WriteProperty("tag", tag);
                    }
    }

    private void WriteVariables(Variables? variables)
    {
        if (variables is null)
            return;

        variables.Match(dict =>
            {
                using (TextWriter.WriteSection("variables:"))
                    foreach (var (key, value) in dict.Values)
                        WriteProperty(key, value);
            },
            list =>
            {
                using (TextWriter.WriteSection("variables:"))
                    foreach (var variable in list.Values)
                        variable.Match(n =>
                            {
                                using (TextWriter.WriteSection($"- name: {Resolve(n.VariableName)}"))
                                {
                                    WriteProperty("value", n.Value);

                                    if (n.ReadOnly is { } readOnly)
                                        WriteProperty("readonly", readOnly);
                                }
                            },
                            g => TextWriter.WriteLine($"- group: {Resolve(g.GroupName)}"),
                            t =>
                            {
                                if (t.Parameters is { Count: > 0 } parameters)
                                    using (TextWriter.WriteSection($"- template: {Resolve(t.TemplatePath)}"))
                                    using (TextWriter.WriteSection("parameters:"))
                                        foreach (var (key, value) in parameters)
                                            WriteProperty(key, value);
                                else
                                    TextWriter.WriteLine($"- template: {Resolve(t.TemplatePath)}");
                            });
            });
    }

    private void WritePool(Pool? pool)
    {
        if (pool is null)
            return;

        pool.Match(pn => WriteProperty("pool", pn.Name),
            ps =>
            {
                using (TextWriter.WriteSection("pool:"))
                {
                    if (ps.VmImage is { } vmImage)
                        WriteProperty("vmImage", vmImage);

                    if (ps.Name is { } name)
                        WriteProperty("name", name);

                    if (ps.Demands is { Count: > 0 } demands)
                        WriteExpressionList("demands", demands);
                }
            });
    }

    private void WriteLockBehavior(TextExpression? lockBehavior)
    {
        if (lockBehavior is not null)
            WriteProperty("lockBehavior", lockBehavior);
    }

    private void WriteStage(Stage stage) =>
        stage.Match(sd =>
            {
                using (TextWriter.WriteSection($"- stage: {Resolve(sd.StageId)}"))
                {
                    if (sd.Group is { } group)
                        WriteProperty("group", group);

                    if (sd.DisplayName is { } displayName)
                        WriteProperty("displayName", displayName);

                    WritePool(sd.Pool);

                    if (sd.DependsOn is { Count: > 0 } dependsOn)
                        WriteExpressionList("dependsOn", dependsOn);

                    if (sd.Condition is { } condition)
                        WriteProperty("condition", condition);

                    WriteVariables(sd.Variables);

                    if (sd.LockBehavior is { } lockBehavior)
                        WriteProperty("lockBehavior", lockBehavior);

                    if (sd.Trigger is { } trigger)
                        WriteProperty("trigger", trigger);

                    if (sd.IsSkippable is { } isSkippable)
                        WriteProperty("isSkippable", isSkippable);

                    if (sd.TemplateContext is { Count: > 0 } templateContext)
                        using (TextWriter.WriteSection("templateContext:"))
                            foreach (var (key, value) in templateContext)
                                WriteProperty(key, value);

                    if (sd.Jobs is { Count: > 0 } jobs)
                        using (TextWriter.WriteSection("jobs:"))
                            foreach (var job in jobs)
                                WriteJob(job);
                }
            },
            t =>
            {
                if (t.Parameters is { Count: > 0 } parameters)
                    using (TextWriter.WriteSection($"- template: {Resolve(t.TemplatePath)}"))
                    using (TextWriter.WriteSection("parameters:"))
                        foreach (var (key, value) in parameters)
                            WriteProperty(key, value);
                else
                    TextWriter.WriteLine($"- template: {Resolve(t.TemplatePath)}");
            });

    private void WriteJob(Job job) =>
        job.Match(WriteRegularJob,
            WriteDeploymentJob,
            t =>
            {
                if (t.Parameters is { Count: > 0 } parameters)
                    using (TextWriter.WriteSection($"- template: {Resolve(t.TemplatePath)}"))
                    using (TextWriter.WriteSection("parameters:"))
                        foreach (var (key, value) in parameters)
                            WriteProperty(key, value);
                else
                    TextWriter.WriteLine($"- template: {Resolve(t.TemplatePath)}");
            });

    private void WriteRegularJob(Job.RegularJob job)
    {
        using var _ = TextWriter.WriteSection($"- job: {Resolve(job.JobId)}");

        if (job.DisplayName is { } displayName)
            WriteProperty("displayName", displayName);

        if (job.DependsOn is { Count: > 0 } dependsOn)
            WriteExpressionList("dependsOn", dependsOn);

        if (job.Condition is { } condition)
            WriteProperty("condition", condition);

        if (job.ContinueOnError is { } continueOnError)
            WriteProperty("continueOnError", continueOnError);

        if (job.TimeoutInMinutes is { } timeout)
            WriteProperty("timeoutInMinutes", timeout);

        if (job.CancelTimeoutInMinutes is { } cancelTimeout)
            WriteProperty("cancelTimeoutInMinutes", cancelTimeout);

        WriteVariables(job.Variables);

        if (job.Strategy is { } strategy)
            WriteJobStrategy(strategy);

        WritePool(job.Pool);
        WriteJobContainer(job.Container);

        if (job.Services is { Count: > 0 } services)
            using (TextWriter.WriteSection("services:"))
                foreach (var (key, value) in services)
                    WriteProperty(key, value);

        if (job.Workspace is { } workspace)
            WriteWorkspace(workspace);

        if (job.Uses is { } uses)
            WriteExplicitResources(uses);

        if (job.TemplateContext is { Count: > 0 } templateContext)
            using (TextWriter.WriteSection("templateContext:"))
                foreach (var (key, value) in templateContext)
                    WriteProperty(key, value);

        if (job.Steps is { Count: > 0 } steps)
            using (TextWriter.WriteSection("steps:"))
                foreach (var step in steps)
                    WriteStep(step);
    }

    private void WriteDeploymentJob(Job.Deployment job)
    {
        using var _ = TextWriter.WriteSection($"- deployment: {Resolve(job.DeploymentId)}");

        if (job.DisplayName is { } displayName)
            WriteProperty("displayName", displayName);

        if (job.DependsOn is { Count: > 0 } dependsOn)
            WriteExpressionList("dependsOn", dependsOn);

        if (job.Condition is { } condition)
            WriteProperty("condition", condition);

        if (job.ContinueOnError is { } continueOnError)
            WriteProperty("continueOnError", continueOnError);

        if (job.TimeoutInMinutes is { } timeout)
            WriteProperty("timeoutInMinutes", timeout);

        if (job.CancelTimeoutInMinutes is { } cancelTimeout)
            WriteProperty("cancelTimeoutInMinutes", cancelTimeout);

        WriteVariables(job.Variables);
        WritePool(job.Pool);
        WriteDeploymentEnvironment(job.Environment);
        WriteDeploymentStrategy(job.Strategy);
        WriteJobContainer(job.Container);

        if (job.Services is { Count: > 0 } services)
            using (TextWriter.WriteSection("services:"))
                foreach (var (key, value) in services)
                    WriteProperty(key, value);

        if (job.Workspace is { } workspace)
            WriteWorkspace(workspace);

        if (job.Uses is { } uses)
            WriteExplicitResources(uses);

        if (job.TemplateContext is { Count: > 0 } templateContext)
            using (TextWriter.WriteSection("templateContext:"))
                foreach (var (key, value) in templateContext)
                    WriteProperty(key, value);
    }

    private void WriteDeploymentEnvironment(DeploymentEnvironment environment) =>
        environment.Match(en => WriteProperty("environment", en.Name),
            es =>
            {
                using (TextWriter.WriteSection("environment:"))
                {
                    WriteProperty("name", es.Name);

                    if (es.ResourceName is { } resourceName)
                        WriteProperty("resourceName", resourceName);

                    if (es.ResourceType is { } resourceType)
                        WriteProperty("resourceType", resourceType);

                    if (es.ResourceId is { } resourceId)
                        WriteProperty("resourceId", resourceId);

                    if (es.Tags is { Count: > 0 } tags)
                        WriteExpressionList("tags", tags);
                }
            });

    private void WriteDeploymentStrategy(DeploymentStrategy strategy) =>
        strategy.Match(ro =>
            {
                using (TextWriter.WriteSection("strategy:"))
                using (TextWriter.WriteSection("runOnce:"))
                {
                    WriteDeploymentHook("preDeploy", ro.PreDeploy);
                    WriteDeploymentHook("deploy", ro.Deploy);
                    WriteDeploymentHook("routeTraffic", ro.RouteTraffic);
                    WriteDeploymentHook("postRouteTraffic", ro.PostRouteTraffic);
                    WriteDeploymentHook("on:", ro.OnSuccess, "success");
                    WriteDeploymentHook("on:", ro.OnFailure, "failure");
                }
            },
            r =>
            {
                using (TextWriter.WriteSection("strategy:"))
                using (TextWriter.WriteSection("rolling:"))
                {
                    if (r.MaxParallel is { } maxParallel)
                        WriteProperty("maxParallel", maxParallel);

                    WriteDeploymentHook("preDeploy", r.PreDeploy);
                    WriteDeploymentHook("deploy", r.Deploy);
                    WriteDeploymentHook("routeTraffic", r.RouteTraffic);
                    WriteDeploymentHook("postRouteTraffic", r.PostRouteTraffic);
                    WriteDeploymentHook("on:", r.OnSuccess, "success");
                    WriteDeploymentHook("on:", r.OnFailure, "failure");
                }
            },
            c =>
            {
                using (TextWriter.WriteSection("strategy:"))
                using (TextWriter.WriteSection("canary:"))
                {
                    WriteExpressionList("increments", c.Increments);

                    WriteDeploymentHook("preDeploy", c.PreDeploy);
                    WriteDeploymentHook("deploy", c.Deploy);
                    WriteDeploymentHook("routeTraffic", c.RouteTraffic);
                    WriteDeploymentHook("postRouteTraffic", c.PostRouteTraffic);
                    WriteDeploymentHook("on:", c.OnSuccess, "success");
                    WriteDeploymentHook("on:", c.OnFailure, "failure");
                }
            });

    private void WriteDeploymentHook(string hookName, DeploymentHook? hook, string? subSection = null)
    {
        if (hook is null)
            return;

        if (subSection is not null)
            using (TextWriter.WriteSection(hookName))
            using (TextWriter.WriteSection($"{subSection}:"))
            {
                WritePool(hook.Pool);

                if (hook.Steps is { Count: > 0 } steps)
                    using (TextWriter.WriteSection("steps:"))
                        foreach (var step in steps)
                            WriteStep(step);
            }
        else
            using (TextWriter.WriteSection($"{hookName}:"))
            {
                WritePool(hook.Pool);

                if (hook.Steps is { Count: > 0 } steps)
                    using (TextWriter.WriteSection("steps:"))
                        foreach (var step in steps)
                            WriteStep(step);
            }
    }

    private void WriteJobStrategy(JobStrategy strategy)
    {
        using var _ = TextWriter.WriteSection("strategy:");

        if (strategy.Matrix is { Count: > 0 } matrix)
            using (TextWriter.WriteSection("matrix:"))
                foreach (var (combinationName, dimensions) in matrix)
                    using (TextWriter.WriteSection($"{combinationName}:"))
                        foreach (var (key, value) in dimensions)
                            TextWriter.WriteLine($"{key}: '{Resolve(value)}'");

        if (strategy.MaxParallel is { } maxParallel)
            WriteProperty("maxParallel", maxParallel);

        if (strategy.Parallel is { } parallel)
            WriteProperty("parallel", parallel);
    }

    private void WriteJobContainer(JobContainer? container)
    {
        if (container is null)
            return;

        container.Match(cn => WriteProperty("container", cn.Name),
            cs =>
            {
                using (TextWriter.WriteSection("container:"))
                {
                    WriteProperty("image", cs.Image);

                    if (cs.Options is { } options)
                        WriteProperty("options", options);

                    if (cs.Endpoint is { } endpoint)
                        WriteProperty("endpoint", endpoint);

                    if (cs.Env is { Count: > 0 } env)
                        using (TextWriter.WriteSection("env:"))
                            foreach (var (key, value) in env)
                                WriteProperty(key, value);

                    if (cs.Ports is { Count: > 0 } ports)
                        WriteExpressionList("ports", ports);

                    if (cs.Volumes is { Count: > 0 } volumes)
                        WriteExpressionList("volumes", volumes);

                    if (cs.MapDockerSocket is { } mapDockerSocket)
                        WriteProperty("mapDockerSocket", mapDockerSocket);
                }
            });
    }

    private void WriteWorkspace(Workspace workspace)
    {
        if (workspace.Clean is { } clean)
            using (TextWriter.WriteSection("workspace:"))
                WriteProperty("clean", clean);
    }

    private void WriteExplicitResources(ExplicitResources uses)
    {
        using var _ = TextWriter.WriteSection("uses:");

        if (uses.Repositories is { Count: > 0 } repos)
            WriteExpressionList("repositories", repos);

        if (uses.Pools is { Count: > 0 } pools)
            WriteExpressionList("pools", pools);
    }

    private void WriteStep(Step step)
    {
        TextWriter.WriteLine();

        step.Match(WriteTaskStep,
            WriteScriptStep,
            WritePowerShellStep,
            WritePwshStep,
            WriteBashStep,
            WriteCheckoutStep,
            WriteDownloadStep,
            WriteDownloadBuildStep,
            WriteGetPackageStep,
            WritePublishStep,
            WriteTemplateStep,
            WriteReviewAppStep);
    }

    private void WriteTaskStep(Step.Task step)
    {
        using var _ = TextWriter.WriteSection($"- task: {Resolve(step.TaskName)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.Inputs is { Count: > 0 } inputs)
            using (TextWriter.WriteSection("inputs:"))
                foreach (var (key, value) in inputs)
                    WriteProperty(key, value);
    }

    private void WriteScriptStep(Step.Script step)
    {
        using var _ = TextWriter.WriteSection($"- script: {Resolve(step.ScriptContent)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.FailOnStderr is { } failOnStderr)
            WriteProperty("failOnStderr", failOnStderr);

        if (step.WorkingDirectory is { } workingDirectory)
            WriteProperty("workingDirectory", workingDirectory);
    }

    private void WritePowerShellStep(Step.PowerShell step)
    {
        using var _ = TextWriter.WriteSection($"- powershell: {Resolve(step.ScriptContent)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.FailOnStderr is { } failOnStderr)
            WriteProperty("failOnStderr", failOnStderr);

        if (step.WorkingDirectory is { } workingDirectory)
            WriteProperty("workingDirectory", workingDirectory);

        if (step.ErrorActionPreference is { } errorActionPreference)
            WriteProperty("errorActionPreference", errorActionPreference);

        if (step.IgnoreLastExitCode is { } ignoreLastExitCode)
            WriteProperty("ignoreLASTEXITCODE", ignoreLastExitCode);
    }

    private void WritePwshStep(Step.Pwsh step)
    {
        using var _ = TextWriter.WriteSection($"- pwsh: {Resolve(step.ScriptContent)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.FailOnStderr is { } failOnStderr)
            WriteProperty("failOnStderr", failOnStderr);

        if (step.WorkingDirectory is { } workingDirectory)
            WriteProperty("workingDirectory", workingDirectory);

        if (step.ErrorActionPreference is { } errorActionPreference)
            WriteProperty("errorActionPreference", errorActionPreference);

        if (step.IgnoreLastExitCode is { } ignoreLastExitCode)
            WriteProperty("ignoreLASTEXITCODE", ignoreLastExitCode);
    }

    private void WriteBashStep(Step.Bash step)
    {
        using var _ = TextWriter.WriteSection($"- bash: {Resolve(step.ScriptContent)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.FailOnStderr is { } failOnStderr)
            WriteProperty("failOnStderr", failOnStderr);

        if (step.WorkingDirectory is { } workingDirectory)
            WriteProperty("workingDirectory", workingDirectory);
    }

    private void WriteCheckoutStep(Step.Checkout step)
    {
        using var _ = TextWriter.WriteSection($"- checkout: {Resolve(step.Repository)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.Clean is { } clean)
            WriteProperty("clean", clean);

        if (step.FetchDepth is { } fetchDepth)
            WriteProperty("fetchDepth", fetchDepth);

        if (step.Lfs is { } lfs)
            WriteProperty("lfs", lfs);

        if (step.Submodules is { } submodules)
            WriteProperty("submodules", submodules);

        if (step.Path is { } path)
            WriteProperty("path", path);

        if (step.PersistCredentials is { } persistCredentials)
            WriteProperty("persistCredentials", persistCredentials);
    }

    private void WriteDownloadStep(Step.Download step)
    {
        using var _ = TextWriter.WriteSection($"- download: {Resolve(step.Pipeline)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.Artifact is { } artifact)
            WriteProperty("artifact", artifact);

        if (step.Patterns is { Count: > 0 } patterns)
            WriteExpressionList("patterns", patterns);

        if (step.Path is { } path)
            WriteProperty("path", path);
    }

    private void WriteDownloadBuildStep(Step.DownloadBuild step)
    {
        using var _ = TextWriter.WriteSection($"- downloadBuild: {Resolve(step.Build)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.Artifact is { } artifact)
            WriteProperty("artifact", artifact);

        if (step.Patterns is { Count: > 0 } patterns)
            WriteExpressionList("patterns", patterns);

        if (step.Path is { } path)
            WriteProperty("path", path);
    }

    private void WriteGetPackageStep(Step.GetPackage step)
    {
        using var _ = TextWriter.WriteSection($"- getPackage: {Resolve(step.Package)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.Version is { } version)
            WriteProperty("version", version);

        if (step.Path is { } path)
            WriteProperty("path", path);
    }

    private void WritePublishStep(Step.Publish step)
    {
        using var _ = TextWriter.WriteSection($"- publish: {Resolve(step.PublishPath)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);

        if (step.Artifact is { } artifact)
            WriteProperty("artifact", artifact);
    }

    private void WriteTemplateStep(Step.Template step)
    {
        if (step.Parameters is { Count: > 0 } parameters)
            using (TextWriter.WriteSection($"- template: {Resolve(step.TemplatePath)}"))
            using (TextWriter.WriteSection("parameters:"))
                foreach (var (key, value) in parameters)
                    WriteProperty(key, value);
        else
            TextWriter.WriteLine($"- template: {Resolve(step.TemplatePath)}");
    }

    private void WriteReviewAppStep(Step.ReviewApp step)
    {
        using var _ = TextWriter.WriteSection($"- reviewApp: {Resolve(step.ReviewAppType)}");

        WriteCommonStepProperties(step.DisplayName,
            step.Name,
            step.Condition,
            step.ContinueOnError,
            step.Enabled,
            step.TimeoutInMinutes,
            step.RetryCountOnTaskFailure,
            step.Env,
            step.Target);
    }

    private void WriteCommonStepProperties(
        TextExpression? displayName,
        TextExpression? name,
        TextExpression? condition,
        TextExpression? continueOnError,
        TextExpression? enabled,
        TextExpression? timeoutInMinutes,
        TextExpression? retryCountOnTaskFailure,
        IReadOnlyDictionary<string, TextExpression>? env,
        StepTarget? target)
    {
        if (displayName is not null)
            WriteProperty("displayName", displayName);

        if (name is not null)
            WriteProperty("name", name);

        if (condition is not null)
            WriteProperty("condition", condition);

        if (continueOnError is not null)
            WriteProperty("continueOnError", continueOnError);

        if (enabled is not null)
            WriteProperty("enabled", enabled);

        if (timeoutInMinutes is not null)
            WriteProperty("timeoutInMinutes", timeoutInMinutes);

        if (retryCountOnTaskFailure is not null)
            WriteProperty("retryCountOnTaskFailure", retryCountOnTaskFailure);

        if (env is { Count: > 0 })
            using (TextWriter.WriteSection("env:"))
                foreach (var (key, value) in env)
                    WriteProperty(key, value);

        if (target is not null)
            WriteStepTarget(target);
    }

    private void WriteStepTarget(StepTarget target) =>
        target.Match(tn => WriteProperty("target", tn.Name),
            ts =>
            {
                using (TextWriter.WriteSection("target:"))
                {
                    if (ts.Container is { } container)
                        WriteProperty("container", container);

                    if (ts.Commands is { } commands)
                        WriteProperty("commands", commands);

                    if (ts.SettableVariables is { } sv)
                    {
                        if (sv.Allowed is { } allowed)
                            WriteProperty("settableVariables", allowed);
                        else if (sv.AllowedVariables is { Count: > 0 } allowedVars)
                            WriteExpressionList("settableVariables", allowedVars);
                    }
                }
            });

    // Helper methods

    private void WriteIncludeExcludeFilters(string name, IncludeExcludeFilters? filters)
    {
        if (filters is null)
            return;

        using var _ = TextWriter.WriteSection($"{name}:");

        if (filters.Include is { Count: > 0 } include)
            WriteExpressionList("include", include);

        if (filters.Exclude is { Count: > 0 } exclude)
            WriteExpressionList("exclude", exclude);
    }

    private void WriteExpressionList(string key, TextExpressionCollection values)
    {
        // If the combined length of the expressions is less than 120 characters,
        // we can write the list as a single line e.g. [ item1, item2, item3 ]
        // Otherwise, we need to write each expression on a separate line.

        var resolvedValues = values
            .Select(Resolve)
            .ToArray();

        if (resolvedValues.Sum(v => v.Length) + (resolvedValues.Length - 1) * 2 <= 120)
        {
            TextWriter.WriteLine($"{key}: [ {string.Join(", ", resolvedValues)} ]");

            return;
        }

        using var _ = TextWriter.WriteSection($"{key}:");

        foreach (var value in values)
            TextWriter.WriteLine($"- {Resolve(value)}");
    }

    private void WriteProperty(string key, TextExpression value) =>
        TextWriter.WriteLine($"{key}: {Resolve(value)}");

    private string Resolve(TextExpression expression) =>
        _expressionFormatter.Format(expression);
}
