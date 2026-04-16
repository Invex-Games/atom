namespace DecSm.Atom;

/// <summary>
///     Represents a semantic version following the Semantic Versioning 2.0.0 specification.
///     Supports parsing, comparison, and conversion operations for version strings in the format
///     MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA].
/// </summary>
/// <remarks>
///     <para>This class implements semantic version comparison where:</para>
///     <list type="bullet">
///         <item>Release versions (without pre-release) are greater than pre-release versions</item>
///         <item>Pre-release versions are compared lexicographically by dot-separated identifiers</item>
///         <item>Numeric identifiers in pre-release are compared numerically</item>
///         <item>Metadata is used only as a final tiebreaker when all other components are equal</item>
///     </list>
///     <para>Supports implicit conversion to/from string and JSON serialization.</para>
/// </remarks>
/// <example>
///     <code>
/// var version1 = SemVer.Parse("1.2.3-alpha.1+build.123");
/// var version2 = new SemVer("2.0.0");
/// bool isNewer = version2 > version1; // true
/// </code>
/// </example>
[PublicAPI]
public sealed partial class SemVer()
    : ISpanParsable<SemVer>, IComparable<SemVer>, IComparisonOperators<SemVer, SemVer, bool>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SemVer" /> class with the specified version components.
    ///     This constructor is used for JSON deserialization.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number.</param>
    /// <param name="patch">The patch version number.</param>
    /// <param name="preRelease">The pre-release version identifier, or null if not a pre-release.</param>
    /// <param name="metadata">The build metadata, or null if no metadata is present.</param>
    [JsonConstructor]
    private SemVer(int major, int minor, int patch, string? preRelease, string? metadata) : this()
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        Metadata = metadata;
    }

    /// <summary>
    ///     Gets the major version number.
    /// </summary>
    /// <value>The major version number, indicating incompatible API changes.</value>
    public int Major { get; private init; }

    /// <summary>
    ///     Gets the minor version number.
    /// </summary>
    /// <value>The minor version number, indicating backwards-compatible functionality additions.</value>
    public int Minor { get; private init; }

    /// <summary>
    ///     Gets the patch version number.
    /// </summary>
    /// <value>The patch version number, indicating backwards-compatible bug fixes.</value>
    public int Patch { get; private init; }

    /// <summary>
    ///     Gets the version prefix in the format "MAJOR.MINOR.PATCH".
    /// </summary>
    /// <value>A string representing the core version without pre-release or metadata components.</value>
    [JsonIgnore]
    public string Prefix => $"{Major}.{Minor}.{Patch}";

    /// <summary>
    ///     Gets the pre-release version identifier.
    /// </summary>
    /// <value>The pre-release identifier string, or null if this is a release version.</value>
    /// <remarks>
    ///     Pre-release identifiers are dot-separated and may contain alphanumeric characters and hyphens.
    /// </remarks>
    public string? PreRelease { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether this version is a pre-release version.
    /// </summary>
    /// <value>true if this version has a pre-release identifier; otherwise, false.</value>
    [JsonIgnore]
    public bool IsPreRelease => PreRelease != null;

    /// <summary>
    ///     Gets the build number extracted from the pre-release identifier.
    /// </summary>
    /// <value>The numeric build number if exactly one number is found in the pre-release; otherwise, 0.</value>
    /// <remarks>
    ///     This property uses <see cref="ExtractBuildNumber" /> to parse the pre-release string.
    ///     Only returns a non-zero value if exactly one numeric sequence is present.
    /// </remarks>
    [JsonIgnore]
    public int BuildNumberFromPreRelease => ExtractBuildNumber(PreRelease);

    /// <summary>
    ///     Gets the build metadata.
    /// </summary>
    /// <value>The build metadata string, or null if no metadata is present.</value>
    /// <remarks>
    ///     Metadata is ignored when determining version precedence but is used as a tiebreaker in comparisons.
    /// </remarks>
    public string? Metadata { get; private init; }

    /// <summary>
    ///     Gets the build number extracted from the metadata.
    /// </summary>
    /// <value>The numeric build number if exactly one number is found in the metadata; otherwise, 0.</value>
    /// <remarks>
    ///     This property uses <see cref="ExtractBuildNumber" /> to parse the metadata string.
    ///     Only returns a non-zero value if exactly one numeric sequence is present.
    /// </remarks>
    [JsonIgnore]
    public int BuildNumberFromMetadata => ExtractBuildNumber(Metadata);

    /// <summary>
    ///     Gets a semantic version representing version 1.0.0.
    /// </summary>
    /// <value>A <see cref="SemVer" /> instance with Major set to 1, Minor set to 0, and Patch set to 0.</value>
    public static SemVer One { get; } = new()
    {
        Major = 1,
        Minor = 0,
        Patch = 0,
    };

    /// <summary>
    ///     Compares the current instance with another <see cref="SemVer" /> and returns an integer that indicates
    ///     whether the current instance precedes, follows, or occurs in the same position in the sort order.
    /// </summary>
    /// <param name="other">The <see cref="SemVer" /> to compare with this instance.</param>
    /// <returns>
    ///     A value that indicates the relative order of the objects being compared:
    ///     Less than zero if this instance precedes <paramref name="other" />;
    ///     Zero if they are equal;
    ///     Greater than zero if this instance follows <paramref name="other" />.
    /// </returns>
    /// <remarks>
    ///     <para>Comparison follows Semantic Versioning precedence rules:</para>
    ///     <list type="number">
    ///         <item>Compare major, minor, and patch versions numerically</item>
    ///         <item>Pre-release versions have lower precedence than normal versions</item>
    ///         <item>Pre-release identifiers are compared lexicographically, with numeric identifiers compared numerically</item>
    ///         <item>Metadata is used only as a final tiebreaker</item>
    ///     </list>
    /// </remarks>
    public int CompareTo(SemVer? other)
    {
        if (other is null)
            return 1;

        var majorComparison = Major.CompareTo(other.Major);

        if (majorComparison != 0)
            return majorComparison;

        var minorComparison = Minor.CompareTo(other.Minor);

        if (minorComparison != 0)
            return minorComparison;

        var patchComparison = Patch.CompareTo(other.Patch);

        if (patchComparison != 0)
            return patchComparison;

        switch (PreRelease, other.PreRelease)
        {
            case (null, not null):
                return 1;
            case (not null, null):
                return -1;
            case (null, null):
                return string.CompareOrdinal(Metadata, other.Metadata);
        }

        var preReleaseParts = PreRelease.Split('.');
        var otherPreReleaseParts = other.PreRelease.Split('.');

        for (var i = 0; i < Math.Min(preReleaseParts.Length, otherPreReleaseParts.Length); i++)
            if (int.TryParse(preReleaseParts[i], out var preReleasePart) &&
                int.TryParse(otherPreReleaseParts[i], out var otherPreReleasePart))
            {
                var preReleasePartComparison = preReleasePart.CompareTo(otherPreReleasePart);

                if (preReleasePartComparison != 0)
                    return preReleasePartComparison;
            }
            else
            {
                var preReleasePartComparison = string.CompareOrdinal(preReleaseParts[i], otherPreReleaseParts[i]);

                if (preReleasePartComparison != 0)
                    return preReleasePartComparison;
            }

        var preReleaseLengthComparison = preReleaseParts.Length.CompareTo(otherPreReleaseParts.Length);

        return preReleaseLengthComparison != 0
            ? preReleaseLengthComparison
            : string.CompareOrdinal(Metadata, other.Metadata);
    }

    public static bool operator >(SemVer left, SemVer right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(SemVer left, SemVer right) =>
        left.CompareTo(right) >= 0;

    public static bool operator <(SemVer left, SemVer right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(SemVer left, SemVer right) =>
        left.CompareTo(right) <= 0;

    public static bool operator ==(SemVer? left, SemVer? right) =>
        (left is null && right is null) || left?.Equals(right) == true;

    public static bool operator !=(SemVer? left, SemVer? right) =>
        !(left == right);

    public static SemVer Parse(string s, IFormatProvider? provider)
    {
        var match = SemVerRegex()
            .Match(s);

        // ReSharper disable once LocalizableElement
        if (!match.Success)
            throw new ArgumentException($"Invalid version string '{s}'.", nameof(s));

        return new()
        {
            Major = int.Parse(match.Groups[1].Value),
            Minor = int.Parse(match.Groups[2].Value),
            Patch = int.Parse(match.Groups[3].Value),
            PreRelease = string.IsNullOrWhiteSpace(match.Groups[4].Value)
                ? null
                : match.Groups[4].Value,
            Metadata = string.IsNullOrWhiteSpace(match.Groups[5].Value)
                ? null
                : match.Groups[5].Value,
        };
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out SemVer result)
    {
        if (s is null)
        {
            result = null;

            return false;
        }

        var match = SemVerRegex()
            .Match(s);

        if (!match.Success ||
            !int.TryParse(match.Groups[1].Value, out var major) ||
            major < 0 ||
            !int.TryParse(match.Groups[2].Value, out var minor) ||
            minor < 0 ||
            !int.TryParse(match.Groups[3].Value, out var patch) ||
            patch < 0)
        {
            result = null;

            return false;
        }

        var preRelease = match.Groups[4].Value;
        var metadata = match.Groups[5].Value;

        result = new()
        {
            Major = major,
            Minor = minor,
            Patch = patch,
            PreRelease = string.IsNullOrWhiteSpace(preRelease)
                ? null
                : preRelease,
            Metadata = string.IsNullOrWhiteSpace(metadata)
                ? null
                : metadata,
        };

        return true;
    }

    public static SemVer Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        Parse(s.ToString(), provider);

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s.ToString(), provider, out result);

    /// <summary>
    ///     Determines whether the current version is between two specified versions (exclusive).
    /// </summary>
    /// <param name="firstBound">The first boundary version.</param>
    /// <param name="secondBound">The second boundary version.</param>
    /// <returns>true if this version is between the two bounds; otherwise, false.</returns>
    /// <remarks>
    ///     The method handles bounds in any order. If both bounds are equal, returns true only if this version equals the
    ///     bounds.
    /// </remarks>
    public bool IsBetween(SemVer firstBound, SemVer secondBound) =>
        firstBound == secondBound
            ? Equals(firstBound)
            : firstBound < secondBound
                ? firstBound < this && this < secondBound
                : secondBound < this && this < firstBound;

    /// <summary>
    ///     Converts this semantic version to a <see cref="System.Version" /> instance.
    /// </summary>
    /// <param name="throwIfContainsPreRelease">If true, throws an exception when pre-release data is present.</param>
    /// <param name="throwIfContainsMetadata">If true, throws an exception when metadata is present.</param>
    /// <returns>A <see cref="System.Version" /> with Major, Minor, and Build (from Patch) components.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="throwIfContainsPreRelease" /> is true and pre-release data is present,
    ///     or when <paramref name="throwIfContainsMetadata" /> is true and metadata is present.
    /// </exception>
    /// <remarks>
    ///     Maps SemVer Major.Minor.Patch to System.Version Major.Minor.Build.
    ///     Pre-release and metadata information is lost in the conversion.
    /// </remarks>
    public Version ToSystemVersion(bool throwIfContainsPreRelease = false, bool throwIfContainsMetadata = false) =>
        throwIfContainsPreRelease && PreRelease is not null
            ? throw new ArgumentException(
                "The SemVer contains a pre-release tag, which is not supported by System.Version.")
            : throwIfContainsMetadata && Metadata is not null
                ? throw new ArgumentException("The SemVer contains metadata, which is not supported by System.Version.")
                : new(Major, Minor, Patch);

    /// <summary>
    ///     Creates a <see cref="SemVer" /> instance from a <see cref="System.Version" />.
    /// </summary>
    /// <param name="version">The System.Version to convert.</param>
    /// <param name="throwIfContainsRevision">If true, throws an exception when the version has a non-zero revision.</param>
    /// <returns>A <see cref="SemVer" /> with Major, Minor, and Patch (from Build) components.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="throwIfContainsRevision" /> is true and the version has a non-zero revision.
    /// </exception>
    /// <remarks>
    ///     Maps System.Version Major.Minor.Build to SemVer Major.Minor.Patch.
    ///     The revision component is ignored unless <paramref name="throwIfContainsRevision" /> is true.
    /// </remarks>
    public static SemVer FromSystemVersion(Version version, bool throwIfContainsRevision = false) =>
        throwIfContainsRevision && version.Revision > 0
            ? throw new ArgumentException("The version contains a revision number, which is not supported by SemVer.")
            : new()
            {
                Major = version.Major,
                Minor = version.Minor,
                Patch = version.Build,
            };

    /// <summary>
    ///     Extracts a single numeric value from the specified input string.
    /// </summary>
    /// <param name="input">The string to extract a number from.</param>
    /// <returns>
    ///     The numeric value if exactly one number is found; otherwise, 0.
    ///     Returns 0 if the input is null, empty, whitespace, or contains zero or multiple numbers.
    /// </returns>
    /// <remarks>
    ///     This method is used by <see cref="BuildNumberFromPreRelease" /> and <see cref="BuildNumberFromMetadata" />
    ///     to extract build numbers from version components.
    /// </remarks>
    /// <example>
    ///     <code>
    /// ExtractBuildNumber("alpha.123") // returns 123
    /// ExtractBuildNumber("beta") // returns 0
    /// ExtractBuildNumber("1.2.3") // returns 0 (multiple numbers)
    /// </code>
    /// </example>
    public static int ExtractBuildNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return 0;

        var matches = NumberRegex()
            .Matches(input);

        return matches.Count is 1
            ? int.Parse(matches[0].Value)
            : 0;
    }

    /// <summary>
    ///     Parses a string representation of a semantic version.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>A <see cref="SemVer" /> equivalent to the version contained in <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="s" /> is not a valid semantic version string.</exception>
    /// <remarks>
    ///     Accepts version strings in the format: MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA]
    ///     where MAJOR, MINOR, and PATCH are non-negative integers.
    /// </remarks>
    public static SemVer Parse(string s) =>
        Parse(s, null);

    /// <summary>
    ///     Tries to parse a string representation of a semantic version using the invariant culture.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="SemVer" /> equivalent to the version contained in
    ///     <paramref name="s" />,
    ///     if the conversion succeeded, or null if the conversion failed.
    /// </param>
    /// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s, null, out result);

    /// <summary>
    ///     Tries to parse a span of characters representing a semantic version using the invariant culture.
    /// </summary>
    /// <param name="s">The span of characters to parse.</param>
    /// <param name="result">
    ///     When this method returns, contains the <see cref="SemVer" /> equivalent to the version contained in
    ///     <paramref name="s" />,
    ///     if the conversion succeeded, or null if the conversion failed.
    /// </param>
    /// <returns>true if <paramref name="s" /> was converted successfully; otherwise, false.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, [MaybeNullWhen(false)] out SemVer result) =>
        TryParse(s.ToString(), out result);

    /// <summary>
    ///     Determines whether the specified <see cref="SemVer" /> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SemVer" /> to compare with the current instance.</param>
    /// <returns>true if the specified <see cref="SemVer" /> is equal to the current instance; otherwise, false.</returns>
    public bool Equals(SemVer? other) =>
        CompareTo(other) == 0;

    /// <summary>
    ///     Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
    public override bool Equals(object? obj) =>
        obj is SemVer semVer && Equals(semVer);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() =>
        HashCode.Combine(Major, Minor, Patch, PreRelease, Metadata);

    /// <summary>
    ///     Converts the string representation of a semantic version to its <see cref="SemVer" /> equivalent.
    /// </summary>
    /// <param name="s">The string representation of the semantic version.</param>
    /// <returns>A <see cref="SemVer" /> object equivalent to the version contained in <paramref name="s" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="s" /> is not a valid semantic version string.</exception>
    public static implicit operator SemVer(string s) =>
        Parse(s);

    /// <summary>
    ///     Converts a <see cref="SemVer" /> to its string representation.
    /// </summary>
    /// <param name="semVer">The <see cref="SemVer" /> to convert.</param>
    /// <returns>The string representation of the semantic version.</returns>
    public static implicit operator string(SemVer semVer) =>
        semVer.ToString();

    /// <summary>
    ///     Returns the string representation of this semantic version.
    /// </summary>
    /// <returns>
    ///     A string in the format MAJOR.MINOR.PATCH[-PRERELEASE][+METADATA],
    ///     where the pre-release and metadata components are included only if present.
    /// </returns>
    /// <example>
    ///     <code>
    /// new SemVer { Major = 1, Minor = 2, Patch = 3 }.ToString() // "1.2.3"
    /// new SemVer { Major = 1, Minor = 2, Patch = 3, PreRelease = "alpha" }.ToString() // "1.2.3-alpha"
    /// new SemVer { Major = 1, Minor = 2, Patch = 3, Metadata = "build.1" }.ToString() // "1.2.3+build.1"
    /// </code>
    /// </example>
    public override string ToString() =>
        (PreRelease, Metadata) switch
        {
            (not null, not null) => $"{Major}.{Minor}.{Patch}-{PreRelease}+{Metadata}",
            (not null, null) => $"{Major}.{Minor}.{Patch}-{PreRelease}",
            (null, not null) => $"{Major}.{Minor}.{Patch}+{Metadata}",
            _ => $"{Major}.{Minor}.{Patch}",
        };

    [GeneratedRegex(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$")]
    private static partial Regex SemVerRegex();

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();
}
