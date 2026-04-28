namespace DecSm.StructuredText.Expressions;

[PublicAPI]
public sealed class TextExpressionCollection : IEnumerable<TextExpression>
{
    private readonly List<TextExpression> _items = [];

    public TextExpressionCollection() { }

    public TextExpressionCollection(IEnumerable<TextExpression> list) : this()
    {
        _items.AddRange(list);
    }

    public int Count => _items.Count;

    public int Capacity => _items.Capacity;

    public void Add(TextExpression item) =>
        _items.Add(item);

    public IEnumerator<TextExpression> GetEnumerator() =>
        _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public TextExpression this[int index] => _items[index];

    public static implicit operator TextExpressionCollection(TextExpression expression) =>
        new([expression]);

    public static implicit operator TextExpressionCollection(TextExpression[] array) =>
        new(array);

    public static implicit operator TextExpressionCollection(List<TextExpression> array) =>
        new(array);

    public static implicit operator TextExpressionCollection(string value) =>
        new([TextExpressions.Raw(value)]);

    public static implicit operator TextExpressionCollection(string[] array) =>
        new(array.Select(TextExpressions.Raw));

    public static implicit operator TextExpressionCollection(List<string> list) =>
        new(list.Select(TextExpressions.Raw));
}

/// <summary>
///     A typed workflow expression collection that documents the expected element result type.
///     The type parameter serves as a compile-time hint for what each expression resolves to.
/// </summary>
[PublicAPI]
public sealed class WorkflowExpressionCollection<T> : IEnumerable<TextExpression>
{
    private readonly TextExpressionCollection _inner;

    public WorkflowExpressionCollection()
    {
        _inner = [];
    }

    public WorkflowExpressionCollection(TextExpressionCollection inner)
    {
        _inner = inner;
    }

    public WorkflowExpressionCollection(IEnumerable<TextExpression> list)
    {
        _inner = new(list);
    }

    public int Count => _inner.Count;

    public int Capacity => _inner.Capacity;

    public void Add(TextExpression item) =>
        _inner.Add(item);

    public IEnumerator<TextExpression> GetEnumerator() =>
        _inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public TextExpression this[int index] => _inner[index];

    [return: NotNullIfNotNull(nameof(collection))]
    public static implicit operator WorkflowExpressionCollection<T>?(TextExpressionCollection? collection) =>
        collection is not null
            ? new(collection)
            : null;

    [return: NotNullIfNotNull(nameof(typed))]
    public static implicit operator TextExpressionCollection?(WorkflowExpressionCollection<T>? typed) =>
        typed?._inner;

    public static implicit operator WorkflowExpressionCollection<T>(TextExpression expression) =>
        new(new([expression]));

    public static implicit operator WorkflowExpressionCollection<T>(TextExpression[] array) =>
        new(new(array));

    public static implicit operator WorkflowExpressionCollection<T>(List<TextExpression> list) =>
        new(new(list));

    public static implicit operator WorkflowExpressionCollection<T>(string value) =>
        new(new([TextExpressions.Raw(value)]));

    public static implicit operator WorkflowExpressionCollection<T>(string[] array) =>
        new(new(array.Select(TextExpressions.Raw)));

    public static implicit operator WorkflowExpressionCollection<T>(List<string> list) =>
        new(new(list.Select(TextExpressions.Raw)));
}
