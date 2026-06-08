namespace Invex.Atom.Workflows.Extensions;

[PublicAPI]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public static class AtomWorkflowsTextExpressionsExtensions
{
    [PublicAPI]
    public sealed class AtomWorkflowTextExpressions
    {
        internal static AtomWorkflowTextExpressions Instance => field ??= new();

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
        public static AtomWorkflowTextExpressions Target => AtomWorkflowTextExpressions.Instance;
    }
}
