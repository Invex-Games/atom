namespace Invex.Atom.Build;

/// <summary>
///     Declares the products of a monorepo: named sets of repository paths that are versioned, tagged and built
///     independently.
/// </summary>
/// <remarks>
///     <para>
///         Implement this interface on the build and declare <see cref="Products" />. Then:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///                 the <see cref="DetectAffectedProducts" /> target works out which products a pull request affects
///                 and publishes them in the <see cref="AffectedProducts" /> variable;
///             </description>
///         </item>
///         <item>
///             <description>
///                 the <c>product</c> parameter (<see cref="ProductName" />) selects the product being built. When it
///                 is set, the build name defaults to the product's name, the default version provider reads the
///                 product folder's <c>Directory.Build.props</c> first, the GitVersion module uses the product's
///                 configuration, and the release tag helpers use the product's tag prefix;
///             </description>
///         </item>
///         <item>
///             <description>
///                 <c>ValidateBuild</c> checks the product declarations.
///             </description>
///         </item>
///     </list>
///     <para>
///         With no <c>product</c> parameter, everything behaves as it does in a build without products.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// [BuildDefinition]
/// [GenerateEntryPoint]
/// internal interface IBuild : IWorkflowBuildDefinition, IGitVersion, IProductsDefinition
/// {
///     IReadOnlyList&lt;Product&gt; IProductsDefinition.Products =>
///     [
///         new("Shared") { Paths = ["src/Shared/**"] },
///         new("AppA")
///         {
///             Paths = ["src/AppA/**"],
///             DependsOn = ["Shared"],
///             GitVersionConfig = RootedFileSystem.AtomRootDirectory / "src/AppA/GitVersion.yml",
///         },
///         new("AppB")
///         {
///             Paths = ["src/AppB/**"],
///             DependsOn = ["Shared"],
///             GitVersionConfig = RootedFileSystem.AtomRootDirectory / "src/AppB/GitVersion.yml",
///         },
///     ];
/// }
///     </code>
/// </example>
[PublicAPI]
public interface IProductsDefinition : IVariablesHelper, IReportsHelper
{
    /// <summary>
    ///     Gets the paths that affect every product by default: <c>global.json</c>, the root <c>Directory.Build.*</c>
    ///     and <c>Directory.Packages.*</c> files, the <c>_atom</c> build project and the generated workflow folders.
    /// </summary>
    static IReadOnlyList<string> DefaultGlobalPaths { get; } =
    [
        "global.json",
        "Directory.Build.*",
        "Directory.Packages.*",
        "_atom/**",
        ".github/workflows/**",
        ".devops/workflows/**",
    ];

    /// <summary>
    ///     Gets the products in the repository.
    /// </summary>
    /// <remarks>
    ///     Product names must be unique, ignoring case. See <see cref="Product" /> for the path syntax.
    /// </remarks>
    IReadOnlyList<Product> Products { get; }

    /// <summary>
    ///     Gets the paths that affect every product, as globs relative to the Atom root directory (normally the
    ///     repository root).
    /// </summary>
    /// <value>By default, <see cref="DefaultGlobalPaths" />.</value>
    /// <remarks>
    ///     A change to a global path affects every product, the paths are part of every product's workflow path
    ///     filters, and GitVersion counts commits that touch them towards every product's version. Override this to add
    ///     paths, for example <c>[.. IProductsDefinition.DefaultGlobalPaths, "NuGet.config"]</c>, or to remove some.
    /// </remarks>
    IReadOnlyList<string> GlobalPaths => DefaultGlobalPaths;

    /// <summary>
    ///     Gets the name of the product being built, or empty (or <see langword="null" />) if none is selected.
    /// </summary>
    /// <remarks>
    ///     Set it with <c>--product AppA</c>, or in a workflow with a parameter injection, as
    ///     <c>WorkflowDefinition.ForProduct</c> does. The name is matched to <see cref="Products" /> ignoring case. Like
    ///     any parameter, it can also come from an environment variable named <c>ProductName</c>, <c>product</c> or
    ///     <c>PRODUCT</c> (on Windows, in any case), so check that the CI environment does not set one for another
    ///     purpose. The empty default lets an interactive run accept an empty answer at the prompt, which selects no
    ///     product.
    /// </remarks>
    [ParamDefinition("product", "The monorepo product to build, such as AppA (default: none).")]
    string? ProductName => GetParam(() => ProductName, string.Empty);

    /// <summary>
    ///     Gets the products the changes affect, as written by <see cref="DetectAffectedProducts" />.
    /// </summary>
    /// <value>
    ///     A comma-wrapped list of product identifiers (<see cref="Product.Id" />), such as <c>,appa,shared,</c>, or
    ///     <c>,</c> when no product is affected. The wrapping commas let a condition match <c>,appa,</c> without also
    ///     matching a product called <c>appa2</c>.
    /// </value>
    [ParamDefinition("affected-products",
        "The products the changes affect, as a comma-wrapped list of product ids such as ,appa,shared,")]
    string? AffectedProducts => GetParam(() => AffectedProducts);

    /// <summary>
    ///     Gets the product being built, or <see langword="null" /> if <see cref="ProductName" /> is not set.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if <see cref="ProductName" /> names a product that is not in <see cref="Products" />.
    /// </exception>
    Product? CurrentProduct =>
        ProductName is { } name && !string.IsNullOrWhiteSpace(name)
            ? GetProduct(name.Trim())
            : null;

    /// <summary>
    ///     Gets the base branch of the pull request being built, or <see langword="null" /> if the run is not for a
    ///     pull request.
    /// </summary>
    /// <value>
    ///     The branch name from <c>GITHUB_BASE_REF</c> (GitHub Actions), or from
    ///     <c>SYSTEM_PULLREQUEST_TARGETBRANCHNAME</c> or <c>SYSTEM_PULLREQUEST_TARGETBRANCH</c> (Azure Pipelines), with
    ///     any <c>refs/heads/</c> prefix removed.
    /// </value>
    /// <remarks>
    ///     <see cref="DetectAffectedProducts" /> compares <c>HEAD</c> with <c>origin/&lt;base&gt;</c>. Override this for
    ///     another CI system.
    /// </remarks>
    string? AffectedProductsBaseRef
    {
        get
        {
            foreach (var name in (string[])
                     ["GITHUB_BASE_REF", "SYSTEM_PULLREQUEST_TARGETBRANCHNAME", "SYSTEM_PULLREQUEST_TARGETBRANCH"])
            {
                var value = Environment
                    .GetEnvironmentVariable(name)
                    ?.Trim();

                if (string.IsNullOrEmpty(value))
                    continue;

                return value.StartsWith("refs/heads/", StringComparison.Ordinal)
                    ? value["refs/heads/".Length..]
                    : value;
            }

            return null;
        }
    }

    /// <summary>
    ///     Gets the target that works out which products the changes affect.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         For a pull request (see <see cref="AffectedProductsBaseRef" />), it lists the files changed between
    ///         <c>origin/&lt;base&gt;</c> and <c>HEAD</c> with <c>git diff --name-only --no-renames --relative</c>, run
    ///         in the Atom root directory. A product is
    ///         affected if a changed file matches its paths, the paths of a product it depends on, or a global path.
    ///         Renamed files count on both sides, so a file moved out of a product still affects it.
    ///     </para>
    ///     <para>
    ///         For any other run (a push, a manual run or a local run), or if git fails (for example because the base
    ///         branch was not fetched), every product is affected.
    ///     </para>
    ///     <para>
    ///         The result is written to the <see cref="AffectedProducts" /> variable. A product job that is conditioned
    ///         on it must depend on this target directly, or the job has no access to the variable.
    ///     </para>
    /// </remarks>
    Target DetectAffectedProducts =>
        t => t
            .DescribedAs("Detects which monorepo products the changes affect.")
            .ProducesVariable(nameof(AffectedProducts))
            .Executes(async cancellationToken =>
            {
                var products = Products;
                IReadOnlyList<Product> affected;
                var baseRef = AffectedProductsBaseRef;

                if (baseRef is null)
                {
                    Logger.LogInformation("Not a pull request build, so every product is affected");
                    affected = products;
                }
                else
                {
                    // --no-renames lists both sides of a move, --relative makes the paths relative to the Atom root
                    // (where git runs), and -z writes raw, NUL-separated names, so git doesn't quote unusual file names.
                    // The process runner reads output line by line, so also split on line breaks.
                    var diff = await ProcessRunner.RunAsync(new("git",
                            ProcessArguments.Join([
                                "diff", "--name-only", "--no-renames", "--relative", "-z", $"origin/{baseRef}...HEAD",
                            ]))
                        {
                            WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                            AllowFailedResult = true,
                            InvocationLogLevel = LogLevel.Debug,
                            OutputLogLevel = LogLevel.Debug,
                        },
                        cancellationToken);

                    if (diff.ExitCode is not 0)
                    {
                        Logger.LogWarning(
                            "Could not list the files changed since origin/{BaseRef} (git exited with code {ExitCode}), so every product is affected. Check that the base branch is fetched",
                            baseRef,
                            diff.ExitCode);

                        affected = products;
                    }
                    else
                    {
                        var changedFiles = diff.Output.Split(['\0', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

                        Logger.LogDebug("Files changed since origin/{BaseRef}: {ChangedFiles}", baseRef, changedFiles);

                        affected = GetAffectedProducts(changedFiles);
                    }
                }

                var value = affected.Count is 0
                    ? ","
                    : $",{string.Join(',', affected.Select(x => x.Id))},";

                await WriteVariable(nameof(AffectedProducts), value, cancellationToken);

                Logger.LogInformation("Affected products: {AffectedProducts}", value);

                AddReportData(affected.Count is 0
                    ? new TextReportData("No products are affected.")
                    {
                        Title = "Affected products",
                    }
                    : new ListReportData(affected
                        .Select(x => x.Name)
                        .ToList())
                    {
                        Title = "Affected products",
                    });
            });

    /// <summary>
    ///     Gets a product by name, ignoring case.
    /// </summary>
    /// <param name="name">The product's name.</param>
    /// <returns>The product.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no product has that name.</exception>
    Product GetProduct(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var products = Products;

        return products.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)) ??
               throw new InvalidOperationException(products.Count is 0
                   ? $"Unknown product '{name}': the build declares no products."
                   : $"Unknown product '{name}'. Known products: {string.Join(", ", products.Select(x => x.Name))}.");
    }

    /// <summary>
    ///     Gets every path that belongs to a product: its own paths, the paths of the products it depends on
    ///     (transitively) and, optionally, <see cref="GlobalPaths" />.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="includeGlobalPaths">Whether to include <see cref="GlobalPaths" />.</param>
    /// <returns>
    ///     The paths, without duplicates, with a trailing <c>/</c> expanded to <c>/**</c> (for example
    ///     <c>src/AppA/**</c>).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="product" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if a product depends on an unknown product, or the dependencies form a cycle.
    /// </exception>
    IReadOnlyList<string> GetProductPaths(Product product, bool includeGlobalPaths = true)
    {
        ArgumentNullException.ThrowIfNull(product);

        var products = Products;
        var paths = new List<string>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var chain = new List<string>();

        Visit(product);

        if (includeGlobalPaths)
            paths.AddRange(GlobalPaths.Select(ProductPathGlob.Normalize));

        return paths
            .Distinct(StringComparer.Ordinal)
            .ToList();

        void Visit(Product current)
        {
            if (chain.Contains(current.Name, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Product dependencies form a cycle: {string.Join(" -> ", chain)} -> {current.Name}.");

            if (!visited.Add(current.Name))
                return;

            chain.Add(current.Name);
            paths.AddRange(current.Paths.Select(ProductPathGlob.Normalize));

            foreach (var dependencyName in current.DependsOn)
            {
                var dependency =
                    products.FirstOrDefault(x =>
                        string.Equals(x.Name, dependencyName, StringComparison.OrdinalIgnoreCase)) ??
                    throw new InvalidOperationException(
                        $"Product '{current.Name}' depends on unknown product '{dependencyName}'.");

                Visit(dependency);
            }

            chain.RemoveAt(chain.Count - 1);
        }
    }

    /// <summary>
    ///     Gets the products that a set of changed files affects.
    /// </summary>
    /// <param name="changedFiles">The changed files, as paths relative to the Atom root directory.</param>
    /// <returns>
    ///     The affected products, in the order of <see cref="Products" />: every product if a file matches a global
    ///     path, otherwise each product whose own or dependency paths a file matches.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="changedFiles" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if a product depends on an unknown product, or the dependencies form a cycle.
    /// </exception>
    IReadOnlyList<Product> GetAffectedProducts(IEnumerable<string> changedFiles)
    {
        ArgumentNullException.ThrowIfNull(changedFiles);

        var files = changedFiles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var products = Products;

        if (files.Count is 0)
            return [];

        var globalPaths = GlobalPaths
            .Select(ProductPathGlob.Normalize)
            .ToList();

        if (files.Any(file => globalPaths.Any(glob => ProductPathGlob.IsMatch(glob, file))))
            return products;

        return products
            .Where(product =>
            {
                var paths = GetProductPaths(product, false);

                return files.Any(file => paths.Any(glob => ProductPathGlob.IsMatch(glob, file)));
            })
            .ToList();
    }
}
