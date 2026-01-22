namespace DecSm.Atom.Workflows.Expressions;

partial record WorkflowExpression
{
    public static implicit operator WorkflowExpression(bool value) =>
        new BooleanExpression(value);

    public static implicit operator WorkflowExpression(double value) =>
        new NumberExpression(value);

    public static implicit operator WorkflowExpression(float value) =>
        new NumberExpression(value);

    public static implicit operator WorkflowExpression(long value) =>
        new NumberExpression(value);

    public static implicit operator WorkflowExpression(int value) =>
        new NumberExpression(value);

    public static implicit operator WorkflowExpression(short value) =>
        new NumberExpression(value);

    public static implicit operator WorkflowExpression(byte value) =>
        new NumberExpression(value);
}

[PublicAPI]
public static class WorkflowExpressionExtensions
{
    extension(WorkflowExpressions)
    {
        [PublicAPI]
        public static BooleanExpression True => new(true);

        [PublicAPI]
        public static BooleanExpression False => new(false);

        [PublicAPI]
        public static NullExpression Null => new();

        [PublicAPI]
        public static BooleanExpression From(bool value) =>
            new(value);

        [PublicAPI]
        public static NumberExpression From(double value) =>
            new(value);

        [PublicAPI]
        public static NumberExpression From(float value) =>
            new(value);

        [PublicAPI]
        public static NumberExpression From(long value) =>
            new(value);

        [PublicAPI]
        public static NumberExpression From(int value) =>
            new(value);

        [PublicAPI]
        public static NumberExpression From(short value) =>
            new(value);

        [PublicAPI]
        public static NumberExpression From(byte value) =>
            new(value);

        [PublicAPI]
        public static StringExpression From(string value) =>
            new(value);
    }
}

[PublicAPI]
public sealed record BooleanExpression(bool Value) : WorkflowExpression;

[PublicAPI]
public sealed record NullExpression : WorkflowExpression;

[PublicAPI]
public sealed record NumberExpression(double Value) : WorkflowExpression;

[PublicAPI]
public sealed record StringExpression(string Value) : WorkflowExpression;
