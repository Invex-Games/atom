namespace Invex.Atom.Build.Tests.ClassTests.Logging;

[TestFixture]
internal sealed class MaskingAnsiConsoleOutputTests
{
    [SetUp]
    public void SetUp()
    {
        _writer = new(new StringBuilder());

        // Create a thin output that writes to our StringWriter
        var rawOutput = new TestAnsiConsoleOutput(_writer);

        // Create a stub param service that masks our known secret
        var paramService = new StubParamService(Secret, MaskedSecret);

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IParamService>(paramService)
            .BuildServiceProvider();

        // Wrap it with our masking output
        var maskingOutput = new MaskingAnsiConsoleOutput(rawOutput, serviceProvider);

        var settings = new AnsiConsoleSettings
        {
            Out = maskingOutput,
        };

        _console = AnsiConsole.Create(settings);
    }

    [TearDown]
    public void TearDown() =>
        _writer.Dispose();

    private const string Secret = "SuperSecretValue123";
    private const string MaskedSecret = "*****";

    private StringWriter _writer = null!;
    private IAnsiConsole _console = null!;

    [Test]
    public void Markup_Output_ShouldMaskSecrets()
    {
        // Arrange
        var markup = new Markup($"Hello [red]{Secret}[/] world");

        // Act
        _console.Write(markup);
        _console.Profile.Out.Writer.Flush();
        var output = _writer.ToString();

        // Assert
        output.ShouldNotContain(Secret);
        output.ShouldContain(MaskedSecret);
        TestContext.Out.WriteLine(output);
    }

    [Test]
    public void Table_Output_ShouldMaskSecrets()
    {
        // Arrange
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Col1");
        table.AddRow($"prefix {Secret} suffix");

        // Act
        _console.Write(table);
        _console.Profile.Out.Writer.Flush();
        var output = _writer.ToString();

        // Assert
        output.ShouldNotContain(Secret);
        output.ShouldContain(MaskedSecret);
        TestContext.Out.WriteLine(output);
    }

    [Test]
    public void Exception_Output_ShouldMaskSecrets()
    {
        // Arrange
        var ex = new InvalidOperationException($"Operation failed with token: {Secret}");

        // Act
        _console.WriteException(ex);
        _console.Profile.Out.Writer.Flush();
        var output = _writer.ToString();

        // Assert
        output.ShouldNotContain(Secret);
        output.ShouldContain(MaskedSecret);
        TestContext.Out.WriteLine(output);
    }

    [Test]
    public async Task WriteAsync_WithSecret_ShouldMaskSecrets()
    {
        // Arrange — write directly through the MaskingTextWriter via the console's output writer
        var writer = _console.Profile.Out.Writer;

        // Act
        await writer.WriteAsync($"Token is {Secret} here");
        await writer.FlushAsync();
        var output = _writer.ToString();

        // Assert
        output.ShouldNotContain(Secret);
        output.ShouldContain(MaskedSecret);
        await TestContext.Out.WriteLineAsync(output);
    }

    [Test]
    public async Task WriteAsync_WithNull_ShouldNotThrow()
    {
        // Arrange
        var writer = _console.Profile.Out.Writer;

        // Act
        await writer.WriteAsync((string?)null);
        await writer.FlushAsync();
        var output = _writer.ToString();

        // Assert
        output.ShouldBeEmpty();
    }

    [Test]
    public async Task WriteAsync_WithEmptyString_ShouldNotThrow()
    {
        // Arrange
        var writer = _console.Profile.Out.Writer;

        // Act
        await writer.WriteAsync(string.Empty);
        await writer.FlushAsync();
        var output = _writer.ToString();

        // Assert
        output.ShouldBeEmpty();
    }

    private sealed class StubParamService(string secret, string mask) : IParamService
    {
        public IDisposable CreateNoCacheScope() =>
            new DummyDisposable();

        public IDisposable CreateDefaultValuesOnlyScope() =>
            new DummyDisposable();

        public IDisposable CreateOverrideSourcesScope(ParamSource sources) =>
            new DummyDisposable();

        public string MaskMatchingSecrets(string text) =>
            text.Replace(secret, mask, StringComparison.OrdinalIgnoreCase);

        public T GetParam<T>(
            Expression<Func<T?>> paramExpression,
            T? defaultValue = default,
            Func<string?, T?>? converter = null) =>
            throw new NotImplementedException();

        public T GetParam<T>(string paramName, T? defaultValue = default, Func<string?, T?>? converter = null) =>
            throw new NotImplementedException();

        public string GetParam(string paramName, string? defaultValue = null) =>
            throw new NotImplementedException();
    }

    private sealed class DummyDisposable : IDisposable
    {
        public void Dispose()
        {
            // no-op
        }
    }

    private sealed class TestAnsiConsoleOutput(TextWriter writer) : IAnsiConsoleOutput
    {
        public int Width => 120;

        public int Height => 40;

        public TextWriter Writer { get; } = writer;

        public bool IsTerminal => false;

        public void SetEncoding(Encoding encoding)
        {
            // Delegate to underlying writer if needed
        }
    }
}
