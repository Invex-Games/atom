namespace DecSm.Atom.StructuredText.Expressions;

/// <summary>
///     An interpolated string handler that creates a <see cref="FormatExpression" /> from an interpolated string.
///     Literal parts are combined into a format string with placeholders, and <see cref="TextExpression" /> holes
///     become arguments.
/// </summary>
[InterpolatedStringHandler]
[PublicAPI]
public ref struct TextExpressionInterpolatedStringHandler(int literalLength, int formattedCount)
{
    private readonly StringBuilder _formatBuilder = new(literalLength + formattedCount * 3);
    private readonly List<TextExpression> _arguments = new(formattedCount);
    private int _argumentIndex = 0;

    public void AppendLiteral(string value) =>
        _formatBuilder.Append(value
            .Replace("{", "{{")
            .Replace("}", "}}"));

    public void AppendFormatted(TextExpression expression)
    {
        _formatBuilder.Append('{');
        _formatBuilder.Append(_argumentIndex++);
        _formatBuilder.Append('}');
        _arguments.Add(expression);
    }

    public void AppendFormatted(string value)
    {
        _formatBuilder.Append('{');
        _formatBuilder.Append(_argumentIndex++);
        _formatBuilder.Append('}');
        _arguments.Add(new StringExpression(value));
    }

    public void AppendFormatted<T>(T value)
        where T : INumber<T>
    {
        _formatBuilder.Append('{');
        _formatBuilder.Append(_argumentIndex++);
        _formatBuilder.Append('}');

        _arguments.Add(new NumberExpression(double.TryParse(value.ToString(), out var result)
            ? result
            : 0));
    }

    public void AppendFormatted(bool value)
    {
        _formatBuilder.Append('{');
        _formatBuilder.Append(_argumentIndex++);
        _formatBuilder.Append('}');
        _arguments.Add(new BooleanExpression(value));
    }

    public FormatExpression ToFormatExpression() =>
        new(new StringExpression(_formatBuilder.ToString()), [.. _arguments]);
}
