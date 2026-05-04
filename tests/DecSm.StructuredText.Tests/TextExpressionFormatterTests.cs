namespace DecSm.StructuredText.Tests;

[TestFixture]
internal sealed class TextExpressionFormatterTests
{
    // A minimal concrete formatter: handles StringExpression, NumberExpression, and BooleanExpression
    // by converting them to RawExpressions. Everything else (NullExpression etc.) returns null → throws.
    private sealed class TestFormatter : TextExpressionFormatter
    {
        protected override TextExpression? Resolve(TextExpression expression) =>
            expression switch
            {
                StringExpression s => new RawExpression(s.Value),
                NumberExpression n => new RawExpression(n.Value.ToString("G")),
                BooleanExpression b => new RawExpression(b.Value
                    ? "true"
                    : "false"),
                _ => null,
            };
    }

    private readonly TestFormatter _formatter = new();

    [Test]
    public void Format_Null_ReturnsNull() =>
        _formatter
            .Format(null)
            .ShouldBeNull();

    [Test]
    public void Format_RawExpression_ReturnsValue() =>
        _formatter
            .Format(new RawExpression("hello"))
            .ShouldBe("hello");

    [Test]
    public void Format_ConcatExpression_ConcatenatesAllParts()
    {
        var expr = new ConcatExpression([new RawExpression("a"), new RawExpression("b"), new RawExpression("c")]);

        _formatter
            .Format(expr)
            .ShouldBe("abc");
    }

    [Test]
    public void Format_ConcatExpression_EachPartIsFormattedRecursively()
    {
        // StringExpression goes through Resolve → RawExpression
        var expr = new ConcatExpression([new StringExpression("x"), new RawExpression("y")]);

        _formatter
            .Format(expr)
            .ShouldBe("xy");
    }

    [Test]
    public void Format_CastExpression_FormatsInnerExpression()
    {
        var inner = new StringExpression("cast-value");
        var cast = inner.Cast<int>();

        _formatter
            .Format(cast)
            .ShouldBe("cast-value");
    }

    [Test]
    public void Format_DoublyNestedCastExpression_FormatsCorrectly()
    {
        var inner = new RawExpression("deep");

        var cast = inner
            .Cast<string>()
            .Cast<object>();

        _formatter
            .Format(cast)
            .ShouldBe("deep");
    }

    [Test]
    public void Format_NumberExpression_ResolvedThroughCustomFormatter() =>
        _formatter
            .Format(new NumberExpression(42))
            .ShouldBe("42");

    [Test]
    public void Format_BooleanExpression_ResolvedThroughCustomFormatter()
    {
        _formatter
            .Format(new BooleanExpression(true))
            .ShouldBe("true");

        _formatter
            .Format(new BooleanExpression(false))
            .ShouldBe("false");
    }

    [Test]
    public void Format_UnknownExpression_ThrowsInvalidOperationException() =>
        // NullExpression is not handled by TestFormatter.Resolve
        Should.Throw<InvalidOperationException>(() => _formatter.Format(new NullExpression()));

    [Test]
    public void Format_WorkflowExpression_WithValue_DelegatesToFormat()
    {
        WorkflowExpression<string>? typed = new RawExpression("wf-val");

        _formatter
            .Format(typed)
            .ShouldBe("wf-val");
    }

    [Test]
    public void Format_WorkflowExpression_Null_ReturnsNull()
    {
        WorkflowExpression<string>? typed = null;

        _formatter
            .Format(typed)
            .ShouldBeNull();
    }
}
