namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
public sealed class WorkflowExpressionCollection : IEnumerable<WorkflowExpression>
{
    private readonly List<WorkflowExpression> _items = [];

    public WorkflowExpressionCollection() { }

    public WorkflowExpressionCollection(IEnumerable<WorkflowExpression> list) : this()
    {
        _items.AddRange(list);
    }

    public int Count => _items.Count;

    public int Capacity => _items.Capacity;

    public void Add(WorkflowExpression item) =>
        _items.Add(item);

    public IEnumerator<WorkflowExpression> GetEnumerator() =>
        _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public WorkflowExpression this[int index] => _items[index];

    public static implicit operator WorkflowExpressionCollection(WorkflowExpression[] array) =>
        new(array);

    public static implicit operator WorkflowExpressionCollection(List<WorkflowExpression> array) =>
        new(array);

    public static implicit operator WorkflowExpressionCollection(string[] array) =>
        new(array.Select(WorkflowExpressions.Raw));

    public static implicit operator WorkflowExpressionCollection(List<string> list) =>
        new(list.Select(WorkflowExpressions.Raw));
}
