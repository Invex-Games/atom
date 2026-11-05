namespace Invex.Atom.Workflows.Options;

/// <summary>
///     A workflow option that gates execution of a target's job on a platform-evaluated condition expression.
/// </summary>
/// <param name="Condition">
///     The condition expression to evaluate. May reference outputs of other targets via
///     <see cref="JobOutputRef" />. Those targets are not added as dependencies: declare them with
///     <c>DependsOn</c>, or workflow resolution logs a warning (the platform can only read outputs of jobs the
///     conditioned job needs).
/// </param>
/// <remarks>
///     Typically created via <c>BuildOptions.Target.RunIfWorkflowCondition(...)</c>. The expression is rendered
///     into the generated workflow file using the syntax of the targeted CI/CD platform.
/// </remarks>
[PublicAPI]
public sealed record TargetCondition(BoolExpr Condition) : IBuildOption, IImplicitTargetDependencyOption
{
    /// <summary>
    ///     Gets the names of targets whose outputs are referenced by <see cref="Condition" />.
    ///     Workflow resolution checks that each is a declared dependency of the conditioned target and logs a
    ///     warning if not.
    /// </summary>
    public IEnumerable<string> TargetNames =>
        Condition
            .DescendantsAndSelf()
            .OfType<JobOutputRef>()
            .Select(x => x.Job);
}
