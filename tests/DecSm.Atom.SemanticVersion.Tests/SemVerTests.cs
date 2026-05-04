namespace DecSm.Atom.SemanticVersion.Tests;

[TestFixture]
internal sealed class SemVerTests
{
    [TestCase("1.0.0", 1, 0, 0, null, null)]
    [TestCase("2.1.0", 2, 1, 0, null, null)]
    [TestCase("3.2.1", 3, 2, 1, null, null)]
    [TestCase("0.0.0", 0, 0, 0, null, null)]
    [TestCase("10.20.30", 10, 20, 30, null, null)]
    [TestCase("1.0.0-alpha", 1, 0, 0, "alpha", null)]
    [TestCase("1.0.0+build", 1, 0, 0, null, "build")]
    [TestCase("1.0.0-alpha+build", 1, 0, 0, "alpha", "build")]
    [TestCase("1.0.0-alpha.1+build.123", 1, 0, 0, "alpha.1", "build.123")]
    [TestCase("1.0.0-alpha.beta.1+exp.sha.5114f85", 1, 0, 0, "alpha.beta.1", "exp.sha.5114f85")]
    public void Parse_ValidVersionString_SetsAllComponents(
        string versionString,
        int expectedMajor,
        int expectedMinor,
        int expectedPatch,
        string? expectedPreRelease,
        string? expectedMetadata)
    {
        var semVer = SemVer.Parse(versionString);

        semVer.ShouldSatisfyAllConditions(() => semVer.Major.ShouldBe(expectedMajor),
            () => semVer.Minor.ShouldBe(expectedMinor),
            () => semVer.Patch.ShouldBe(expectedPatch),
            () => semVer.PreRelease.ShouldBe(expectedPreRelease),
            () => semVer.Metadata.ShouldBe(expectedMetadata));
    }

    [TestCase("1.0")]
    [TestCase("1.0.0.0")]
    [TestCase("1..0")]
    [TestCase("1.0.0-")]
    [TestCase("1.0.0+")]
    [TestCase("-1.0.0")]
    [TestCase("1.-1.0")]
    [TestCase("1.1.-1")]
    [TestCase("a.0.0")]
    [TestCase("1.a.0")]
    [TestCase("1.1.a")]
    [TestCase("")]
    public void Parse_InvalidVersionString_ThrowsArgumentException(string versionString) =>
        Should.Throw<ArgumentException>(() => SemVer.Parse(versionString));

    [TestCase("1.0.0", 1, 0, 0, null, null)]
    [TestCase("1.0.0-alpha+build", 1, 0, 0, "alpha", "build")]
    [TestCase("10.20.30", 10, 20, 30, null, null)]
    public void Parse_ValidVersionSpan_SetsAllComponents(
        string versionString,
        int expectedMajor,
        int expectedMinor,
        int expectedPatch,
        string? expectedPreRelease,
        string? expectedMetadata)
    {
        var semVer = SemVer.Parse(versionString.AsSpan(), null);

        semVer.ShouldSatisfyAllConditions(() => semVer.Major.ShouldBe(expectedMajor),
            () => semVer.Minor.ShouldBe(expectedMinor),
            () => semVer.Patch.ShouldBe(expectedPatch),
            () => semVer.PreRelease.ShouldBe(expectedPreRelease),
            () => semVer.Metadata.ShouldBe(expectedMetadata));
    }

    [TestCase("1.0")]
    [TestCase("1.0.0.0")]
    [TestCase("1..0")]
    [TestCase("1.0.0-")]
    [TestCase("1.0.0+")]
    public void Parse_InvalidVersionSpan_ThrowsArgumentException(string versionString) =>
        Should.Throw<ArgumentException>(() => SemVer.Parse(versionString.AsSpan(), null));

    [TestCase("1.0.0", true)]
    [TestCase("1.0.0-alpha+build", true)]
    [TestCase("1.0", false)]
    [TestCase("1.0.0.0", false)]
    [TestCase("not-a-version", false)]
    public void TryParse_StringWithProvider_ReturnsExpectedResult(string versionString, bool expectedResult)
    {
        var result = SemVer.TryParse(versionString, null, out _);

        result.ShouldBe(expectedResult);
    }

    [Test]
    public void TryParse_NullString_ReturnsFalse()
    {
        var result = SemVer.TryParse(null, null, out var parsed);

        result.ShouldBeFalse();
        parsed.ShouldBeNull();
    }

    [TestCase("1.0.0", true)]
    [TestCase("1.0.0-alpha+build", true)]
    [TestCase("1.0", false)]
    public void TryParse_StringNoProvider_ReturnsExpectedResult(string versionString, bool expectedResult)
    {
        var result = SemVer.TryParse(versionString, out _);

        result.ShouldBe(expectedResult);
    }

    [Test]
    public void TryParse_NullStringNoProvider_ReturnsFalse()
    {
        var result = SemVer.TryParse(null, out var parsed);

        result.ShouldBeFalse();
        parsed.ShouldBeNull();
    }

    [TestCase("1.0.0", true)]
    [TestCase("1.0.0-alpha+build", true)]
    [TestCase("1.0", false)]
    public void TryParse_SpanWithProvider_ReturnsExpectedResult(string versionString, bool expectedResult)
    {
        var result = SemVer.TryParse(versionString.AsSpan(), null, out _);

        result.ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", true)]
    [TestCase("1.0", false)]
    public void TryParse_SpanNoProvider_ReturnsExpectedResult(string versionString, bool expectedResult)
    {
        var result = SemVer.TryParse(versionString.AsSpan(), out _);

        result.ShouldBe(expectedResult);
    }

    [Test]
    public void TryParse_ValidString_SetsOutputComponents()
    {
        var result = SemVer.TryParse("1.2.3-alpha+build.123", out var parsed);

        result.ShouldBeTrue();

        parsed.ShouldSatisfyAllConditions(() => parsed!.Major.ShouldBe(1),
            () => parsed!.Minor.ShouldBe(2),
            () => parsed!.Patch.ShouldBe(3),
            () => parsed!.PreRelease.ShouldBe("alpha"),
            () => parsed!.Metadata.ShouldBe("build.123"));
    }

    [Test]
    public void Prefix_ReturnsCorrectFormat()
    {
        var semVer = SemVer.Parse("1.2.3-alpha+build");

        semVer.Prefix.ShouldBe("1.2.3");
    }

    [TestCase("1.0.0-alpha", true)]
    [TestCase("1.0.0", false)]
    public void IsPreRelease_ReturnsExpectedResult(string versionString, bool expectedResult) =>
        SemVer
            .Parse(versionString)
            .IsPreRelease
            .ShouldBe(expectedResult);

    [Test]
    public void One_HasExpectedValues() =>
        SemVer.One.ShouldSatisfyAllConditions(() => SemVer.One.Major.ShouldBe(1),
            () => SemVer.One.Minor.ShouldBe(0),
            () => SemVer.One.Patch.ShouldBe(0),
            () => SemVer.One.PreRelease.ShouldBeNull(),
            () => SemVer.One.Metadata.ShouldBeNull());

    [TestCase("1.2.3", "1.2.3")]
    [TestCase("1.2.3-alpha", "1.2.3-alpha")]
    [TestCase("1.2.3+build.1", "1.2.3+build.1")]
    [TestCase("1.2.3-alpha+build.1", "1.2.3-alpha+build.1")]
    public void ToString_ReturnsCorrectFormat(string versionString, string expectedString) =>
        SemVer
            .Parse(versionString)
            .ToString()
            .ShouldBe(expectedString);

    [TestCase("1.0.0", 1, 0, 0, null, null)]
    [TestCase("1.0.0-alpha+build", 1, 0, 0, "alpha", "build")]
    public void ImplicitConversionFromString_ParsesCorrectly(
        string versionString,
        int expectedMajor,
        int expectedMinor,
        int expectedPatch,
        string? expectedPreRelease,
        string? expectedMetadata)
    {
        SemVer semVer = versionString;

        semVer.ShouldSatisfyAllConditions(() => semVer.Major.ShouldBe(expectedMajor),
            () => semVer.Minor.ShouldBe(expectedMinor),
            () => semVer.Patch.ShouldBe(expectedPatch),
            () => semVer.PreRelease.ShouldBe(expectedPreRelease),
            () => semVer.Metadata.ShouldBe(expectedMetadata));
    }

    [TestCase("1.0.0", "1.0.0")]
    [TestCase("1.0.0-alpha+build", "1.0.0-alpha+build")]
    public void ImplicitConversionToString_ReturnsStringRepresentation(string versionString, string expectedString)
    {
        SemVer semVer = versionString;

        string actualString = semVer;

        actualString.ShouldBe(expectedString);
    }

    [TestCase("1.0.0", "1.0.0", 0)]
    [TestCase("2.0.0", "1.0.0", 1)]
    [TestCase("1.0.0", "2.0.0", -1)]
    [TestCase("1.2.0", "1.1.0", 1)]
    [TestCase("1.0.2", "1.0.1", 1)]
    [TestCase("1.0.0", "1.0.0-alpha", 1)] // release > prerelease
    [TestCase("1.0.0-alpha", "1.0.0", -1)] // prerelease < release
    [TestCase("1.0.0-2", "1.0.0-1", 1)] // numeric prerelease identifiers compared numerically
    [TestCase("1.0.0-1", "1.0.0-2", -1)]
    [TestCase("1.0.0-alpha", "1.0.0-alpha", 0)]
    public void CompareTo_ReturnsExpectedValue(string version1, string version2, int expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        semVer1
            .CompareTo(semVer2)
            .ShouldBe(expectedResult);
    }

    [Test]
    public void CompareTo_Null_ReturnsPositive() =>
        SemVer
            .Parse("1.0.0")
            .CompareTo(null)
            .ShouldBePositive();

    [Test]
    public void CompareTo_PreReleaseLexicographic_BetaGreaterThanAlpha()
    {
        // "beta" > "alpha" by ASCII: 'b'(98) > 'a'(97)
        var result = SemVer
            .Parse("1.0.0-beta")
            .CompareTo(SemVer.Parse("1.0.0-alpha"));

        result.ShouldBePositive();
    }

    [Test]
    public void CompareTo_PreReleaseDotSeparatedNumeric_ComparesNumerically()
    {
        // alpha.10 > alpha.9 because 10 > 9 numerically (not lexicographically)
        var greater = SemVer.Parse("1.0.0-alpha.10");
        var lesser = SemVer.Parse("1.0.0-alpha.9");

        greater
            .CompareTo(lesser)
            .ShouldBePositive();

        lesser
            .CompareTo(greater)
            .ShouldBeNegative();
    }

    [Test]
    public void CompareTo_LongerPreReleaseGreaterThanShorter()
    {
        // "alpha.1.2" > "alpha.1" because it has more identifiers
        var longer = SemVer.Parse("1.0.0-alpha.1.2");
        var shorter = SemVer.Parse("1.0.0-alpha.1");

        longer
            .CompareTo(shorter)
            .ShouldBePositive();
    }

    [Test]
    public void CompareTo_BothNullPreRelease_UsesMetadataAsTiebreaker()
    {
        // When all version components and prerelease are equal, metadata breaks the tie
        var withBuild2 = SemVer.Parse("1.0.0+build.2");
        var withBuild1 = SemVer.Parse("1.0.0+build.1");

        withBuild2
            .CompareTo(withBuild1)
            .ShouldBePositive();
    }

    [Test]
    public void CompareTo_EqualPreRelease_UsesMetadataAsTiebreaker()
    {
        var withMeta = SemVer.Parse("1.0.0-alpha+build.2");
        var noMeta = SemVer.Parse("1.0.0-alpha+build.1");

        withMeta
            .CompareTo(noMeta)
            .ShouldBePositive();
    }

    [TestCase("2.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "2.0.0", false)]
    [TestCase("1.0.0", "1.0.0", false)]
    public void OperatorGreaterThan_ReturnsExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 > semVer2).ShouldBe(expectedResult);
    }

    [TestCase("2.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "1.0.0", true)]
    [TestCase("1.0.0", "2.0.0", false)]
    public void OperatorGreaterThanOrEqual_ReturnsExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 >= semVer2).ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "2.0.0", true)]
    [TestCase("2.0.0", "1.0.0", false)]
    [TestCase("1.0.0", "1.0.0", false)]
    public void OperatorLessThan_ReturnsExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 < semVer2).ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "2.0.0", true)]
    [TestCase("1.0.0", "1.0.0", true)]
    [TestCase("2.0.0", "1.0.0", false)]
    public void OperatorLessThanOrEqual_ReturnsExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 <= semVer2).ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "1.0.0", true)]
    [TestCase("1.0.0-alpha+build", "1.0.0-alpha+build", true)]
    [TestCase("1.0.0", "2.0.0", false)]
    [TestCase("1.0.0", "1.0.0-alpha", false)]
    public void OperatorEquality_ReturnsExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 == semVer2).ShouldBe(expectedResult);
    }

    [Test]
    public void OperatorEquality_BothNull_ReturnsTrue()
    {
        SemVer? left = null;
        SemVer? right = null;

        (left == right).ShouldBeTrue();
    }

    [Test]
    public void OperatorEquality_LeftNull_ReturnsFalse()
    {
        SemVer? left = null;
        var right = SemVer.Parse("1.0.0");

        (left == right).ShouldBeFalse();
    }

    [Test]
    public void OperatorEquality_RightNull_ReturnsFalse()
    {
        var left = SemVer.Parse("1.0.0");
        SemVer? right = null;

        (left == right).ShouldBeFalse();
    }

    [TestCase("1.0.0", "2.0.0", true)]
    [TestCase("1.0.0", "1.0.0", false)]
    public void OperatorInequality_ReturnsExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        (semVer1 != semVer2).ShouldBe(expectedResult);
    }

    [TestCase("1.0.0", "1.0.0", true)]
    [TestCase("1.0.0-alpha", "1.0.0-alpha", true)]
    [TestCase("1.0.0", "1.0.0-alpha", false)]
    [TestCase("1.0.0+build", "1.0.0", false)]
    public void Equals_SemVer_ReturnsExpectedResult(string version1, string version2, bool expectedResult)
    {
        var semVer1 = SemVer.Parse(version1);
        var semVer2 = SemVer.Parse(version2);

        semVer1
            .Equals(semVer2)
            .ShouldBe(expectedResult);
    }

    [Test]
    public void Equals_Null_ReturnsFalse() =>
        SemVer
            .Parse("1.0.0")
            .Equals(null)
            .ShouldBeFalse();

    [Test]
    public void Equals_Object_SameSemVer_ReturnsTrue()
    {
        var semVer = SemVer.Parse("1.0.0");

        semVer
            .Equals((object)SemVer.Parse("1.0.0"))
            .ShouldBeTrue();
    }

    [Test]
    public void Equals_Object_DifferentType_ReturnsFalse()
    {
        var semVer = SemVer.Parse("1.0.0");
        // Use a boxed int — cannot be implicitly converted to SemVer, so Equals(object?) is exercised
        object notASemVer = 42;

        semVer
            .Equals(notASemVer)
            .ShouldBeFalse();
    }

    [Test]
    public void GetHashCode_EqualVersions_HaveSameHashCode()
    {
        var hash1 = SemVer
            .Parse("1.2.3-alpha+build")
            .GetHashCode();

        var hash2 = SemVer
            .Parse("1.2.3-alpha+build")
            .GetHashCode();

        hash1.ShouldBe(hash2);
    }

    [Test]
    public void GetHashCode_DifferentVersions_HaveDifferentHashCodes()
    {
        var hash1 = SemVer
            .Parse("1.0.0")
            .GetHashCode();

        var hash2 = SemVer
            .Parse("2.0.0")
            .GetHashCode();

        hash1.ShouldNotBe(hash2);
    }

    [TestCase("1.0.0", "2.0.0", "1.5.0", true)] // strictly between
    [TestCase("2.0.0", "1.0.0", "1.5.0", true)] // reversed bounds — still works
    [TestCase("1.0.0", "2.0.0", "1.0.0", false)] // at lower bound (exclusive)
    [TestCase("1.0.0", "2.0.0", "2.0.0", false)] // at upper bound (exclusive)
    [TestCase("1.0.0", "2.0.0", "0.9.0", false)] // below range
    [TestCase("1.0.0", "2.0.0", "2.1.0", false)] // above range
    [TestCase("1.0.0", "1.0.0", "1.0.0", true)] // equal bounds, version equals
    [TestCase("1.0.0", "1.0.0", "2.0.0", false)] // equal bounds, version differs
    public void IsBetween_ReturnsExpectedResult(
        string firstBound,
        string secondBound,
        string version,
        bool expectedResult)
    {
        var first = SemVer.Parse(firstBound);
        var second = SemVer.Parse(secondBound);
        var semVer = SemVer.Parse(version);

        semVer
            .IsBetween(first, second)
            .ShouldBe(expectedResult);
    }

    [Test]
    public void ToSystemVersion_MapsMajorMinorPatch()
    {
        var semVer = SemVer.Parse("3.2.1");
        var sysVer = semVer.ToSystemVersion();

        sysVer.ShouldSatisfyAllConditions(() => sysVer.Major.ShouldBe(3),
            () => sysVer.Minor.ShouldBe(2),
            () => sysVer.Build.ShouldBe(1));
    }

    [Test]
    public void ToSystemVersion_DefaultParams_SilentlyIgnoresPreReleaseAndMetadata()
    {
        var semVer = SemVer.Parse("1.2.3-alpha+build.1");
        var sysVer = semVer.ToSystemVersion();

        sysVer.Major.ShouldBe(1);
        sysVer.Minor.ShouldBe(2);
        sysVer.Build.ShouldBe(3);
    }

    [Test]
    public void ToSystemVersion_ThrowIfPreRelease_WithPreRelease_ThrowsArgumentException() =>
        Should.Throw<ArgumentException>(() => SemVer
            .Parse("1.0.0-alpha")
            .ToSystemVersion(true));

    [Test]
    public void ToSystemVersion_ThrowIfPreRelease_WithoutPreRelease_Succeeds()
    {
        var sysVer = SemVer
            .Parse("1.0.0")
            .ToSystemVersion(true);

        sysVer.Major.ShouldBe(1);
    }

    [Test]
    public void ToSystemVersion_ThrowIfMetadata_WithMetadata_ThrowsArgumentException() =>
        Should.Throw<ArgumentException>(() => SemVer
            .Parse("1.0.0+build")
            .ToSystemVersion(throwIfContainsMetadata: true));

    [Test]
    public void ToSystemVersion_ThrowIfMetadata_WithoutMetadata_Succeeds()
    {
        var sysVer = SemVer
            .Parse("1.0.0")
            .ToSystemVersion(throwIfContainsMetadata: true);

        sysVer.Major.ShouldBe(1);
    }

    [TestCase(1, 0, 0, 0, false, 1, 0, 0)]
    [TestCase(1, 2, 3, 0, false, 1, 2, 3)]
    [TestCase(1, 2, 3, 4, false, 1, 2, 3)] // revision ignored when flag is false
    public void FromSystemVersion_MapsMajorMinorBuild(
        int major,
        int minor,
        int build,
        int revision,
        bool throwIfContainsRevision,
        int expectedMajor,
        int expectedMinor,
        int expectedPatch)
    {
        var version = new Version(major, minor, build, revision);
        var result = SemVer.FromSystemVersion(version, throwIfContainsRevision);

        result.ShouldSatisfyAllConditions(() => result.Major.ShouldBe(expectedMajor),
            () => result.Minor.ShouldBe(expectedMinor),
            () => result.Patch.ShouldBe(expectedPatch),
            () => result.PreRelease.ShouldBeNull(),
            () => result.Metadata.ShouldBeNull());
    }

    [Test]
    public void FromSystemVersion_ThrowIfRevision_WithNonZeroRevision_ThrowsArgumentException() =>
        Should.Throw<ArgumentException>(() => SemVer.FromSystemVersion(new(1, 2, 3, 4), true));

    [Test]
    public void FromSystemVersion_ThrowIfRevision_WithZeroRevision_Succeeds()
    {
        var result = SemVer.FromSystemVersion(new(1, 2, 3, 0), true);

        result.Patch.ShouldBe(3);
    }

    [TestCase("rc.1", 1)]
    [TestCase("alpha.123", 123)]
    [TestCase("SNAPSHOT-123", 123)]
    [TestCase("42", 42)]
    [TestCase("alpha.1227", 1227)]
    [TestCase("alpha3.valid", 3)] // single number embedded in identifier
    [TestCase("---RC-SNAPSHOT.12--N-A", 12)]
    [TestCase("beta", 0)] // no number
    [TestCase("alpha", 0)]
    [TestCase("prerelease", 0)]
    [TestCase("alpha.beta", 0)] // no number
    [TestCase("DEV-SNAPSHOT", 0)]
    [TestCase("1.2.3", 0)] // multiple numbers
    [TestCase("alpha3.4valid", 0)] // multiple numbers
    [TestCase("3alpha.4valid.1", 0)] // multiple numbers
    [TestCase("---RC-SNAPSHOT.12.9.1--.12", 0)]
    [TestCase("alpha-a.b-c-somethinglong", 0)]
    public void ExtractBuildNumber_ReturnsExpectedValue(string input, int expected) =>
        SemVer
            .ExtractBuildNumber(input)
            .ShouldBe(expected);

    [Test]
    public void ExtractBuildNumber_Null_ReturnsZero() =>
        SemVer
            .ExtractBuildNumber(null)
            .ShouldBe(0);

    [Test]
    public void ExtractBuildNumber_Empty_ReturnsZero() =>
        SemVer
            .ExtractBuildNumber("")
            .ShouldBe(0);

    [Test]
    public void ExtractBuildNumber_Whitespace_ReturnsZero() =>
        SemVer
            .ExtractBuildNumber("   ")
            .ShouldBe(0);

    [Test]
    public void BuildNumberFromPreRelease_WithSingleNumber_ReturnsNumber() =>
        SemVer
            .Parse("1.0.0-rc.42")
            .BuildNumberFromPreRelease
            .ShouldBe(42);

    [Test]
    public void BuildNumberFromPreRelease_WithNoNumber_ReturnsZero() =>
        SemVer
            .Parse("1.0.0-alpha")
            .BuildNumberFromPreRelease
            .ShouldBe(0);

    [Test]
    public void BuildNumberFromPreRelease_WithNoPreRelease_ReturnsZero() =>
        SemVer
            .Parse("1.0.0")
            .BuildNumberFromPreRelease
            .ShouldBe(0);

    [Test]
    public void BuildNumberFromMetadata_WithSingleNumber_ReturnsNumber() =>
        SemVer
            .Parse("1.0.0+build.99")
            .BuildNumberFromMetadata
            .ShouldBe(99);

    [Test]
    public void BuildNumberFromMetadata_WithNoMetadata_ReturnsZero() =>
        SemVer
            .Parse("1.0.0")
            .BuildNumberFromMetadata
            .ShouldBe(0);

    [TestCase("1.0.0")]
    [TestCase("1.0.0-alpha")]
    [TestCase("1.0.0+build")]
    [TestCase("1.0.0-alpha+build")]
    public void JsonSerialization_RoundTrip_PreservesVersion(string versionString)
    {
        var semVer = SemVer.Parse(versionString);

        var json = JsonSerializer.Serialize(semVer, JsonSerializerOptions.Default);
        var restored = JsonSerializer.Deserialize<SemVer>(json, JsonSerializerOptions.Default);

        restored.ShouldNotBeNull();

        restored
            .ToString()
            .ShouldBe(versionString);
    }
}
