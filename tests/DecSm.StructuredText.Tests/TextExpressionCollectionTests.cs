namespace DecSm.StructuredText.Tests;

[TestFixture]
internal sealed class TextExpressionCollectionTests
{
    [Test]
    public void DefaultConstructor_IsEmpty()
    {
        var collection = new TextExpressionCollection();

        collection.Count.ShouldBe(0);
    }

    [Test]
    public void Constructor_FromIEnumerable_ContainsAllItems()
    {
        var items = new TextExpression[] { new RawExpression("a"), new RawExpression("b") };
        var collection = new TextExpressionCollection(items);

        collection.Count.ShouldBe(2);
    }

    [Test]
    public void Add_IncreasesCount()
    {
        var collection = new TextExpressionCollection
        {
            new RawExpression("x"),
        };

        collection.Count.ShouldBe(1);
    }

    [Test]
    public void Indexer_ReturnsItemAtIndex()
    {
        var a = new RawExpression("a");
        var b = new RawExpression("b");
        var collection = new TextExpressionCollection([a, b]);

        collection[0]
            .ShouldBe(a);

        collection[1]
            .ShouldBe(b);
    }

    [Test]
    public void GetEnumerator_YieldsAllItems()
    {
        var items = new TextExpression[] { new RawExpression("a"), new RawExpression("b") };
        var collection = new TextExpressionCollection(items);

        collection
            .ToList()
            .ShouldBe(items);
    }

    [Test]
    public void ImplicitFromTextExpression_WrapsInCollection()
    {
        var expr = new RawExpression("x");
        TextExpressionCollection collection = expr;

        collection.Count.ShouldBe(1);

        collection[0]
            .ShouldBe(expr);
    }

    [Test]
    public void ImplicitFromTextExpressionArray_ContainsAllItems()
    {
        var items = new TextExpression[] { new RawExpression("a"), new RawExpression("b") };
        TextExpressionCollection collection = items;

        collection
            .ToList()
            .ShouldBe(items);
    }

    [Test]
    public void ImplicitFromTextExpressionList_ContainsAllItems()
    {
        var items = new List<TextExpression>
        {
            new RawExpression("a"),
            new RawExpression("b"),
        };

        TextExpressionCollection collection = items;

        collection
            .ToList()
            .ShouldBe(items);
    }

    [Test]
    public void ImplicitFromString_CreatesRawExpression()
    {
        TextExpressionCollection collection = "hello";

        collection.Count.ShouldBe(1);

        collection[0]
            .ShouldBe(new RawExpression("hello"));
    }

    [Test]
    public void ImplicitFromStringArray_CreatesRawExpressionsForEach()
    {
        TextExpressionCollection collection = new[] { "a", "b" };

        collection.Count.ShouldBe(2);

        collection[0]
            .ShouldBe(new RawExpression("a"));

        collection[1]
            .ShouldBe(new RawExpression("b"));
    }

    [Test]
    public void ImplicitFromStringList_CreatesRawExpressionsForEach()
    {
        TextExpressionCollection collection = new List<string>
        {
            "x",
            "y",
        };

        collection.Count.ShouldBe(2);

        collection[0]
            .ShouldBe(new RawExpression("x"));

        collection[1]
            .ShouldBe(new RawExpression("y"));
    }

    [Test]
    public void WorkflowExpressionCollection_DefaultConstructor_IsEmpty()
    {
        var collection = new WorkflowExpressionCollection<string>();

        collection.Count.ShouldBe(0);
    }

    [Test]
    public void WorkflowExpressionCollection_Add_IncreasesCount()
    {
        var collection = new WorkflowExpressionCollection<string>
        {
            new RawExpression("x"),
        };

        collection.Count.ShouldBe(1);
    }

    [Test]
    public void WorkflowExpressionCollection_ImplicitFromTextExpressionCollection_WrapsInner()
    {
        var inner = new TextExpressionCollection([new RawExpression("a")]);
        WorkflowExpressionCollection<string> typed = inner;

        typed.Count.ShouldBe(1);
    }

    [Test]
    public void WorkflowExpressionCollection_ImplicitToTextExpressionCollection_ReturnsInner()
    {
        var typed = new WorkflowExpressionCollection<string>([new RawExpression("a")]);
        TextExpressionCollection inner = typed;

        inner.ShouldNotBeNull();
        inner.Count.ShouldBe(1);
    }

    [Test]
    public void WorkflowExpressionCollection_ImplicitFromNull_ReturnsNull()
    {
        TextExpressionCollection? nullCollection = null;
        WorkflowExpressionCollection<string>? typed = nullCollection;

        typed.ShouldBeNull();
    }

    [Test]
    public void WorkflowExpressionCollection_ImplicitToNull_ReturnsNull()
    {
        WorkflowExpressionCollection<string>? typed = null;
        TextExpressionCollection? inner = typed;

        inner.ShouldBeNull();
    }

    [Test]
    public void WorkflowExpressionCollection_ImplicitFromString_CreatesRawExpression()
    {
        WorkflowExpressionCollection<string> collection = "hello";

        collection.Count.ShouldBe(1);

        collection[0]
            .ShouldBe(new RawExpression("hello"));
    }

    [Test]
    public void WorkflowExpressionCollection_ImplicitFromStringArray_CreatesRawExpressions()
    {
        WorkflowExpressionCollection<string> collection = new[] { "a", "b" };

        collection.Count.ShouldBe(2);

        collection[0]
            .ShouldBe(new RawExpression("a"));
    }
}
