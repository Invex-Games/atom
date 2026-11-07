namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Writes and checks the GitVersion configuration file of each monorepo product that sets
///     <see cref="Product.GitVersionConfig" />.
/// </summary>
/// <remarks>
///     <para>
///         Each product's file is the repository's root GitVersion configuration (the first of
///         <see cref="ProductValidator.RootGitVersionConfigFileNames" />) with its top-level <c>tag-prefix</c> and
///         <c>ignore</c> settings replaced: <c>tag-prefix</c> becomes the product's <see cref="Product.TagPrefix" />,
///         and <c>ignore.paths</c> becomes one regular expression that matches every path outside the product's paths,
///         its dependencies' paths and the global paths, so that GitVersion only counts commits that touch them.
///     </para>
///     <para>
///         The files are owned by Atom: <c>atom gen</c> (and any local run) rewrites them, and a headless run fails if
///         one is missing or has been edited.
///     </para>
/// </remarks>
internal sealed class ProductGitVersionConfigGenerator(
    IBuildDefinition buildDefinition,
    IRootedFileSystem fileSystem,
    ILogger<ProductGitVersionConfigGenerator> logger
)
{
    /// <summary>
    ///     Gets the problems with the build's product declarations.
    /// </summary>
    /// <returns>
    ///     One message per problem; empty if they are valid or no product sets <see cref="Product.GitVersionConfig" />.
    /// </returns>
    internal IReadOnlyList<string> GetProductErrors() =>
        buildDefinition is IProductsDefinition productsDefinition &&
        productsDefinition.Products.Any(x => x.GitVersionConfig is not null)
            ? ProductValidator.GetErrors(productsDefinition, fileSystem)
            : [];

    /// <summary>
    ///     Gets the GitVersion configuration files Atom manages, with their expected contents.
    /// </summary>
    /// <returns>
    ///     One entry per product that sets <see cref="Product.GitVersionConfig" />; empty if the build does not
    ///     implement <see cref="IProductsDefinition" />.
    /// </returns>
    /// <exception cref="AtomException">
    ///     Thrown if a product's configuration path is a root configuration file or is not below the Atom root
    ///     directory.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown if the product dependencies are invalid.</exception>
    internal IReadOnlyList<(Product Product, RootedPath Path, string Content)> GetExpectedConfigs()
    {
        if (buildDefinition is not IProductsDefinition productsDefinition)
            return [];

        var products = productsDefinition
            .Products
            .Where(x => x.GitVersionConfig is not null)
            .ToList();

        if (products.Count is 0)
            return [];

        var rootConfigPath = ProductValidator
            .RootGitVersionConfigFileNames
            .Select(x => fileSystem.AtomRootDirectory / x)
            .FirstOrDefault(x => fileSystem.File.Exists(x));

        var rootConfig = rootConfigPath is null
            ? null
            : RemoveTopLevelKeys(NormalizeContent(fileSystem.File.ReadAllText(rootConfigPath)),
                ["tag-prefix", "ignore"]);

        var rootConfigName = rootConfigPath is null
            ? null
            : fileSystem.Path.GetFileName(rootConfigPath.Path);

        // Every name GitVersion would find by itself, whether or not it exists yet: writing a product's settings
        // there would apply them to every build without a product. Compared ignoring case, because the Windows and
        // macOS file systems do.
        var rootConfigNames = ProductValidator
            .RootGitVersionConfigFileNames
            .Select(x => fileSystem.Path.GetFullPath(fileSystem.AtomRootDirectory / x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var repositoryPrefix = GetRepositoryPrefix() ?? string.Empty;
        var result = new List<(Product, RootedPath, string)>();

        foreach (var product in products)
        {
            var path = fileSystem.CreateRootedPath(fileSystem.Path.GetFullPath(product.GitVersionConfig!));

            if (AtomRootPath.GetRelativePathBelowRoot(fileSystem, path) is null)
                throw new AtomException(
                    $"The GitVersion configuration of product '{product.Name}' is '{product.GitVersionConfig}', which " +
                    $"is not below the Atom root directory '{fileSystem.AtomRootDirectory}'.");

            if (rootConfigNames.Contains(path.Path))
                throw new AtomException(
                    $"The GitVersion configuration of product '{product.Name}' is the root configuration file " +
                    $"'{fileSystem.Path.GetFileName(path.Path)}'. Give the product its own file, such as 'src/{product.Name}/GitVersion.yml'.");

            var ignoreRegex =
                ProductPathGlob.ToGitVersionIgnoreRegex(productsDefinition.GetProductPaths(product), repositoryPrefix);

            var builder = new StringBuilder();

            builder.Append(
                $"# Generated by Atom for the product '{product.Name}'. Do not edit: run 'atom gen' to update it.\n");

            builder.Append(rootConfigName is null
                ? "# Settings: GitVersion's defaults, with this product's tag prefix and ignored paths.\n"
                : $"# Settings: the root {rootConfigName}, with this product's tag prefix and ignored paths.\n");

            builder.Append('\n');

            if (rootConfig is { Length: > 0 })
            {
                builder.Append(rootConfig.TrimEnd('\n'));
                builder.Append("\n\n");
            }

            builder.Append($"tag-prefix: {YamlQuote(Regex.Escape(product.TagPrefix))}\n");
            builder.Append("ignore:\n");
            builder.Append("  paths:\n");
            builder.Append($"    - {YamlQuote(ignoreRegex)}\n");

            result.Add((product, path, builder.ToString()));
        }

        return result;
    }

    /// <summary>
    ///     Writes every product configuration whose contents differ from the expected contents.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task that completes when the files are written.</returns>
    internal async Task Generate(CancellationToken cancellationToken)
    {
        foreach (var (product, path, content) in GetExpectedConfigs())
        {
            if (await IsUpToDate(path, content, cancellationToken))
                continue;

            if (path.Parent is { } directory && !directory.DirectoryExists)
                fileSystem.Directory.CreateDirectory(directory);

            await fileSystem.File.WriteAllTextAsync(path, content.ReplaceLineEndings(), cancellationToken);

            logger.LogInformation("Wrote the GitVersion configuration of product {Product} to {Path}",
                product.Name,
                path);
        }
    }

    /// <summary>
    ///     Gets the product configurations that are missing or differ from the expected contents.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The paths of the outdated files, relative to the Atom root directory.</returns>
    /// <remarks>
    ///     Without a git repository around the Atom root (for example in a source export), the expected files cannot be
    ///     worked out, so none is reported; GitVersion cannot run there either.
    /// </remarks>
    internal async Task<IReadOnlyList<string>> GetOutdatedConfigs(CancellationToken cancellationToken)
    {
        var outdated = new List<string>();

        if (GetRepositoryPrefix() is null)
        {
            logger.LogDebug("No git repository was found, so the product GitVersion configurations are not checked");

            return outdated;
        }

        foreach (var (product, path, content) in GetExpectedConfigs())
        {
            if (await IsUpToDate(path, content, cancellationToken))
                continue;

            logger.LogDebug("The GitVersion configuration of product {Product} at {Path} is outdated. Expected:\n{Content}",
                product.Name,
                path,
                content);

            outdated.Add(fileSystem
                .Path
                .GetRelativePath(fileSystem.AtomRootDirectory, path)
                .Replace('\\', '/'));
        }

        return outdated;
    }

    /// <summary>
    ///     Gets the path of the Atom root directory relative to the git repository root (the nearest folder at or
    ///     above it that contains <c>.git</c>), with <c>/</c> separators; empty if they are the same, or
    ///     <see langword="null" /> if there is no repository.
    /// </summary>
    /// <remarks>
    ///     Atom paths are relative to the Atom root, but GitVersion matches <c>ignore.paths</c> against paths relative
    ///     to the repository root.
    /// </remarks>
    private string? GetRepositoryPrefix()
    {
        var atomRoot = fileSystem.AtomRootDirectory;

        for (var directory = atomRoot; directory is not null; directory = directory.Parent)
        {
            var gitPath = directory / ".git";

            // .git is a file in a worktree or submodule
            if (gitPath.DirectoryExists || gitPath.FileExists)
                return fileSystem.Path.GetRelativePath(directory, atomRoot) is var relative && relative is not "."
                    ? relative.Replace('\\', '/')
                    : string.Empty;
        }

        return null;
    }

    private async Task<bool> IsUpToDate(RootedPath path, string content, CancellationToken cancellationToken) =>
        path.FileExists &&
        NormalizeContent(await fileSystem.File.ReadAllTextAsync(path, cancellationToken)) == content;

    /// <summary>
    ///     Removes a leading byte order mark and normalises line endings to <c>\n</c>.
    /// </summary>
    internal static string NormalizeContent(string content) =>
        content
            .TrimStart('﻿')
            .ReplaceLineEndings("\n");

    /// <summary>
    ///     Removes top-level YAML keys, with their nested lines, from a YAML document.
    /// </summary>
    /// <param name="content">The YAML document, with <c>\n</c> line endings.</param>
    /// <param name="keys">The keys to remove.</param>
    /// <returns>The document without the keys.</returns>
    /// <remarks>
    ///     A key's block ends at the next line that starts in the first column and is not a comment: another key or a
    ///     document marker. Indented lines inside the block are removed with it. A comment in the first column (as
    ///     IDEs write when commenting out a line) does not end the block: it is removed if more of the block follows,
    ///     and kept with the next key otherwise. Blank lines are kept, and runs of them are collapsed to one.
    /// </remarks>
    internal static string RemoveTopLevelKeys(string content, IReadOnlyCollection<string> keys)
    {
        var lines = content.Split('\n');
        var kept = new List<string>(lines.Length);
        var pendingComments = new List<string>();
        var skipping = false;

        foreach (var line in lines)
        {
            var blank = line.Trim().Length is 0;

            if (!skipping)
            {
                if (line.Length > 0 && !char.IsWhiteSpace(line[0]) && keys.Any(key => IsKeyLine(line, key)))
                    skipping = true;
                else
                    kept.Add(line);

                continue;
            }

            // Blank lines are kept, so that removing a block doesn't join its neighbours
            if (blank)
            {
                kept.Add(line);

                continue;
            }

            if (line.StartsWith('#'))
            {
                pendingComments.Add(line);

                continue;
            }

            if (char.IsWhiteSpace(line[0]))
            {
                // Still inside the removed block, so the comments before this line were part of it
                pendingComments.Clear();

                continue;
            }

            // The block ends at this line, so the comments before it belong to what follows
            kept.AddRange(pendingComments);
            pendingComments.Clear();

            skipping = keys.Any(key => IsKeyLine(line, key));

            if (!skipping)
                kept.Add(line);
        }

        // Comments at the end of the document, after the removed block
        kept.AddRange(pendingComments);

        // Collapse the blank lines left behind where a block was removed
        var result = new StringBuilder();
        var previousBlank = false;

        foreach (var line in kept)
        {
            var blank = line.Trim().Length is 0;

            if (blank && previousBlank)
                continue;

            result.Append(line);
            result.Append('\n');
            previousBlank = blank;
        }

        return result
            .ToString()
            .Trim('\n');

        // The key may be quoted, such as "tag-prefix": v
        static bool IsKeyLine(string line, string key) =>
            IsKeyLineWith(line, key) || IsKeyLineWith(line, $"\"{key}\"") || IsKeyLineWith(line, $"'{key}'");

        static bool IsKeyLineWith(string line, string key) =>
            line.StartsWith(key, StringComparison.Ordinal) &&
            line
                .AsSpan(key.Length)
                .TrimStart(' ')
                .StartsWith(":", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Quotes a value as a single-quoted YAML scalar.
    /// </summary>
    private static string YamlQuote(string value) =>
        $"'{value.Replace("'", "''")}'";
}
