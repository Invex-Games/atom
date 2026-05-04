namespace DecSm.StructuredText.Expressions;

partial record TextExpression
{
    public static implicit operator TextExpression(bool value) =>
        new BooleanExpression(value);

    public static implicit operator TextExpression(double value) =>
        new NumberExpression(value);

    public static implicit operator TextExpression(float value) =>
        new NumberExpression(value);

    public static implicit operator TextExpression(long value) =>
        new NumberExpression(value);

    public static implicit operator TextExpression(int value) =>
        new NumberExpression(value);

    public static implicit operator TextExpression(short value) =>
        new NumberExpression(value);

    public static implicit operator TextExpression(byte value) =>
        new NumberExpression(value);

    public static implicit operator TextExpression(string value) =>
        new RawExpression(value);
}

public static partial class WorkflowExpressionExtensions
{
    extension(TextExpressions)
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
        [OverloadResolutionPriority(1)]
        public static FormatExpression Format(TextExpressionInterpolatedStringHandler handler) =>
            handler.ToFormatExpression();

        [PublicAPI]
        public static FormatExpression Format(string text)
        {
            var handler = new TextExpressionInterpolatedStringHandler(text.Length, 0);
            handler.AppendLiteral(text);

            return handler.ToFormatExpression();
        }
    }
}

[PublicAPI]
public sealed record BooleanExpression(bool Value) : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record NullExpression : TextExpression;

[PublicAPI]
public sealed record NumberExpression(double Value) : TextExpression, ITextExpression<double>;

[PublicAPI]
public sealed record StringExpression(string Value) : TextExpression, ITextExpression<string>;
