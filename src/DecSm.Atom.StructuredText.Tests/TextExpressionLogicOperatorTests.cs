namespace DecSm.Atom.StructuredText.Tests;

[TestFixture]
internal sealed class TextExpressionLogicOperatorTests
{
    private static readonly RawExpression _a = new("a");
    private static readonly RawExpression _b = new("b");
    private static readonly RawExpression _c = new("c");

    [Test]
    public void Not_Method_WrapsSource()
    {
        var expr = _a.Not();

        expr
            .ShouldBeOfType<NotExpression>()
            .Source
            .ShouldBe(_a);
    }

    [Test]
    public void Not_Operator_WrapsSource()
    {
        var expr = !_a;

        expr
            .ShouldBeOfType<NotExpression>()
            .Source
            .ShouldBe(_a);
    }

    [Test]
    public void And_Method_CombinesSelfWithExpressions()
    {
        var expr = _a.And(_b, _c);

        var and = expr.ShouldBeOfType<AndExpression>();
        and.Source.ShouldBe(new TextExpression[] { _a, _b, _c });
    }

    [Test]
    public void And_Operator_CombinesTwoExpressions()
    {
        var expr = _a & _b;

        var and = expr.ShouldBeOfType<AndExpression>();
        and.Source.ShouldBe(new TextExpression[] { _a, _b });
    }

    [Test]
    public void Or_Method_CombinesSelfWithExpressions()
    {
        var expr = _a.Or(_b, _c);

        var or = expr.ShouldBeOfType<OrExpression>();
        or.Source.ShouldBe(new TextExpression[] { _a, _b, _c });
    }

    [Test]
    public void Or_Operator_CombinesTwoExpressions()
    {
        var expr = _a | _b;

        var or = expr.ShouldBeOfType<OrExpression>();
        or.Source.ShouldBe(new TextExpression[] { _a, _b });
    }

    [Test]
    public void EqualTo_TextExpression_CreatesEqualExpression()
    {
        var expr = _a.EqualTo(_b);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(_a), () => expr.Right.ShouldBe(_b));
    }

    [Test]
    public void EqualTo_StringRaw_CreatesEqualExpressionWithRawExpression()
    {
        var expr = _a.EqualTo("raw");

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(_a),
            () => expr.Right.ShouldBe(new RawExpression("raw")));
    }

    [Test]
    public void EqualToString_CreatesEqualExpressionWithStringExpression()
    {
        var expr = _a.EqualToString("str");

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(_a),
            () => expr.Right.ShouldBe(new StringExpression("str")));
    }

    [Test]
    public void NotEqualTo_TextExpression_CreatesNotEqualExpression()
    {
        var expr = _a.NotEqualTo(_b);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(_a), () => expr.Right.ShouldBe(_b));
    }

    [Test]
    public void NotEqualTo_StringRaw_WrapsInRawExpression()
    {
        var expr = _a.NotEqualTo("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void NotEqualToString_WrapsInStringExpression()
    {
        var expr = _a.NotEqualToString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }

    [Test]
    public void LessThan_TextExpression_CreatesLessThanExpression()
    {
        var expr = _a.LessThan(_b);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(_a), () => expr.Right.ShouldBe(_b));
    }

    [Test]
    public void LessThan_String_WrapsInRawExpression()
    {
        var expr = _a.LessThan("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void LessThanString_WrapsInStringExpression()
    {
        var expr = _a.LessThanString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }

    [Test]
    public void GreaterThan_TextExpression_CreatesGreaterThanExpression()
    {
        var expr = _a.GreaterThan(_b);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(_a), () => expr.Right.ShouldBe(_b));
    }

    [Test]
    public void GreaterThan_String_WrapsInRawExpression()
    {
        var expr = _a.GreaterThan("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void GreaterThanString_WrapsInStringExpression()
    {
        var expr = _a.GreaterThanString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }

    [Test]
    public void LessThanOrEqualTo_TextExpression_CreatesLessThanOrEqualToExpression()
    {
        var expr = _a.LessThanOrEqualTo(_b);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(_a), () => expr.Right.ShouldBe(_b));
    }

    [Test]
    public void LessThanOrEqualTo_String_WrapsInRawExpression()
    {
        var expr = _a.LessThanOrEqualTo("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void LessThanOrEqualToString_WrapsInStringExpression()
    {
        var expr = _a.LessThanOrEqualToString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }

    [Test]
    public void GreaterThanOrEqualTo_TextExpression_CreatesGreaterThanOrEqualToExpression()
    {
        var expr = _a.GreaterThanOrEqualTo(_b);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(_a), () => expr.Right.ShouldBe(_b));
    }

    [Test]
    public void GreaterThanOrEqualTo_String_WrapsInRawExpression()
    {
        var expr = _a.GreaterThanOrEqualTo("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void GreaterThanOrEqualToString_WrapsInStringExpression()
    {
        var expr = _a.GreaterThanOrEqualToString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }
}
