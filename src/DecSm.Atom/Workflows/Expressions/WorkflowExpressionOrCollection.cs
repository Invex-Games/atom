namespace DecSm.Atom.Workflows.Expressions;

[PublicAPI]
[Union]
public partial record WorkflowExpressionOrCollection
{
    public sealed partial record Expression(WorkflowExpression Value);

    public sealed partial record Collection(WorkflowExpressionCollection Value);

    // bool

    public static WorkflowExpressionOrCollection From(IEnumerable<bool> value) =>
        new Collection(new(value.Select(WorkflowExpression (x) => new BooleanExpression(x))));

    public static implicit operator WorkflowExpressionOrCollection(bool[] value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(List<bool> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(ReadOnlyCollection<bool> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(bool value) =>
        new Expression(new BooleanExpression(value));

    // double

    public static WorkflowExpressionOrCollection From(IEnumerable<double> value) =>
        new Collection(new(value.Select(WorkflowExpression (x) => new NumberExpression(x))));

    public static implicit operator WorkflowExpressionOrCollection(double[] value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(List<double> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(ReadOnlyCollection<double> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(double value) =>
        new Expression(new NumberExpression(value));

    // float

    public static WorkflowExpressionOrCollection From(IEnumerable<float> value) =>
        new Collection(new(value.Select(WorkflowExpression (x) => new NumberExpression(x))));

    public static implicit operator WorkflowExpressionOrCollection(float[] value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(List<float> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(ReadOnlyCollection<float> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(float value) =>
        new Expression(new NumberExpression(value));

    // long

    public static WorkflowExpressionOrCollection From(IEnumerable<long> value) =>
        new Collection(new(value.Select(WorkflowExpression (x) => new NumberExpression(x))));

    public static implicit operator WorkflowExpressionOrCollection(long[] value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(List<long> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(ReadOnlyCollection<long> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(long value) =>
        new Expression(new NumberExpression(value));

    // int

    public static WorkflowExpressionOrCollection From(IEnumerable<int> value) =>
        new Collection(new(value.Select(WorkflowExpression (x) => new NumberExpression(x))));

    public static implicit operator WorkflowExpressionOrCollection(int[] value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(List<int> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(ReadOnlyCollection<int> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(int value) =>
        new Expression(new NumberExpression(value));

    // short

    public static WorkflowExpressionOrCollection From(IEnumerable<short> value) =>
        new Collection(new(value.Select(WorkflowExpression (x) => new NumberExpression(x))));

    public static implicit operator WorkflowExpressionOrCollection(short[] value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(List<short> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(ReadOnlyCollection<short> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(short value) =>
        new Expression(new NumberExpression(value));

    // string

    public static WorkflowExpressionOrCollection From(IEnumerable<string> value) =>
        new Collection(new(value.Select(WorkflowExpression (x) => new StringExpression(x))));

    public static implicit operator WorkflowExpressionOrCollection(string[] value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(List<string> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(ReadOnlyCollection<string> value) =>
        From(value);

    public static implicit operator WorkflowExpressionOrCollection(string value) =>
        new Expression(new RawExpression(value));
}
