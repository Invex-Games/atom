namespace DecSm.Atom.Tests.ClassTests.Logging;

[TestFixture]
public class ReportLoggerTests
{
    [SetUp]
    public void Setup()
    {
        _scopeProvider = new();
        _logger = new(_scopeProvider);
    }

    private ReportLogger _logger;
    private TestScopeProvider _scopeProvider;

    [Test]
    public void IsEnabled_WhenLogLevelIsNone_ReturnsFalse()
    {
        // Act
        var result = _logger.IsEnabled(LogLevel.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void IsEnabled_WhenLogLevelIsNotNone_ReturnsTrue()
    {
        // Act
        var result = _logger.IsEnabled(LogLevel.Information);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void BeginScope_WhenCalled_ReturnsDisposable()
    {
        // Act
        var scope = _logger.BeginScope(new Dictionary<string, object>());

        // Assert
        scope.ShouldNotBeNull();
    }

    [Test]
    public void Log_WhenLogLevelIsNotEnabled_DoesNotLog()
    {
        // Arrange
        const LogLevel logLevel = LogLevel.Debug;
        var eventId = new EventId(1, "TestEvent");
        const string state = "TestState";
        Exception? exception = null;
        ServiceStaticAccessor<ReportService>.Service = new();

        // Act
        _logger.Log(logLevel, eventId, state, exception, (s, _) => s);

        // Assert
        ServiceStaticAccessor<ReportService>
            .Service
            .GetReportData()
            .ShouldBeEmpty();
    }

    [Test]
    public void Log_WhenLogLevelIsEnabled_LogsMessage()
    {
        // Arrange
        const LogLevel logLevel = LogLevel.Error;
        var eventId = new EventId(1, "TestEvent");
        const string state = "TestState";
        Exception? exception = null;
        ServiceStaticAccessor<ReportService>.Service = new();

        // Act
        _logger.Log(logLevel, eventId, state, exception, (s, _) => s);

        // Assert
        ServiceStaticAccessor<ReportService>
            .Service
            .GetReportData()
            .ShouldNotBeEmpty();
    }

    private class TestScopeProvider : IExternalScopeProvider
    {
        public void ForEachScope<TState>(Action<object, TState> callback, TState state)
        {
            // Simulate scope data
            var scopeData = new Dictionary<string, object>
            {
                { "Command", "TestCommand" },
            };

            callback(scopeData, state);
        }

        public IDisposable Push(object? state) =>
            NullScope.Instance;
    }
}
