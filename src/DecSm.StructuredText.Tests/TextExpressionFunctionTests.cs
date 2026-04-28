namespace DecSm.StructuredText.Tests;

[TestFixture]
internal sealed class TextExpressionFunctionTests
{
    private static readonly RawExpression _src = new("source");
    private static readonly RawExpression _pat = new("pattern");

    [Test]
    public void Contains_TextExpression_SetsSourceAndPattern()
    {
        var expr = _src.Contains(_pat);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(_src), () => expr.Pattern.ShouldBe(_pat));
    }

    [Test]
    public void Contains_String_WrapsPatternInStringExpression()
    {
        var expr = _src.Contains("pat");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(_src),
            () => expr.Pattern.ShouldBe(new StringExpression("pat")));
    }

    [Test]
    public void ContainedIn_TextExpression_CollectionIsSourceThisIsPattern()
    {
        // "this is contained in collection" → ContainsExpression { Source=collection, Pattern=this }
        var collection = new RawExpression("collection");
        var expr = _src.ContainedIn(collection);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(collection), () => expr.Pattern.ShouldBe(_src));
    }

    [Test]
    public void ContainedIn_String_CollectionIsSourceAndThisIsPattern()
    {
        // "this is contained in collection" → ContainsExpression { Source = collection, Pattern = this }
        // (consistent with ContainedIn(TextExpression) overload)
        var expr = _src.ContainedIn("collection");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("collection")),
            () => expr.Pattern.ShouldBe(_src));
    }

    [Test]
    public void StartsWith_TextExpression_SetsSourceAndPattern()
    {
        var expr = _src.StartsWith(_pat);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(_src), () => expr.Pattern.ShouldBe(_pat));
    }

    [Test]
    public void StartsWith_String_WrapsPatternInStringExpression()
    {
        var expr = _src.StartsWith("prefix");

        expr.Pattern.ShouldBe(new StringExpression("prefix"));
    }

    [Test]
    public void IsStartOf_TextExpression_SwapsSourceAndPattern()
    {
        // _src.IsStartOf(target) means "target starts with _src"
        var target = new RawExpression("target");
        var expr = _src.IsStartOf(target);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(target), () => expr.Pattern.ShouldBe(_src));
    }

    [Test]
    public void IsStartOf_String_WrapsPatternInStringExpression()
    {
        var expr = _src.IsStartOf("target");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("target")),
            () => expr.Pattern.ShouldBe(_src));
    }

    [Test]
    public void EndsWith_TextExpression_SetsSourceAndPattern()
    {
        var expr = _src.EndsWith(_pat);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(_src), () => expr.Pattern.ShouldBe(_pat));
    }

    [Test]
    public void EndsWith_String_WrapsPatternInStringExpression()
    {
        var expr = _src.EndsWith("suffix");

        expr.Pattern.ShouldBe(new StringExpression("suffix"));
    }

    [Test]
    public void IsEndOf_TextExpression_SwapsSourceAndPattern()
    {
        var target = new RawExpression("target");
        var expr = _src.IsEndOf(target);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(target), () => expr.Pattern.ShouldBe(_src));
    }

    [Test]
    public void IsEndOfString_WrapsPatternInStringExpression()
    {
        var expr = _src.IsEndOfString("target");

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(new StringExpression("target")),
            () => expr.Pattern.ShouldBe(_src));
    }

    [Test]
    public void Coalesce_TextExpressions_IncludesThisAndParams()
    {
        var expr = _src.Coalesce(_pat, new RawExpression("fallback"));

        expr.Source.ShouldBe([_src, _pat, new RawExpression("fallback")]);
    }

    [Test]
    public void Coalesce_Strings_WrapsEachInStringExpression()
    {
        var expr = _src.Coalesce("a", "b");

        expr.Source.ShouldBe([_src, new StringExpression("a"), new StringExpression("b")]);
    }

    [Test]
    public void Format_TextExpressions_SetsSourceAndArguments()
    {
        var expr = _src.Format(_pat);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(_src), () => expr.Arguments.ShouldBe([_pat]));
    }

    [Test]
    public void FormatString_WrapsArgumentsInStringExpressions()
    {
        var expr = _src.FormatString("arg1", "arg2");

        expr.Arguments.ShouldBe([new StringExpression("arg1"), new StringExpression("arg2")]);
    }

    [Test]
    public void OperatorPlus_SingleTextExpression_CreatesFormatExpression()
    {
        var expr = _src + _pat;

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(_src), () => expr.Arguments.ShouldBe([_pat]));
    }

    [Test]
    public void OperatorPlus_TextExpressionArray_CreatesFormatExpression()
    {
        var expr = _src + [_pat, new RawExpression("c")];

        expr.Arguments.Length.ShouldBe(2);
    }

    [Test]
    public void OperatorPlus_String_WrapsInStringExpression()
    {
        var expr = _src + "arg";

        expr.Arguments.ShouldBe([new StringExpression("arg")]);
    }

    [Test]
    public void OperatorPlus_StringArray_WrapsEachInStringExpression()
    {
        var expr = _src + ["x", "y"];

        expr.Arguments.ShouldBe([new StringExpression("x"), new StringExpression("y")]);
    }

    [Test]
    public void Join_TextExpression_SetsSourceAndSeparator()
    {
        var sep = new StringExpression(",");
        var expr = _src.Join(sep);

        expr.ShouldSatisfyAllConditions(() => expr.Source.ShouldBe(_src), () => expr.OptionalSeparator.ShouldBe(sep));
    }

    [Test]
    public void JoinString_WrapsSeparatorInStringExpression()
    {
        var expr = _src.JoinString(", ");

        expr.OptionalSeparator.ShouldBe(new StringExpression(", "));
    }

    [Test]
    public void ToJson_WrapsSource()
    {
        var expr = _src.ToJson();

        expr.Source.ShouldBe(_src);
    }

    [Test]
    public void HashFiles_WrapsSource()
    {
        var expr = _src.HashFiles();

        expr.Source.ShouldBe(_src);
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
