namespace Invex.Atom.Module.Dotnet.Tests.Cli;

[TestFixture]
public class VerbosityTests
{
    [TestCase(VerbosityOptions.Quiet, "quiet")]
    [TestCase(VerbosityOptions.Minimal, "minimal")]
    [TestCase(VerbosityOptions.Normal, "normal")]
    [TestCase(VerbosityOptions.Detailed, "detailed")]
    [TestCase(VerbosityOptions.Diagnostic, "diagnostic")]
    public void ToCliArg_ReturnsLowercaseValue(VerbosityOptions verbosity, string expected) =>
        verbosity
            .ToCliArg()
            .ShouldBe(expected);

    [Test]
    public void GeneratedOptions_EmitLowercaseVerbosity() =>
        new DotnetBuildOptions
            {
                Verbosity = VerbosityOptions.Quiet,
            }
            .ToArguments()
            .ShouldBe(["--verbosity", "quiet"]);
}
