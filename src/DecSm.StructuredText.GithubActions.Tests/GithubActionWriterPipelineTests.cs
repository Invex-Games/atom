using Environment = System.Environment;

namespace DecSm.StructuredText.GithubActions.Tests;

[TestFixture]
internal sealed class GithubActionWriterPipelineTests
{
    [Test]
    public void Write_ActionWithName_WritesNameAtTop()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Name = "My CI",
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldStartWith("name: My CI");
    }

    [Test]
    public void Write_ActionWithName_WritesBlankLineAfterName()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Name = "My CI",
        };

        var output = GithubActionWriterHelper.Write(action);
        var nameEnd = output.IndexOf("name: My CI", StringComparison.Ordinal) + "name: My CI".Length;

        output
            .Substring(nameEnd)
            .ShouldStartWith(Environment.NewLine + Environment.NewLine);
    }

    [Test]
    public void Write_ActionWithRunName_WritesRunName()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            RunName = new RawExpression("Run ${{ github.run_number }}"),
        };

        var output = GithubActionWriterHelper.Write(action);
        // The Format helper quotes strings that don't start with ${{ but end with }}
        output.ShouldContain("run-name: 'Run ${{ github.run_number }}'");
    }

    [Test]
    public void Write_ActionWithoutNameOrRunName_DoesNotWriteBlankLineBeforeOn()
    {
        var action = GithubActionWriterHelper.MinimalAction();
        var output = GithubActionWriterHelper.Write(action);

        output.ShouldStartWith("on:");
    }

    [Test]
    public void Write_ActionWithOnlyName_WritesBlankLineThenOn()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Name = "CI",
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("CI" + Environment.NewLine + Environment.NewLine + "on:");
    }

    [Test]
    public void Write_ActionWithPermissionsReadAll_WritesReadAll()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Permissions = new Permissions.All(PermissionsLevel.Read),
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("permissions: read-all");
    }

    [Test]
    public void Write_ActionWithPermissionsWriteAll_WritesWriteAll()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Permissions = new Permissions.All(PermissionsLevel.Write),
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("permissions: write-all");
    }

    [Test]
    public void Write_ActionWithPermissionsNone_WritesEmptyBraces()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Permissions = new Permissions.All(PermissionsLevel.None),
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("permissions: { }");
    }

    [Test]
    public void Write_ActionWithExactPermissions_WritesPermissionsSection()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Permissions = new Permissions.Exact(new()
            {
                Contents = PermissionsLevel.Read,
                PullRequests = PermissionsLevel.Write,
            }),
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("permissions:");
        output.ShouldContain("contents: read");
        output.ShouldContain("pull-requests: write");
    }

    [Test]
    public void Write_ActionWithExactPermissions_WritesIdTokenAsIdDash()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Permissions = new Permissions.Exact(new()
            {
                IdTokens = PermissionsLevel.Write,
            }),
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("id-token: write");
    }

    [Test]
    public void Write_ActionWithPermissions_WritesBlankLineAfterPermissions()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Permissions = new Permissions.All(PermissionsLevel.Read),
        };

        var output = GithubActionWriterHelper.Write(action);
        var permIdx = output.IndexOf("permissions: read-all", StringComparison.Ordinal);
        var jobsIdx = output.IndexOf("jobs:", StringComparison.Ordinal);

        // There should be blank line content between permissions and jobs
        output
            .Substring(permIdx, jobsIdx - permIdx)
            .ShouldContain(Environment.NewLine + Environment.NewLine);
    }

    [Test]
    public void Write_ActionWithEnv_WritesEnvSection()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Env = new Dictionary<string, TextExpression>
            {
                ["MY_VAR"] = new RawExpression("my-value"),
                ["OTHER"] = new RawExpression("other-value"),
            },
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("env:");
        output.ShouldContain("MY_VAR: my-value");
        output.ShouldContain("OTHER: other-value");
    }

    [Test]
    public void Write_ActionWithNullEnv_DoesNotWriteEnvSection()
    {
        var action = GithubActionWriterHelper.MinimalAction();
        var output = GithubActionWriterHelper.Write(action);
        output.ShouldNotContain("env:");
    }

    [Test]
    public void Write_ActionWithConcurrency_WritesConcurrencySection()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Concurrency = new()
            {
                Group = new RawExpression("my-group"),
            },
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("concurrency:");
        output.ShouldContain("my-group");
    }

    [Test]
    public void Write_ActionWithConcurrencyAndCancelInProgress_WritesCancelFlag()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Concurrency = new()
            {
                Group = new RawExpression("group-${{ github.ref }}"),
                CancelInProgress = new BooleanExpression(true),
            },
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("cancel-in-progress: true");
    }

    [Test]
    public void Write_Action_AlwaysWritesJobsSection()
    {
        var action = GithubActionWriterHelper.MinimalAction();
        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("jobs:");
    }

    [Test]
    public void Write_Action_JobsSectionAppearsAfterOn()
    {
        var action = GithubActionWriterHelper.MinimalAction();
        var output = GithubActionWriterHelper.Write(action);

        output
            .IndexOf("on:", StringComparison.Ordinal)
            .ShouldBeLessThan(output.IndexOf("jobs:", StringComparison.Ordinal));
    }

    [Test]
    public void Write_Action_MultipleJobs_WritesAll()
    {
        var action = GithubActionWriterHelper.MinimalAction() with
        {
            Jobs = [GithubActionWriterHelper.MinimalJob("job1"), GithubActionWriterHelper.MinimalJob("job2")],
        };

        var output = GithubActionWriterHelper.Write(action);
        output.ShouldContain("job1:");
        output.ShouldContain("job2:");
    }
}
