namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops;

internal sealed class DevopsExpressionFormatter : TextExpressionFormatter
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

            // Compile-time template expression: ${{ expression }}
            EvaluateExpression evaluateExpression => $"${{{{ {Format(evaluateExpression.Expression)} }}}}",

            // Runtime expression: $[ expression ]
            DevopsRuntimeExpression runtimeExpression => $"$[ {Format(runtimeExpression.Expression)} ]",

            // Macro variable expansion: $(variable)
            DevopsMacroExpression macroExpression => $"$({Format(macroExpression.Variable)})",

            // Logic Operators

            NotExpression notExpression => $"not({Format(notExpression.Source)})",

            AndExpression andExpression => andExpression.Source.Length switch
            {
                0 => string.Empty,
                1 => Format(andExpression.Source[0]),
                2 => $"and({Format(andExpression.Source[0])}, {Format(andExpression.Source[1])})",
                _ => Format(new AndExpression([
                    new RawExpression($"and({Format(andExpression.Source[0])}, {Format(andExpression.Source[1])})"),
                    ..andExpression.Source[2..],
                ])),
            },

            OrExpression orExpression => orExpression.Source.Length switch
            {
                0 => string.Empty,
                1 => Format(orExpression.Source[0]),
                2 => $"or({Format(orExpression.Source[0])}, {Format(orExpression.Source[1])})",
                _ => Format(new OrExpression([
                    new RawExpression($"or({Format(orExpression.Source[0])}, {Format(orExpression.Source[1])})"),
                    ..orExpression.Source[2..],
                ])),
            },

            EqualExpression equalExpression => $"eq({Format(equalExpression.Left)}, {Format(equalExpression.Right)})",

            NotEqualExpression notEqualExpression =>
                $"ne({Format(notEqualExpression.Left)}, {Format(notEqualExpression.Right)})",

            LessThanExpression lessThanExpression =>
                $"lt({Format(lessThanExpression.Left)}, {Format(lessThanExpression.Right)})",

            LessThanOrEqualToExpression lessThanOrEqualExpression =>
                $"le({Format(lessThanOrEqualExpression.Left)}, {Format(lessThanOrEqualExpression.Right)})",

            GreaterThanExpression greaterThanExpression =>
                $"gt({Format(greaterThanExpression.Left)}, {Format(greaterThanExpression.Right)})",

            GreaterThanOrEqualToExpression greaterThanOrEqualExpression =>
                $"ge({Format(greaterThanOrEqualExpression.Left)}, {Format(greaterThanOrEqualExpression.Right)})",

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

            ToJsonExpression toJsonExpression => $"convertToJson({Format(toJsonExpression.Source)})",

            HashFilesExpression hashFilesExpression => $"hashFiles({Format(hashFilesExpression.Source)})",

            // Workflows

            TargetOutputExpression jobOutputExpression => jobOutputExpression.OutputName is { Length: > 0 }
                ? $"dependencies.{jobOutputExpression.TargetName}.outputs['{jobOutputExpression.TargetName}.{jobOutputExpression.OutputName}']"
                : $"dependencies.{jobOutputExpression.TargetName}.outputs",

            TargetOutcomeExpression targetOutcomeExpression => $"dependencies.{targetOutcomeExpression.Target}.result",

            TargetOutcomeTypeExpression { Type: TargetOutcomeTypeExpression.OutcomeType.Success } => "'Succeeded'",
            TargetOutcomeTypeExpression { Type: TargetOutcomeTypeExpression.OutcomeType.Failure } => "'Failed'",
            TargetOutcomeTypeExpression { Type: TargetOutcomeTypeExpression.OutcomeType.Cancelled } => "'Canceled'",

            StepOutputExpression stepOutputExpression =>
                $"steps.{stepOutputExpression.StepName}.outputs['{stepOutputExpression.OutputName}']",

            StepOutcomeExpression stepOutcomeExpression => $"steps.{stepOutcomeExpression.StepName}.outcome",

            StepOutcomeTypeExpression { Type: StepOutcomeTypeExpression.OutcomeType.Success } => "'Succeeded'",
            StepOutcomeTypeExpression { Type: StepOutcomeTypeExpression.OutcomeType.Failure } => "'Failed'",
            StepOutcomeTypeExpression { Type: StepOutcomeTypeExpression.OutcomeType.Cancelled } => "'Canceled'",
            StepOutcomeTypeExpression { Type: StepOutcomeTypeExpression.OutcomeType.Skipped } => "'Skipped'",

            // Other

            _ => throw new ArgumentOutOfRangeException(nameof(expression)),
        };
}
