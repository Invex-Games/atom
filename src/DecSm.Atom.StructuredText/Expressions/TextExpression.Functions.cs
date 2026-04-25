namespace DecSm.Atom.StructuredText.Expressions;

partial record TextExpression
{
    public ContainsExpression Contains(TextExpression pattern) =>
        new(this, pattern);

    public ContainsExpression Contains(string pattern) =>
        new(this, new StringExpression(pattern));

    public ContainsExpression ContainedIn(TextExpression collection) =>
        new(collection, this);

    public ContainsExpression ContainedIn(string collection) =>
        new(this, new StringExpression(collection));

    public CoalesceExpression Coalesce(params TextExpression[] source) =>
        new([this, ..source]);

    public CoalesceExpression Coalesce(params string[] source) =>
        new([this, ..source.Select(x => new StringExpression(x))]);

    public StartsWithExpression StartsWith(TextExpression pattern) =>
        new(this, pattern);

    public StartsWithExpression StartsWith(string pattern) =>
        new(this, new StringExpression(pattern));

    public StartsWithExpression IsStartOf(TextExpression pattern) =>
        new(pattern, this);

    public StartsWithExpression IsStartOf(string pattern) =>
        new(new StringExpression(pattern), this);

    public EndsWithExpression EndsWith(TextExpression pattern) =>
        new(this, pattern);

    public EndsWithExpression EndsWith(string pattern) =>
        new(this, new StringExpression(pattern));

    public EndsWithExpression IsEndOf(TextExpression pattern) =>
        new(pattern, this);

    public EndsWithExpression IsEndOfString(string pattern) =>
        new(new StringExpression(pattern), this);

    public FormatExpression Format(params TextExpression[] arguments) =>
        new(this, arguments);

    public FormatExpression FormatString(params string[] arguments) =>
        new(this,
            arguments
                .Select(x => new StringExpression(x))
                .ToArray<TextExpression>());

    public static FormatExpression operator +(TextExpression source, TextExpression argument) =>
        new(source, argument);

    public static FormatExpression operator +(TextExpression source, TextExpression[] arguments) =>
        new(source, arguments);

    public static FormatExpression operator +(TextExpression source, string argument) =>
        new(source, new StringExpression(argument));

    public static FormatExpression operator +(TextExpression source, string[] arguments) =>
        new(source,
            arguments
                .Select(x => new StringExpression(x))
                .ToArray<TextExpression>());

    public JoinExpression Join(TextExpression separator) =>
        new(this, separator);

    public JoinExpression JoinString(string separator) =>
        new(this, new StringExpression(separator));

    public ToJsonExpression ToJson() =>
        new(this);

    public HashFilesExpression HashFiles() =>
        new(this);
}

[PublicAPI]
public sealed record ContainsExpression(TextExpression Source, TextExpression Pattern)
    : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record CoalesceExpression(params TextExpression[] Source) : TextExpression
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
public sealed record StartsWithExpression(TextExpression Source, TextExpression Pattern)
    : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record EndsWithExpression(TextExpression Source, TextExpression Pattern)
    : TextExpression, ITextExpression<bool>;

[PublicAPI]
public sealed record FormatExpression(TextExpression Source, params TextExpression[] Arguments)
    : TextExpression, ITextExpression<string>
{
    protected override bool PrintMembers(StringBuilder builder)
    {
        if (base.PrintMembers(builder))
            builder.Append(", ");

        builder.Append(Source);
        builder.Append(", ");

        builder.Append("[ ");

        for (var i = 0; i < Arguments.Length; i++)
        {
            if (i < Arguments.Length - 1)
                builder.Append(", ");

            builder.Append(Arguments[i]);
        }

        builder.Append(" ]");

        return true;
    }
}

[PublicAPI]
public sealed record JoinExpression(TextExpression Source, TextExpression? OptionalSeparator)
    : TextExpression, ITextExpression<string>;

[PublicAPI]
public sealed record ToJsonExpression(TextExpression Source) : TextExpression, ITextExpression<string>;

[PublicAPI]
public sealed record HashFilesExpression(TextExpression Source) : TextExpression, ITextExpression<string>;
