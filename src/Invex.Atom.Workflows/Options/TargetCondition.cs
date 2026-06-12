namespace Invex.Atom.Workflows.Options;

/// <summary>
///     A workflow option that gates execution of a target's job on a platform-evaluated condition expression.
/// </summary>
/// <param name="Condition">
///     The condition expression to evaluate. May reference outputs of other targets via
///     <see cref="TargetOutputExpression" />, which implicitly adds those targets as dependencies.
/// </param>
/// <remarks>
///     Typically created via <c>BuildOptions.Target.RunIfWorkflowCondition(...)</c>. The expression is rendered
///     into the generated workflow file using the syntax of the targeted CI/CD platform.
/// </remarks>
[PublicAPI]
public sealed record TargetCondition(TextExpression Condition) : IBuildOption, IImplicitTargetDependencyOption
{
    /// <summary>
    ///     Gets the names of targets whose outputs are referenced by <see cref="Condition" />.
    ///     These targets are implicitly added as dependencies of the conditioned target.
    /// </summary>
    public IEnumerable<string> TargetNames =>
        TextExpressionUtils
            .Flatten(Condition)
            .OfType<TargetOutputExpression>()
            .Select(x => x.TargetName);
}
