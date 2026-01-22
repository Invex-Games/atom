namespace DecSm.Atom.Module.GithubWorkflows;

internal sealed class GithubExpressionWriter(IBuildDefinition buildDefinition) : IWorkflowExpressionWriter
{
    public bool Enabled => buildDefinition is IGithubWorkflows;

    public int Priority => 1000;

    public WorkflowExpression? Write(IWorkflowExpressionGenerator generator, WorkflowExpression expression) =>
        expression switch
        {
            // Values

            BooleanExpression booleanExpression => booleanExpression.Value
                ? "true"
                : "false",

            NullExpression => string.Empty,

            NumberExpression numberExpression => numberExpression.Value.ToString(CultureInfo.InvariantCulture),

            StringExpression stringExpression => $"'{stringExpression.Value.Replace("'", "''")}'",

            // Accessors

            IndexAccessExpression indexAccessExpression =>
                $"{generator.Write(indexAccessExpression.Array)}[{generator.Write(indexAccessExpression.Index)}]",

            PropertyAccessExpression propertyAccessExpression =>
                $"{generator.Write(propertyAccessExpression.Object)}.{generator.Write(propertyAccessExpression.Property)}",

            EvaluateExpression evaluateExpression => $"${{{{ {generator.Write(evaluateExpression.Expression)} }}}}",

            // LogicOperators

            NotExpression notExpression => $"!{generator.Write(notExpression.Source)}",

            AndExpression andExpression => string.Join(" && ", andExpression.Source.Select(generator.Write)),

            OrExpression orExpression => string.Join(" || ", orExpression.Source.Select(generator.Write)),

            EqualExpression equalExpression =>
                $"{generator.Write(equalExpression.Left)} == {generator.Write(equalExpression.Right)}",

            NotEqualExpression notEqualExpression =>
                $"{generator.Write(notEqualExpression.Left)} != {generator.Write(notEqualExpression.Right)}",

            LessThanExpression lessThanExpression =>
                $"{generator.Write(lessThanExpression.Left)} < {generator.Write(lessThanExpression.Right)}",

            LessThanOrEqualExpression lessThanOrEqualExpression =>
                $"{generator.Write(lessThanOrEqualExpression.Left)} <= {generator.Write(lessThanOrEqualExpression.Right)}",

            GreaterThanExpression greaterThanExpression =>
                $"{generator.Write(greaterThanExpression.Left)} > {generator.Write(greaterThanExpression.Right)}",

            GreaterThanOrEqualExpression greaterThanOrEqualExpression =>
                $"{generator.Write(greaterThanOrEqualExpression.Left)} >= {generator.Write(greaterThanOrEqualExpression.Right)}",

            // Functions

            ContainsExpression containsExpression =>
                $"contains({generator.Write(containsExpression.Source)}, {generator.Write(containsExpression.Pattern)})",

            CoalesceExpression coalesceExpression => coalesceExpression.Source.Length switch
            {
                0 => string.Empty,
                1 => generator.Write(coalesceExpression.Source[0]),
                2 =>
                    $"coalesce({generator.Write(coalesceExpression.Source[0])}, {generator.Write(coalesceExpression.Source[1])})",
                > 2 =>
                    $"coalesce({new CoalesceExpression(coalesceExpression.Source[..^1])}, {generator.Write(coalesceExpression.Source[^1])})",
                _ => throw new ArgumentOutOfRangeException(nameof(expression)),
            },

            StartsWithExpression startsWithExpression =>
                $"startsWith({generator.Write(startsWithExpression.Source)}, {generator.Write(startsWithExpression.Pattern)})",

            EndsWithExpression endsWithExpression =>
                $"endsWith({generator.Write(endsWithExpression.Source)}, {generator.Write(endsWithExpression.Pattern)})",

            FormatExpression formatExpression => formatExpression.Arguments.Length switch
            {
                0 => generator.Write(formatExpression.Source),
                1 =>
                    $"format({generator.Write(formatExpression.Source)}, {generator.Write(formatExpression.Arguments[0])})",
                > 1 =>
                    $"format({generator.Write(formatExpression.Source)}, {string.Join(", ", formatExpression.Arguments.Select(generator.Write))})",
                _ => throw new ArgumentOutOfRangeException(nameof(expression)),
            },

            JoinExpression joinExpression => joinExpression.OptionalSeparator is null
                ? $"join({generator.Write(joinExpression.Source)})"
                : $"join({generator.Write(joinExpression.Source)}, {generator.Write(joinExpression.OptionalSeparator)})",

            ToJsonExpression toJsonExpression => $"toJSON({generator.Write(toJsonExpression.Source)})",

            HashFilesExpression hashFilesExpression => $"hashFiles({generator.Write(hashFilesExpression.Source)})",

            // Workflows

            TargetOutputExpression jobOutputExpression => jobOutputExpression.OutputName is { Length: > 0 }
                ? $"needs.{jobOutputExpression.TargetName}.outputs.{jobOutputExpression.OutputName}"
                : $"needs.{jobOutputExpression.TargetName}.outputs",

            TargetOutcomeExpression targetOutcomeExpression => $"needs.{targetOutcomeExpression.Target}.status",

            TargetOutcomeTypeExpression targetOutcomeTypeExpression => targetOutcomeTypeExpression.Type switch
            {
                TargetOutcomeTypeExpression.OutcomeType.Success => "succeeded",
                TargetOutcomeTypeExpression.OutcomeType.Failure => "failed",
                TargetOutcomeTypeExpression.OutcomeType.Cancelled => "cancelled",
                _ => throw new ArgumentOutOfRangeException(nameof(expression)),
            },

            StepOutputExpression stepOutputExpression =>
                $"steps.{stepOutputExpression.StepName}.outputs.{stepOutputExpression.OutputName}",

            StepOutcomeExpression stepOutcomeExpression => $"steps.{stepOutcomeExpression.StepName}.outcome",

            StepOutcomeTypeExpression stepOutcomeTypeExpression => stepOutcomeTypeExpression.Type switch
            {
                StepOutcomeTypeExpression.OutcomeType.Success => "success",
                StepOutcomeTypeExpression.OutcomeType.Failure => "failure",
                StepOutcomeTypeExpression.OutcomeType.Cancelled => "cancelled",
                StepOutcomeTypeExpression.OutcomeType.Skipped => "skipped",
                _ => throw new ArgumentOutOfRangeException(nameof(expression)),
            },

            _ => (WorkflowExpression?)null,
        };
}
