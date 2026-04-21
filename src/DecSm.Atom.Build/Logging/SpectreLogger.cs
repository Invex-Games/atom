using DecSm.Atom.Build.Util.Scope;

namespace DecSm.Atom.Build.Logging;

/// <summary>
///     An internal logger implementation that writes formatted log messages to the console using Spectre.Console.
/// </summary>
/// <param name="categoryName">The category name for the logger.</param>
/// <param name="scopeProvider">The external scope provider for accessing scope data.</param>
internal sealed partial class SpectreLogger(
    string categoryName,
    IServiceProvider serviceProvider,
    IExternalScopeProvider? scopeProvider
) : ILogger
{
    private IParamService ParamService => field ??= serviceProvider.GetRequiredService<IParamService>();

    private IAnsiConsole AnsiConsole => field ??= serviceProvider.GetRequiredService<IAnsiConsole>();

    /// <summary>
    ///     Checks if the given <paramref name="logLevel" /> is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns><c>true</c> if the log level is not <see cref="LogLevel.None" />; otherwise, <c>false</c>.</returns>
    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None;

    /// <summary>
    ///     Begins a logical operation scope.
    /// </summary>
    /// <param name="state">The identifier for the scope.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <returns>A disposable object that ends the logical operation scope on disposal.</returns>
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull =>
        scopeProvider?.Push(state) ?? NullScope.Instance;

    /// <summary>
    ///     Writes a log entry to the console using Spectre.Console formatting.
    /// </summary>
    /// <remarks>
    ///     This method formats log messages based on their level, with colors and prefixes. It filters out Trace and Debug
    ///     messages unless verbose logging is enabled. It also handles special formatting for process output and exceptions.
    /// </remarks>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        switch (logLevel)
        {
            case LogLevel.None:
            case LogLevel.Debug or LogLevel.Trace when !LogOptions.IsVerboseEnabled:
                return;
        }

        string levelText;
        string levelColour;
        var levelBackground = string.Empty;
        var messageStyle = string.Empty;

        switch (logLevel)
        {
            case LogLevel.Trace:
                levelText = "TRC";
                levelColour = "tan";
                messageStyle = "dim";

                break;

            case LogLevel.Debug:
                levelText = "DBG";
                levelColour = "chartreuse3";
                messageStyle = "dim";

                break;

            case LogLevel.Information:
                levelText = "INF";
                levelColour = "dodgerblue1";

                break;

            case LogLevel.Warning:
                levelText = "WRN";
                levelColour = "darkorange";

                break;

            case LogLevel.Error:
                levelText = "ERR";
                levelColour = "red3_1";

                break;

            case LogLevel.Critical:
                levelText = "FTL";
                levelColour = "white";
                levelBackground = " on red3_1";

                break;

            case LogLevel.None:
            default:
                throw new UnreachableException();
        }

        var time = DateTimeOffset.Now;
        string? command = null;
        var processOutput = false;

        scopeProvider?.ForEachScope((scopeData, _) =>
            {
                if (scopeData is not Dictionary<string, object> scopeValues)
                    return;

                if (scopeValues.GetValueOrDefault("Command") is string currentCommand)
                    command = currentCommand;

                if (scopeValues.GetValueOrDefault("$ProcessOutput") is true)
                    processOutput = true;
            },
            state);

        var message = formatter(state, exception);

        if (message is "(null)")
            return;

        // If the message contains any secrets, we want to mask them
        message = ParamService.MaskMatchingSecrets(message);

        message = message.EscapeMarkup();

        if (processOutput)
        {
            messageStyle = ErrorTypeRegex()
                .IsMatch(message)
                ? "red"
                : "dim";

            var columns = new Columns(new Text("                "),
                new Markup(messageStyle is { Length: > 0 }
                    ? $"[{messageStyle}]{message}[/]"
                    : message).LeftJustified()).Collapse();

            AnsiConsole.Write(columns);

            return;
        }

        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn("Info")
            .AddColumn("Message")
            .AddRow($"[dim]{time:yy-MM-dd zzz}[/]",
                $"[dim]{FormatCategoryName(categoryName.EscapeMarkup(), command)}:[/]")
            .AddRow($"[dim]{time:HH:mm:ss.fff}[/] [bold {levelColour}{levelBackground}]{levelText}[/]",
                messageStyle is { Length: > 0 }
                    ? $"[{messageStyle}]{message}[/]"
                    : message)
            .AddRow(string.Empty);

        if (exception is not null)
        {
            if (RuntimeFeature.IsDynamicCodeSupported)
            {
                const ExceptionFormats exceptionFormat = ExceptionFormats.ShortenPaths |
                                                         ExceptionFormats.ShortenTypes |
                                                         ExceptionFormats.ShortenMethods;

                table.AddRow(new Text(string.Empty), exception.GetRenderable(exceptionFormat));
            }
            else
            {
                table.AddRow(new Text(string.Empty), new Text(exception.ToString()));
            }

            table.AddRow(string.Empty);
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    ///     A regular expression to detect common error-related keywords in log messages.
    /// </summary>
    [GeneratedRegex("error|exception|fail", RegexOptions.IgnoreCase, "en-AU")]
    private static partial Regex ErrorTypeRegex();

    /// <summary>
    ///     Formats the logger category name, prepending the command name if available.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <param name="command">The current command context, if any.</param>
    /// <returns>The formatted category name.</returns>
    private static string FormatCategoryName(string name, string? command)
    {
        // TODO: Restore?
        // if (name == typeof(AtomService).FullName)
        //     return "Atom";

        return command is not null
            ? $"{command} | {name}"
            : name;
    }
}
