namespace DecSm.StructuredText.GithubActions.Tests;

[TestFixture]
internal sealed class GithubActionWriterOnTests
{
    [Test]
    public void WriteOn_Push_WithBranches_WritesBranchesInline()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.Push
            {
                Branches = ["main"],
                BranchesIgnore = null,
                Tags = null,
                TagsIgnore = null,
                Paths = null,
                PathsIgnore = null,
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("push:");
        output.ShouldContain("branches: [ main ]");
    }

    [Test]
    public void WriteOn_Push_WithMultipleBranches_WritesBranches()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.Push
            {
                Branches = ["main", "develop"],
                BranchesIgnore = null,
                Tags = null,
                TagsIgnore = null,
                Paths = null,
                PathsIgnore = null,
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("branches:");
        output.ShouldContain("main");
        output.ShouldContain("develop");
    }

    [Test]
    public void WriteOn_Push_WithBranchesIgnore_WritesBranchesIgnore()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.Push
            {
                Branches = null,
                BranchesIgnore = ["dependabot/**"],
                Tags = null,
                TagsIgnore = null,
                Paths = null,
                PathsIgnore = null,
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("branches-ignore:");
    }

    [Test]
    public void WriteOn_Push_WithTags_WritesTags()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.Push
            {
                Branches = null,
                BranchesIgnore = null,
                Tags = ["v*"],
                TagsIgnore = null,
                Paths = null,
                PathsIgnore = null,
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("tags:");
        output.ShouldContain("v*");
    }

    [Test]
    public void WriteOn_Push_NoBranchesOrTags_WritesPushWithoutContent()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.Push
            {
                Branches = null,
                BranchesIgnore = null,
                Tags = null,
                TagsIgnore = null,
                Paths = null,
                PathsIgnore = null,
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("push:");
    }

    [Test]
    public void WriteOn_PullRequest_WithTypes_WritesTypes()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.PullRequest([On.PullRequest.PullRequestType.opened, On.PullRequest.PullRequestType.synchronized])
            {
                Branches = null,
                BranchesIgnore = null,
                Tags = null,
                TagsIgnore = null,
                Paths = null,
                PathsIgnore = null,
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("pull_request:");
        output.ShouldContain("types:");
        output.ShouldContain("opened");
        output.ShouldContain("synchronized");
    }

    [Test]
    public void WriteOn_PullRequest_WithBranches_WritesBranches()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.PullRequest([])
            {
                Branches = ["main"],
                BranchesIgnore = null,
                Tags = null,
                TagsIgnore = null,
                Paths = null,
                PathsIgnore = null,
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("pull_request:");
        output.ShouldContain("branches:");
    }

    [Test]
    public void WriteOn_WorkflowDispatch_NoInputs_WritesWorkflowDispatch()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.WorkflowDispatch([])]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("workflow_dispatch:");
    }

    [Test]
    public void WriteOn_WorkflowDispatch_WithStringInput_WritesInputSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.WorkflowDispatch([
                new WorkflowDispatchInput.String
                {
                    Name = "environment",
                    Description = "Target environment",
                    Required = true,
                    Default = "staging",
                },
            ]),
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("inputs:");
        output.ShouldContain("environment:");
        output.ShouldContain("description: Target environment");
        output.ShouldContain("required: true");
        output.ShouldContain("default: staging");
        output.ShouldContain("type: string");
    }

    [Test]
    public void WriteOn_WorkflowDispatch_WithChoiceInput_WritesOptions()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.WorkflowDispatch([
                new WorkflowDispatchInput.Choice
                {
                    Name = "env",
                    Description = null,
                    Required = null,
                    Default = null,
                    Options = ["dev", "staging", "prod"],
                },
            ]),
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("type: choice");
        output.ShouldContain("options:");
        output.ShouldContain("dev");
        output.ShouldContain("staging");
        output.ShouldContain("prod");
    }

    [Test]
    public void WriteOn_WorkflowDispatch_WithBooleanInput_WritesType()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.WorkflowDispatch([
                new WorkflowDispatchInput.Boolean
                {
                    Name = "dry-run",
                    Description = null,
                    Required = null,
                    Default = null,
                },
            ]),
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("type: boolean");
    }

    [Test]
    public void WriteOn_WorkflowDispatch_WithNumberInput_WritesType()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.WorkflowDispatch([
                new WorkflowDispatchInput.Number
                {
                    Name = "retries",
                    Description = null,
                    Required = null,
                    Default = null,
                },
            ]),
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("type: number");
    }

    [Test]
    public void WriteOn_Schedule_WithCron_WritesScheduleSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Schedule(["0 2 * * 1"])]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("schedule:");
        output.ShouldContain("cron:");
        output.ShouldContain("0 2 * * 1");
    }

    [Test]
    public void WriteOn_WorkflowCall_WritesWorkflowCallLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.WorkflowCall()]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("workflow_call");
    }

    // ── on: simple events (no properties) ────────────────────────────────────

    [Test]
    public void WriteOn_Create_WritesCreateLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Create()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("create");
    }

    [Test]
    public void WriteOn_Delete_WritesDeleteLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Delete()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("delete");
    }

    [Test]
    public void WriteOn_Deployment_WritesDeploymentLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Deployment()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("deployment");
    }

    [Test]
    public void WriteOn_DeploymentStatus_WritesDeploymentStatusLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.DeploymentStatus()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("deployment_status");
    }

    [Test]
    public void WriteOn_Fork_WritesForkLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Fork()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("fork");
    }

    [Test]
    public void WriteOn_Gollum_WritesGollumLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Gollum()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("gollum");
    }

    [Test]
    public void WriteOn_PageBuild_WritesPageBuildLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.PageBuild()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("page_build");
    }

    [Test]
    public void WriteOn_Public_WritesPublicLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Public()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("public");
    }

    [Test]
    public void WriteOn_Status_WritesStatusLine()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Status()]);

        GithubActionWriterHelper
            .Write(action)
            .ShouldContain("status");
    }

    // ── on: events with types ─────────────────────────────────────────────────

    [Test]
    public void WriteOn_Issues_WithTypes_WritesIssuesSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.Issues([On.Issues.IssuesType.opened, On.Issues.IssuesType.closed]),
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("issues:");
        output.ShouldContain("types:");
        output.ShouldContain("opened");
        output.ShouldContain("closed");
    }

    [Test]
    public void WriteOn_Release_WithTypes_WritesReleaseSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Release([On.Release.ReleaseType.published])]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("release:");
        output.ShouldContain("types:");
        output.ShouldContain("published");
    }

    [Test]
    public void WriteOn_Watch_WithTypes_WritesWatchSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.Watch(On.Watch.WatchType.started)]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("watch:");
        output.ShouldContain("types:");
        output.ShouldContain("started");
    }

    [Test]
    public void WriteOn_MergeGroup_WithTypes_WritesMergeGroupSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.MergeGroup([On.MergeGroup.MergeGroupType.checks_requested]),
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("merge_group:");
    }

    [Test]
    public void WriteOn_RepositoryDispatch_WithTypes_WritesRepositoryDispatchSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([new On.RepositoryDispatch(["my-event", "other-event"])]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("repository_dispatch:");
        output.ShouldContain("my-event");
    }

    [Test]
    public void WriteOn_WorkflowRun_WithWorkflowsAndBranches_WritesWorkflowRunSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.WorkflowRun
            {
                Workflows = ["CI"],
                Branches = ["main"],
                Types = [On.WorkflowRun.WorkflowDispatchTypes.completed],
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("workflow_run:");
        output.ShouldContain("workflows:");
        output.ShouldContain("CI");
        output.ShouldContain("branches:");
        output.ShouldContain("types:");
    }

    [Test]
    public void WriteOn_ImageVersion_WithNamesAndVersions_WritesImageVersionSection()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.ImageVersion
            {
                Names = ["my-image"],
                Versions = ["1.0.0"],
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("image_version:");
        output.ShouldContain("names:");
        output.ShouldContain("versions:");
    }

    // ── on: ordering ─────────────────────────────────────────────────────────

    [Test]
    public void WriteOn_MultipleEvents_AreOrderedAlphabetically()
    {
        var action = GithubActionWriterHelper.MinimalAction([
            new On.WorkflowDispatch([]),
            new On.Push
            {
                Branches = ["main"],
                BranchesIgnore = null,
                Tags = null,
                TagsIgnore = null,
                Paths = null,
                PathsIgnore = null,
            },
        ]);

        var output = GithubActionWriterHelper.Write(action);
        var pushIdx = output.IndexOf("push:", StringComparison.Ordinal);
        var wdIdx = output.IndexOf("workflow_dispatch:", StringComparison.Ordinal);

        // push (P) comes before workflow_dispatch (W) alphabetically
        pushIdx.ShouldBeLessThan(wdIdx);
    }
}
