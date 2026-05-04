namespace DecSm.StructuredText.AzureDevopsPipelines.Tests;

[TestFixture]
internal sealed class DevopsPipelineWriterStageTests
{
    // ── StageDefinition ───────────────────────────────────────────────────────

    [Test]
    public void WriteStage_StageDefinition_Minimal_WritesStageId()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("BuildStage"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("- stage: BuildStage");
    }

    [Test]
    public void WriteStage_StageDefinition_WithDisplayName_WritesDisplayName()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("BuildStage"),
            DisplayName = new RawExpression("Build Application"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("displayName: Build Application");
    }

    [Test]
    public void WriteStage_StageDefinition_WithGroup_WritesGroup()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("BuildStage"),
            Group = new RawExpression("BuildGroup"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("group: BuildGroup");
    }

    [Test]
    public void WriteStage_StageDefinition_WithDependsOn_WritesDependsOnList()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("TestStage"),
            DependsOn = ["BuildStage"],
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("dependsOn: [ BuildStage ]");
    }

    [Test]
    public void WriteStage_StageDefinition_WithCondition_WritesCondition()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("DeployStage"),
            Condition = new RawExpression("succeeded()"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("condition: succeeded()");
    }

    [Test]
    public void WriteStage_StageDefinition_WithPool_WritesPool()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("BuildStage"),
            Pool = new Pool.PoolSpec
            {
                VmImage = new RawExpression("ubuntu-latest"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("pool:");
        output.ShouldContain("vmImage: ubuntu-latest");
    }

    [Test]
    public void WriteStage_StageDefinition_WithLockBehavior_WritesLockBehavior()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("DeployStage"),
            LockBehavior = new RawExpression("sequential"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("lockBehavior: sequential");
    }

    [Test]
    public void WriteStage_StageDefinition_WithTrigger_WritesTrigger()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("ManualStage"),
            Trigger = new RawExpression("manual"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("trigger: manual");
    }

    [Test]
    public void WriteStage_StageDefinition_WithIsSkippable_WritesIsSkippable()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("RequiredStage"),
            IsSkippable = new BooleanExpression(false),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("isSkippable: false");
    }

    [Test]
    public void WriteStage_StageDefinition_WithTemplateContext_WritesTemplateContext()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("BuildStage"),
            TemplateContext = new Dictionary<string, TextExpression>
            {
                ["region"] = new RawExpression("eastus"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("templateContext:");
        output.ShouldContain("region: eastus");
    }

    [Test]
    public void WriteStage_StageDefinition_WithVariables_WritesVariables()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("BuildStage"),
            Variables = new Variables.Dictionary
            {
                Values = new Dictionary<string, TextExpression>
                {
                    ["STAGE_VAR"] = new RawExpression("stage-value"),
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("variables:");
        output.ShouldContain("STAGE_VAR: stage-value");
    }

    [Test]
    public void WriteStage_StageDefinition_WithJobs_WritesJobsSection()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("BuildStage"),
            Jobs =
            [
                new Job.RegularJob
                {
                    JobId = new RawExpression("CompileJob"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("- stage: BuildStage");
        output.ShouldContain("jobs:");
        output.ShouldContain("- job: CompileJob");
    }

    [Test]
    public void WriteStage_StageDefinition_WithMultipleJobs_WritesAll()
    {
        Stage stage = new Stage.StageDefinition
        {
            StageId = new RawExpression("TestStage"),
            Jobs =
            [
                new Job.RegularJob
                {
                    JobId = new RawExpression("UnitTests"),
                },
                new Job.RegularJob
                {
                    JobId = new RawExpression("IntegrationTests"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("- job: UnitTests");
        output.ShouldContain("- job: IntegrationTests");
    }

    // ── Stage: Template ───────────────────────────────────────────────────────

    [Test]
    public void WriteStage_Template_NoParams_WritesInlineLine()
    {
        Stage stage = new Stage.Template
        {
            TemplatePath = new RawExpression("templates/build-stage.yml"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("- template: templates/build-stage.yml");
    }

    [Test]
    public void WriteStage_Template_WithParams_WritesSection()
    {
        Stage stage = new Stage.Template
        {
            TemplatePath = new RawExpression("templates/build-stage.yml"),
            Parameters = new Dictionary<string, TextExpression>
            {
                ["stageEnv"] = new RawExpression("production"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages = [stage],
        });

        output.ShouldContain("- template: templates/build-stage.yml");
        output.ShouldContain("parameters:");
        output.ShouldContain("stageEnv: production");
    }

    // ── Multiple stages ───────────────────────────────────────────────────────

    [Test]
    public void WriteStages_Multiple_WritesAllInOrder()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages =
            [
                new Stage.StageDefinition
                {
                    StageId = new RawExpression("BuildStage"),
                },
                new Stage.StageDefinition
                {
                    StageId = new RawExpression("TestStage"),
                },
                new Stage.StageDefinition
                {
                    StageId = new RawExpression("DeployStage"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);

        var buildIndex = output.IndexOf("- stage: BuildStage", StringComparison.Ordinal);
        var testIndex = output.IndexOf("- stage: TestStage", StringComparison.Ordinal);
        var deployIndex = output.IndexOf("- stage: DeployStage", StringComparison.Ordinal);

        buildIndex.ShouldBeLessThan(testIndex);
        testIndex.ShouldBeLessThan(deployIndex);
    }

    // ── Full nested stages → jobs → steps pipeline ────────────────────────────

    [Test]
    public void Write_FullNestedPipeline_WithStagesJobsAndSteps_WritesCorrectly()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithStages
        {
            Stages =
            [
                new Stage.StageDefinition
                {
                    StageId = new RawExpression("CI"),
                    Jobs =
                    [
                        new Job.RegularJob
                        {
                            JobId = new RawExpression("Build"),
                            Pool = new Pool.PoolSpec
                            {
                                VmImage = new RawExpression("ubuntu-latest"),
                            },
                            Steps =
                            [
                                new Step.Checkout
                                {
                                    Repository = new RawExpression("self"),
                                },
                                new Step.Script
                                {
                                    ScriptContent = new RawExpression("dotnet build"),
                                },
                            ],
                        },
                    ],
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        output.ShouldContain("stages:");
        output.ShouldContain("- stage: CI");
        output.ShouldContain("jobs:");
        output.ShouldContain("- job: Build");
        output.ShouldContain("vmImage: ubuntu-latest");
        output.ShouldContain("steps:");
        output.ShouldContain("- checkout: self");
        output.ShouldContain("- script: dotnet build");
    }

    // ── Complete real-world-like pipeline ─────────────────────────────────────

    [Test]
    public void Write_CompleteRealWorldPipeline_ProducesExpectedYaml()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithStages
        {
            Trigger = new Trigger.BranchList
            {
                Branches = ["main"],
            },
            Variables = new Variables.Dictionary
            {
                Values = new Dictionary<string, TextExpression>
                {
                    ["buildConfiguration"] = new RawExpression("Release"),
                },
            },
            Stages =
            [
                new Stage.StageDefinition
                {
                    StageId = new RawExpression("Build"),
                    Jobs =
                    [
                        new Job.RegularJob
                        {
                            JobId = new RawExpression("BuildAndTest"),
                            Pool = new Pool.PoolSpec
                            {
                                VmImage = new RawExpression("ubuntu-latest"),
                            },
                            Steps =
                            [
                                new Step.Checkout
                                {
                                    Repository = new RawExpression("self"),
                                },
                                new Step.Task
                                {
                                    TaskName = new RawExpression("DotNetCoreCLI@2"),
                                    DisplayName = new RawExpression("Restore"),
                                    Inputs = new Dictionary<string, TextExpression>
                                    {
                                        ["command"] = new RawExpression("restore"),
                                    },
                                },
                                new Step.Task
                                {
                                    TaskName = new RawExpression("DotNetCoreCLI@2"),
                                    DisplayName = new RawExpression("Build"),
                                    Inputs = new Dictionary<string, TextExpression>
                                    {
                                        ["command"] = new RawExpression("build"),
                                        ["arguments"] = new RawExpression("--configuration $(buildConfiguration)"),
                                    },
                                },
                            ],
                        },
                    ],
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);

        output.ShouldContain("trigger:");
        output.ShouldContain("branches:");
        output.ShouldContain("include: [ main ]");
        output.ShouldContain("variables:");
        output.ShouldContain("buildConfiguration: Release");
        output.ShouldContain("stages:");
        output.ShouldContain("- stage: Build");
        output.ShouldContain("jobs:");
        output.ShouldContain("- job: BuildAndTest");
        output.ShouldContain("vmImage: ubuntu-latest");
        output.ShouldContain("steps:");
        output.ShouldContain("- checkout: self");
        output.ShouldContain("- task: DotNetCoreCLI@2");
        output.ShouldContain("displayName: Restore");
        output.ShouldContain("command: restore");
        output.ShouldContain("command: build");
    }
}
