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
    ///     The file system, used to report paths that name an existing folder without a trailing <c>/</c>; or
    ///     <see langword="null" /> to skip that check.
    /// </param>
    /// <returns>One message per problem; empty if the declarations are valid.</returns>
    public static IReadOnlyList<string> GetErrors(IProductsDefinition definition, IRootedFileSystem? fileSystem = null)
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

            if (product.GitVersionConfig is { } config &&
                RootGitVersionConfigFileNames.Contains(NormalizeRelativePath(config), StringComparer.OrdinalIgnoreCase))
                errors.Add(
                    $"The GitVersion configuration of product '{product.Name}' is '{config.Trim()}', which GitVersion also reads for builds without a product. Give the product its own file, such as 'src/{product.Name}/GitVersion.yml'.");

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
            .Where(x => !string.IsNullOrWhiteSpace(x.GitVersionConfig))
            .GroupBy(x => NormalizeRelativePath(x.GitVersionConfig!), StringComparer.OrdinalIgnoreCase)
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
    ///     The GitVersion configuration file names that GitVersion finds by itself in the repository root.
    /// </summary>
    private static readonly string[] RootGitVersionConfigFileNames =
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
    private static IEnumerable<string> GetFolderErrors(IEnumerable<string> paths, IRootedFileSystem? fileSystem) =>
        fileSystem is null
            ? []
            : paths.Where(path =>
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
    ///     Normalises a relative path for comparison: trims it, uses <c>/</c> as the separator and resolves <c>.</c>
    ///     and <c>..</c> segments.
    /// </summary>
    private static string NormalizeRelativePath(string path)
    {
        var segments = new List<string>();

        foreach (var segment in path
                     .Trim()
                     .Replace('\\', '/')
                     .Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment is ".")
                continue;

            if (segment is ".." && segments.Count > 0 && segments[^1] is not "..")
                segments.RemoveAt(segments.Count - 1);
            else
                segments.Add(segment);
        }

        return string.Join('/', segments);
    }

    /// <summary>
    ///     Matches a valid product name.
    /// </summary>
    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]*$")]
    private static partial Regex NameRegex();
}
