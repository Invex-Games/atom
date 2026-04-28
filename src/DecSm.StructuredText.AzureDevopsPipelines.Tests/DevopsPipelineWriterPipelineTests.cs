namespace DecSm.StructuredText.AzureDevopsPipelines.Tests;

[TestFixture]
internal sealed class DevopsPipelineWriterPipelineTests
{
    // ── PipelineWithSteps ─────────────────────────────────────────────────────

    [Test]
    public void Write_PipelineWithSteps_Minimal_WritesStepsSection()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hello"),
                },
            ],
        };

        string[] lines = ["steps:", "  - script: echo hello"];

        PipelineWriterHelper
            .Write(pipeline)
            .ShouldBe(PipelineWriterHelper.JoinLines(lines));
    }

    [Test]
    public void Write_PipelineWithSteps_WithName_WritesNameFirst()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithSteps
        {
            Name = new RawExpression("$(Build.DefinitionName)"),
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        };

        string[] lines = ["name: $(Build.DefinitionName)", "", "steps:", "  - script: echo hi"];

        PipelineWriterHelper
            .Write(pipeline)
            .ShouldBe(PipelineWriterHelper.JoinLines(lines));
    }

    [Test]
    public void Write_PipelineWithSteps_WithAppendCommitMessage_WritesProperty()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithSteps
        {
            AppendCommitMessageToRunName = new BooleanExpression(false),
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        output.ShouldContain("appendCommitMessageToRunName: false");
    }

    [Test]
    public void Write_PipelineWithSteps_WithPool_WritesPoolBeforeSteps()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithSteps
        {
            Pool = new Pool.PoolSpec
            {
                VmImage = new RawExpression("ubuntu-latest"),
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        var poolIndex = output.IndexOf("pool:", StringComparison.Ordinal);
        var stepsIndex = output.IndexOf("steps:", StringComparison.Ordinal);
        poolIndex.ShouldBeLessThan(stepsIndex);
    }

    [Test]
    public void Write_PipelineWithSteps_WithServices_WritesServicesSection()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithSteps
        {
            Services = new Dictionary<string, TextExpression>
            {
                ["redis"] = new RawExpression("redis:6"),
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        output.ShouldContain("services:");
        output.ShouldContain("redis: redis:6");
    }

    [Test]
    public void Write_PipelineWithSteps_WithWorkspace_WritesWorkspaceSection()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithSteps
        {
            Workspace = new()
            {
                Clean = new BooleanExpression(true),
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        output.ShouldContain("workspace:");
        output.ShouldContain("clean: true");
    }

    [Test]
    public void Write_PipelineWithSteps_WithContinueOnError_WritesContinueOnErrorProperty()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithSteps
        {
            ContinueOnError = new BooleanExpression(true),
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        output.ShouldContain("continueOnError: true");
    }

    // ── PipelineWithJobs ──────────────────────────────────────────────────────

    [Test]
    public void Write_PipelineWithJobs_Minimal_WritesJobsSection()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs =
            [
                new Job.RegularJob
                {
                    JobId = new RawExpression("MyJob"),
                },
            ],
        };

        string[] lines = ["", "jobs:", "", "  - job: MyJob"];

        PipelineWriterHelper
            .Write(pipeline)
            .ShouldBe(PipelineWriterHelper.JoinLines(lines));
    }

    [Test]
    public void Write_PipelineWithJobs_MultipleJobs_WritesEmptyLineBetweenJobs()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs =
            [
                new Job.RegularJob
                {
                    JobId = new RawExpression("Job1"),
                },
                new Job.RegularJob
                {
                    JobId = new RawExpression("Job2"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);

        var job1Index = output.IndexOf("- job: Job1", StringComparison.Ordinal);
        var job2Index = output.IndexOf("- job: Job2", StringComparison.Ordinal);

        job1Index.ShouldBeLessThan(job2Index);
        output.ShouldContain("- job: Job1");
        output.ShouldContain("- job: Job2");
    }

    [Test]
    public void Write_PipelineWithJobs_WithPool_WritesPool()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithJobs
        {
            Pool = new Pool.PoolName
            {
                Name = new RawExpression("my-pool"),
            },
            Jobs =
            [
                new Job.RegularJob
                {
                    JobId = new RawExpression("MyJob"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        output.ShouldContain("pool: my-pool");
    }

    // ── PipelineWithStages ────────────────────────────────────────────────────

    [Test]
    public void Write_PipelineWithStages_Minimal_WritesStagesSection()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages =
            [
                new Stage.StageDefinition
                {
                    StageId = new RawExpression("BuildStage"),
                },
            ],
        };

        string[] lines = ["stages:", "  - stage: BuildStage"];

        PipelineWriterHelper
            .Write(pipeline)
            .ShouldBe(PipelineWriterHelper.JoinLines(lines));
    }

    [Test]
    public void Write_PipelineWithStages_WithLockBehavior_WritesLockBehavior()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithStages
        {
            LockBehavior = new RawExpression("runLatest"),
            Stages =
            [
                new Stage.StageDefinition
                {
                    StageId = new RawExpression("BuildStage"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        output.ShouldContain("lockBehavior: runLatest");
    }

    // ── PipelineWithExtends ───────────────────────────────────────────────────

    [Test]
    public void Write_PipelineWithExtends_Minimal_WritesExtendsSection()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithExtends
        {
            Extends = new()
            {
                Template = new RawExpression("templates/base.yml"),
            },
        };

        string[] lines = ["extends:", "  template: templates/base.yml"];

        PipelineWriterHelper
            .Write(pipeline)
            .ShouldBe(PipelineWriterHelper.JoinLines(lines));
    }

    [Test]
    public void Write_PipelineWithExtends_WithParameters_WritesParametersUnderExtends()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithExtends
        {
            Extends = new()
            {
                Template = new RawExpression("templates/base.yml"),
                Parameters = new Dictionary<string, TextExpression>
                {
                    ["environment"] = new RawExpression("production"),
                    ["runTests"] = new BooleanExpression(true),
                },
            },
        };

        var output = PipelineWriterHelper.Write(pipeline);
        output.ShouldContain("extends:");
        output.ShouldContain("template: templates/base.yml");
        output.ShouldContain("parameters:");
        output.ShouldContain("environment: production");
        output.ShouldContain("runTests: true");
    }

    [Test]
    public void Write_PipelineWithExtends_WithExtends_ParametersNested()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithExtends
        {
            Extends = new()
            {
                Template = new RawExpression("templates/secure.yml"),
                Parameters = new Dictionary<string, TextExpression>
                {
                    ["param1"] = new RawExpression("val1"),
                },
            },
        };

        string[] lines = ["extends:", "  template: templates/secure.yml", "  parameters:", "    param1: val1"];

        PipelineWriterHelper
            .Write(pipeline)
            .ShouldBe(PipelineWriterHelper.JoinLines(lines));
    }

    // ── DefaultTextWriter property ────────────────────────────────────────────

    [Test]
    public void DevopsPipelineWriter_DefaultTextWriter_HasDefaultIndentSize()
    {
        var writer = new DevopsPipelineWriter();
        writer.TextWriter.IndentSize.ShouldBe(2);
    }

    [Test]
    public void DevopsPipelineWriter_AfterWrite_TextWriterContainsOutput()
    {
        var writer = new DevopsPipelineWriter();

        writer.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        });

        writer
            .TextWriter
            .ToString()
            .ShouldNotBeEmpty();
    }
}
