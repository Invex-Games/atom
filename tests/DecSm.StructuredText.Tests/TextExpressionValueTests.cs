namespace DecSm.StructuredText.Tests;

[TestFixture]
internal sealed class TextExpressionValueTests
{
    [Test]
    public void ImplicitOperator_Bool_CreatesBooleanExpression()
    {
        TextExpression expr = true;

        expr
            .ShouldBeOfType<BooleanExpression>()
            .Value
            .ShouldBeTrue();
    }

    [Test]
    public void ImplicitOperator_Int_CreatesNumberExpression()
    {
        TextExpression expr = 42;

        expr
            .ShouldBeOfType<NumberExpression>()
            .Value
            .ShouldBe(42.0);
    }

    [Test]
    public void ImplicitOperator_Long_CreatesNumberExpression()
    {
        TextExpression expr = 100L;

        expr
            .ShouldBeOfType<NumberExpression>()
            .Value
            .ShouldBe(100.0);
    }

    [Test]
    public void ImplicitOperator_Double_CreatesNumberExpression()
    {
        TextExpression expr = 3.14;

        expr
            .ShouldBeOfType<NumberExpression>()
            .Value
            .ShouldBe(3.14);
    }

    [Test]
    public void ImplicitOperator_Float_CreatesNumberExpression()
    {
        TextExpression expr = 1.5f;

        expr
            .ShouldBeOfType<NumberExpression>()
            .Value
            .ShouldBe(1.5, 0.001);
    }

    [Test]
    public void ImplicitOperator_Short_CreatesNumberExpression()
    {
        TextExpression expr = (short)7;

        expr
            .ShouldBeOfType<NumberExpression>()
            .Value
            .ShouldBe(7.0);
    }

    [Test]
    public void ImplicitOperator_Byte_CreatesNumberExpression()
    {
        TextExpression expr = (byte)255;

        expr
            .ShouldBeOfType<NumberExpression>()
            .Value
            .ShouldBe(255.0);
    }

    [Test]
    public void ImplicitOperator_String_CreatesRawExpression()
    {
        TextExpression expr = "hello";

        expr
            .ShouldBeOfType<RawExpression>()
            .Value
            .ShouldBe("hello");
    }

    [Test]
    public void TextExpressions_True_ReturnsBooleanExpressionTrue() =>
        TextExpressions.True.ShouldBe(new(true));

    [Test]
    public void TextExpressions_False_ReturnsBooleanExpressionFalse() =>
        TextExpressions.False.ShouldBe(new(false));

    [Test]
    public void TextExpressions_Null_ReturnsNullExpression() =>
        TextExpressions.Null.ShouldBeOfType<NullExpression>();

    [Test]
    public void TextExpressions_Raw_ReturnsRawExpression()
    {
        var expr = TextExpressions.Raw("raw");

        expr.ShouldBe(new("raw"));
    }

    [Test]
    public void TextExpressions_From_String_ReturnsStringExpression()
    {
        var expr = TextExpressions.From("str");

        expr.ShouldBe(new("str"));
    }

    [Test]
    public void TextExpressions_From_Bool_ReturnsBooleanExpression()
    {
        var expr = TextExpressions.From(false);

        expr.ShouldBe(new(false));
    }

    [Test]
    public void TextExpressions_From_Number_ReturnsNumberExpression()
    {
        var expr = TextExpressions.From(99);

        expr
            .ShouldBeOfType<NumberExpression>()
            .Value
            .ShouldBe(99.0);
    }

    [Test]
    public void Evaluate_ReturnsEvaluateExpression()
    {
        var inner = new RawExpression("x");
        var expr = inner.Evaluate();

        expr
            .ShouldBeOfType<EvaluateExpression>()
            .Expression
            .ShouldBe(inner);
    }

    [Test]
    public void Cast_ReturnsCastExpressionWithInner()
    {
        var inner = new RawExpression("x");
        var expr = inner.Cast<int>();

        expr
            .ShouldBeOfType<CastExpression<int>>()
            .Inner
            .ShouldBe(inner);
    }

    [Test]
    public void Indexer_Int_ReturnsIndexAccessExpression()
    {
        var array = new RawExpression("arr");
        var expr = array[2];

        expr.ShouldSatisfyAllConditions(() => expr.Array.ShouldBe(array),
            () => expr.Index.ShouldBe(new NumberExpression(2)));
    }

    [Test]
    public void Indexer_String_ReturnsPropertyAccessExpression()
    {
        var obj = new RawExpression("obj");
        var expr = obj["name"];

        expr.ShouldSatisfyAllConditions(() => expr.Object.ShouldBe(obj),
            () => expr.Property.ShouldBe(new RawExpression("name")));
    }

    [Test]
    public void WorkflowExpression_ImplicitFrom_TextExpression_SetsValue()
    {
        var raw = new RawExpression("val");
        WorkflowExpression<string> typed = raw;

        ((TextExpression)typed).ShouldBe(raw);
    }

    [Test]
    public void WorkflowExpression_ImplicitTo_TextExpression_ReturnsInner()
    {
        var raw = new RawExpression("val");
        WorkflowExpression<string> typed = raw;

        TextExpression back = typed;

        back.ShouldBe(raw);
    }

    [Test]
    public void WorkflowExpression_ImplicitNullable_FromNull_ReturnsNull()
    {
        TextExpression? nullExpr = null;
        WorkflowExpression<string>? typed = nullExpr;

        typed.ShouldBeNull();
    }

    [Test]
    public void WorkflowExpression_ImplicitNullable_ToNull_ReturnsNull()
    {
        WorkflowExpression<string>? typed = null;
        TextExpression? back = typed;

        back.ShouldBeNull();
    }
}
