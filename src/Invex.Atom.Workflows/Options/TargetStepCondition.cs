namespace Invex.Atom.Workflows.Options;

/// <summary>
///     A workflow option that gates execution of a target's individual step (rather than its whole job)
///     on a platform-evaluated condition expression.
/// </summary>
/// <param name="Condition">
///     The condition expression to evaluate. May reference outputs of other targets via
///     <see cref="TargetOutputExpression" />, which implicitly adds those targets as dependencies.
/// </param>
/// <seealso cref="TargetCondition" />
[PublicAPI]
public sealed record TargetStepCondition(TextExpression Condition) : IBuildOption, IImplicitTargetDependencyOption
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
