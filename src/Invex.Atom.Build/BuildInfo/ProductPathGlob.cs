namespace Invex.Atom.Build.BuildInfo;

/// <summary>
///     Converts and matches the path globs used by <see cref="Product.Paths" /> and
///     <see cref="IProductsDefinition.GlobalPaths" />.
/// </summary>
/// <remarks>
///     <para>
///         The dialect is the common subset of GitHub path filters, Azure DevOps path filters and git's
///         <c>:(glob)</c> pathspecs: <c>**</c> as a whole path segment matches any number of segments, and <c>*</c>
///         matches any characters except <c>/</c>. A glob ending in <c>/</c> means everything below that folder, and a
///         glob with no wildcard and no trailing <c>/</c> means one file. Matching is ordinal and case-sensitive, like
///         git (GitVersion's <c>ignore.paths</c> is the exception: it ignores case from 6.8).
///     </para>
///     <para>
///         Other glob syntax (<c>?</c>, <c>+</c>, <c>[...]</c>, <c>{...}</c>, a leading <c>!</c>) and <c>\</c> separators
///         are not supported, because the platforms disagree on them (GitHub treats <c>?</c> and <c>+</c> as
///         quantifiers of the preceding character); <see cref="GetError" /> reports them. The platforms also disagree
///         on a folder named without a trailing <c>/</c>: git and Azure DevOps match everything below it, GitHub and
///         GitVersion only the path itself. <see cref="ProductValidator" /> reports a path like that when the folder
///         exists.
///     </para>
/// </remarks>
internal static class ProductPathGlob
{
    private static readonly ConcurrentDictionary<string, Regex> MatchRegexCache = new(StringComparer.Ordinal);

    /// <summary>
    ///     Normalises a glob: trims it and turns a trailing <c>/</c> into <c>/**</c>.
    /// </summary>
    /// <param name="glob">The glob to normalise.</param>
    /// <returns>The normalised glob, for example <c>src/AppA/**</c> for <c>src/AppA/</c>.</returns>
    public static string Normalize(string glob)
    {
        var trimmed = glob.Trim();

        return trimmed.EndsWith('/')
            ? $"{trimmed}**"
            : trimmed;
    }

    /// <summary>
    ///     Gets the reason a glob is not supported, or <see langword="null" /> if it is.
    /// </summary>
    /// <param name="glob">The glob to check.</param>
    /// <returns>A description of the problem, or <see langword="null" />.</returns>
    public static string? GetError(string? glob)
    {
        if (string.IsNullOrWhiteSpace(glob))
            return "it is empty";

        var normalized = Normalize(glob);

        if (normalized.StartsWith('/'))
            return "it starts with '/'; paths are relative to the repository root";

        if (normalized.StartsWith("./", StringComparison.Ordinal))
            return "it starts with './'; paths are relative to the repository root";

        if (normalized.Contains('\\'))
            return "it contains '\\'; use '/' as the separator";

        if (normalized
            .Split('/')
            .Any(segment => segment is "." or ".."))
            return "it contains a '.' or '..' segment; write the path from the repository root without them";

        if (normalized.IndexOfAny(['?', '+']) >= 0)
            return "it contains '?' or '+', which GitHub path filters treat as quantifiers of the preceding character";

        if (normalized.IndexOfAny(['[', ']', '{', '}']) >= 0 || normalized.StartsWith('!'))
            return "it uses glob syntax other than '*' and '**'";

        if (normalized.Contains("***", StringComparison.Ordinal) ||
            normalized
                .Split('/')
                .Any(segment => segment.Contains("**", StringComparison.Ordinal) && segment is not "**"))
            return "it uses '**' inside a path segment; '**' must be a whole segment, such as 'src/**/bin'";

        return null;
    }

    /// <summary>
    ///     Checks whether a glob names one path: it has no wildcard and does not end in <c>/</c>.
    /// </summary>
    /// <param name="glob">The glob.</param>
    /// <returns><see langword="true" /> if the glob is a literal path; otherwise, <see langword="false" />.</returns>
    public static bool IsLiteral(string glob)
    {
        var trimmed = glob.Trim();

        return !trimmed.EndsWith('/') && !trimmed.Contains('*');
    }

    /// <summary>
    ///     Converts a glob to the body of a regular expression, without anchors.
    /// </summary>
    /// <param name="glob">The glob, which <see cref="Normalize" /> is applied to.</param>
    /// <param name="separator">The pattern that matches a path separator, such as <c>/</c> or <c>[/\\]</c>.</param>
    /// <param name="notSeparator">The pattern that matches any character but a separator, such as <c>[^/]</c>.</param>
    /// <returns>The regular expression body.</returns>
    public static string ToRegexBody(string glob, string separator, string notSeparator)
    {
        var segments = Normalize(glob)
            .Split('/');

        var builder = new StringBuilder();

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var isLast = i == segments.Length - 1;

            if (segment is "**")
            {
                // "a/**" matches everything below a; "**/b" and "a/**/b" match zero or more folders
                builder.Append(isLast
                    ? ".*"
                    : $"(?:.*{separator})?");

                continue;
            }

            foreach (var c in segment)
                builder.Append(c switch
                {
                    '*' => $"{notSeparator}*",
                    _ => Regex.Escape(c.ToString()),
                });

            if (!isLast)
                builder.Append(separator);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Checks whether a repository-relative file path matches a glob.
    /// </summary>
    /// <param name="glob">The glob.</param>
    /// <param name="path">The file path, using <c>/</c> or <c>\</c> as the separator.</param>
    /// <returns><see langword="true" /> if the path matches the glob; otherwise, <see langword="false" />.</returns>
    public static bool IsMatch(string glob, string path)
    {
        var regex = MatchRegexCache.GetOrAdd(glob,
            // Singleline and \z, because a git path can contain a newline
            static g => new($@"\A{ToRegexBody(g, "/", "[^/]")}\z", RegexOptions.CultureInvariant | RegexOptions.Singleline));

        return regex.IsMatch(path.Replace('\\', '/'));
    }

    /// <summary>
    ///     Converts a glob to a git pathspec with the <c>glob</c> magic, such as <c>:(glob)src/AppA/**</c>.
    /// </summary>
    /// <param name="glob">The glob, which <see cref="Normalize" /> is applied to.</param>
    /// <returns>The pathspec.</returns>
    public static string ToGitPathspec(string glob) =>
        $":(glob){Normalize(glob)}";

    /// <summary>
    ///     Builds the GitVersion <c>ignore.paths</c> regular expression that matches every path not covered by
    ///     <paramref name="globs" />, so that GitVersion ignores commits that touch none of them.
    /// </summary>
    /// <param name="globs">The globs whose commits GitVersion should count, relative to the Atom root directory.</param>
    /// <param name="repositoryPrefix">
    ///     The path of the Atom root directory relative to the git repository root, such as
    ///     <c>samples/Monorepo</c>, or empty if they are the same. GitVersion matches paths relative to the repository
    ///     root, so the globs are anchored below this folder.
    /// </param>
    /// <returns>
    ///     A regular expression such as <c>^(?!(?:src[/\\]AppA[/\\].*|global\.json)$).*</c>. Separators match both
    ///     <c>/</c> and <c>\</c>, because GitVersion sees paths with the platform's separator.
    /// </returns>
    public static string ToGitVersionIgnoreRegex(IEnumerable<string> globs, string repositoryPrefix = "")
    {
        var alternatives = globs
            .Select(x => ToRegexBody(x, @"[/\\]", @"[^/\\]"))
            .Distinct(StringComparer.Ordinal);

        var prefix = string.Concat(repositoryPrefix
            .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => $@"{Regex.Escape(segment)}[/\\]"));

        return $"^(?!{prefix}(?:{string.Join('|', alternatives)})$).*";
    }
}
