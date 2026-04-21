using System.Globalization;

namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops;

internal sealed class DevopsExpressionFormatter(IBuildDefinition buildDefinition) : IWorkflowExpressionFormatter
{
    public bool Enabled => buildDefinition is IDevopsWorkflows;

    public int Priority => 1000;

    public WorkflowExpression? Write(IWorkflowExpressionResolver resolver, WorkflowExpression expression) =>
        expression switch
        {
            // Values

            null => (WorkflowExpression?)null,

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
                $"{resolver.Resolve(indexAccessExpression.Array)}[{resolver.Resolve(indexAccessExpression.Index)}]",

            PropertyAccessExpression propertyAccessExpression =>
                $"{resolver.Resolve(propertyAccessExpression.Object)}.{resolver.Resolve(propertyAccessExpression.Property)}",

            // Compile-time template expression: ${{ expression }}
            EvaluateExpression evaluateExpression => $"${{{{ {resolver.Resolve(evaluateExpression.Expression)} }}}}",

            // Runtime expression: $[ expression ]
            DevopsRuntimeExpression runtimeExpression => $"$[ {resolver.Resolve(runtimeExpression.Expression)} ]",

            // Macro variable expansion: $(variable)
            DevopsMacroExpression macroExpression => $"$({resolver.Resolve(macroExpression.Variable)})",

            // Logic Operators

            NotExpression notExpression => $"not({resolver.Resolve(notExpression.Source)})",

            AndExpression andExpression => andExpression.Source.Length switch
            {
                0 => string.Empty,
                1 => resolver.Resolve(andExpression.Source[0]),
                2 => $"and({resolver.Resolve(andExpression.Source[0])}, {resolver.Resolve(andExpression.Source[1])})",
                _ => resolver.Resolve(new AndExpression([
                    new RawExpression(
                        $"and({resolver.Resolve(andExpression.Source[0])}, {resolver.Resolve(andExpression.Source[1])})"),
                    ..andExpression.Source[2..],
                ])),
            },

            OrExpression orExpression => orExpression.Source.Length switch
            {
                0 => string.Empty,
                1 => resolver.Resolve(orExpression.Source[0]),
                2 => $"or({resolver.Resolve(orExpression.Source[0])}, {resolver.Resolve(orExpression.Source[1])})",
                _ => resolver.Resolve(new OrExpression([
                    new RawExpression(
                        $"or({resolver.Resolve(orExpression.Source[0])}, {resolver.Resolve(orExpression.Source[1])})"),
                    ..orExpression.Source[2..],
                ])),
            },

            EqualExpression equalExpression =>
                $"eq({resolver.Resolve(equalExpression.Left)}, {resolver.Resolve(equalExpression.Right)})",

            NotEqualExpression notEqualExpression =>
                $"ne({resolver.Resolve(notEqualExpression.Left)}, {resolver.Resolve(notEqualExpression.Right)})",

            LessThanExpression lessThanExpression =>
                $"lt({resolver.Resolve(lessThanExpression.Left)}, {resolver.Resolve(lessThanExpression.Right)})",

            LessThanOrEqualToExpression lessThanOrEqualExpression =>
                $"le({resolver.Resolve(lessThanOrEqualExpression.Left)}, {resolver.Resolve(lessThanOrEqualExpression.Right)})",

            GreaterThanExpression greaterThanExpression =>
                $"gt({resolver.Resolve(greaterThanExpression.Left)}, {resolver.Resolve(greaterThanExpression.Right)})",

            GreaterThanOrEqualToExpression greaterThanOrEqualExpression =>
                $"ge({resolver.Resolve(greaterThanOrEqualExpression.Left)}, {resolver.Resolve(greaterThanOrEqualExpression.Right)})",

            // Functions

            ContainsExpression containsExpression =>
                $"contains({resolver.Resolve(containsExpression.Source)}, {resolver.Resolve(containsExpression.Pattern)})",

            CoalesceExpression coalesceExpression => coalesceExpression.Source.Length switch
            {
                0 => string.Empty,
                1 => resolver.Resolve(coalesceExpression.Source[0]),
                2 =>
                    $"coalesce({resolver.Resolve(coalesceExpression.Source[0])}, {resolver.Resolve(coalesceExpression.Source[1])})",
                > 2 =>
                    $"coalesce({resolver.Resolve(new CoalesceExpression(coalesceExpression.Source[..^1]))}, {resolver.Resolve(coalesceExpression.Source[^1])})",
                _ => throw new ArgumentOutOfRangeException(nameof(expression)),
            },

            StartsWithExpression startsWithExpression =>
                $"startsWith({resolver.Resolve(startsWithExpression.Source)}, {resolver.Resolve(startsWithExpression.Pattern)})",

            EndsWithExpression endsWithExpression =>
                $"endsWith({resolver.Resolve(endsWithExpression.Source)}, {resolver.Resolve(endsWithExpression.Pattern)})",

            FormatExpression formatExpression => formatExpression.Arguments.Length switch
            {
                0 => resolver.Resolve(formatExpression.Source),
                1 =>
                    $"format({resolver.Resolve(formatExpression.Source)}, {resolver.Resolve(formatExpression.Arguments[0])})",
                > 1 =>
                    $"format({resolver.Resolve(formatExpression.Source)}, {string.Join(", ", formatExpression.Arguments.Select(resolver.Resolve))})",
                _ => throw new ArgumentOutOfRangeException(nameof(expression)),
            },

            JoinExpression joinExpression => joinExpression.OptionalSeparator is null
                ? $"join({resolver.Resolve(joinExpression.Source)})"
                : $"join({resolver.Resolve(joinExpression.Source)}, {resolver.Resolve(joinExpression.OptionalSeparator)})",

            ToJsonExpression toJsonExpression => $"convertToJson({resolver.Resolve(toJsonExpression.Source)})",

            HashFilesExpression hashFilesExpression => $"hashFiles({resolver.Resolve(hashFilesExpression.Source)})",

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
