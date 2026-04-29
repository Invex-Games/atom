namespace DecSm.StructuredText.Tests;

[TestFixture]
internal sealed class TextExpressionFunctionTests
{
    private static readonly RawExpression Src = new("source");
    private static readonly RawExpression Pat = new("pattern");

    [Test]
    public void Contains_TextExpression_SetsSourceAndPattern()
    {
        var expr = Src.Contains(Pat);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(Src), () => expr.Pattern.ShouldBe(Pat));
    }

    [Test]
    public void Contains_String_WrapsPatternInStringExpression()
    {
        var expr = Src.Contains("pat");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(Src),
            () => expr.Pattern.ShouldBe(new StringExpression("pat")));
    }

    [Test]
    public void ContainedIn_TextExpression_CollectionIsSourceThisIsPattern()
    {
        // "this is contained in collection" → ContainsExpression { Source=collection, Pattern=this }
        var collection = new RawExpression("collection");
        var expr = Src.ContainedIn(collection);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(collection), () => expr.Pattern.ShouldBe(Src));
    }

    [Test]
    public void ContainedIn_String_CollectionIsSourceAndThisIsPattern()
    {
        // "this is contained in collection" → ContainsExpression { Source = collection, Pattern = this }
        // (consistent with ContainedIn(TextExpression) overload)
        var expr = Src.ContainedIn("collection");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("collection")),
            () => expr.Pattern.ShouldBe(Src));
    }

    [Test]
    public void StartsWith_TextExpression_SetsSourceAndPattern()
    {
        var expr = Src.StartsWith(Pat);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(Src), () => expr.Pattern.ShouldBe(Pat));
    }

    [Test]
    public void StartsWith_String_WrapsPatternInStringExpression()
    {
        var expr = Src.StartsWith("prefix");

        expr.Pattern.ShouldBe(new StringExpression("prefix"));
    }

    [Test]
    public void IsStartOf_TextExpression_SwapsSourceAndPattern()
    {
        // _src.IsStartOf(target) means "target starts with _src"
        var target = new RawExpression("target");
        var expr = Src.IsStartOf(target);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(target), () => expr.Pattern.ShouldBe(Src));
    }

    [Test]
    public void IsStartOf_String_WrapsPatternInStringExpression()
    {
        var expr = Src.IsStartOf("target");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("target")),
            () => expr.Pattern.ShouldBe(Src));
    }

    [Test]
    public void EndsWith_TextExpression_SetsSourceAndPattern()
    {
        var expr = Src.EndsWith(Pat);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(Src), () => expr.Pattern.ShouldBe(Pat));
    }

    [Test]
    public void EndsWith_String_WrapsPatternInStringExpression()
    {
        var expr = Src.EndsWith("suffix");

        expr.Pattern.ShouldBe(new StringExpression("suffix"));
    }

    [Test]
    public void IsEndOf_TextExpression_SwapsSourceAndPattern()
    {
        var target = new RawExpression("target");
        var expr = Src.IsEndOf(target);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(target), () => expr.Pattern.ShouldBe(Src));
    }

    [Test]
    public void IsEndOfString_WrapsPatternInStringExpression()
    {
        var expr = Src.IsEndOfString("target");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("target")),
            () => expr.Pattern.ShouldBe(Src));
    }

    [Test]
    public void Coalesce_TextExpressions_IncludesThisAndParams()
    {
        var expr = Src.Coalesce(Pat, new RawExpression("fallback"));

        expr.Source.ShouldBe([Src, Pat, new RawExpression("fallback")]);
    }

    [Test]
    public void Coalesce_Strings_WrapsEachInStringExpression()
    {
        var expr = Src.Coalesce("a", "b");

        expr.Source.ShouldBe([Src, new StringExpression("a"), new StringExpression("b")]);
    }

    [Test]
    public void Format_TextExpressions_SetsSourceAndArguments()
    {
        var expr = Src.Format(Pat);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(Src), () => expr.Arguments.ShouldBe([Pat]));
    }

    [Test]
    public void FormatString_WrapsArgumentsInStringExpressions()
    {
        var expr = Src.FormatString("arg1", "arg2");

        expr.Arguments.ShouldBe([new StringExpression("arg1"), new StringExpression("arg2")]);
    }

    [Test]
    public void OperatorPlus_SingleTextExpression_CreatesFormatExpression()
    {
        var expr = Src + Pat;

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(Src), () => expr.Arguments.ShouldBe([Pat]));
    }

    [Test]
    public void OperatorPlus_TextExpressionArray_CreatesFormatExpression()
    {
        var expr = Src + [Pat, new RawExpression("c")];

        expr.Arguments.Length.ShouldBe(2);
    }

    [Test]
    public void OperatorPlus_String_WrapsInStringExpression()
    {
        var expr = Src + "arg";

        expr.Arguments.ShouldBe([new StringExpression("arg")]);
    }

    [Test]
    public void OperatorPlus_StringArray_WrapsEachInStringExpression()
    {
        var expr = Src + ["x", "y"];

        expr.Arguments.ShouldBe([new StringExpression("x"), new StringExpression("y")]);
    }

    [Test]
    public void Join_TextExpression_SetsSourceAndSeparator()
    {
        var sep = new StringExpression(",");
        var expr = Src.Join(sep);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(Src), () => expr.OptionalSeparator.ShouldBe(sep));
    }

    [Test]
    public void JoinString_WrapsSeparatorInStringExpression()
    {
        var expr = Src.JoinString(", ");

        expr.OptionalSeparator.ShouldBe(new StringExpression(", "));
    }

    [Test]
    public void ToJson_WrapsSource()
    {
        var expr = Src.ToJson();

        expr.Source.ShouldBe(Src);
    }

    [Test]
    public void HashFiles_WrapsSource()
    {
        var expr = Src.HashFiles();

        expr.Source.ShouldBe(Src);
    }

    [Test]
    public void Concat_CreatesConcatExpressionWithAllItems()
    {
        var items = new TextExpression[] { new RawExpression("a"), new RawExpression("b") };
        var expr = TextExpressions.Concat(items);

        expr.Values.ShouldBe(items);
    }

    [Test]
    public void ConcatWithSeparator_InterleavesSeparatorBetweenItems()
    {
        var sep = new StringExpression(",");
        var items = new TextExpression[] { new RawExpression("a"), new RawExpression("b"), new RawExpression("c") };

        var expr = TextExpressions.ConcatWithSeparator(sep, items);

        // Expected: [a, ",", b, ",", c]
        var values = expr.Values.ToList();
        values.Count.ShouldBe(5);

        values[0]
            .ShouldBe(new RawExpression("a"));

        values[1]
            .ShouldBe(sep);

        values[2]
            .ShouldBe(new RawExpression("b"));

        values[3]
            .ShouldBe(sep);

        values[4]
            .ShouldBe(new RawExpression("c"));
    }

    [Test]
    public void TextExpressionUtils_Join_InterleavesSeparator()
    {
        var expressions = new TextExpression[]
        {
            new RawExpression("x"), new RawExpression("y"), new RawExpression("z"),
        };

        var separator = new StringExpression("|");

        var result = expressions
            .Join(separator)
            .ToList();

        // Expected: [x, |, y, |, z]
        result.Count.ShouldBe(5);

        result[0]
            .ShouldBe(new RawExpression("x"));

        result[1]
            .ShouldBe(separator);

        result[2]
            .ShouldBe(new RawExpression("y"));

        result[3]
            .ShouldBe(separator);

        result[4]
            .ShouldBe(new RawExpression("z"));
    }
}
