namespace DecSm.StructuredText.GithubActions;

[PublicAPI]
public sealed class GithubExpressionFormatter : TextExpressionFormatter
{
    protected override TextExpression? Resolve(TextExpression expression) =>
        expression switch
        {
            // Values

            null => (TextExpression?)null,

            RawExpression => throw new InvalidOperationException(
                "Literal expressions should have already been resolved."),

            BooleanExpression booleanExpression => booleanExpression.Value
                ? "true"
                : "false",

            NullExpression => string.Empty,

            NumberExpression numberExpression => numberExpression.Value.ToString(CultureInfo.InvariantCulture),

            StringExpression x => $"'{x.Value}'",

            // Accessors

            IndexAccessExpression indexAccessExpression =>
                $"{Format(indexAccessExpression.Array)}[{Format(indexAccessExpression.Index)}]",

            PropertyAccessExpression propertyAccessExpression =>
                $"{Format(propertyAccessExpression.Object)}.{Format(propertyAccessExpression.Property)}",

            EvaluateExpression evaluateExpression => $"${{{{ {Format(evaluateExpression.Expression)} }}}}",

            // LogicOperators

            NotExpression notExpression => $"!{Format(notExpression.Source)}",

            AndExpression andExpression => string.Join(" && ", andExpression.Source.Select(Format)),

            OrExpression orExpression => string.Join(" || ", orExpression.Source.Select(Format)),

            EqualExpression equalExpression => $"{Format(equalExpression.Left)} == {Format(equalExpression.Right)}",

            NotEqualExpression notEqualExpression =>
                $"{Format(notEqualExpression.Left)} != {Format(notEqualExpression.Right)}",

            LessThanExpression lessThanExpression =>
                $"{Format(lessThanExpression.Left)} < {Format(lessThanExpression.Right)}",

            LessThanOrEqualToExpression lessThanOrEqualExpression =>
                $"{Format(lessThanOrEqualExpression.Left)} <= {Format(lessThanOrEqualExpression.Right)}",

            GreaterThanExpression greaterThanExpression =>
                $"{Format(greaterThanExpression.Left)} > {Format(greaterThanExpression.Right)}",

            GreaterThanOrEqualToExpression greaterThanOrEqualExpression =>
                $"{Format(greaterThanOrEqualExpression.Left)} >= {Format(greaterThanOrEqualExpression.Right)}",

            // Functions

            ContainsExpression containsExpression =>
                $"contains({Format(containsExpression.Source)}, {Format(containsExpression.Pattern)})",

            CoalesceExpression coalesceExpression => coalesceExpression.Source.Length switch
            {
                0 => string.Empty,
                1 => Format(coalesceExpression.Source[0]),
                2 => $"coalesce({Format(coalesceExpression.Source[0])}, {Format(coalesceExpression.Source[1])})",
                > 2 =>
                    $"coalesce({Format(new CoalesceExpression(coalesceExpression.Source[..^1]))}, {Format(coalesceExpression.Source[^1])})",
                _ => throw new ArgumentOutOfRangeException(nameof(expression)),
            },

            StartsWithExpression startsWithExpression =>
                $"startsWith({Format(startsWithExpression.Source)}, {Format(startsWithExpression.Pattern)})",

            EndsWithExpression endsWithExpression =>
                $"endsWith({Format(endsWithExpression.Source)}, {Format(endsWithExpression.Pattern)})",

            FormatExpression formatExpression => formatExpression.Arguments.Length switch
            {
                0 => Format(formatExpression.Source),
                1 => $"format({Format(formatExpression.Source)}, {Format(formatExpression.Arguments[0])})",
                > 1 =>
                    $"format({Format(formatExpression.Source)}, {string.Join(", ", formatExpression.Arguments.Select(Format))})",
                _ => throw new ArgumentOutOfRangeException(nameof(expression)),
            },

            JoinExpression joinExpression => joinExpression.OptionalSeparator is null
                ? $"join({Format(joinExpression.Source)})"
                : $"join({Format(joinExpression.Source)}, {Format(joinExpression.OptionalSeparator)})",

            ToJsonExpression toJsonExpression => $"toJSON({Format(toJsonExpression.Source)})",

            HashFilesExpression hashFilesExpression => $"hashFiles({Format(hashFilesExpression.Source)})",

            // Workflows

            TargetOutputExpression jobOutputExpression => jobOutputExpression.OutputName is { Length: > 0 }
                ? $"needs.{jobOutputExpression.TargetName}.outputs.{jobOutputExpression.OutputName}"
                : $"needs.{jobOutputExpression.TargetName}.outputs",

            TargetOutcomeExpression targetOutcomeExpression => $"needs.{targetOutcomeExpression.Target}.status",

            TargetOutcomeTypeExpression { Type: TargetOutcomeTypeExpression.OutcomeType.Success } => "succeeded",
            TargetOutcomeTypeExpression { Type: TargetOutcomeTypeExpression.OutcomeType.Failure } => "failed",
            TargetOutcomeTypeExpression { Type: TargetOutcomeTypeExpression.OutcomeType.Cancelled } => "cancelled",

            StepOutputExpression stepOutputExpression =>
                $"steps.{stepOutputExpression.StepName}.outputs.{stepOutputExpression.OutputName}",

            StepOutcomeExpression stepOutcomeExpression => $"steps.{stepOutcomeExpression.StepName}.outcome",

            StepOutcomeTypeExpression { Type: StepOutcomeTypeExpression.OutcomeType.Success } => "success",
            StepOutcomeTypeExpression { Type: StepOutcomeTypeExpression.OutcomeType.Failure } => "failure",
            StepOutcomeTypeExpression { Type: StepOutcomeTypeExpression.OutcomeType.Cancelled } => "cancelled",
            StepOutcomeTypeExpression { Type: StepOutcomeTypeExpression.OutcomeType.Skipped } => "skipped",

            // Other

            _ => throw new ArgumentOutOfRangeException(nameof(expression)),
        };
}
