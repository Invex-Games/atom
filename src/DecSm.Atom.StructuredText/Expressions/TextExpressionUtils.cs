namespace DecSm.Atom.StructuredText.Expressions;

[PublicAPI]
public static class TextExpressionUtils
{
    extension(IEnumerable<TextExpression> expressions)
    {
        public IEnumerable<TextExpression> Join(TextExpression separator)
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
}
