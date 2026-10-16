namespace Invex.Atom.Workflows.Extensions;

/// <summary>
///     Extends <see cref="Tex" /> with references to Atom target outputs.
/// </summary>
[PublicAPI]
public static class AtomTexExtensions
{
    extension(Tex)
    {
        /// <summary>
        ///     References a parameter produced as an output by another target. The producing target is not added as a
        ///     dependency automatically: declare <paramref name="targetName" /> as a dependency of the consuming target,
        ///     or the output is unavailable (Atom logs a warning when a condition references an undeclared dependency).
        /// </summary>
        /// <typeparam name="T">The type of the build definition.</typeparam>
        /// <param name="buildDefinition">The build definition used to resolve the parameter's argument name.</param>
        /// <param name="targetName">The name of the target that produces the output.</param>
        /// <param name="paramName">The programmatic (C#) name of the parameter to reference.</param>
        /// <returns>A job-output reference; Atom names the producing step after its job.</returns>
        public static StringExpr ParamOutput<T>(T buildDefinition, string targetName, string paramName)
            where T : IBuildDefinition =>
            Tex.JobOutput(targetName,
                buildDefinition.ParamDefinitions.FirstOrDefault(p => p.Key == paramName).Value.ArgName,
                targetName);
    }
}
