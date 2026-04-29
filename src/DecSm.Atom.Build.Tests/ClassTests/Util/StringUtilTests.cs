namespace DecSm.Atom.Build.Tests.ClassTests.Util;

[TestFixture]
public class StringUtilTests
{
    [Test]
    public void GetLevenshteinDistance_BothNullOrEmpty_ReturnsZero()
    {
        string? a = null;
        string? b = null;

        a
            .GetLevenshteinDistance(b)
            .ShouldBe(0);
    }

    [Test]
    public void GetLevenshteinDistance_FirstNullOrEmpty_ReturnsLengthOfSecond()
    {
        string? a = null;

        a
            .GetLevenshteinDistance("abc")
            .ShouldBe(3);
    }

    [Test]
    public void GetLevenshteinDistance_SecondNullOrEmpty_ReturnsLengthOfFirst() =>
        "abc"
            .GetLevenshteinDistance(null)
            .ShouldBe(3);

    [Test]
    public void GetLevenshteinDistance_IdenticalStrings_ReturnsZero() =>
        "test"
            .GetLevenshteinDistance("test")
            .ShouldBe(0);

    [Test]
    public void GetLevenshteinDistance_KittenToSitting_ReturnsThree() =>
        // classic Levenshtein example
        "kitten"
            .GetLevenshteinDistance("sitting")
            .ShouldBe(3);

    [Test]
    public void GetLevenshteinDistance_SingleCharDiff_ReturnsOne() =>
        "abc"
            .GetLevenshteinDistance("abd")
            .ShouldBe(1);

    [Test]
    public void GetLevenshteinDistance_CompletelyDifferent_ReturnsSumOfLengths() =>
        // "abc" → "xyz": 3 substitutions
        "abc"
            .GetLevenshteinDistance("xyz")
            .ShouldBe(3);

    [Test]
    public void GetLevenshteinDistance_IsSymmetric()
    {
        var d1 = "sunday".GetLevenshteinDistance("saturday");
        var d2 = "saturday".GetLevenshteinDistance("sunday");
        d1.ShouldBe(d2);
    }

    [Test]
    public void GetLevenshteinDistance_EmptyToNonempty_ReturnsLength() =>
        ""
            .GetLevenshteinDistance("hello")
            .ShouldBe(5);

    [Test]
    public void SanitizeForLogging_NullInput_ReturnsNull()
    {
        string? s = null;

        s
            .SanitizeForLogging()
            .ShouldBeNull();
    }

    [Test]
    public void SanitizeForLogging_EmptyInput_ReturnsEmpty() =>
        ""
            .SanitizeForLogging()
            .ShouldBe("");

    [Test]
    public void SanitizeForLogging_NoNewlines_ReturnsSameString() =>
        "Normal text"
            .SanitizeForLogging()
            .ShouldBe("Normal text");

    [Test]
    public void SanitizeForLogging_WithNewline_ReplacesWithSpace() =>
        "Hello\nWorld"
            .SanitizeForLogging()
            .ShouldBe("Hello World");

    [Test]
    public void SanitizeForLogging_WithCarriageReturn_ReplacesWithSpace() =>
        "Hello\rWorld"
            .SanitizeForLogging()
            .ShouldBe("Hello World");

    [Test]
    public void SanitizeForLogging_WithCrLf_ReplacesWithSpace() =>
        "Hello\r\nWorld"
            .SanitizeForLogging()
            .ShouldBe("Hello World");

    [Test]
    public void SanitizeForLogging_StripNewlinesFalse_PreservesNewlines()
    {
        var result = "Hello\nWorld".SanitizeForLogging(false);
        result.ShouldBe("Hello\nWorld");
    }

    [Test]
    public void SanitizeForLogging_WithinMaxLength_ReturnsEntireString()
    {
        var s = new string('A', 100);

        s
            .SanitizeForLogging(maxLength: 100)
            .ShouldBe(s);
    }

    [Test]
    public void SanitizeForLogging_ExceedsMaxLength_TruncatesWithEllipsis()
    {
        var s = new string('A', 200);
        var result = s.SanitizeForLogging(maxLength: 100);
        result.ShouldStartWith("AAA");
        result.Length.ShouldBe(103); // 100 chars + "..."
        result.ShouldEndWith("...");
    }

    [Test]
    public void SanitizeSecrets_NullInput_ReturnsNull()
    {
        string? s = null;

        s
            .SanitizeSecrets([])
            .ShouldBeNull();
    }

    [Test]
    public void SanitizeSecrets_EmptyInput_ReturnsEmpty() =>
        ""
            .SanitizeSecrets(["secret"])
            .ShouldBe("");

    [Test]
    public void SanitizeSecrets_NoSecrets_ReturnsUnchanged() =>
        "Hello World"
            .SanitizeSecrets([])
            .ShouldBe("Hello World");

    [Test]
    public void SanitizeSecrets_EmptySecretEntries_ReturnsUnchanged() =>
        "Hello World"
            .SanitizeSecrets(["", null!])
            .ShouldBe("Hello World");

    [Test]
    public void SanitizeSecrets_LongSecret_ReplacesWithFiveStars() =>
        "This is a SecretValue in text."
            .SanitizeSecrets(["SecretValue"])
            .ShouldBe("This is a ***** in text.");

    [Test]
    public void SanitizeSecrets_ShortSecret_ReplacesWithMatchingStarCount() =>
        // "abc" has length 3 → 3 stars
        "prefix abc suffix"
            .SanitizeSecrets(["abc"])
            .ShouldBe("prefix *** suffix");

    [Test]
    public void SanitizeSecrets_MultipleSecrets_ReplacesAll()
    {
        var result = "alpha is secret1 and beta is secret2".SanitizeSecrets(["secret1", "secret2"]);
        result.ShouldNotContain("secret1");
        result.ShouldNotContain("secret2");
    }

    [Test]
    public void SanitizeSecrets_CaseInsensitive_ReplacesSecret() =>
        "SECRETVALUE"
            .SanitizeSecrets(["secretvalue"])
            .ShouldBe("*****");

    [Test]
    public void SanitizeSecrets_StringShorterThanAllSecrets_ReturnsUnchanged() =>
        // "abc" is 3 chars, secret is 10 chars → the input is shorter than any secret
        "abc"
            .SanitizeSecrets(["longsecret"])
            .ShouldBe("abc");
}
