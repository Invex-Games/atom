using Environment = System.Environment;

namespace DecSm.StructuredText.GithubActions.Tests;

[TestFixture]
internal sealed class GithubActionWriterStepTests
{
    private static string WriteStep(Step step) =>
        GithubActionWriterHelper.Write(GithubActionWriterHelper.MinimalAction() with
        {
            Jobs =
            [
                GithubActionWriterHelper.MinimalJob() with
                {
                    Steps = [step],
                },
            ],
        });

    [Test]
    public void WriteStep_UsesStep_WritesUsesHeader()
    {
        var step = new Step.UsesStep
        {
            Uses = new RawExpression("actions/checkout@v4"),
        };

        var output = WriteStep(step);
        output.ShouldContain("- uses: actions/checkout@v4");
    }

    [Test]
    public void WriteStep_UsesStep_WithName_WritesNameAsHeader()
    {
        var step = new Step.UsesStep
        {
            Name = new RawExpression("Checkout"),
            Uses = new RawExpression("actions/checkout@v4"),
        };

        var output = WriteStep(step);
        output.ShouldContain("- name: Checkout");
        output.ShouldContain("uses: actions/checkout@v4");
    }

    [Test]
    public void WriteStep_UsesStep_WithId_WritesId()
    {
        var step = new Step.UsesStep
        {
            Id = "my-step",
            Uses = new RawExpression("actions/checkout@v4"),
        };

        var output = WriteStep(step);
        output.ShouldContain("id: my-step");
    }

    [Test]
    public void WriteStep_UsesStep_WithIf_WritesIf()
    {
        var step = new Step.UsesStep
        {
            If = new RawExpression("github.event_name == 'push'"),
            Uses = new RawExpression("actions/checkout@v4"),
        };

        var output = WriteStep(step);
        output.ShouldContain("if: github.event_name == 'push'");
    }

    [Test]
    public void WriteStep_UsesStep_WithWith_WritesWithSection()
    {
        var step = new Step.UsesStep
        {
            Uses = new RawExpression("actions/setup-node@v4"),
            With = new Dictionary<string, TextExpressionCollection>
            {
                ["node-version"] = new([new RawExpression("20")]),
                ["cache"] = new([new RawExpression("npm")]),
            },
        };

        var output = WriteStep(step);
        output.ShouldContain("with:");
        output.ShouldContain("node-version: 20");
        output.ShouldContain("cache: npm");
    }

    [Test]
    public void WriteStep_UsesStep_WithMultiValueWith_WritesWithSection()
    {
        var step = new Step.UsesStep
        {
            Uses = new RawExpression("my/action@v1"),
            With = new Dictionary<string, TextExpressionCollection>
            {
                ["arg"] = new([new RawExpression("line1"), new RawExpression("line2")]),
            },
        };

        var output = WriteStep(step);
        output.ShouldContain("with:");
        output.ShouldContain("arg:");
    }

    [Test]
    public void WriteStep_UsesStep_WithEnv_WritesEnvSection()
    {
        var step = new Step.UsesStep
        {
            Uses = new RawExpression("actions/checkout@v4"),
            Env = new Dictionary<string, TextExpression>
            {
                ["MY_TOKEN"] = new RawExpression("${{ secrets.TOKEN }}"),
            },
        };

        var output = WriteStep(step);
        output.ShouldContain("env:");
        output.ShouldContain("MY_TOKEN:");
    }

    [Test]
    public void WriteStep_UsesStep_WithWorkingDirectory_WritesWorkingDirectory()
    {
        var step = new Step.UsesStep
        {
            Uses = new RawExpression("actions/checkout@v4"),
            WorkingDirectory = new RawExpression("./src"),
        };

        var output = WriteStep(step);
        output.ShouldContain("working-directory: ./src");
    }

    [Test]
    public void WriteStep_UsesStep_WithContinueOnError_WritesContinueOnError()
    {
        var step = new Step.UsesStep
        {
            Uses = new RawExpression("actions/checkout@v4"),
            ContinueOnError = new BooleanExpression(true),
        };

        var output = WriteStep(step);
        output.ShouldContain("continue-on-error: true");
    }

    [Test]
    public void WriteStep_UsesStep_WithTimeoutMinutes_WritesTimeoutMinutes()
    {
        var step = new Step.UsesStep
        {
            Uses = new RawExpression("actions/checkout@v4"),
            TimeoutMinutes = new NumberExpression(5),
        };

        var output = WriteStep(step);
        output.ShouldContain("timeout-minutes: 5");
    }

    [Test]
    public void WriteStep_RunStep_SingleLine_WritesRunHeader()
    {
        var step = new Step.RunStep
        {
            Run = [new RawExpression("echo hello")],
        };

        var output = WriteStep(step);
        output.ShouldContain("- run: echo hello");
    }

    [Test]
    public void WriteStep_RunStep_MultipleLines_WritesBlockWithPipe()
    {
        var step = new Step.RunStep
        {
            Run = [new RawExpression("echo line1"), new RawExpression("echo line2")],
        };

        var output = WriteStep(step);
        output.ShouldContain("run: |");
        output.ShouldContain("echo line1");
        output.ShouldContain("echo line2");
    }

    [Test]
    public void WriteStep_RunStep_WithShell_WritesShellProperty()
    {
        var step = new Step.RunStep
        {
            Run = [new RawExpression("echo hello")],
            Shell = new RawExpression("pwsh"),
        };

        var output = WriteStep(step);
        output.ShouldContain("shell: pwsh");
    }

    [Test]
    public void WriteStep_RunStep_WithName_WritesNameThenRun()
    {
        var step = new Step.RunStep
        {
            Name = new RawExpression("Run tests"),
            Run = [new RawExpression("dotnet test")],
        };

        var output = WriteStep(step);
        output.ShouldContain("- name: Run tests");
        output.ShouldContain("run: dotnet test");

        var nameIdx = output.IndexOf("name: Run tests", StringComparison.Ordinal);
        var runIdx = output.IndexOf("run: dotnet test", StringComparison.Ordinal);
        nameIdx.ShouldBeLessThan(runIdx);
    }

    [Test]
    public void WriteStep_RunStep_WithWorkingDirectory_WritesWorkingDirectory()
    {
        var step = new Step.RunStep
        {
            Run = [new RawExpression("npm install")],
            WorkingDirectory = new RawExpression("./frontend"),
        };

        var output = WriteStep(step);
        output.ShouldContain("working-directory: ./frontend");
    }

    [Test]
    public void WriteStep_RunStep_WithEnv_WritesEnvSection()
    {
        var step = new Step.RunStep
        {
            Run = [new RawExpression("echo $MY_VAR")],
            Env = new Dictionary<string, TextExpression>
            {
                ["MY_VAR"] = new RawExpression("hello"),
            },
        };

        var output = WriteStep(step);
        output.ShouldContain("env:");
        output.ShouldContain("MY_VAR: hello");
    }

    [Test]
    public void WriteStep_RunStep_WithContinueOnError_WritesContinueOnError()
    {
        var step = new Step.RunStep
        {
            Run = [new RawExpression("exit 1")],
            ContinueOnError = new BooleanExpression(true),
        };

        var output = WriteStep(step);
        output.ShouldContain("continue-on-error: true");
    }

    [Test]
    public void WriteStep_RunStep_WithTimeoutMinutes_WritesTimeoutMinutes()
    {
        var step = new Step.RunStep
        {
            Run = [new RawExpression("npm test")],
            TimeoutMinutes = new NumberExpression(10),
        };

        var output = WriteStep(step);
        output.ShouldContain("timeout-minutes: 10");
    }

    [Test]
    public void WriteStep_MultipleSteps_EachSeparatedByBlankLine()
    {
        var job = GithubActionWriterHelper.MinimalJob() with
        {
            Steps =
            [
                new Step.UsesStep
                {
                    Uses = new RawExpression("actions/checkout@v4"),
                },
                new Step.RunStep
                {
                    Run = [new RawExpression("echo hello")],
                },
            ],
        };

        var output = GithubActionWriterHelper.Write(GithubActionWriterHelper.MinimalAction() with
        {
            Jobs = [job],
        });

        var checkoutIdx = output.IndexOf("actions/checkout@v4", StringComparison.Ordinal);
        var echoIdx = output.IndexOf("echo hello", StringComparison.Ordinal);
        var between = output.Substring(checkoutIdx, echoIdx - checkoutIdx);

        // There should be at least one blank line between the steps
        between.ShouldContain(Environment.NewLine + Environment.NewLine);
    }

    [Test]
    public void WriteStep_RunStep_WithValueContainingColon_QuotesValue()
    {
        var step = new Step.UsesStep
        {
            Uses = new RawExpression("actions/checkout@v4"),
            With = new Dictionary<string, TextExpressionCollection>
            {
                ["key"] = new([new RawExpression("value: with colon")]),
            },
        };

        var output = WriteStep(step);
        // "value: with colon" contains ": " which triggers quoting
        output.ShouldContain("'value: with colon'");
    }
}
