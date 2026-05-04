namespace DecSm.Atom.TestUtils;

[PublicAPI]
public sealed class TestLoggerProvider : ILoggerProvider
{
    public TestLogger Logger { get; } = new();

    public void Dispose() { }

    public ILogger CreateLogger(string categoryName) =>
        Logger;
}
