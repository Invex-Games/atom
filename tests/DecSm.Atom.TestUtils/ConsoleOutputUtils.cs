namespace DecSm.Atom.TestUtils;

[PublicAPI]
public static partial class ConsoleOutputUtils
{
    [GeneratedRegex(@"^\d\d-\d\d-\d\d [+\-]\d\d:\d\d")]
    public static partial Regex LogDateRegex();

    [GeneratedRegex(@"\d\d:\d\d:\d\d\.?\d+")]
    public static partial Regex LogTimeRegex();

    public static string SanitizeLogDateTime(string logOutput)
    {
        logOutput = LogDateRegex()
            .Replace(logOutput, "00-00-00 +00:00");

        logOutput = LogTimeRegex()
            .Replace(logOutput, "00:00:00.000");

        return logOutput;
    }
}
