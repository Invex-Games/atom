namespace DecSm.StructuredText.AzureDevopsPipelines.Tests;

[TestFixture]
internal sealed class DevopsPipelineWriterJobTests
{
    // ── RegularJob ────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_RegularJob_Minimal_WritesJobId()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("- job: MyJob");
    }

    [Test]
    public void WriteJob_RegularJob_WithDisplayName_WritesDisplayName()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
            DisplayName = new RawExpression("My Build Job"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("displayName: My Build Job");
    }

    [Test]
    public void WriteJob_RegularJob_WithDependsOn_WritesDependsOnList()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("DeployJob"),
            DependsOn = ["BuildJob", "TestJob"],
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("dependsOn:");
        output.ShouldContain("- BuildJob");
        output.ShouldContain("- TestJob");
    }

    [Test]
    public void WriteJob_RegularJob_WithCondition_WritesCondition()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
            Condition = new RawExpression("succeeded()"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("condition: succeeded()");
    }

    [Test]
    public void WriteJob_RegularJob_WithTimeouts_WritesTimeouts()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
            TimeoutInMinutes = new NumberExpression(60),
            CancelTimeoutInMinutes = new NumberExpression(5),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("timeoutInMinutes: 60");
        output.ShouldContain("cancelTimeoutInMinutes: 5");
    }

    [Test]
    public void WriteJob_RegularJob_WithSteps_WritesStepsSection()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hello"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("steps:");
        output.ShouldContain("- script: echo hello");
    }

    [Test]
    public void WriteJob_RegularJob_WithPool_WritesPool()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
            Pool = new Pool.PoolSpec
            {
                VmImage = new RawExpression("windows-latest"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("pool:");
        output.ShouldContain("vmImage: windows-latest");
    }

    [Test]
    public void WriteJob_RegularJob_WithServices_WritesServices()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("IntegrationTestJob"),
            Services = new Dictionary<string, TextExpression>
            {
                ["postgres"] = new RawExpression("postgres:14"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("services:");
        output.ShouldContain("postgres: postgres:14");
    }

    [Test]
    public void WriteJob_RegularJob_WithWorkspace_WritesWorkspace()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
            Workspace = new()
            {
                Clean = new BooleanExpression(true),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("workspace:");
        output.ShouldContain("clean: true");
    }

    [Test]
    public void WriteJob_RegularJob_WithTemplateContext_WritesTemplateContext()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
            TemplateContext = new Dictionary<string, TextExpression>
            {
                ["key1"] = new RawExpression("val1"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("templateContext:");
        output.ShouldContain("key1: val1");
    }

    [Test]
    public void WriteJob_RegularJob_WithUses_WritesUsesSection()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MyJob"),
            Uses = new()
            {
                Repositories = new[] { "myRepo" },
                Pools = new[] { "myPool" },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("uses:");
        output.ShouldContain("repositories:");
        output.ShouldContain("- myRepo");
        output.ShouldContain("pools:");
        output.ShouldContain("- myPool");
    }

    [Test]
    public void WriteJob_RegularJob_WithStrategy_WritesStrategy()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("MatrixJob"),
            Strategy = new()
            {
                Matrix = new Dictionary<string, IReadOnlyDictionary<string, TextExpression>>
                {
                    ["windows"] = new Dictionary<string, TextExpression>
                    {
                        ["os"] = new RawExpression("windows-latest"),
                    },
                    ["linux"] = new Dictionary<string, TextExpression>
                    {
                        ["os"] = new RawExpression("ubuntu-latest"),
                    },
                },
                MaxParallel = new NumberExpression(2),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("strategy:");
        output.ShouldContain("matrix:");
        output.ShouldContain("windows:");
        output.ShouldContain("os: 'windows-latest'");
        output.ShouldContain("linux:");
        output.ShouldContain("os: 'ubuntu-latest'");
        output.ShouldContain("maxParallel: 2");
    }

    [Test]
    public void WriteJob_RegularJob_WithParallelStrategy_WritesParallel()
    {
        Job job = new Job.RegularJob
        {
            JobId = new RawExpression("ParallelJob"),
            Strategy = new()
            {
                Parallel = new NumberExpression(5),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("strategy:");
        output.ShouldContain("parallel: 5");
    }

    // ── Deployment Job ────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_Deployment_Minimal_WritesDeploymentLine()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("MyDeploy"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("production"),
            },
            Strategy = new DeploymentStrategy.RunOnce(),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("- deployment: MyDeploy");
        output.ShouldContain("environment: production");
    }

    [Test]
    public void WriteJob_Deployment_WithEnvironmentSpec_WritesEnvironmentSection()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("MyDeploy"),
            Environment = new DeploymentEnvironment.EnvironmentSpec
            {
                Name = new RawExpression("production"),
                ResourceName = new RawExpression("myVM"),
                ResourceType = new RawExpression("VirtualMachine"),
                ResourceId = new NumberExpression(12345),
                Tags = new[] { "primary" },
            },
            Strategy = new DeploymentStrategy.RunOnce(),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("environment:");
        output.ShouldContain("name: production");
        output.ShouldContain("resourceName: myVM");
        output.ShouldContain("resourceType: VirtualMachine");
        output.ShouldContain("resourceId: 12345");
        output.ShouldContain("tags:");
        output.ShouldContain("- primary");
    }

    [Test]
    public void WriteJob_Deployment_RunOnce_WithDeploy_WritesRunOnceStrategy()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("DeployProd"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.RunOnce
            {
                Deploy = new()
                {
                    Steps =
                    [
                        new Step.Script
                        {
                            ScriptContent = new RawExpression("echo deploying"),
                        },
                    ],
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("strategy:");
        output.ShouldContain("runOnce:");
        output.ShouldContain("deploy:");
        output.ShouldContain("steps:");
        output.ShouldContain("- script: echo deploying");
    }

    [Test]
    public void WriteJob_Deployment_RunOnce_WithOnSuccess_WritesOnSection()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("DeployProd"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.RunOnce
            {
                OnSuccess = new()
                {
                    Steps =
                    [
                        new Step.Script
                        {
                            ScriptContent = new RawExpression("echo success"),
                        },
                    ],
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("on:");
        output.ShouldContain("success:");
        output.ShouldContain("- script: echo success");
    }

    [Test]
    public void WriteJob_Deployment_RunOnce_WithOnFailure_WritesOnFailureSection()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("DeployProd"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.RunOnce
            {
                OnFailure = new()
                {
                    Steps =
                    [
                        new Step.Script
                        {
                            ScriptContent = new RawExpression("echo failed"),
                        },
                    ],
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("on:");
        output.ShouldContain("failure:");
        output.ShouldContain("- script: echo failed");
    }

    [Test]
    public void WriteJob_Deployment_Rolling_WithMaxParallel_WritesRollingStrategy()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("DeployRolling"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.Rolling
            {
                MaxParallel = new NumberExpression(25),
                Deploy = new()
                {
                    Steps =
                    [
                        new Step.Script
                        {
                            ScriptContent = new RawExpression("echo rolling"),
                        },
                    ],
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("rolling:");
        output.ShouldContain("maxParallel: 25");
        output.ShouldContain("deploy:");
        output.ShouldContain("- script: echo rolling");
    }

    [Test]
    public void WriteJob_Deployment_Canary_WithIncrements_WritesCanaryStrategy()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("DeployCanary"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.Canary
            {
                Increments = new TextExpression[] { new NumberExpression(10), new NumberExpression(50) },
                Deploy = new()
                {
                    Steps =
                    [
                        new Step.Script
                        {
                            ScriptContent = new RawExpression("echo canary"),
                        },
                    ],
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("canary:");
        output.ShouldContain("increments:");
        output.ShouldContain("- 10");
        output.ShouldContain("- 50");
        output.ShouldContain("- script: echo canary");
    }

    [Test]
    public void WriteJob_Deployment_WithHook_HasPool_WritesPoolInHook()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("MyDeploy"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.RunOnce
            {
                Deploy = new()
                {
                    Pool = new Pool.PoolSpec
                    {
                        VmImage = new RawExpression("ubuntu-latest"),
                    },
                    Steps =
                    [
                        new Step.Script
                        {
                            ScriptContent = new RawExpression("echo deploy"),
                        },
                    ],
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("vmImage: ubuntu-latest");
    }

    [Test]
    public void WriteJob_Deployment_WithAllBasicHooks_WritesAll()
    {
        var simpleHook = new DeploymentHook
        {
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo step"),
                },
            ],
        };

        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("FullDeploy"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.RunOnce
            {
                PreDeploy = simpleHook,
                Deploy = simpleHook,
                RouteTraffic = simpleHook,
                PostRouteTraffic = simpleHook,
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("preDeploy:");
        output.ShouldContain("deploy:");
        output.ShouldContain("routeTraffic:");
        output.ShouldContain("postRouteTraffic:");
    }

    // ── Template Job ──────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_Template_NoParams_WritesInlineLine()
    {
        Job job = new Job.Template
        {
            TemplatePath = new RawExpression("templates/build-job.yml"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("- template: templates/build-job.yml");
    }

    [Test]
    public void WriteJob_Template_WithParams_WritesSection()
    {
        Job job = new Job.Template
        {
            TemplatePath = new RawExpression("templates/build-job.yml"),
            Parameters = new Dictionary<string, TextExpression>
            {
                ["buildType"] = new RawExpression("release"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("- template: templates/build-job.yml");
        output.ShouldContain("parameters:");
        output.ShouldContain("buildType: release");
    }

    // ── Deployment additional options ─────────────────────────────────────────

    [Test]
    public void WriteJob_Deployment_WithServices_WritesServices()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("MyDeploy"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.RunOnce(),
            Services = new Dictionary<string, TextExpression>
            {
                ["db"] = new RawExpression("postgres:14"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("services:");
        output.ShouldContain("db: postgres:14");
    }

    [Test]
    public void WriteJob_Deployment_WithTemplateContext_WritesTemplateContext()
    {
        Job job = new Job.Deployment
        {
            DeploymentId = new RawExpression("MyDeploy"),
            Environment = new DeploymentEnvironment.EnvironmentName
            {
                Name = new RawExpression("prod"),
            },
            Strategy = new DeploymentStrategy.RunOnce(),
            TemplateContext = new Dictionary<string, TextExpression>
            {
                ["region"] = new RawExpression("eastus"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithJobs
        {
            Jobs = [job],
        });

        output.ShouldContain("templateContext:");
        output.ShouldContain("region: eastus");
    }
}
