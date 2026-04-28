namespace DecSm.StructuredText.Expressions;

[PublicAPI]
public interface ITextExpression<TResult>;

[PublicAPI]
public abstract partial record TextExpression
{
    public IndexAccessExpression this[int index] => new(this, new NumberExpression(index));

    public PropertyAccessExpression this[string property] => new(this, new RawExpression(property));

    public EvaluateExpression Evaluate() =>
        new(this);

    /// <summary>
    ///     Reinterprets this expression as having a different result type.
    ///     The underlying expression is unchanged; only the compile-time type marker is affected.
    /// </summary>
    public CastExpression<TTo> Cast<TTo>() =>
        new(this);
}

/// <summary>
///     Wraps an expression to reinterpret its result type at compile time.
///     The formatter should treat this transparently — it simply delegates to the inner expression.
/// </summary>
[PublicAPI]
public sealed record CastExpression<TTo>(TextExpression Inner) : TextExpression, ITextExpression<TTo>;

/// <summary>
///     A typed workflow expression that documents the expected result type of the expression.
///     The type parameter serves as a compile-time hint for what the expression resolves to.
/// </summary>
[PublicAPI]
public readonly record struct WorkflowExpression<T>(TextExpression Value)
{
    public static implicit operator WorkflowExpression<T>(TextExpression expression) =>
        new(expression);

    public static implicit operator TextExpression(WorkflowExpression<T> typed) =>
        typed.Value;

    [return: NotNullIfNotNull(nameof(expression))]
    public static implicit operator WorkflowExpression<T>?(TextExpression? expression) =>
        expression is not null
            ? new(expression)
            : null;

    [return: NotNullIfNotNull(nameof(typed))]
    public static implicit operator TextExpression?(WorkflowExpression<T>? typed) =>
        typed?.Value;
}

[PublicAPI]
public sealed record RawExpression(string Value) : TextExpression, ITextExpression<string>;

[PublicAPI]
public sealed record EvaluateExpression(TextExpression Expression) : TextExpression, ITextExpression<string>;

[PublicAPI]
public sealed record IndexAccessExpression(TextExpression Array, TextExpression Index)
    : TextExpression, ITextExpression<string>;

[PublicAPI]
public sealed record PropertyAccessExpression(TextExpression Object, TextExpression Property)
    : TextExpression, ITextExpression<string>;

[PublicAPI]
public sealed record ConcatExpression(IEnumerable<TextExpression> Values) : TextExpression, ITextExpression<string>;

[PublicAPI]
public static partial class WorkflowExpressionExtensions
{
    extension(TextExpressions)
    {
        public static ConcatExpression Concat(IEnumerable<TextExpression> expressions) =>
            new(expressions);

        public static ConcatExpression ConcatWithSeparator(
            TextExpression separator,
            IEnumerable<TextExpression> expressions) =>
            new(Join(separator, expressions));
    }

    private static IEnumerable<TextExpression> Join(TextExpression separator, IEnumerable<TextExpression> expressions)
    {
        var list = expressions.ToList();

        yield return list[0];

        foreach (var expression in list.Skip(1))
        {
            yield return separator;
            yield return expression;
        }
    }
}
