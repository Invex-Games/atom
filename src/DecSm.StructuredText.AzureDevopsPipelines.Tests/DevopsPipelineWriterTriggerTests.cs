namespace DecSm.StructuredText.AzureDevopsPipelines.Tests;

[TestFixture]
internal sealed class DevopsPipelineWriterTriggerTests
{
    private static DevopsPipeline.DevopsPipelineWithSteps SimplePipeline(
        Trigger? trigger = null,
        Pr? pr = null,
        IReadOnlyList<Parameter>? parameters = null,
        IReadOnlyList<Schedule>? schedules = null) =>
        new()
        {
            Trigger = trigger,
            Pr = pr,
            Parameters = parameters,
            Schedules = schedules,
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        };

    // ── Trigger: None ─────────────────────────────────────────────────────────

    [Test]
    public void WriteTrigger_None_WritesTriggerNone()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(new Trigger.None()));
        output.ShouldContain("trigger: none");
    }

    [Test]
    public void WriteTrigger_None_DoesNotWriteTriggerSection()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(new Trigger.None()));
        output.ShouldNotContain("trigger:" + Environment.NewLine);
    }

    // ── Trigger: BranchList ───────────────────────────────────────────────────

    [Test]
    public void WriteTrigger_BranchList_WritesBranchesSection()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(new Trigger.BranchList
        {
            Branches = new[] { "main", "develop" },
        }));

        output.ShouldContain("trigger:");
        output.ShouldContain("branches:");
        output.ShouldContain("- main");
        output.ShouldContain("- develop");
    }

    [Test]
    public void WriteTrigger_BranchList_CorrectStructure()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(new Trigger.BranchList
        {
            Branches = new[] { "main" },
        }));

        string[] lines = ["trigger:", "  branches:", "    - main"];

        output.ShouldContain(PipelineWriterHelper.JoinLines(lines));
    }

    // ── Trigger: Full ─────────────────────────────────────────────────────────

    [Test]
    public void WriteTrigger_Full_WithBatch_WritesBatch()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(new Trigger.Full
        {
            Batch = new BooleanExpression(true),
        }));

        output.ShouldContain("trigger:");
        output.ShouldContain("batch: true");
    }

    [Test]
    public void WriteTrigger_Full_WithBranchesIncludeExclude_WritesBothLists()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(new Trigger.Full
        {
            Branches = new()
            {
                Include = new[] { "main", "release/*" },
                Exclude = new[] { "feature/*" },
            },
        }));

        output.ShouldContain("branches:");
        output.ShouldContain("include:");
        output.ShouldContain("- main");
        output.ShouldContain("- release/*");
        output.ShouldContain("exclude:");
        output.ShouldContain("- feature/*");
    }

    [Test]
    public void WriteTrigger_Full_WithPaths_WritesPathsSection()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(new Trigger.Full
        {
            Paths = new()
            {
                Include = new[] { "src/**" },
            },
        }));

        output.ShouldContain("paths:");
        output.ShouldContain("- src/**");
    }

    [Test]
    public void WriteTrigger_Full_WithTags_WritesTagsSection()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(new Trigger.Full
        {
            Tags = new()
            {
                Include = new[] { "v*" },
            },
        }));

        output.ShouldContain("tags:");
        output.ShouldContain("- v*");
    }

    [Test]
    public void WriteTrigger_Null_WritesNothing()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline());
        output.ShouldNotContain("trigger");
    }

    // ── PR: None ──────────────────────────────────────────────────────────────

    [Test]
    public void WritePr_None_WritesPrNone()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(pr: new Pr.None()));
        output.ShouldContain("pr: none");
    }

    // ── PR: BranchList ────────────────────────────────────────────────────────

    [Test]
    public void WritePr_BranchList_WritesBranchesSection()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(pr: new Pr.BranchList
        {
            Branches = new[] { "main" },
        }));

        output.ShouldContain("pr:");
        output.ShouldContain("branches:");
        output.ShouldContain("- main");
    }

    // ── PR: Full ──────────────────────────────────────────────────────────────

    [Test]
    public void WritePr_Full_WithAutoCancel_WritesAutoCancel()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(pr: new Pr.Full
        {
            AutoCancel = new BooleanExpression(false),
        }));

        output.ShouldContain("pr:");
        output.ShouldContain("autoCancel: false");
    }

    [Test]
    public void WritePr_Full_WithDrafts_WritesDrafts()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(pr: new Pr.Full
        {
            Drafts = new BooleanExpression(true),
        }));

        output.ShouldContain("drafts: true");
    }

    [Test]
    public void WritePr_Full_WithBranchesAndPaths_WritesBothSections()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(pr: new Pr.Full
        {
            Branches = new()
            {
                Include = new[] { "main" },
            },
            Paths = new()
            {
                Exclude = new[] { "docs/**" },
            },
        }));

        output.ShouldContain("branches:");
        output.ShouldContain("- main");
        output.ShouldContain("paths:");
        output.ShouldContain("- docs/**");
    }

    [Test]
    public void WritePr_Null_WritesNothing()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(pr: null));
        output.ShouldNotContain("pr");
    }

    // ── Parameters ────────────────────────────────────────────────────────────

    [Test]
    public void WriteParameters_SingleParameter_WritesParametersSection()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(parameters:
        [
            new()
            {
                Name = new RawExpression("myParam"),
            },
        ]));

        output.ShouldContain("parameters:");
        output.ShouldContain("- name: myParam");
    }

    [Test]
    public void WriteParameters_WithAllOptions_WritesAllProperties()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(parameters:
        [
            new()
            {
                Name = new RawExpression("env"),
                DisplayName = new RawExpression("Environment"),
                Type = new RawExpression("string"),
                Default = new RawExpression("dev"),
                Values = new[] { "dev", "staging", "prod" },
            },
        ]));

        output.ShouldContain("- name: env");
        output.ShouldContain("displayName: Environment");
        output.ShouldContain("type: string");
        output.ShouldContain("default: dev");
        output.ShouldContain("values:");
        output.ShouldContain("- dev");
        output.ShouldContain("- staging");
        output.ShouldContain("- prod");
    }

    [Test]
    public void WriteParameters_Null_WritesNothing()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(parameters: null));
        output.ShouldNotContain("parameters:");
    }

    [Test]
    public void WriteParameters_Empty_WritesNothing()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(parameters: []));
        output.ShouldNotContain("parameters:");
    }

    // ── Schedules ─────────────────────────────────────────────────────────────

    [Test]
    public void WriteSchedules_SingleSchedule_WritesSchedulesSection()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(schedules:
        [
            new()
            {
                Cron = new RawExpression("0 */6 * * *"),
            },
        ]));

        output.ShouldContain("schedules:");
        output.ShouldContain("- cron: 0 */6 * * *");
    }

    [Test]
    public void WriteSchedules_WithAllOptions_WritesAllProperties()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(schedules:
        [
            new()
            {
                Cron = new RawExpression("0 0 * * 0"),
                DisplayName = new RawExpression("Weekly Sunday"),
                Branches = new()
                {
                    Include = new[] { "main" },
                },
                Always = new BooleanExpression(true),
            },
        ]));

        output.ShouldContain("- cron: 0 0 * * 0");
        output.ShouldContain("displayName: Weekly Sunday");
        output.ShouldContain("always: true");
        output.ShouldContain("branches:");
        output.ShouldContain("- main");
    }

    [Test]
    public void WriteSchedules_Null_WritesNothing()
    {
        var output = PipelineWriterHelper.Write(SimplePipeline(schedules: null));
        output.ShouldNotContain("schedules:");
    }
}
