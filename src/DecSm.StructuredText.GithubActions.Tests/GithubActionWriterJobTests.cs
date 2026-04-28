namespace DecSm.StructuredText.GithubActions.Tests;

[TestFixture]
internal sealed class GithubActionWriterJobTests
{
    private static GithubAction ActionWithJob(Job job) =>
        GithubActionWriterHelper.MinimalAction() with
        {
            Jobs = [job],
        };

    private static string WriteJob(Job job) =>
        GithubActionWriterHelper.Write(ActionWithJob(job));

    // ── Basics ────────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WritesJobNameAsSection()
    {
        var output = WriteJob(GithubActionWriterHelper.MinimalJob("my-job"));
        output.ShouldContain("my-job:");
    }

    // ── needs ─────────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithNeeds_WritesNeedsInline()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Needs = [new RawExpression("other-job")],
        };

        var output = WriteJob(job);
        output.ShouldContain("needs: [ other-job ]");
    }

    [Test]
    public void WriteJob_WithMultipleNeeds_WritesAll()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Needs = [new RawExpression("job1"), new RawExpression("job2")],
        };

        var output = WriteJob(job);
        output.ShouldContain("needs:");
        output.ShouldContain("job1");
        output.ShouldContain("job2");
    }

    [Test]
    public void WriteJob_WithNoNeeds_DoesNotWriteNeedsProperty()
    {
        var output = WriteJob(GithubActionWriterHelper.MinimalJob());
        output.ShouldNotContain("needs:");
    }

    // ── if: ───────────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithIf_WritesIfProperty()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            If = new RawExpression("github.event_name == 'push'"),
        };

        var output = WriteJob(job);
        output.ShouldContain("if: github.event_name == 'push'");
    }

    [Test]
    public void WriteJob_WithNoIf_DoesNotWriteIfProperty()
    {
        var output = WriteJob(GithubActionWriterHelper.MinimalJob());
        output.ShouldNotContain("if:");
    }

    // ── runs-on ───────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithSingleLabel_WritesRunsOnInline()
    {
        var output = WriteJob(GithubActionWriterHelper.MinimalJob());
        output.ShouldContain("runs-on: ubuntu-latest");
    }

    [Test]
    public void WriteJob_WithMultipleLabels_WritesRunsOnSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            RunsOn = new()
            {
                Labels = [new RawExpression("self-hosted"), new RawExpression("linux")],
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("runs-on:");
        output.ShouldContain("labels:");
        output.ShouldContain("self-hosted");
        output.ShouldContain("linux");
    }

    [Test]
    public void WriteJob_WithGroup_WritesRunsOnSectionWithGroup()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            RunsOn = new()
            {
                Labels = [],
                Group = new RawExpression("my-runner-group"),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("runs-on:");
        output.ShouldContain("group: my-runner-group");
    }

    // ── snapshot ──────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithSnapshot_WritesSnapshotSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Snapshot = new()
            {
                ImageName = new RawExpression("my-image"),
                Version = new RawExpression("1.0.0"),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("snapshot:");
        output.ShouldContain("image-name: my-image");
        output.ShouldContain("version: 1.0.0");
    }

    [Test]
    public void WriteJob_WithSnapshotNoVersion_WritesInlineSnapshot()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Snapshot = new()
            {
                ImageName = new RawExpression("my-image"),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("snapshot: my-image");
    }

    // ── environment ───────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithEnvironmentAndUrl_WritesEnvironmentSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Environment = new()
            {
                Name = new RawExpression("production"),
                UrlValue = new RawExpression("https://example.com"),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("environment:");
        output.ShouldContain("name: production");
        output.ShouldContain("url: https://example.com");
    }

    [Test]
    public void WriteJob_WithEnvironmentNoUrl_WritesInlineEnvironment()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Environment = new()
            {
                Name = new RawExpression("staging"),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("environment: staging");
    }

    // ── concurrency ───────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithConcurrency_WritesConcurrencySection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Concurrency = new()
            {
                Group = new RawExpression("${{ github.workflow }}-${{ github.ref }}"),
                CancelInProgress = new BooleanExpression(true),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("concurrency:");
        output.ShouldContain("cancel-in-progress: true");
    }

    // ── outputs ───────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithOutputs_WritesOutputsSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Outputs = new Dictionary<string, TextExpression>
            {
                ["artifact-path"] = new RawExpression("${{ steps.build.outputs.path }}"),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("outputs:");
        output.ShouldContain("artifact-path:");
    }

    // ── env ───────────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithEnv_WritesEnvSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Env = new Dictionary<string, TextExpression>
            {
                ["NODE_ENV"] = new RawExpression("production"),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("env:");
        output.ShouldContain("NODE_ENV: production");
    }

    // ── timeout-minutes ───────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithTimeoutMinutes_WritesTimeoutMinutes()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            TimeoutMinutes = new NumberExpression(30),
        };

        var output = WriteJob(job);
        output.ShouldContain("timeout-minutes: 30");
    }

    // ── strategy / matrix ─────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithStrategy_WritesStrategyAndMatrixSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Strategy = new()
            {
                Matrix = new()
                {
                    Map = new Dictionary<string, TextExpressionCollection>
                    {
                        ["os"] = new([new RawExpression("ubuntu-latest"), new RawExpression("windows-latest")]),
                    },
                },
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("strategy:");
        output.ShouldContain("matrix:");
        output.ShouldContain("os:");
        output.ShouldContain("ubuntu-latest");
        output.ShouldContain("windows-latest");
    }

    [Test]
    public void WriteJob_WithStrategyFailFast_WritesFailFast()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Strategy = new()
            {
                FailFast = new BooleanExpression(false),
                Matrix = new(),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("fail-fast: false");
    }

    [Test]
    public void WriteJob_WithStrategyMaxParallel_WritesMaxParallel()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Strategy = new()
            {
                MaxParallel = new NumberExpression(2),
                Matrix = new(),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("max-parallel: 2");
    }

    [Test]
    public void WriteJob_WithMatrixInclude_WritesIncludeEntries()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Strategy = new()
            {
                Matrix = new()
                {
                    Include =
                    [
                        new Dictionary<string, TextExpression>
                        {
                            ["os"] = new RawExpression("macos-latest"),
                            ["version"] = new RawExpression("14"),
                        },
                    ],
                },
            },
        };

        var output = WriteJob(job);
        // The first include key is used as the section header via string interpolation (not formatted)
        // Subsequent keys are written as proper properties via WriteProperty
        output.ShouldContain("macos-latest");
        output.ShouldContain("version: 14");
    }

    // ── continue-on-error ─────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithContinueOnError_WritesContinueOnError()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            ContinueOnError = new BooleanExpression(true),
        };

        var output = WriteJob(job);
        output.ShouldContain("continue-on-error: true");
    }

    // ── container ─────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithContainer_WritesContainerSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Container = new()
            {
                Image = new RawExpression("node:18"),
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("container:");
        output.ShouldContain("image: node:18");
    }

    [Test]
    public void WriteJob_WithContainerCredentials_WritesCredentialsSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Container = new()
            {
                Image = new RawExpression("ghcr.io/my/image"),
                Credentials = new()
                {
                    Username = new RawExpression("${{ github.actor }}"),
                    Password = new RawExpression("${{ secrets.GITHUB_TOKEN }}"),
                },
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("credentials:");
        output.ShouldContain("username:");
        output.ShouldContain("password:");
    }

    [Test]
    public void WriteJob_WithContainerPorts_WritesPortsProperty()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Container = new()
            {
                Image = new RawExpression("redis"),
                Ports = [new RawExpression("6379")],
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("ports:");
        output.ShouldContain("6379");
    }

    // ── services ──────────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithServices_WritesServicesSection()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Services = new Dictionary<string, Container>
            {
                ["redis"] = new()
                {
                    Image = new RawExpression("redis:6"),
                },
            },
        };

        var output = WriteJob(job);
        output.ShouldContain("services:");
        output.ShouldContain("redis:");
        output.ShouldContain("image: redis:6");
    }

    // ── permissions ───────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithJobPermissionsDifferentFromAction_WritesPermissions()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Permissions = new Permissions.All(PermissionsLevel.Read),
            Jobs =
            [
                GithubActionWriterHelper.MinimalJob() with
                {
                    Permissions = new Permissions.Exact(new()
                    {
                        Contents = PermissionsLevel.Write,
                    }),
                },
            ],
        };

        var output = GithubActionWriterHelper.Write(action);

        // Both job-level and action-level permissions exist; job-level differs so it should be written twice
        output
            .Split("permissions:")
            .Length
            .ShouldBeGreaterThanOrEqualTo(3); // At least two occurrences
    }

    // ── steps section ─────────────────────────────────────────────────────────

    [Test]
    public void WriteJob_WithSteps_WritesStepsSection()
    {
        var output = WriteJob(GithubActionWriterHelper.MinimalJob());
        output.ShouldContain("steps:");
    }
}
