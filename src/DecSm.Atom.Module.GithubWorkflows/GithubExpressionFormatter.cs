namespace DecSm.Atom.Module.GithubWorkflows;

internal sealed class GithubExpressionFormatter(IBuildDefinition buildDefinition) : IWorkflowExpressionFormatter
{
    public bool Enabled => buildDefinition is IGithubWorkflows;

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

            EvaluateExpression evaluateExpression => $"${{{{ {resolver.Resolve(evaluateExpression.Expression)} }}}}",

            // LogicOperators

            NotExpression notExpression => $"!{resolver.Resolve(notExpression.Source)}",

            AndExpression andExpression => string.Join(" && ", andExpression.Source.Select(resolver.Resolve)),

            OrExpression orExpression => string.Join(" || ", orExpression.Source.Select(resolver.Resolve)),

            EqualExpression equalExpression =>
                $"{resolver.Resolve(equalExpression.Left)} == {resolver.Resolve(equalExpression.Right)}",

            NotEqualExpression notEqualExpression =>
                $"{resolver.Resolve(notEqualExpression.Left)} != {resolver.Resolve(notEqualExpression.Right)}",

            LessThanExpression lessThanExpression =>
                $"{resolver.Resolve(lessThanExpression.Left)} < {resolver.Resolve(lessThanExpression.Right)}",

            LessThanOrEqualToExpression lessThanOrEqualExpression =>
                $"{resolver.Resolve(lessThanOrEqualExpression.Left)} <= {resolver.Resolve(lessThanOrEqualExpression.Right)}",

            GreaterThanExpression greaterThanExpression =>
                $"{resolver.Resolve(greaterThanExpression.Left)} > {resolver.Resolve(greaterThanExpression.Right)}",

            GreaterThanOrEqualToExpression greaterThanOrEqualExpression =>
                $"{resolver.Resolve(greaterThanOrEqualExpression.Left)} >= {resolver.Resolve(greaterThanOrEqualExpression.Right)}",

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
                    $"coalesce({new CoalesceExpression(coalesceExpression.Source[..^1])}, {resolver.Resolve(coalesceExpression.Source[^1])})",
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

            ToJsonExpression toJsonExpression => $"toJSON({resolver.Resolve(toJsonExpression.Source)})",

            HashFilesExpression hashFilesExpression => $"hashFiles({resolver.Resolve(hashFilesExpression.Source)})",

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
