namespace Invex.Atom.Build.BuildInfo;

/// <summary>
///     Gives services access to the monorepo product being built, if any.
/// </summary>
/// <remarks>
///     The service is always registered. In a build that does not implement <see cref="IProductsDefinition" />, or
///     when no <c>product</c> parameter is given, <see cref="Current" /> is <see langword="null" /> and everything
///     behaves as it does without products. Build code can read <see cref="IProductsDefinition.CurrentProduct" />
///     directly instead.
/// </remarks>
[PublicAPI]
public interface IProductContext
{
    /// <summary>
    ///     Gets the product being built, or <see langword="null" /> if none is selected.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the <c>product</c> parameter names a product the build does not declare.
    /// </exception>
    Product? Current { get; }
}

/// <summary>
///     The default <see cref="IProductContext" />, which reads the product from the build definition.
/// </summary>
/// <param name="buildDefinition">The build definition.</param>
internal sealed class ProductContext(IBuildDefinition buildDefinition) : IProductContext
{
    /// <inheritdoc />
    public Product? Current =>
        buildDefinition is IProductsDefinition products
            ? products.CurrentProduct
            : null;
}
