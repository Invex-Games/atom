namespace DecSm.Atom.StructuredText.Tests;

[TestFixture]
internal sealed class StructuredTextBuilderTests
{
    [Test]
    public void Constructor_DefaultIndentSize_IsTwo()
    {
        var builder = new StructuredTextBuilder();

        builder.IndentSize.ShouldBe(2);
    }

    [Test]
    public void Constructor_CustomIndentSize_SetsProperty()
    {
        var builder = new StructuredTextBuilder(4);

        builder.IndentSize.ShouldBe(4);
    }

    [Test]
    public void Constructor_InitialState()
    {
        var builder = new StructuredTextBuilder();

        builder.ShouldSatisfyAllConditions(() => builder.Indent.ShouldBe(0),
            () => builder.Version.ShouldBe(1),
            () => builder
                .ToString()
                .ShouldBeEmpty());
    }

    [Test]
    public void Write_WithText_AtZeroIndent_WritesWithoutLeadingSpaces()
    {
        var builder = new StructuredTextBuilder();

        builder.Write("hello");

        builder
            .ToString()
            .ShouldBe("hello");
    }

    [Test]
    public void Write_WithNull_WritesNothing()
    {
        var builder = new StructuredTextBuilder();

        builder.Write();

        builder
            .ToString()
            .ShouldBeEmpty();
    }

    [Test]
    public void Write_WithIndentFalse_SkipsLeadingSpaces()
    {
        var builder = new StructuredTextBuilder();
        using var _ = builder.WriteSection();

        builder.Write("text", false);

        builder
            .ToString()
            .ShouldBe("text");
    }

    [Test]
    public void Write_WithIndentTrue_AtIndentOne_PrefixesTwoSpaces()
    {
        var builder = new StructuredTextBuilder();
        using var _ = builder.WriteSection();

        builder.Write("text");

        builder
            .ToString()
            .ShouldBe("  text");
    }

    [Test]
    public void Write_WithCustomIndentSize_UsesCorrectSpaceCount()
    {
        var builder = new StructuredTextBuilder(4);
        using var _ = builder.WriteSection();

        builder.Write("text");

        builder
            .ToString()
            .ShouldBe("    text");
    }

    [Test]
    public void WriteLine_WithText_AppendsNewLine()
    {
        var builder = new StructuredTextBuilder();

        builder.WriteLine("line");

        builder
            .ToString()
            .ShouldBe("line" + Environment.NewLine);
    }

    [Test]
    public void WriteLine_WithNull_WritesEmptyNewLine()
    {
        var builder = new StructuredTextBuilder();

        builder.WriteLine();

        builder
            .ToString()
            .ShouldBe(Environment.NewLine);
    }

    [Test]
    public void WriteLine_AtIndentOne_PrefixesTwoSpaces()
    {
        var builder = new StructuredTextBuilder();
        using var _ = builder.WriteSection();

        builder.WriteLine("line");

        builder
            .ToString()
            .ShouldBe("  line" + Environment.NewLine);
    }

    [Test]
    public void WriteLine_MultipleLines_AppendInOrder()
    {
        var builder = new StructuredTextBuilder();

        builder.WriteLine("a");
        builder.WriteLine("b");

        var expected = "a" + Environment.NewLine + "b" + Environment.NewLine;

        builder
            .ToString()
            .ShouldBe(expected);
    }

    [Test]
    public void WriteSection_WithHeader_WritesHeaderAndIncrementsIndent()
    {
        var builder = new StructuredTextBuilder();

        using (builder.WriteSection("# Header"))
        {
            builder.Indent.ShouldBe(1);
            builder.WriteLine("body");
        }

        builder.ShouldSatisfyAllConditions(() => builder.Indent.ShouldBe(0),
            () => builder
                .ToString()
                .ShouldBe("# Header" + Environment.NewLine + "  body" + Environment.NewLine));
    }

    [Test]
    public void WriteSection_WithNullHeader_DoesNotWriteHeader()
    {
        var builder = new StructuredTextBuilder();

        using (builder.WriteSection())
            builder.Write("body");

        builder
            .ToString()
            .ShouldBe("  body");
    }

    [Test]
    public void WriteSection_WithEmptyHeader_DoesNotWriteHeader()
    {
        var builder = new StructuredTextBuilder();

        using (builder.WriteSection(""))
            builder.Write("body");

        builder
            .ToString()
            .ShouldBe("  body");
    }

    [Test]
    public void WriteSection_Dispose_DecrementsIndent()
    {
        var builder = new StructuredTextBuilder();

        var scope = builder.WriteSection();

        builder.Indent.ShouldBe(1);

        scope.Dispose();

        builder.Indent.ShouldBe(0);
    }

    [Test]
    public void WriteSection_Nested_IndentIncreasesEachLevel()
    {
        var builder = new StructuredTextBuilder();

        using (builder.WriteSection())
        {
            builder.Indent.ShouldBe(1);

            using (builder.WriteSection())
                builder.Indent.ShouldBe(2);

            builder.Indent.ShouldBe(1);
        }

        builder.Indent.ShouldBe(0);
    }

    [Test]
    public void WriteSection_AfterReset_DoesNotDecrementIndentOfOldScope()
    {
        var builder = new StructuredTextBuilder();

        var oldScope = builder.WriteSection();

        builder.Reset();

        builder
            .WriteSection()
            .Dispose(); // new scope increments and decrements back to 0

        // Disposing old scope should be a no-op because Version changed
        oldScope.Dispose();

        builder.Indent.ShouldBe(0);
    }

    [Test]
    public void Reset_ClearsStringBuilder()
    {
        var builder = new StructuredTextBuilder();

        builder.WriteLine("before");
        builder.Reset();

        builder
            .ToString()
            .ShouldBeEmpty();
    }

    [Test]
    public void Reset_ResetsIndentToZero()
    {
        var builder = new StructuredTextBuilder();

        using (builder.WriteSection())
            builder.Reset();

        builder.Indent.ShouldBe(0);
    }

    [Test]
    public void Reset_IncrementsVersion()
    {
        var builder = new StructuredTextBuilder();
        var originalVersion = builder.Version;

        builder.Reset();

        builder.Version.ShouldBe(originalVersion + 1);
    }

    [Test]
    public void Reset_WithCustomIndentSize_SetsNewIndentSize()
    {
        var builder = new StructuredTextBuilder();

        builder.Reset(4);

        builder.IndentSize.ShouldBe(4);
    }

    [Test]
    public void ToString_ReturnsStringBuilderContent()
    {
        var builder = new StructuredTextBuilder();

        builder.Write("hello");

        builder
            .ToString()
            .ShouldBe(builder.StringBuilder.ToString());
    }
}
