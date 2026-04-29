namespace DecSm.Atom.TestUtils;

[PublicAPI]
public sealed class TestLogger : ILogger
{
    public StringBuilder LogContent { get; } = new();

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        LogContent.AppendLine(formatter(state, exception));

    public bool IsEnabled(LogLevel logLevel) =>
        true;

    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull =>
        new ActionScope();
}
