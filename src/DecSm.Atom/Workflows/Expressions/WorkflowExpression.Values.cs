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

    public static implicit operator WorkflowExpression(string value) =>
        new RawExpression(value);
}

public static partial class WorkflowExpressionExtensions
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
        public static RawExpression Raw(string value) =>
            new(value);

        [PublicAPI]
        public static StringExpression From(string value) =>
            new(value);

        [PublicAPI]
        public static BooleanExpression From(bool value) =>
            new(value);

        [PublicAPI]
        public static NumberExpression From<T>(T value)
            where T : INumber<T> =>
            new(double.TryParse(value.ToString(), out var result)
                ? result
                : 0);

        [PublicAPI]
        public static FormatExpression Format(WorkflowExpressionInterpolatedStringHandler handler) =>
            handler.ToFormatExpression();
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
