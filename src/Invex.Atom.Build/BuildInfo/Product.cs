namespace Invex.Atom.Build.BuildInfo;

/// <summary>
///     A product in a monorepo: a named set of repository paths with its own version, release tags and, optionally,
///     its own pipelines.
/// </summary>
/// <param name="Name">
///     The product's name, such as <c>AppA</c>. It becomes the build name when the product is being built, and its
///     lowercase form (<see cref="Id" />) is used in the affected-products list and the default tag prefix. Use letters,
///     digits, <c>.</c>, <c>-</c> and <c>_</c>, starting with a letter or digit.
/// </param>
/// <remarks>
///     <para>
///         Products are declared in <see cref="IProductsDefinition.Products" />. A product with no pipelines of its own,
///         such as code shared by several products, is just a path group: list it in the other products'
///         <see cref="DependsOn" /> so that a change to it affects them too.
///     </para>
///     <para>
///         Paths are globs relative to the Atom root directory (normally the repository root), using <c>/</c> as
///         the separator: <c>**</c> as a whole
///         path segment matches any number of folders, and <c>*</c> matches any characters except <c>/</c>. A path
///         ending in <c>/</c> means everything below that folder, so <c>src/AppA/</c> is the same as
///         <c>src/AppA/**</c>; a path with no wildcard and no trailing <c>/</c> means one file. <c>?</c>, <c>+</c> and
///         other glob syntax are not supported, because GitHub, Azure DevOps and git disagree on them.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// IReadOnlyList&lt;Product&gt; IProductsDefinition.Products =>
/// [
///     new("Shared") { Paths = ["src/Shared/**"] },
///     new("AppA")
///     {
///         Paths = ["src/AppA/**"],
///         DependsOn = ["Shared"],
///         Directory = RootedFileSystem.AtomRootDirectory / "src/AppA",
///         GitVersionConfig = RootedFileSystem.AtomRootDirectory / "src/AppA/GitVersion.yml",
///     },
/// ];
///     </code>
///     <para>
///         <see cref="Directory" /> and <see cref="GitVersionConfig" /> are built from the Atom root directory, so declare
///         the products in a property getter rather than an initializer. An interface build uses the
///         <c>RootedFileSystem</c> property, as above; a build class uses its generated <c>FileSystem</c> property, such as
///         <c>FileSystem.AtomRootDirectory / "src/AppA"</c>.
///     </para>
/// </example>
[PublicAPI]
public sealed record Product(string Name)
{
    /// <summary>
    ///     Gets the product's identifier: its name in lowercase invariant form, such as <c>appa</c>.
    /// </summary>
    /// <remarks>
    ///     The identifier is what <see cref="IProductsDefinition.DetectAffectedProducts" /> writes to the
    ///     affected-products list, and it is the start of the default <see cref="TagPrefix" />.
    /// </remarks>
    public string Id => Name.ToLowerInvariant();

    /// <summary>
    ///     Gets the product's own paths, as globs relative to the Atom root directory (normally the repository root),
    ///     such as <c>src/AppA/**</c>.
    /// </summary>
    public IReadOnlyList<string> Paths { get; init; } = [];

    /// <summary>
    ///     Gets the names of the products this product depends on, such as <c>Shared</c>.
    /// </summary>
    /// <remarks>
    ///     A change to a dependency's paths affects this product, and the dependency's paths count towards this
    ///     product's version and release tag commit. Dependencies are followed transitively and matched by name,
    ///     ignoring case.
    /// </remarks>
    public IReadOnlyList<string> DependsOn { get; init; } = [];

    /// <summary>
    ///     Gets the product's folder, such as <c>RootedFileSystem.AtomRootDirectory / "src/AppA"</c>, or
    ///     <see langword="null" /> if it has none.
    /// </summary>
    /// <remarks>
    ///     When the product is being built, the default build version provider reads the version from this folder's
    ///     <c>Directory.Build.props</c> before the repository root's.
    /// </remarks>
    public RootedPath? Directory { get; init; }

    /// <summary>
    ///     Gets the product's GitVersion configuration file, such as
    ///     <c>RootedFileSystem.AtomRootDirectory / "src/AppA/GitVersion.yml"</c>, or <see langword="null" /> if the
    ///     product has none.
    /// </summary>
    /// <remarks>
    ///     When the product is being built, the GitVersion module uses this file unless <c>--gitversion-config</c> is
    ///     given, and <c>atom gen</c> writes it from the repository's root GitVersion configuration with this product's
    ///     <see cref="TagPrefix" /> and <c>ignore.paths</c>. Set it for every product that is built with GitVersion:
    ///     without it, GitVersion uses the root configuration, which counts every commit and reads the root tag prefix
    ///     rather than <see cref="TagPrefix" />. It must be below the Atom root directory, and it must not be one of the
    ///     files GitVersion finds by itself in the repository root, such as <c>GitVersion.yml</c>.
    /// </remarks>
    public RootedPath? GitVersionConfig { get; init; }

    /// <summary>
    ///     Gets the product's release tag prefix.
    /// </summary>
    /// <value>The prefix set at initialisation, or <see cref="Id" /> followed by <c>/v</c> (for example <c>appa/v</c>).</value>
    /// <remarks>
    ///     No product's prefix may equal or start another product's, ignoring case (for example <c>v</c> and
    ///     <c>vision/v</c>), or one product's tags would also match the other's.
    /// </remarks>
    public string TagPrefix
    {
        get => field ?? $"{Id}/v";
        init;
    }
}
