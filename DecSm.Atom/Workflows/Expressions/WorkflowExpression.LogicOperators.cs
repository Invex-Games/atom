namespace DecSm.Atom.Workflows.Expressions;

partial record WorkflowExpression
{
    public WorkflowExpression Not() =>
        new NotExpression(this);

    public static NotExpression operator !(WorkflowExpression source) =>
        new(source);

    public WorkflowExpression And(params WorkflowExpression[] expressions) =>
        expressions.Length is 0
            ? this
            : new AndExpression([this, ..expressions]);

    public static AndExpression operator &(WorkflowExpression left, WorkflowExpression right) =>
        new(left, right);

    public WorkflowExpression Or(params WorkflowExpression[] expressions) =>
        expressions.Length is 0
            ? this
            : new OrExpression([this, ..expressions]);

    public static OrExpression operator |(WorkflowExpression left, WorkflowExpression right) =>
        new(left, right);

    public WorkflowExpression EqualTo(WorkflowExpression right) =>
        new EqualExpression(this, right);

    public WorkflowExpression EqualTo(string right) =>
        new EqualExpression(this, new RawExpression(right));

    public WorkflowExpression EqualToString(string right) =>
        new EqualExpression(this, new StringExpression(right));

    public WorkflowExpression NotEqualTo(WorkflowExpression right) =>
        new NotEqualExpression(this, right);

    public WorkflowExpression NotEqualTo(string right) =>
        new NotEqualExpression(this, new RawExpression(right));

    public WorkflowExpression NotEqualToString(string right) =>
        new NotEqualExpression(this, new StringExpression(right));

    public WorkflowExpression LessThan(WorkflowExpression right) =>
        new LessThanExpression(this, right);

    public WorkflowExpression LessThan(string right) =>
        new LessThanExpression(this, new RawExpression(right));

    public WorkflowExpression LessThanString(string right) =>
        new LessThanExpression(this, new StringExpression(right));

    public WorkflowExpression GreaterThan(WorkflowExpression right) =>
        new GreaterThanExpression(this, right);

    public WorkflowExpression GreaterThan(string right) =>
        new GreaterThanExpression(this, new RawExpression(right));

    public WorkflowExpression GreaterThanString(string right) =>
        new GreaterThanExpression(this, new StringExpression(right));

    public WorkflowExpression LessThanOrEqualTo(WorkflowExpression right) =>
        new LessThanOrEqualToExpression(this, right);

    public WorkflowExpression LessThanOrEqualTo(string right) =>
        new LessThanOrEqualToExpression(this, new RawExpression(right));

    public WorkflowExpression LessThanOrEqualToString(string right) =>
        new LessThanOrEqualToExpression(this, new StringExpression(right));

    public WorkflowExpression GreaterThanOrEqualTo(WorkflowExpression right) =>
        new GreaterThanOrEqualToExpression(this, right);

    public WorkflowExpression GreaterThanOrEqualTo(string right) =>
        new GreaterThanOrEqualToExpression(this, new RawExpression(right));

    public WorkflowExpression GreaterThanOrEqualToString(string right) =>
        new GreaterThanOrEqualToExpression(this, new StringExpression(right));
}

[PublicAPI]
public sealed record NotExpression(WorkflowExpression Source) : WorkflowExpression;

[PublicAPI]
public sealed record AndExpression(params WorkflowExpression[] Source) : WorkflowExpression
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
public sealed record OrExpression(params WorkflowExpression[] Source) : WorkflowExpression
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
public sealed record EqualExpression(WorkflowExpression Left, WorkflowExpression Right) : WorkflowExpression;

[PublicAPI]
public sealed record NotEqualExpression(WorkflowExpression Left, WorkflowExpression Right) : WorkflowExpression;

[PublicAPI]
public sealed record LessThanExpression(WorkflowExpression Left, WorkflowExpression Right) : WorkflowExpression;

[PublicAPI]
public sealed record GreaterThanExpression(WorkflowExpression Left, WorkflowExpression Right) : WorkflowExpression;

[PublicAPI]
public sealed record LessThanOrEqualToExpression(WorkflowExpression Left, WorkflowExpression Right)
    : WorkflowExpression;

[PublicAPI]
public sealed record GreaterThanOrEqualToExpression(WorkflowExpression Left, WorkflowExpression Right)
    : WorkflowExpression;
