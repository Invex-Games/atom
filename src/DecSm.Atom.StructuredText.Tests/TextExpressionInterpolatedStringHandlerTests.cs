using System.Diagnostics.CodeAnalysis;

namespace DecSm.Atom.StructuredText.Tests;

[TestFixture]
internal sealed class TextExpressionInterpolatedStringHandlerTests
{
    [Test]
    public void Format_LiteralOnly_ProducesStringExpressionSourceWithNoArguments()
    {
        var expr = TextExpressions.Format($"hello world");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("hello world")),
            () => expr.Arguments.ShouldBeEmpty());
    }

    [Test]
    public void Format_LiteralWithCurlyBraces_EscapesThem()
    {
        // Braces in the literal part must be doubled so the resulting format string
        // does not clash with the {0} argument placeholders.
        var expr = TextExpressions.Format($"{{literal braces}}");

        expr.Source.ShouldBe(new StringExpression("{{literal braces}}"));
    }

    [Test]
    public void Format_WithTextExpressionHole_ProducesPlaceholderAndArgument()
    {
        var arg = new RawExpression("val");
        var expr = TextExpressions.Format($"prefix-{arg}-suffix");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("prefix-{0}-suffix")),
            () => expr.Arguments.Length.ShouldBe(1),
            () => expr
                .Arguments[0]
                .ShouldBe(arg));
    }

    [Test]
    public void Format_WithMultipleTextExpressionHoles_IndexesSequentially()
    {
        var a = new RawExpression("a");
        var b = new RawExpression("b");
        var expr = TextExpressions.Format($"{a}+{b}");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("{0}+{1}")),
            () => expr
                .Arguments[0]
                .ShouldBe(a),
            () => expr
                .Arguments[1]
                .ShouldBe(b));
    }

    [Test]
    [SuppressMessage("Roslynator", "RCS1105:Unnecessary interpolation")]
    [SuppressMessage("ReSharper", "StringLiteralAsInterpolationArgument")]
    public void Format_WithStringHole_WrapsInStringExpression()
    {
        var expr = TextExpressions.Format($"Hello {"world"}!");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("Hello {0}!")),
            () => expr
                .Arguments[0]
                .ShouldBe(new StringExpression("world")));
    }

    [Test]
    public void Format_WithIntHole_WrapsInNumberExpression()
    {
        var expr = TextExpressions.Format($"value={42}");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("value={0}")),
            () => expr
                .Arguments[0]
                .ShouldBe(new NumberExpression(42)));
    }

    [Test]
    public void Format_WithBoolHole_WrapsInBooleanExpression()
    {
        var expr = TextExpressions.Format($"flag={true}");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("flag={0}")),
            () => expr
                .Arguments[0]
                .ShouldBe(new BooleanExpression(true)));
    }

    [Test]
    public void Format_MixedHoles_ProducesCorrectArgumentsInOrder()
    {
        var textArg = new RawExpression("x");
        var expr = TextExpressions.Format($"a={textArg} b={"str"} c={99} d={false}");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("a={0} b={1} c={2} d={3}")),
            () => expr
                .Arguments[0]
                .ShouldBe(textArg),
            () => expr
                .Arguments[1]
                .ShouldBe(new StringExpression("str")),
            () => expr
                .Arguments[2]
                .ShouldBe(new NumberExpression(99)),
            () => expr
                .Arguments[3]
                .ShouldBe(new BooleanExpression(false)));
    }
}
