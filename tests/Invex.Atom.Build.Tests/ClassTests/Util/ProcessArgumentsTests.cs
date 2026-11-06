namespace Invex.Atom.Build.Tests.ClassTests.Util;

[TestFixture]
internal sealed class ProcessArgumentsTests
{
    private static readonly string[][] RoundTripCases =
    [
        ["log"],
        ["log", "-1", "--format=%H", "--", "src/My App/", "src/Shared/"],
        ["C:\\My Projects\\App"],
        [""],
        ["a", "", "b"],
        ["say \"hello\""],
        ["trailing\\"],
        ["C:\\path with space\\"],
        ["back\\\\\"quote"],
        ["tab\tseparated"],
    ];

    [TestCase("simple", "simple")]
    [TestCase("--format=%H", "--format=%H")]
    [TestCase("", "\"\"")]
    [TestCase("has space", "\"has space\"")]
    [TestCase("say \"hi\"", "\"say \\\"hi\\\"\"")]
    [TestCase("C:\\dir\\", "C:\\dir\\")]
    [TestCase("C:\\my dir\\", "\"C:\\my dir\\\\\"")]
    public void Quote_EscapesAsExpected(string argument, string expected) =>
        ProcessArguments
            .Quote(argument)
            .ShouldBe(expected);

    [Test]
    public void Join_SeparatesWithSingleSpaces() =>
        ProcessArguments
            .Join(["log", "--", "src/My App/", "src/Shared/"])
            .ShouldBe("log -- \"src/My App/\" src/Shared/");

    [TestCaseSource(nameof(RoundTripCases))]
    public void Join_RoundTripsThroughRealProcess(string[] arguments)
    {
        Assume.That(!OperatingSystem.IsWindows(), "Requires /usr/bin/printf on a non-Windows platform.");

        // ProcessStartInfo.Arguments is split by .NET itself on Unix using the same rules as Windows.
        var startInfo = new ProcessStartInfo("/usr/bin/printf", ProcessArguments.Join(["%s\\0", ..arguments]))
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        using var process = System.Diagnostics.Process.Start(startInfo)!;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        output
            .Split('\0')[..^1]
            .ShouldBe(arguments);
    }
}
