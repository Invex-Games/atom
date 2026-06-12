namespace Invex.Atom.Workflows.Extensions;

/// <summary>
///     Extends the <c>TextExpressions</c> anchor class with factories for Atom target-output expressions.
/// </summary>
[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class AtomWorkflowsTextExpressionsExtensions
{
    /// <summary>
    ///     Provides factories for creating expressions that reference Atom target outputs.
    /// </summary>
    [PublicAPI]
    public sealed class AtomWorkflowTextExpressions
    {
        internal static AtomWorkflowTextExpressions Instance => field ??= new();

        /// <summary>
        ///     Creates an expression that references a parameter produced as an output by another target.
        /// </summary>
        /// <typeparam name="T">The type of the build definition.</typeparam>
        /// <param name="buildDefinition">The build definition used to resolve the parameter's argument name.</param>
        /// <param name="targetName">The name of the target that produces the output.</param>
        /// <param name="paramName">The programmatic (C#) name of the parameter to reference.</param>
        /// <returns>A <see cref="TargetOutputExpression" /> referencing the target's output.</returns>
        public TargetOutputExpression ParamOutput<T>(T buildDefinition, string targetName, string paramName)
            where T : IBuildDefinition =>
            new()
            {
                TargetName = targetName,
                OutputName = buildDefinition.ParamDefinitions.FirstOrDefault(p => p.Key == paramName)
                    .Value.ArgName,
            };
    }

    extension(TextExpressions)
    {
        /// <summary>
        ///     Gets factories for creating expressions that reference Atom target outputs.
        /// </summary>
        public static AtomWorkflowTextExpressions Target => AtomWorkflowTextExpressions.Instance;
    }
}
