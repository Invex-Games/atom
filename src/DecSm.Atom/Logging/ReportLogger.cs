namespace DecSm.Atom.Logging;

/// <summary>
///     An internal logger implementation that captures log entries to be included in the final build report.
/// </summary>
/// <remarks>
///     This logger filters for significant events (Warning, Error, Critical) and sends them to the
///     <see cref="ReportService" />. It also masks any secrets found in the log messages.
/// </remarks>
/// <param name="scopeProvider">The external scope provider for accessing scope data.</param>
internal sealed class ReportLogger(IExternalScopeProvider? scopeProvider) : ILogger
{
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
    ///     Writes a log entry.
    /// </summary>
    /// <remarks>
    ///     This method filters out log levels below Warning. For qualifying entries, it masks secrets,
    ///     extracts the command context from the scope, and adds the log data to the <see cref="ReportService" />.
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

        if (logLevel is LogLevel.Debug or LogLevel.Trace or LogLevel.Information)
            return;

        var time = DateTimeOffset.Now;
        string? command = null;

        scopeProvider?.ForEachScope((scopeData, _) =>
            {
                if (scopeData is not Dictionary<string, object> scopeValues)
                    return;

                if (scopeValues.GetValueOrDefault("Command") is string currentCommand)
                    command = currentCommand;
            },
            state);

        var message = formatter(state, exception);

        if (message is "(null)")
            return;

        // If the message contains any secrets, we don't want to log it
        message = ServiceStaticAccessor<IParamService>.Service?.MaskMatchingSecrets(message) ?? message;

        ServiceStaticAccessor<ReportService>.Service?.AddReportData(
            new LogReportData(message, exception, logLevel, time),
            command);
    }
}
