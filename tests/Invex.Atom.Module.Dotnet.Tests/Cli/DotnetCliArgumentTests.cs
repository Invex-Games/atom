using FakeItEasy;
using Invex.Process;

namespace Invex.Atom.Module.Dotnet.Tests.Cli;

[TestFixture]
public class DotnetCliArgumentTests
{
    private static (IDotnetCli Cli, List<ProcessRunOptions> Calls) CreateSut()
    {
        var calls = new List<ProcessRunOptions>();
        var processRunner = A.Fake<IProcessRunner>();

        A
            .CallTo(() => processRunner.RunAsync(A<ProcessRunOptions>._, A<CancellationToken>._))
            .ReturnsLazily((ProcessRunOptions options, CancellationToken _) =>
            {
                calls.Add(options);

                return Task.FromResult(new ProcessRunResult(options, 0, string.Empty, string.Empty));
            });

        return (new DotnetCli(processRunner), calls);
    }

    [Test]
    public void Flags_AreEmittedOnlyWhenTrue()
    {
        new DotnetBuildOptions
            {
                NoRestore = true,
                NoLogo = false,
            }
            .ToArguments()
            .ShouldBe(["--no-restore"]);

        new DotnetBuildOptions()
            .ToArguments()
            .ShouldBeEmpty();
    }

    [Test]
    public void ValueTakingBooleans_EmitTheirValue()
    {
        new DotnetBuildOptions
            {
                SelfContained = false,
            }
            .ToArguments()
            .ShouldBe(["--self-contained", "false"]);

        new DotnetBuildOptions
            {
                SelfContained = true,
            }
            .ToArguments()
            .ShouldBe(["--self-contained", "true"]);
    }

    [Test]
    public void Values_AreSeparateArguments()
    {
        new DotnetBuildOptions
            {
                Configuration = "Release",
                Output = "/tmp/my output",
                Property = new Dictionary<string, string>
                {
                    ["A"] = "1",
                    ["B"] = "two words",
                },
                Source = ["https://a", "https://b"],
            }
            .ToArguments()
            .ShouldBe([
                "--configuration",
                "Release",
                "--output",
                "/tmp/my output",
                "--property",
                "A=1",
                "--property",
                "B=two words",
                "--source",
                "https://a",
                "--source",
                "https://b",
            ]);
    }

    [Test]
    public async Task Build_QuotesPathsWithSpaces()
    {
        var (cli, calls) = CreateSut();

        await cli.Build("/src/my project/App.csproj",
            new()
            {
                Configuration = "Release",
                Output = "/out/with space",
                Verbosity = VerbosityOptions.Quiet,
            });

        calls.Count.ShouldBe(1);
        calls[0].Name.ShouldBe("dotnet");

        calls[0]
            .Args
            .ShouldBe(
                "build \"/src/my project/App.csproj\" --configuration Release --output \"/out/with space\" --verbosity quiet");
    }

    [Test]
    public async Task Command_WithoutArgumentsOrOptions_IncludesCommandName()
    {
        var (cli, calls) = CreateSut();

        await cli.BuildServerShutdown(new()
        {
            MsBuild = true,
        });

        await cli.Workload();

        calls[0].Args.ShouldBe("build-server shutdown --msbuild");
        calls[1].Args.ShouldBe("workload");
    }

    [Test]
    public async Task ProcessRunOptions_NameAndArgsAreOverridden()
    {
        var (cli, calls) = CreateSut();

        await cli.Restore("a.csproj",
            processRunOptions: new("ignored", "ignored")
            {
                AllowFailedResult = true,
            });

        calls[0].Name.ShouldBe("dotnet");
        calls[0].Args.ShouldBe("restore a.csproj");
        calls[0].AllowFailedResult.ShouldBeTrue();
    }

    [Test]
    public async Task ForwardedArguments_ArePassedVerbatim()
    {
        var (cli, calls) = CreateSut();

        await cli.ToolExecute("dotnet-reportgenerator-globaltool",
            ["-reports:/a b/coverage.xml", "-reporttypes:HtmlInline;JsonSummary"],
            new()
            {
                Yes = true,
            });

        calls[0]
            .Args
            .ShouldBe(
                "tool execute dotnet-reportgenerator-globaltool \"-reports:/a b/coverage.xml\" -reporttypes:HtmlInline;JsonSummary --yes");
    }
}
