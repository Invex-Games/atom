namespace DecSm.StructuredText.AzureDevopsPipelines.Tests;

[TestFixture]
internal sealed class DevopsPipelineWriterStepTests
{
    [Test]
    public void WriteStep_Task_Minimal_WritesTaskLine()
    {
        Step step = new Step.Task
        {
            TaskName = new RawExpression("DotNetCoreCLI@2"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- task: DotNetCoreCLI@2");
    }

    [Test]
    public void WriteStep_Task_WithInputs_WritesInputsSection()
    {
        Step step = new Step.Task
        {
            TaskName = new RawExpression("DotNetCoreCLI@2"),
            Inputs = new Dictionary<string, TextExpression>
            {
                ["command"] = new RawExpression("build"),
                ["projects"] = new RawExpression("**/*.csproj"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("inputs:");
        output.ShouldContain("command: build");
        output.ShouldContain("projects: **/*.csproj");
    }

    [Test]
    public void WriteStep_Task_WithCommonProperties_WritesAll()
    {
        Step step = new Step.Task
        {
            TaskName = new RawExpression("MyTask@1"),
            DisplayName = new RawExpression("Run My Task"),
            Name = new RawExpression("myStep"),
            Condition = new RawExpression("succeeded()"),
            ContinueOnError = new BooleanExpression(true),
            Enabled = new BooleanExpression(false),
            TimeoutInMinutes = new NumberExpression(30),
            RetryCountOnTaskFailure = new NumberExpression(2),
            Env = new Dictionary<string, TextExpression>
            {
                ["MY_TOKEN"] = new RawExpression("$(mySecret)"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("displayName: Run My Task");
        output.ShouldContain("name: myStep");
        output.ShouldContain("condition: succeeded()");
        output.ShouldContain("continueOnError: true");
        output.ShouldContain("enabled: false");
        output.ShouldContain("timeoutInMinutes: 30");
        output.ShouldContain("retryCountOnTaskFailure: 2");
        output.ShouldContain("env:");
        output.ShouldContain("MY_TOKEN: $(mySecret)");
    }

    [Test]
    public void WriteStep_Script_WritesScriptLine()
    {
        Step step = new Step.Script
        {
            ScriptContent = new RawExpression("echo hello"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- script: echo hello");
    }

    [Test]
    public void WriteStep_Script_WithFailOnStderr_WritesProperty()
    {
        Step step = new Step.Script
        {
            ScriptContent = new RawExpression("echo hello"),
            FailOnStderr = new BooleanExpression(true),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("failOnStderr: true");
    }

    [Test]
    public void WriteStep_Script_WithWorkingDirectory_WritesProperty()
    {
        Step step = new Step.Script
        {
            ScriptContent = new RawExpression("npm install"),
            WorkingDirectory = new RawExpression("$(Build.SourcesDirectory)/frontend"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("workingDirectory: $(Build.SourcesDirectory)/frontend");
    }

    [Test]
    public void WriteStep_PowerShell_WritesPowerShellLine()
    {
        Step step = new Step.PowerShell
        {
            ScriptContent = new RawExpression("Write-Host 'hello'"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- powershell: Write-Host 'hello'");
    }

    [Test]
    public void WriteStep_PowerShell_WithErrorActionPreference_WritesProperty()
    {
        Step step = new Step.PowerShell
        {
            ScriptContent = new RawExpression("do-something"),
            ErrorActionPreference = new RawExpression("continue"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("errorActionPreference: continue");
    }

    [Test]
    public void WriteStep_PowerShell_WithIgnoreLastExitCode_WritesProperty()
    {
        Step step = new Step.PowerShell
        {
            ScriptContent = new RawExpression("do-something"),
            IgnoreLastExitCode = new BooleanExpression(true),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("ignoreLASTEXITCODE: true");
    }

    // ── Pwsh step ─────────────────────────────────────────────────────────────

    [Test]
    public void WriteStep_Pwsh_WritesPwshLine()
    {
        Step step = new Step.Pwsh
        {
            ScriptContent = new RawExpression("Write-Output 'hello'"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- pwsh: Write-Output 'hello'");
    }

    [Test]
    public void WriteStep_Pwsh_WithAllPsProperties_WritesAll()
    {
        Step step = new Step.Pwsh
        {
            ScriptContent = new RawExpression("do-something"),
            FailOnStderr = new BooleanExpression(true),
            WorkingDirectory = new RawExpression("/src"),
            ErrorActionPreference = new RawExpression("stop"),
            IgnoreLastExitCode = new BooleanExpression(false),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("failOnStderr: true");
        output.ShouldContain("workingDirectory: /src");
        output.ShouldContain("errorActionPreference: stop");
        output.ShouldContain("ignoreLASTEXITCODE: false");
    }

    // ── Bash step ─────────────────────────────────────────────────────────────

    [Test]
    public void WriteStep_Bash_WritesBashLine()
    {
        Step step = new Step.Bash
        {
            ScriptContent = new RawExpression("echo hello"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- bash: echo hello");
    }

    [Test]
    public void WriteStep_Bash_WithFailOnStderrAndWorkingDirectory_WritesBoth()
    {
        Step step = new Step.Bash
        {
            ScriptContent = new RawExpression("make build"),
            FailOnStderr = new BooleanExpression(false),
            WorkingDirectory = new RawExpression("$(Build.SourcesDirectory)"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("failOnStderr: false");
        output.ShouldContain("workingDirectory: $(Build.SourcesDirectory)");
    }

    // ── Checkout step ─────────────────────────────────────────────────────────

    [Test]
    public void WriteStep_Checkout_WithSelf_WritesCheckoutSelf()
    {
        Step step = new Step.Checkout
        {
            Repository = new RawExpression("self"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- checkout: self");
    }

    [Test]
    public void WriteStep_Checkout_WithNone_WritesCheckoutNone()
    {
        Step step = new Step.Checkout
        {
            Repository = new RawExpression("none"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- checkout: none");
    }

    [Test]
    public void WriteStep_Checkout_WithAllOptions_WritesAllProperties()
    {
        Step step = new Step.Checkout
        {
            Repository = new RawExpression("self"),
            Clean = new BooleanExpression(true),
            FetchDepth = new NumberExpression(0),
            Lfs = new BooleanExpression(true),
            Submodules = new BooleanExpression(false),
            Path = new RawExpression("$(Build.SourcesDirectory)/src"),
            PersistCredentials = new BooleanExpression(true),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("clean: true");
        output.ShouldContain("fetchDepth: 0");
        output.ShouldContain("lfs: true");
        output.ShouldContain("submodules: false");
        output.ShouldContain("path: $(Build.SourcesDirectory)/src");
        output.ShouldContain("persistCredentials: true");
    }

    // ── Download step ─────────────────────────────────────────────────────────

    [Test]
    public void WriteStep_Download_WritesDownloadLine()
    {
        Step step = new Step.Download
        {
            Pipeline = new RawExpression("current"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- download: current");
    }

    [Test]
    public void WriteStep_Download_WithAllOptions_WritesAllProperties()
    {
        Step step = new Step.Download
        {
            Pipeline = new RawExpression("myUpstream"),
            Artifact = new RawExpression("drop"),
            Patterns = new[] { "**/*.zip" },
            Path = new RawExpression("$(Pipeline.Workspace)/drop"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("artifact: drop");
        output.ShouldContain("patterns:");
        output.ShouldContain("- **/*.zip");
        output.ShouldContain("path: $(Pipeline.Workspace)/drop");
    }

    // ── DownloadBuild step ────────────────────────────────────────────────────

    [Test]
    public void WriteStep_DownloadBuild_WritesDownloadBuildLine()
    {
        Step step = new Step.DownloadBuild
        {
            Build = new RawExpression("myBuild"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- downloadBuild: myBuild");
    }

    [Test]
    public void WriteStep_DownloadBuild_WithAllOptions_WritesAllProperties()
    {
        Step step = new Step.DownloadBuild
        {
            Build = new RawExpression("myBuild"),
            Artifact = new RawExpression("logs"),
            Patterns = new[] { "**/*.log" },
            Path = new RawExpression("$(Build.StagingDirectory)"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("artifact: logs");
        output.ShouldContain("- **/*.log");
        output.ShouldContain("path: $(Build.StagingDirectory)");
    }

    // ── GetPackage step ───────────────────────────────────────────────────────

    [Test]
    public void WriteStep_GetPackage_WritesGetPackageLine()
    {
        Step step = new Step.GetPackage
        {
            Package = new RawExpression("myPackage"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- getPackage: myPackage");
    }

    [Test]
    public void WriteStep_GetPackage_WithVersionAndPath_WritesBoth()
    {
        Step step = new Step.GetPackage
        {
            Package = new RawExpression("myNuGet"),
            Version = new RawExpression("1.2.3"),
            Path = new RawExpression("$(Build.ArtifactStagingDirectory)"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("version: 1.2.3");
        output.ShouldContain("path: $(Build.ArtifactStagingDirectory)");
    }

    // ── Publish step ──────────────────────────────────────────────────────────

    [Test]
    public void WriteStep_Publish_WritesPublishLine()
    {
        Step step = new Step.Publish
        {
            PublishPath = new RawExpression("$(Build.ArtifactStagingDirectory)"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- publish: $(Build.ArtifactStagingDirectory)");
    }

    [Test]
    public void WriteStep_Publish_WithArtifact_WritesArtifactProperty()
    {
        Step step = new Step.Publish
        {
            PublishPath = new RawExpression("$(Build.ArtifactStagingDirectory)"),
            Artifact = new RawExpression("drop"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("artifact: drop");
    }

    // ── Template step ─────────────────────────────────────────────────────────

    [Test]
    public void WriteStep_Template_NoParams_WritesInlineLine()
    {
        Step step = new Step.Template
        {
            TemplatePath = new RawExpression("templates/common-steps.yml"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- template: templates/common-steps.yml");
    }

    [Test]
    public void WriteStep_Template_WithParams_WritesSection()
    {
        Step step = new Step.Template
        {
            TemplatePath = new RawExpression("templates/deploy.yml"),
            Parameters = new Dictionary<string, TextExpression>
            {
                ["environment"] = new RawExpression("prod"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- template: templates/deploy.yml");
        output.ShouldContain("parameters:");
        output.ShouldContain("environment: prod");
    }

    // ── ReviewApp step ────────────────────────────────────────────────────────

    [Test]
    public void WriteStep_ReviewApp_WritesReviewAppLine()
    {
        Step step = new Step.ReviewApp
        {
            ReviewAppType = new RawExpression("kubernetes"),
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("- reviewApp: kubernetes");
    }

    // ── StepTarget ────────────────────────────────────────────────────────────

    [Test]
    public void WriteStepTarget_TargetName_WritesSimpleTargetProperty()
    {
        Step step = new Step.Task
        {
            TaskName = new RawExpression("MyTask@1"),
            Target = new StepTarget.TargetName
            {
                Name = new RawExpression("myContainer"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("target: myContainer");
    }

    [Test]
    public void WriteStepTarget_TargetSpec_WithContainer_WritesContainerInSection()
    {
        Step step = new Step.Task
        {
            TaskName = new RawExpression("MyTask@1"),
            Target = new StepTarget.TargetSpec
            {
                Container = new RawExpression("myContainer"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("target:");
        output.ShouldContain("container: myContainer");
    }

    [Test]
    public void WriteStepTarget_TargetSpec_WithCommands_WritesCommandsProperty()
    {
        Step step = new Step.Task
        {
            TaskName = new RawExpression("MyTask@1"),
            Target = new StepTarget.TargetSpec
            {
                Commands = new RawExpression("restricted"),
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("target:");
        output.ShouldContain("commands: restricted");
    }

    [Test]
    public void WriteStepTarget_TargetSpec_WithSettableVariables_AllowedBool_WritesProperty()
    {
        Step step = new Step.Task
        {
            TaskName = new RawExpression("MyTask@1"),
            Target = new StepTarget.TargetSpec
            {
                SettableVariables = new()
                {
                    Allowed = new BooleanExpression(false),
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("settableVariables: false");
    }

    [Test]
    public void WriteStepTarget_TargetSpec_WithSettableVariables_List_WritesList()
    {
        Step step = new Step.Task
        {
            TaskName = new RawExpression("MyTask@1"),
            Target = new StepTarget.TargetSpec
            {
                SettableVariables = new()
                {
                    AllowedVariables = new[] { "MY_VAR", "ANOTHER_VAR" },
                },
            },
        };

        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps = [step],
        });

        output.ShouldContain("settableVariables:");
        output.ShouldContain("- MY_VAR");
        output.ShouldContain("- ANOTHER_VAR");
    }

    // ── JobContainer inline ───────────────────────────────────────────────────

    [Test]
    public void WriteJobContainer_ContainerName_WritesSimpleContainerProperty()
    {
        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Container = new JobContainer.ContainerName
            {
                Name = new RawExpression("myContainer"),
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        });

        output.ShouldContain("container: myContainer");
    }

    [Test]
    public void WriteJobContainer_ContainerSpec_WritesSection()
    {
        var output = PipelineWriterHelper.Write(new DevopsPipeline.DevopsPipelineWithSteps
        {
            Container = new JobContainer.ContainerSpec
            {
                Image = new RawExpression("ubuntu:22.04"),
                Options = new RawExpression("--user root"),
                Endpoint = new RawExpression("myRegistry"),
                Env = new Dictionary<string, TextExpression>
                {
                    ["ENV_VAR"] = new RawExpression("val"),
                },
                Ports = new[] { "8080:80" },
                Volumes = new[] { "/opt:/opt" },
                MapDockerSocket = new BooleanExpression(true),
            },
            Steps =
            [
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo hi"),
                },
            ],
        });

        output.ShouldContain("container:");
        output.ShouldContain("image: ubuntu:22.04");
        output.ShouldContain("options: --user root");
        output.ShouldContain("endpoint: myRegistry");
        output.ShouldContain("ENV_VAR: val");
        output.ShouldContain("- 8080:80");
        output.ShouldContain("- /opt:/opt");
        output.ShouldContain("mapDockerSocket: true");
    }

    // ── Multiple steps ────────────────────────────────────────────────────────

    [Test]
    public void WriteSteps_Multiple_WritesAllInOrder()
    {
        var pipeline = new DevopsPipeline.DevopsPipelineWithSteps
        {
            Steps =
            [
                new Step.Checkout
                {
                    Repository = new RawExpression("self"),
                },
                new Step.Script
                {
                    ScriptContent = new RawExpression("echo build"),
                },
                new Step.Publish
                {
                    PublishPath = new RawExpression("drop"),
                },
            ],
        };

        var output = PipelineWriterHelper.Write(pipeline);
        var checkoutIndex = output.IndexOf("- checkout:", StringComparison.Ordinal);
        var scriptIndex = output.IndexOf("- script:", StringComparison.Ordinal);
        var publishIndex = output.IndexOf("- publish:", StringComparison.Ordinal);

        checkoutIndex.ShouldBeLessThan(scriptIndex);
        scriptIndex.ShouldBeLessThan(publishIndex);
    }
}
