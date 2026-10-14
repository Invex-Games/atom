namespace Invex.Atom.Module.Dotnet.Cli;

/// <summary>
///     Converts a list of individual arguments into a single command-line string that the process launcher splits
///     back into exactly the same arguments.
/// </summary>
/// <remarks>
///     <see cref="ProcessRunOptions" /> only accepts a pre-joined argument string, which
///     <see cref="System.Diagnostics.ProcessStartInfo.Arguments" /> splits using the Windows
///     <c>CommandLineToArgvW</c> rules on every platform. This class quotes and escapes each argument according to
///     those rules (the same algorithm .NET uses for <see cref="System.Diagnostics.ProcessStartInfo.ArgumentList" />),
///     so values containing spaces, quotes or backslashes survive intact.
/// </remarks>
internal static class CommandLineArguments
{
    /// <summary>
    ///     Joins <paramref name="arguments" /> into a single command-line string, quoting each one as needed.
    /// </summary>
    /// <param name="arguments">The individual arguments, in order. Empty strings are preserved as <c>""</c>.</param>
    /// <returns>The command-line string.</returns>
    public static string Join(IEnumerable<string> arguments)
    {
        var builder = new StringBuilder();

        foreach (var argument in arguments)
        {
            if (builder.Length > 0)
                builder.Append(' ');

            AppendQuoted(builder, argument);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Quotes and escapes a single argument so it is parsed back as exactly one argument with the same value.
    /// </summary>
    /// <param name="argument">The argument value.</param>
    /// <returns>
    ///     The argument unchanged if it is non-empty and contains no whitespace or double quotes; otherwise the
    ///     argument wrapped in double quotes with embedded quotes and their preceding backslashes escaped.
    /// </returns>
    public static string Quote(string argument)
    {
        var builder = new StringBuilder(argument.Length + 2);
        AppendQuoted(builder, argument);

        return builder.ToString();
    }

    private static void AppendQuoted(StringBuilder builder, string argument)
    {
        if (argument.Length > 0 && !argument.Any(static c => char.IsWhiteSpace(c) || c is '"'))
        {
            builder.Append(argument);

            return;
        }

        builder.Append('"');

        var index = 0;

        while (index < argument.Length)
        {
            var c = argument[index++];

            if (c is '\\')
            {
                var backslashCount = 1;

                while (index < argument.Length && argument[index] is '\\')
                {
                    index++;
                    backslashCount++;
                }

                if (index == argument.Length)
                {
                    // Backslashes before the closing quote must be doubled
                    builder.Append('\\', backslashCount * 2);
                }
                else if (argument[index] is '"')
                {
                    // Backslashes before an escaped quote must be doubled, plus one to escape the quote
                    builder.Append('\\', backslashCount * 2 + 1);
                    builder.Append('"');
                    index++;
                }
                else
                {
                    // Backslashes not followed by a quote are literal
                    builder.Append('\\', backslashCount);
                }

                continue;
            }

            if (c is '"')
            {
                builder.Append('\\');
                builder.Append('"');

                continue;
            }

            builder.Append(c);
        }

        builder.Append('"');
    }
}
