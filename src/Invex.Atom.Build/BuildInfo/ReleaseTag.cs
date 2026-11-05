namespace Invex.Atom.Build.BuildInfo;

/// <summary>
///     Formats and parses git release tags of the form <c>&lt;prefix&gt;&lt;version&gt;</c>, such as <c>v1.2.0</c> or
///     <c>appa/v1.2.0-rc.1</c>.
/// </summary>
/// <remarks>
///     <para>
///         A release tag is a prefix followed by a <see cref="SemVer" />. Every method takes the prefix explicitly:
///         usually <see cref="DefaultPrefix" /> (<c>v</c>), or a monorepo product's own prefix, such as
///         <c>appa/v</c>, so that each product's tags can be found and versioned independently. The prefix may be
///         empty, for tags like <c>1.2.0</c>.
///     </para>
///     <para>
///         In a build, prefer the <see cref="IReleaseTagHelper" /> members
///         (<see cref="IReleaseTagHelper.ReleaseTagFor" />, <see cref="IReleaseTagHelper.TryParseReleaseTag" /> and
///         <see cref="IReleaseTagHelper.ReleaseTagGlob" />), which use the build's release tag prefix. Use this class
///         when you need a specific prefix.
///     </para>
///     <para>
///         Prefix matching is ordinal and case-sensitive.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// ReleaseTag.For(SemVer.Parse("1.2.0"), ReleaseTag.DefaultPrefix); // "v1.2.0"
/// ReleaseTag.For(SemVer.Parse("1.2.0-rc.1"), "appa/v"); // "appa/v1.2.0-rc.1"
/// ReleaseTag.TryParse("appa/v1.2.0", "appa/v", out var version); // true, version is 1.2.0
/// ReleaseTag.TryParse("appa/v1.2.0", "v", out _); // false, the prefix does not match
/// ReleaseTag.Glob("appa/v"); // "appa/v*"
///     </code>
/// </example>
[PublicAPI]
public static class ReleaseTag
{
    /// <summary>
    ///     The default release tag prefix, <c>v</c>.
    /// </summary>
    public const string DefaultPrefix = "v";

    /// <summary>
    ///     Gets the release tag for a version.
    /// </summary>
    /// <param name="version">The version to tag.</param>
    /// <param name="prefix">
    ///     The tag prefix, such as <see cref="DefaultPrefix" /> (<c>v</c>) or <c>appa/v</c>. May be empty, but not
    ///     <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <paramref name="prefix" /> followed by the full version string, including any pre-release and build metadata
    ///     components, for example <c>v1.2.0</c> or <c>appa/v1.2.0-rc.1</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="version" /> or <paramref name="prefix" /> is <see langword="null" />.
    /// </exception>
    public static string For(SemVer version, string prefix)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(prefix);

        return $"{prefix}{version}";
    }

    /// <summary>
    ///     Tries to parse a release tag with the given prefix.
    /// </summary>
    /// <param name="tag">The tag to parse, such as <c>v1.2.0</c>. May be <see langword="null" />.</param>
    /// <param name="prefix">
    ///     The tag prefix, such as <see cref="DefaultPrefix" /> (<c>v</c>) or <c>appa/v</c>. May be empty, but not
    ///     <see langword="null" />.
    /// </param>
    /// <param name="version">
    ///     When this method returns <see langword="true" />, the version that follows the prefix; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="tag" /> starts with <paramref name="prefix" /> (ordinal,
    ///     case-sensitive) and the rest of the tag is a valid semantic version; otherwise, <see langword="false" />.
    ///     For example, <c>appa/v1.2.0</c> does not parse with the <c>v</c> prefix, and <c>vnext</c> does not parse
    ///     with any prefix.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="prefix" /> is <see langword="null" />.</exception>
    public static bool TryParse(
        [NotNullWhen(true)] string? tag,
        string prefix,
        [NotNullWhen(true)] out SemVer? version)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        version = null;

        return tag is not null &&
               tag.StartsWith(prefix, StringComparison.Ordinal) &&
               SemVer.TryParse(tag.AsSpan(prefix.Length), out version);
    }

    /// <summary>
    ///     Gets a glob pattern that matches release tags with the given prefix, for use with
    ///     <c>git tag --list &lt;pattern&gt;</c>.
    /// </summary>
    /// <param name="prefix">
    ///     The tag prefix, such as <see cref="DefaultPrefix" /> (<c>v</c>) or <c>appa/v</c>. May be empty, but not
    ///     <see langword="null" />.
    /// </param>
    /// <returns><paramref name="prefix" /> followed by <c>*</c>, for example <c>v*</c> or <c>appa/v*</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="prefix" /> is <see langword="null" />.</exception>
    /// <remarks>
    ///     The pattern is broader than a release tag: <c>v*</c> also matches tags such as <c>vnext</c>, and any other
    ///     tag that starts with <c>v</c>, including another product's tags (for example <c>vendor/v1.0.0</c>). Filter the
    ///     listed tags with <see cref="TryParse" /> using the same prefix. The prefix is not escaped, so glob characters
    ///     in it (<c>*</c>, <c>?</c>, <c>[</c>) keep their glob meaning.
    /// </remarks>
    public static string Glob(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);

        return $"{prefix}*";
    }
}
