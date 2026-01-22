namespace DecSm.Atom.Workflows.Expressions;

partial record WorkflowExpression
{
    public WorkflowExpression Contains(WorkflowExpression pattern) =>
        new ContainsExpression(this, pattern);

    public WorkflowExpression ContainsString(string pattern) =>
        new ContainsExpression(this, new StringExpression(pattern));

    public WorkflowExpression ContainedIn(WorkflowExpression collection) =>
        new ContainsExpression(collection, this);

    public WorkflowExpression ContainedInString(string collection) =>
        new ContainsExpression(this, new StringExpression(collection));

    public WorkflowExpression Coalesce(params WorkflowExpression[] source) =>
        source.Length is 0
            ? this
            : new CoalesceExpression([this, ..source]);

    public WorkflowExpression CoalesceString(params string[] source) =>
        source.Length is 0
            ? this
            : new CoalesceExpression([this, ..source.Select(x => new StringExpression(x))]);

    public WorkflowExpression StartsWith(WorkflowExpression pattern) =>
        new StartsWithExpression(this, pattern);

    public WorkflowExpression StartsWithString(string pattern) =>
        new StartsWithExpression(this, new StringExpression(pattern));

    public WorkflowExpression IsStartOf(WorkflowExpression pattern) =>
        new StartsWithExpression(pattern, this);

    public WorkflowExpression IsStartOfString(string pattern) =>
        new StartsWithExpression(new StringExpression(pattern), this);

    public WorkflowExpression EndsWith(WorkflowExpression pattern) =>
        new EndsWithExpression(this, pattern);

    public WorkflowExpression EndsWithString(string pattern) =>
        new EndsWithExpression(this, new StringExpression(pattern));

    public WorkflowExpression IsEndOf(WorkflowExpression pattern) =>
        new EndsWithExpression(pattern, this);

    public WorkflowExpression IsEndOfString(string pattern) =>
        new EndsWithExpression(new StringExpression(pattern), this);

    public WorkflowExpression Format(params WorkflowExpression[] arguments) =>
        new FormatExpression(this, arguments);

    public WorkflowExpression FormatString(params string[] arguments) =>
        new FormatExpression(this,
            arguments
                .Select(x => new StringExpression(x))
                .ToArray<WorkflowExpression>());

    public WorkflowExpression Join(WorkflowExpression separator) =>
        new JoinExpression(this, separator);

    public WorkflowExpression JoinString(string separator) =>
        new JoinExpression(this, new StringExpression(separator));

    public WorkflowExpression ToJson() =>
        new ToJsonExpression(this);

    public WorkflowExpression HashFiles() =>
        new HashFilesExpression(this);
}

[PublicAPI]
public sealed record ContainsExpression(WorkflowExpression Source, WorkflowExpression Pattern) : WorkflowExpression;

[PublicAPI]
public sealed record CoalesceExpression(params WorkflowExpression[] Source) : WorkflowExpression
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
public sealed record StartsWithExpression(WorkflowExpression Source, WorkflowExpression Pattern) : WorkflowExpression;

[PublicAPI]
public sealed record EndsWithExpression(WorkflowExpression Source, WorkflowExpression Pattern) : WorkflowExpression;

[PublicAPI]
public sealed record FormatExpression(WorkflowExpression Source, params WorkflowExpression[] Arguments)
    : WorkflowExpression
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
public sealed record JoinExpression(WorkflowExpression Source, WorkflowExpression? OptionalSeparator)
    : WorkflowExpression;

[PublicAPI]
public sealed record ToJsonExpression(WorkflowExpression Source) : WorkflowExpression;

[PublicAPI]
public sealed record HashFilesExpression(WorkflowExpression Source) : WorkflowExpression;
