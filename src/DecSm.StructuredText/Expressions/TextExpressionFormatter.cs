namespace DecSm.StructuredText.Expressions;

[PublicAPI]
public interface ITextExpressionFormatter
{
    [return: NotNullIfNotNull("expression")]
    string? Format(TextExpression? expression);

    string? Format<T>(WorkflowExpression<T>? expression) =>
        expression.HasValue
            ? Format(expression.Value.Value)
            : null;
}

[PublicAPI]
public abstract class TextExpressionFormatter : ITextExpressionFormatter
{
    [return: NotNullIfNotNull("expression")]
    public string? Format(TextExpression? expression)
    {
        while (true)
        {
            switch (expression)
            {
                case null:
                    return null;

                case RawExpression raw:
                    return raw.Value;

                case ConcatExpression concat:
                    return string.Concat(concat.Values.Select(Format));
            }

            // Unwrap CastExpression transparently — it only affects compile-time typing
            if (expression.GetType() is { IsGenericType: true } type &&
                type.GetGenericTypeDefinition() == typeof(CastExpression<>))
            {
                var inner = (TextExpression)type.GetProperty(nameof(CastExpression<>.Inner))!.GetValue(expression)!;

                expression = inner;

                continue;
            }

            if (Resolve(expression) is { } result)
            {
                expression = result;

                continue;
            }

            throw new InvalidOperationException($"No writer found to handle expression {expression}");
        }
    }

    protected abstract TextExpression? Resolve(TextExpression expression);
}
