using System.Diagnostics;
using System.Text;

namespace Invex.Atom.Module.Dotnet.Tests.Cli;

[TestFixture]
public class CommandLineArgumentsTests
{
    private static readonly string[][] RoundTripCases =
    [
        ["build"],
        ["C:\\My Projects\\App.csproj"],
        ["/home/user/my project/app.csproj", "--output", "/tmp/out dir/"],
        [""],
        ["a", "", "b"],
        ["say \"hello\""],
        ["trailing\\"],
        ["trailing space\\"],
        ["C:\\path with space\\"],
        ["\\\\server\\share\\dir with space\\\\"],
        ["back\\\\\"quote"],
        ["tab\tseparated"],
        ["\""],
        ["\\"],
        ["Key=Value with spaces;Other=1"],
        ["-reports:/a b/**/coverage.cobertura.xml", "-reporttypes:HtmlInline;JsonSummary"],
    ];

    [TestCase("simple", "simple")]
    [TestCase("--no-restore", "--no-restore")]
    [TestCase("", "\"\"")]
    [TestCase("has space", "\"has space\"")]
    [TestCase("say \"hi\"", "\"say \\\"hi\\\"\"")]
    [TestCase("C:\\dir\\", "C:\\dir\\")]
    [TestCase("C:\\my dir\\", "\"C:\\my dir\\\\\"")]
    [TestCase("a\\\"b", "\"a\\\\\\\"b\"")]
    public void Quote_EscapesAsExpected(string argument, string expected) =>
        CommandLineArguments
            .Quote(argument)
            .ShouldBe(expected);

    [Test]
    public void Join_SeparatesWithSingleSpaces() =>
        CommandLineArguments
            .Join(["build", "my project.csproj", "--configuration", "Release"])
            .ShouldBe("build \"my project.csproj\" --configuration Release");

    [TestCaseSource(nameof(RoundTripCases))]
    public void Join_RoundTripsThroughWindowsArgvRules(string[] arguments) =>
        ParseWindowsCommandLine(CommandLineArguments.Join(arguments))
            .ShouldBe(arguments);

    [TestCaseSource(nameof(RoundTripCases))]
    public void Join_RoundTripsThroughRealProcess(string[] arguments)
    {
        Assume.That(!OperatingSystem.IsWindows() && File.Exists("/usr/bin/printf"),
            "Requires /usr/bin/printf on a non-Windows platform.");

        // ProcessStartInfo.Arguments is split by .NET itself on Unix using the same rules as Windows.
        var startInfo = new ProcessStartInfo("/usr/bin/printf",
            CommandLineArguments.Join(new[] { "%s\\0" }.Concat(arguments)))
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

    /// <summary>
    ///     Independent implementation of the CommandLineToArgvW splitting rules (post-2008 behaviour), as used by
    ///     .NET to split <see cref="ProcessStartInfo.Arguments" />.
    /// </summary>
    private static List<string> ParseWindowsCommandLine(string commandLine)
    {
        var result = new List<string>();
        var i = 0;

        while (true)
        {
            while (i < commandLine.Length && commandLine[i] is ' ' or '\t')
                i++;

            if (i >= commandLine.Length)
                return result;

            var current = new StringBuilder();
            var inQuotes = false;

            while (i < commandLine.Length && (inQuotes || commandLine[i] is not (' ' or '\t')))
            {
                if (commandLine[i] is '\\')
                {
                    var slashes = 0;

                    while (i < commandLine.Length && commandLine[i] is '\\')
                    {
                        slashes++;
                        i++;
                    }

                    if (i < commandLine.Length && commandLine[i] is '"')
                    {
                        current.Append('\\', slashes / 2);

                        if (slashes % 2 is 1)
                        {
                            current.Append('"');
                            i++;
                        }
                    }
                    else
                    {
                        current.Append('\\', slashes);
                    }

                    continue;
                }

                if (commandLine[i] is '"')
                {
                    if (inQuotes && i + 1 < commandLine.Length && commandLine[i + 1] is '"')
                    {
                        current.Append('"');
                        i += 2;

                        continue;
                    }

                    inQuotes = !inQuotes;
                    i++;

                    continue;
                }

                current.Append(commandLine[i]);
                i++;
            }

            result.Add(current.ToString());
        }
    }
}
