namespace Invex.Atom.Build.BuildInfo;

/// <summary>
///     Provides helper methods for release tags: formatting and parsing tag names with the build's release tag
///     prefix, and finding the commit a release tag should be put on.
/// </summary>
/// <remarks>
///     The build's release tag prefix is <see cref="ReleaseTagPrefix" />: the current monorepo product's
///     <see cref="Product.TagPrefix" /> when one is being built (see <see cref="IProductsDefinition.CurrentProduct" />),
///     otherwise <see cref="ReleaseTag.DefaultPrefix" /> (<c>v</c>). To format or parse a tag with another prefix, use
///     <see cref="ReleaseTag" />, which takes the prefix explicitly.
/// </remarks>
/// <example>
///     <code>
/// var tag = ReleaseTagFor(BuildVersion); // "v1.2.0", or "appa/v1.2.0" when building the product AppA
///
/// // Tag a product on the latest commit that touched its paths, so GitVersion with ignore.paths can see the tag.
/// var commit = await GetReleaseTagCommit(cancellationToken); // the current product's paths, with the global paths
///
/// var appB = GetProduct("AppB"); // another product's paths, with the global paths
/// var appBCommit = await GetReleaseTagCommit(appB, cancellationToken);
/// var appBTag = ReleaseTag.For(BuildVersion, appB.TagPrefix); // "appb/v1.2.0"
///
/// var docsCommit = await GetReleaseTagCommit( // only these files and folders
///     [RootedFileSystem.AtomRootDirectory / "docs", RootedFileSystem.AtomRootDirectory / "README.md"],
///     cancellationToken);
///     </code>
/// </example>
/// <seealso cref="ReleaseTag" />
[PublicAPI]
public interface IReleaseTagHelper : IBuildAccessor
{
    /// <summary>
    ///     Gets the build's release tag prefix.
    /// </summary>
    /// <value>
    ///     The current monorepo product's <see cref="Product.TagPrefix" /> (for example <c>appa/v</c>) when the build
    ///     implements <see cref="IProductsDefinition" /> and a product is selected; otherwise
    ///     <see cref="ReleaseTag.DefaultPrefix" /> (<c>v</c>).
    /// </value>
    /// <exception cref="InvalidOperationException">Thrown if the <c>product</c> parameter names an unknown product.</exception>
    string ReleaseTagPrefix =>
        this is IProductsDefinition { CurrentProduct: { } product }
            ? product.TagPrefix
            : ReleaseTag.DefaultPrefix;

    /// <summary>
    ///     Gets a glob pattern that matches the build's release tags, for use with <c>git tag --list &lt;pattern&gt;</c>.
    /// </summary>
    /// <value>The build's release tag prefix followed by <c>*</c>, for example <c>v*</c>.</value>
    /// <remarks>
    ///     The pattern is broader than a release tag (<c>v*</c> also matches <c>vnext</c>), so filter the listed tags
    ///     with <see cref="TryParseReleaseTag" />. See <see cref="ReleaseTag.Glob" />.
    /// </remarks>
    string ReleaseTagGlob => ReleaseTag.Glob(ReleaseTagPrefix);

    /// <summary>
    ///     Gets the release tag for a version, using the build's release tag prefix.
    /// </summary>
    /// <param name="version">The version to tag.</param>
    /// <returns>
    ///     The build's release tag prefix followed by the full version string, including any pre-release and build
    ///     metadata components, for example <c>v1.2.0</c> or <c>v1.2.0-rc.1</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="version" /> is <see langword="null" />.</exception>
    /// <remarks>See <see cref="ReleaseTag.For" />.</remarks>
    string ReleaseTagFor(SemVer version) =>
        ReleaseTag.For(version, ReleaseTagPrefix);

    /// <summary>
    ///     Tries to parse a release tag that uses the build's release tag prefix.
    /// </summary>
    /// <param name="tag">The tag to parse, such as <c>v1.2.0</c>. May be <see langword="null" />.</param>
    /// <param name="version">
    ///     When this method returns <see langword="true" />, the version that follows the prefix; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="tag" /> starts with the build's release tag prefix (ordinal,
    ///     case-sensitive) and the rest of the tag is a valid semantic version; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>See <see cref="ReleaseTag.TryParse" />.</remarks>
    bool TryParseReleaseTag([NotNullWhen(true)] string? tag, [NotNullWhen(true)] out SemVer? version) =>
        ReleaseTag.TryParse(tag, ReleaseTagPrefix, out version);

    /// <summary>
    ///     Gets the commit that the current product's release tag should be put on: the latest commit that touched the
    ///     product's paths, or <c>HEAD</c> if no product is being built.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The full hash of the commit to tag.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the <c>product</c> parameter names an unknown product, or the product dependencies are invalid.
    /// </exception>
    /// <exception cref="StepFailedException">
    ///     Thrown if a product is being built and the repository is a shallow clone, or if git finds no commit.
    /// </exception>
    /// <exception cref="ProcessException">Thrown if git exits with a non-zero code.</exception>
    /// <remarks>
    ///     When a product is being built, this is <see cref="GetReleaseTagCommit(Product, CancellationToken)" /> for
    ///     <see cref="IProductsDefinition.CurrentProduct" />.
    /// </remarks>
    Task<string> GetReleaseTagCommit(CancellationToken cancellationToken = default) =>
        this is IProductsDefinition { CurrentProduct: { } product }
            ? GetReleaseTagCommit(product, cancellationToken)
            : GetReleaseTagCommitForPathspecs([], cancellationToken);

    /// <summary>
    ///     Gets the commit that a product's release tag should be put on: the latest commit that touched the
    ///     product's paths.
    /// </summary>
    /// <param name="product">
    ///     The product, such as <c>GetProduct("AppB")</c>. It need not be the product being built.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The full hash of the commit to tag.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="product" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the build does not implement <see cref="IProductsDefinition" />, or the product dependencies are
    ///     invalid.
    /// </exception>
    /// <exception cref="StepFailedException">
    ///     Thrown if the repository is a shallow clone, or if git finds no commit.
    /// </exception>
    /// <exception cref="ProcessException">Thrown if git exits with a non-zero code.</exception>
    /// <remarks>
    ///     <para>
    ///         The paths are the ones GitVersion counts for the product: its own paths, the paths of the products it
    ///         depends on and the global paths (see <see cref="IProductsDefinition.GetProductPaths" />). They are
    ///         matched as globs, as in <see cref="Product.Paths" />, by passing them to git as <c>:(glob)</c> pathspecs.
    ///     </para>
    ///     <para>
    ///         Like <see cref="GetReleaseTagCommit(IEnumerable{RootedPath}, CancellationToken)" />, this needs the full
    ///         history and compares each commit with its first parent only.
    ///     </para>
    /// </remarks>
    async Task<string> GetReleaseTagCommit(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        return this is IProductsDefinition products
            ? await GetReleaseTagCommitForPathspecs(products
                    .GetProductPaths(product)
                    .Select(ProductPathGlob.ToGitPathspec)
                    .ToList(),
                cancellationToken)
            : throw new InvalidOperationException(
                $"GetReleaseTagCommit(Product) requires a build that implements {nameof(IProductsDefinition)}.");
    }

    /// <summary>
    ///     Gets the commit that a release tag should be put on: the latest commit that touched any of
    ///     <paramref name="paths" />, or <c>HEAD</c> if no paths are given.
    /// </summary>
    /// <param name="paths">
    ///     Files or folders at or below the Atom root directory, such as
    ///     <c>RootedFileSystem.AtomRootDirectory / "src/AppA"</c>. A folder includes everything below it, so the Atom
    ///     root directory itself means every file below it, which differs from the whole repository when the Atom root
    ///     is a subfolder of the repository. The paths are
    ///     matched literally, with no glob syntax; to match a product's globs, use
    ///     <see cref="GetReleaseTagCommit(Product, CancellationToken)" />. An empty collection means the whole
    ///     repository, so the result is the <c>HEAD</c> commit.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The full hash of the commit to tag.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="paths" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown if any path is <see langword="null" />, or is not at or below the Atom root directory.
    /// </exception>
    /// <exception cref="StepFailedException">
    ///     Thrown if paths are given and the repository is a shallow clone, or if git finds no commit, for example
    ///     because the paths were never committed.
    /// </exception>
    /// <exception cref="ProcessException">
    ///     Thrown if git exits with a non-zero code, for example when the Atom root directory is not in a git
    ///     repository or the repository has no commits yet.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         This runs <c>git log -1 --first-parent --format=%H -- &lt;paths&gt;</c> in
    ///         the Atom root directory (<c>AtomRootDirectory</c>), passing each path relative to it as a
    ///         <c>:(literal)</c> pathspec. A product's release tag must be on the latest
    ///         commit that touched the product's paths: GitVersion ignores a tag on a commit that its
    ///         <c>ignore.paths</c> setting excludes, so a tag on a commit that touched only another product would not be
    ///         seen. Like <c>ignore.paths</c>, it compares each commit with its first parent, so in a repository that
    ///         merges with merge commits it finds the merge commit, not the merged commit behind it.
    ///     </para>
    ///     <para>
    ///         When paths are given, the full history is needed. In a shallow clone the oldest fetched commit appears to
    ///         touch every file, so the answer could be silently wrong; this method fails instead. In CI, check out with
    ///         full history, for example <c>fetch-depth: 0</c> for <c>actions/checkout</c>.
    ///     </para>
    ///     <para>
    ///         The result is not cached; each call runs git.
    ///     </para>
    /// </remarks>
    async Task<string> GetReleaseTagCommit(IEnumerable<RootedPath> paths, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var pathspecs = new List<string>();

        foreach (var path in paths)
        {
            if (path is null)
                throw new ArgumentException("Paths must not contain null entries.", nameof(paths));

            var relativePath = AtomRootPath.GetRelativePathAtOrBelowRoot(RootedFileSystem, path) ??
                               throw new ArgumentException(
                                   $"Path '{path}' is not at or below the Atom root directory '{RootedFileSystem.AtomRootDirectory}'.",
                                   nameof(paths));

            pathspecs.Add($":(literal){relativePath}");
        }

        return await GetReleaseTagCommitForPathspecs(pathspecs, cancellationToken);
    }

    /// <summary>
    ///     Runs git to find the latest commit that touched any of <paramref name="pathspecs" />, or the <c>HEAD</c>
    ///     commit if there are none.
    /// </summary>
    /// <param name="pathspecs">Git pathspecs relative to the Atom root directory, each passed as one argument.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The full hash of the commit.</returns>
    private async Task<string> GetReleaseTagCommitForPathspecs(
        IReadOnlyList<string> pathspecs,
        CancellationToken cancellationToken)
    {
        if (pathspecs.Count > 0)
        {
            var shallowResult = await ProcessRunner.RunAsync(new("git", ["rev-parse", "--is-shallow-repository"])
                {
                    WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                    InvocationLogLevel = LogLevel.Debug,
                    OutputLogLevel = LogLevel.Debug,
                },
                cancellationToken);

            if (shallowResult.Output.Trim() is "true")
                throw new StepFailedException(
                    "Cannot find the commit to tag because the repository is a shallow clone, so the latest commit " +
                    $"touching '{string.Join("', '", pathspecs)}' cannot be determined reliably. " +
                    "Fetch the full history (for example, fetch-depth: 0 for actions/checkout).");
        }

        // --first-parent compares each commit with its first parent only, as GitVersion's ignore.paths does, so a
        // merge commit that brought in a change to the paths is found rather than the merged commit behind it.
        // --no-show-signature keeps the output to the hash alone when the user's Git config sets log.showSignature
        string[] args = pathspecs.Count > 0
            ? ["log", "-1", "--first-parent", "--no-show-signature", "--format=%H", "--", ..pathspecs]
            : ["log", "-1", "--no-show-signature", "--format=%H"];

        var logResult = await ProcessRunner.RunAsync(new("git", ProcessArguments.Join(args))
            {
                WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                InvocationLogLevel = LogLevel.Debug,
                OutputLogLevel = LogLevel.Debug,
            },
            cancellationToken);

        var commit = logResult.Output.Trim();

        if (commit.Length is 0)
            throw new StepFailedException(pathspecs.Count > 0
                ? $"Cannot find the commit to tag: git found no commit touching '{string.Join("', '", pathspecs)}'. " +
                  "Check that the paths are correct and committed, and that the full history is fetched " +
                  "(for example, fetch-depth: 0 for actions/checkout)."
                : "Cannot find the commit to tag: git found no HEAD commit. " +
                  "Check that the repository has commits and that the full history is fetched " +
                  "(for example, fetch-depth: 0 for actions/checkout).");

        Logger.LogDebug("Release tag commit for paths {Paths} is {Commit}", pathspecs, commit);

        return commit;
    }
}
