namespace DecSm.StructuredText.Tests;

[TestFixture]
internal sealed class TextExpressionLogicOperatorTests
{
    private static readonly RawExpression A = new("a");
    private static readonly RawExpression B = new("b");
    private static readonly RawExpression C = new("c");

    [Test]
    public void Not_Method_WrapsSource()
    {
        var expr = A.Not();

        expr
            .ShouldBeOfType<NotExpression>()
            .Source
            .ShouldBe(A);
    }

    [Test]
    public void Not_Operator_WrapsSource()
    {
        var expr = !A;

        expr
            .ShouldBeOfType<NotExpression>()
            .Source
            .ShouldBe(A);
    }

    [Test]
    public void And_Method_CombinesSelfWithExpressions()
    {
        var expr = A.And(B, C);

        var and = expr.ShouldBeOfType<AndExpression>();
        and.Source.ShouldBe(new TextExpression[] { A, B, C });
    }

    [Test]
    public void And_Operator_CombinesTwoExpressions()
    {
        var expr = A & B;

        var and = expr.ShouldBeOfType<AndExpression>();
        and.Source.ShouldBe(new TextExpression[] { A, B });
    }

    [Test]
    public void Or_Method_CombinesSelfWithExpressions()
    {
        var expr = A.Or(B, C);

        var or = expr.ShouldBeOfType<OrExpression>();
        or.Source.ShouldBe(new TextExpression[] { A, B, C });
    }

    [Test]
    public void Or_Operator_CombinesTwoExpressions()
    {
        var expr = A | B;

        var or = expr.ShouldBeOfType<OrExpression>();
        or.Source.ShouldBe(new TextExpression[] { A, B });
    }

    [Test]
    public void EqualTo_TextExpression_CreatesEqualExpression()
    {
        var expr = A.EqualTo(B);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(A), () => expr.Right.ShouldBe(B));
    }

    [Test]
    public void EqualTo_StringRaw_CreatesEqualExpressionWithRawExpression()
    {
        var expr = A.EqualTo("raw");

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(A),
            () => expr.Right.ShouldBe(new RawExpression("raw")));
    }

    [Test]
    public void EqualToString_CreatesEqualExpressionWithStringExpression()
    {
        var expr = A.EqualToString("str");

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(A),
            () => expr.Right.ShouldBe(new StringExpression("str")));
    }

    [Test]
    public void NotEqualTo_TextExpression_CreatesNotEqualExpression()
    {
        var expr = A.NotEqualTo(B);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(A), () => expr.Right.ShouldBe(B));
    }

    [Test]
    public void NotEqualTo_StringRaw_WrapsInRawExpression()
    {
        var expr = A.NotEqualTo("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void NotEqualToString_WrapsInStringExpression()
    {
        var expr = A.NotEqualToString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }

    [Test]
    public void LessThan_TextExpression_CreatesLessThanExpression()
    {
        var expr = A.LessThan(B);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(A), () => expr.Right.ShouldBe(B));
    }

    [Test]
    public void LessThan_String_WrapsInRawExpression()
    {
        var expr = A.LessThan("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void LessThanString_WrapsInStringExpression()
    {
        var expr = A.LessThanString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }

    [Test]
    public void GreaterThan_TextExpression_CreatesGreaterThanExpression()
    {
        var expr = A.GreaterThan(B);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(A), () => expr.Right.ShouldBe(B));
    }

    [Test]
    public void GreaterThan_String_WrapsInRawExpression()
    {
        var expr = A.GreaterThan("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void GreaterThanString_WrapsInStringExpression()
    {
        var expr = A.GreaterThanString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }

    [Test]
    public void LessThanOrEqualTo_TextExpression_CreatesLessThanOrEqualToExpression()
    {
        var expr = A.LessThanOrEqualTo(B);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(A), () => expr.Right.ShouldBe(B));
    }

    [Test]
    public void LessThanOrEqualTo_String_WrapsInRawExpression()
    {
        var expr = A.LessThanOrEqualTo("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void LessThanOrEqualToString_WrapsInStringExpression()
    {
        var expr = A.LessThanOrEqualToString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }

    [Test]
    public void GreaterThanOrEqualTo_TextExpression_CreatesGreaterThanOrEqualToExpression()
    {
        var expr = A.GreaterThanOrEqualTo(B);

        expr.ShouldSatisfyAllConditions(() => expr.Left.ShouldBe(A), () => expr.Right.ShouldBe(B));
    }

    [Test]
    public void GreaterThanOrEqualTo_String_WrapsInRawExpression()
    {
        var expr = A.GreaterThanOrEqualTo("raw");

        expr.Right.ShouldBe(new RawExpression("raw"));
    }

    [Test]
    public void GreaterThanOrEqualToString_WrapsInStringExpression()
    {
        var expr = A.GreaterThanOrEqualToString("str");

        expr.Right.ShouldBe(new StringExpression("str"));
    }
}
