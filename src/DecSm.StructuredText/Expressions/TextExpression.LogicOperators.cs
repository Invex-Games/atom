namespace DecSm.StructuredText.Expressions;

partial record TextExpression
{
    public NotExpression Not() =>
        new(this);

    public static NotExpression operator !(TextExpression source) =>
        new(source);

    public AndExpression And(params TextExpression[] expressions) =>
        new([this, ..expressions]);

    public static AndExpression operator &(TextExpression left, TextExpression right) =>
        new(left, right);

    public OrExpression Or(params TextExpression[] expressions) =>
        new([this, ..expressions]);

    public static OrExpression operator |(TextExpression left, TextExpression right) =>
        new(left, right);

    public EqualExpression EqualTo(TextExpression right) =>
        new(this, right);

    public EqualExpression EqualTo(string right) =>
        new(this, new RawExpression(right));

    public EqualExpression EqualToString(string right) =>
        new(this, new StringExpression(right));

    public NotEqualExpression NotEqualTo(TextExpression right) =>
        new(this, right);

    public NotEqualExpression NotEqualTo(string right) =>
        new(this, new RawExpression(right));

    public NotEqualExpression NotEqualToString(string right) =>
        new(this, new StringExpression(right));

    public LessThanExpression LessThan(TextExpression right) =>
        new(this, right);

    public LessThanExpression LessThan(string right) =>
        new(this, new RawExpression(right));

    public LessThanExpression LessThanString(string right) =>
        new(this, new StringExpression(right));

    public GreaterThanExpression GreaterThan(TextExpression right) =>
        new(this, right);

    public GreaterThanExpression GreaterThan(string right) =>
        new(this, new RawExpression(right));

    public GreaterThanExpression GreaterThanString(string right) =>
        new(this, new StringExpression(right));

    public LessThanOrEqualToExpression LessThanOrEqualTo(TextExpression right) =>
        new(this, right);

    public LessThanOrEqualToExpression LessThanOrEqualTo(string right) =>
        new(this, new RawExpression(right));

    public LessThanOrEqualToExpression LessThanOrEqualToString(string right) =>
        new(this, new StringExpression(right));

    public GreaterThanOrEqualToExpression GreaterThanOrEqualTo(TextExpression right) =>
        new(this, right);

    public GreaterThanOrEqualToExpression GreaterThanOrEqualTo(string right) =>
        new(this, new RawExpression(right));

    public GreaterThanOrEqualToExpression GreaterThanOrEqualToString(string right) =>
        new(this, new StringExpression(right));
}

[PublicAPI]
public sealed record NotExpression(TextExpression Source) : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record AndExpression(params TextExpression[] Source) : TextExpression, ITextExpression<bool>
{
    protected override bool PrintMembers(StringBuilder builder)
    {
        if (base.PrintMembers(builder))
            builder.Append(", ");

        builder.Append("[ ");

        for (var i = 0; i < Source.Length; i++)
        {
            if (i < Source.Length - 1)
                builder.Append(", ");

            builder.Append(Source[i]);
        }

        builder.Append(" ]");

        return true;
    }
}

[PublicAPI]
public sealed record OrExpression(params TextExpression[] Source) : TextExpression, ITextExpression<bool>
{
    protected override bool PrintMembers(StringBuilder builder)
    {
        if (base.PrintMembers(builder))
            builder.Append(", ");

        builder.Append("[ ");

        for (var i = 0; i < Source.Length; i++)
        {
            if (i < Source.Length - 1)
                builder.Append(", ");

            builder.Append(Source[i]);
        }

        builder.Append(" ]");

        return true;
    }
}

[PublicAPI]
public sealed record EqualExpression(TextExpression Left, TextExpression Right) : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record NotEqualExpression(TextExpression Left, TextExpression Right)
    : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record LessThanExpression(TextExpression Left, TextExpression Right)
    : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record GreaterThanExpression(TextExpression Left, TextExpression Right)
    : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record LessThanOrEqualToExpression(TextExpression Left, TextExpression Right)
    : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record GreaterThanOrEqualToExpression(TextExpression Left, TextExpression Right)
    : TextExpression, ITextExpression<bool>;
