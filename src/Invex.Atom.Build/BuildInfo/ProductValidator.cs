namespace Invex.Atom.Build.BuildInfo;

/// <summary>
///     Checks the product declarations of an <see cref="IProductsDefinition" />.
/// </summary>
internal static partial class ProductValidator
{
    /// <summary>
    ///     Gets the problems with a build's product declarations.
    /// </summary>
    /// <param name="definition">The build.</param>
    /// <param name="fileSystem">
    ///     The file system, used to report paths that name an existing folder without a trailing <c>/</c> and to
    ///     check each <see cref="Product.GitVersionConfig" /> against the Atom root directory.
    /// </param>
    /// <returns>One message per problem; empty if the declarations are valid.</returns>
    public static IReadOnlyList<string> GetErrors(IProductsDefinition definition, IRootedFileSystem fileSystem)
    {
        var errors = new List<string>();
        var products = definition.Products;

        foreach (var product in products)
        {
            if (!NameRegex()
                    .IsMatch(product.Name))
                errors.Add(
                    $"Product name '{product.Name}' is not valid. Use letters, digits, '.', '-' and '_', starting with a letter or digit.");

            if (product.Paths.Count is 0)
                errors.Add($"Product '{product.Name}' has no paths.");

            errors.AddRange(product
                .Paths
                .Select(path => (Path: path, Error: ProductPathGlob.GetError(path)))
                .Where(x => x.Error is not null)
                .Select(x => $"Path '{x.Path}' of product '{product.Name}' is not supported: {x.Error}."));

            errors.AddRange(GetFolderErrors(product.Paths, fileSystem)
                .Select(x => $"Path '{x}' of product '{product.Name}' names a folder; add a trailing '/', such as '{x.Trim()}/', to include everything below it."));

            if (product.GitVersionConfig is { } config)
            {
                var relativeConfig = AtomRootPath.GetRelativePathBelowRoot(fileSystem, config);

                if (relativeConfig is null)
                    errors.Add(
                        $"The GitVersion configuration of product '{product.Name}' is '{config}', which is not below the Atom root directory '{fileSystem.AtomRootDirectory}'. Use a file below it, such as 'src/{product.Name}/GitVersion.yml'.");
                else if (RootGitVersionConfigFileNames.Contains(relativeConfig, StringComparer.OrdinalIgnoreCase))
                    errors.Add(
                        $"The GitVersion configuration of product '{product.Name}' is '{relativeConfig}', which GitVersion also reads for builds without a product. Give the product its own file, such as 'src/{product.Name}/GitVersion.yml'.");
            }

            errors.AddRange(product
                .DependsOn
                .Where(x => string.Equals(x, product.Name, StringComparison.OrdinalIgnoreCase))
                .Select(_ => $"Product '{product.Name}' depends on itself."));

            if (product.TagPrefix.Length is 0)
                errors.Add($"Product '{product.Name}' has an empty tag prefix.");
        }

        errors.AddRange(definition
            .GlobalPaths
            .Select(path => (Path: path, Error: ProductPathGlob.GetError(path)))
            .Where(x => x.Error is not null)
            .Select(x => $"Global path '{x.Path}' is not supported: {x.Error}."));

        errors.AddRange(GetFolderErrors(definition.GlobalPaths, fileSystem)
            .Select(x => $"Global path '{x}' names a folder; add a trailing '/', such as '{x.Trim()}/', to include everything below it."));

        errors.AddRange(products
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x => $"Products {string.Join(", ", x.Select(p => $"'{p.Name}'"))} share the name '{x.Key}'."));

        // A tag prefix that starts another product's prefix would match that product's tags too, in GitVersion, the
        // release tag helpers and the GitHub release guard (whose startsWith ignores case)
        for (var i = 0; i < products.Count; i++)
        for (var j = i + 1; j < products.Count; j++)
        {
            var (a, b) = (products[i], products[j]);

            if (a.TagPrefix.Length is 0 || b.TagPrefix.Length is 0)
                continue;

            if (string.Equals(a.TagPrefix, b.TagPrefix, StringComparison.OrdinalIgnoreCase))
                errors.Add($"Products '{a.Name}', '{b.Name}' share the tag prefix '{a.TagPrefix}'.");
            else if (b.TagPrefix.StartsWith(a.TagPrefix, StringComparison.OrdinalIgnoreCase))
                errors.Add(
                    $"The tag prefix '{a.TagPrefix}' of product '{a.Name}' starts the tag prefix '{b.TagPrefix}' of product '{b.Name}', so '{a.Name}' would match '{b.Name}''s release tags.");
            else if (a.TagPrefix.StartsWith(b.TagPrefix, StringComparison.OrdinalIgnoreCase))
                errors.Add(
                    $"The tag prefix '{b.TagPrefix}' of product '{b.Name}' starts the tag prefix '{a.TagPrefix}' of product '{a.Name}', so '{b.Name}' would match '{a.Name}''s release tags.");
        }

        errors.AddRange(products
            .Select(x => (Product: x,
                Config: x.GitVersionConfig is { } config
                    ? AtomRootPath.GetRelativePathBelowRoot(fileSystem, config)
                    : null))
            .Where(x => x.Config is not null)
            .GroupBy(x => x.Config!, x => x.Product, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .Select(x =>
                $"Products {string.Join(", ", x.Select(p => $"'{p.Name}'"))} share the GitVersion configuration '{x.Key}'."));

        // Unknown dependencies and cycles; a problem reached from several products is reported once
        foreach (var product in products)
            try
            {
                _ = definition.GetProductPaths(product, false);
            }
            catch (InvalidOperationException ex)
            {
                errors.Add(ex.Message);
            }

        return errors
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    ///     Gets the GitVersion configuration file names that GitVersion finds by itself in the repository root, in the
    ///     order it looks for them.
    /// </summary>
    public static IReadOnlyList<string> RootGitVersionConfigFileNames { get; } =
    [
        "GitVersion.yml", "GitVersion.yaml", ".GitVersion.yml", ".GitVersion.yaml",
    ];

    /// <summary>
    ///     Gets the literal paths (no wildcard, no trailing <c>/</c>) that name an existing folder below the Atom root.
    /// </summary>
    /// <remarks>
    ///     git and Azure DevOps match such a path as a folder, but GitHub path filters, the affected-product detection
    ///     and GitVersion match only the path itself, so the platforms would disagree about the product.
    /// </remarks>
    /// <remarks>
    ///     A git submodule or a symbolic link is one entry to git, so its literal path is right and is not reported.
    /// </remarks>
    private static IEnumerable<string> GetFolderErrors(IEnumerable<string> paths, IRootedFileSystem fileSystem) =>
        paths.Where(path =>
        {
            if (string.IsNullOrWhiteSpace(path) ||
                ProductPathGlob.GetError(path) is not null ||
                !ProductPathGlob.IsLiteral(path))
                return false;

            var directory = fileSystem.AtomRootDirectory / path.Trim();

            if (!directory.DirectoryExists)
                return false;

            var isSubmodule = (directory / ".git").PathExists;
            var isLink = fileSystem.DirectoryInfo.New(directory).LinkTarget is not null;

            return !isSubmodule && !isLink;
        });

    /// <summary>
    ///     Matches a valid product name.
    /// </summary>
    [GeneratedRegex(@"\A[A-Za-z0-9][A-Za-z0-9._-]*\z")]
    private static partial Regex NameRegex();
}
