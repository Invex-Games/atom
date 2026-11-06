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

        /// <summary>
        ///     Checks whether <see cref="IProductsDefinition.DetectAffectedProducts" /> found a monorepo product affected.
        /// </summary>
        /// <param name="productName">The product's name; it is compared by its lowercase identifier.</param>
        /// <returns>
        ///     A condition such as <c>contains(needs.DetectAffectedProducts.outputs.affected-products, ',appa,')</c>.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="productName" /> is null, empty or whitespace.</exception>
        /// <remarks>
        ///     Like <see cref="ParamOutput{T}" />, this does not add a dependency: the job that uses the condition must
        ///     depend directly on <c>DetectAffectedProducts</c>. On Azure DevOps, combine it with <c>Tex.Success()</c>,
        ///     because a custom condition replaces the default success check.
        /// </remarks>
        public static BoolExpr ProductAffected(string productName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(productName);

            return Tex
                .JobOutput(nameof(IProductsDefinition.DetectAffectedProducts),
                    AffectedProductsArgName,
                    nameof(IProductsDefinition.DetectAffectedProducts))
                .Contains($",{new Product(productName.Trim()).Id},");
        }
    }

    /// <summary>
    ///     The argument name of <see cref="IProductsDefinition.AffectedProducts" />.
    /// </summary>
    private const string AffectedProductsArgName = "affected-products";
}
