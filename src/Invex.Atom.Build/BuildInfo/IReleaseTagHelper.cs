namespace Invex.Atom.Build.BuildInfo;

/// <summary>
///     Provides helper methods for creating release tags, such as finding the commit a release tag should be put on.
/// </summary>
/// <remarks>
///     Use <see cref="ReleaseTag" /> to format and parse the tag names themselves.
/// </remarks>
/// <example>
///     <code>
/// // Tag AppA on the latest commit that touched its paths, so GitVersion with ignore.paths can see the tag.
/// var commit = await GetReleaseTagCommit(["src/AppA/", "src/Shared/"], cancellationToken);
/// var tag = ReleaseTag.For(BuildVersion, "appa/v");
///     </code>
/// </example>
/// <seealso cref="ReleaseTag" />
[PublicAPI]
public interface IReleaseTagHelper : IBuildAccessor
{
    /// <summary>
    ///     Gets the commit that a release tag should be put on: the latest commit that touched any of
    ///     <paramref name="paths" />, or <c>HEAD</c> if no paths are given.
    /// </summary>
    /// <param name="paths">
    ///     Git pathspecs relative to the Atom root directory (<c>AtomRootDirectory</c>), such as
    ///     <c>src/AppA/</c>. Each one is passed to git as a single argument (quoted if it contains spaces). An empty
    ///     collection means the whole repository, so the result is the <c>HEAD</c> commit.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The full hash of the commit to tag.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="paths" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown if any path is <see langword="null" />, empty or whitespace.</exception>
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
    ///         This runs <c>git log -1 --format=%H -- &lt;paths&gt;</c> in
    ///         the Atom root directory (<c>AtomRootDirectory</c>). A product's release tag must be on the latest
    ///         commit that touched the product's paths: GitVersion ignores a tag on a commit that its
    ///         <c>ignore.paths</c> setting excludes, so a tag on a commit that touched only another product would not be
    ///         seen.
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
    async Task<string> GetReleaseTagCommit(IEnumerable<string> paths, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var pathList = paths.ToList();

        if (pathList.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Paths must not contain null, empty or whitespace entries.", nameof(paths));

        if (pathList.Count > 0)
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
                    $"touching '{string.Join("', '", pathList)}' cannot be determined reliably. " +
                    "Fetch the full history (for example, fetch-depth: 0 for actions/checkout).");
        }

        string[] args = pathList.Count > 0
            ? ["log", "-1", "--format=%H", "--", ..pathList]
            : ["log", "-1", "--format=%H"];

        var logResult = await ProcessRunner.RunAsync(new("git", ProcessArguments.Join(args))
            {
                WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                InvocationLogLevel = LogLevel.Debug,
                OutputLogLevel = LogLevel.Debug,
            },
            cancellationToken);

        var commit = logResult.Output.Trim();

        if (commit.Length is 0)
            throw new StepFailedException(pathList.Count > 0
                ? $"Cannot find the commit to tag: git found no commit touching '{string.Join("', '", pathList)}'. " +
                  "Check that the paths are correct and committed, and that the full history is fetched " +
                  "(for example, fetch-depth: 0 for actions/checkout)."
                : "Cannot find the commit to tag: git found no HEAD commit. " +
                  "Check that the repository has commits and that the full history is fetched " +
                  "(for example, fetch-depth: 0 for actions/checkout).");

        Logger.LogDebug("Release tag commit for paths {Paths} is {Commit}", pathList, commit);

        return commit;
    }
}
