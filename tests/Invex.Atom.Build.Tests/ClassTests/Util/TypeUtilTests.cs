namespace Invex.Atom.Build.Tests.ClassTests.Util;

[TestFixture]
internal sealed class TypeUtilTests
{
    [Test]
    public void Convert_NullInput_ReturnsDefault()
    {
        TypeUtil
            .Convert<string>(null, null)
            .ShouldBeNull();

        TypeUtil
            .Convert<int>(null, null)
            .ShouldBe(0);

        TypeUtil
            .Convert<bool>(null, null)
            .ShouldBe(false);
    }

    [Test]
    public void Convert_WithCustomConverter_UsesConverter()
    {
        var result = TypeUtil.Convert("99", _ => 42);
        result.ShouldBe(42);
    }

    [Test]
    public void Convert_String_ReturnsSameValue() =>
        TypeUtil
            .Convert<string>("hello", null)
            .ShouldBe("hello");

    [TestCase("true", true)]
    [TestCase("True", true)]
    [TestCase("TRUE", true)]
    [TestCase("false", false)]
    [TestCase("False", false)]
    public void Convert_Bool_ParsesCorrectly(string input, bool expected) =>
        TypeUtil
            .Convert<bool>(input, null)
            .ShouldBe(expected);

    [Test]
    public void Convert_Bool_InvalidInput_ThrowsException() =>
        // TryConvert fails, then TypeDescriptor.ConvertFromInvariantString throws
        Should.Throw<Exception>(() => TypeUtil.Convert<bool>("notabool", null));

    [Test]
    public void Convert_Char_SingleChar_ReturnsChar() =>
        TypeUtil
            .Convert<char>("A", null)
            .ShouldBe('A');

    [Test]
    public void Convert_Char_MultipleChars_ThrowsException() =>
        // TryConvertSimple returns false for multi-char; TypeDescriptor throws
        Should.Throw<Exception>(() => TypeUtil.Convert<char>("AB", null));

    [Test]
    public void Convert_Int_ParsesCorrectly() =>
        TypeUtil
            .Convert<int>("42", null)
            .ShouldBe(42);

    [Test]
    public void Convert_Int_InvalidInput_ThrowsException() =>
        Should.Throw<Exception>(() => TypeUtil.Convert<int>("notanint", null));

    [Test]
    public void Convert_Long_ParsesCorrectly() =>
        TypeUtil
            .Convert<long>("9999999999", null)
            .ShouldBe(9999999999L);

    [Test]
    public void Convert_Short_ParsesCorrectly() =>
        TypeUtil
            .Convert<short>("100", null)
            .ShouldBe((short)100);

    [Test]
    public void Convert_Byte_ParsesCorrectly() =>
        TypeUtil
            .Convert<byte>("255", null)
            .ShouldBe((byte)255);

    [Test]
    public void Convert_SByte_ParsesCorrectly() =>
        TypeUtil
            .Convert<sbyte>("-10", null)
            .ShouldBe((sbyte)-10);

    [Test]
    public void Convert_UInt_ParsesCorrectly() =>
        TypeUtil
            .Convert<uint>("4000000000", null)
            .ShouldBe(4000000000u);

    [Test]
    public void Convert_ULong_ParsesCorrectly() =>
        TypeUtil
            .Convert<ulong>("18446744073709551615", null)
            .ShouldBe(ulong.MaxValue);

    [Test]
    public void Convert_UShort_ParsesCorrectly() =>
        TypeUtil
            .Convert<ushort>("65535", null)
            .ShouldBe(ushort.MaxValue);

    [Test]
    public void Convert_Float_ParsesCorrectly() =>
        TypeUtil
            .Convert<float>("3.14", null)
            .ShouldBeInRange(3.13f, 3.15f);

    [Test]
    public void Convert_Double_ParsesCorrectly() =>
        TypeUtil
            .Convert<double>("2.718281828", null)
            .ShouldBeInRange(2.71, 2.72);

    [Test]
    public void Convert_Decimal_ParsesCorrectly() =>
        TypeUtil
            .Convert<decimal>("123.456", null)
            .ShouldBe(123.456m);

    [Test]
    public void Convert_Guid_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();

        TypeUtil
            .Convert<Guid>(guid.ToString(), null)
            .ShouldBe(guid);
    }

    [Test]
    public void Convert_Guid_InvalidInput_ThrowsException() =>
        Should.Throw<Exception>(() => TypeUtil.Convert<Guid>("not-a-guid", null));

    [Test]
    public void Convert_DateTime_ParsesCorrectly()
    {
        var dt = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Unspecified);

        TypeUtil
            .Convert<DateTime>("2024-06-15", null)
            .ShouldBe(dt);
    }

    [Test]
    public void Convert_TimeSpan_ParsesCorrectly() =>
        TypeUtil
            .Convert<TimeSpan>("01:30:00", null)
            .ShouldBe(TimeSpan.FromMinutes(90));

    [Test]
    public void Convert_DateOnly_ParsesCorrectly() =>
        TypeUtil
            .Convert<DateOnly>("2024-06-15", null)
            .ShouldBe(new(2024, 6, 15));

    [Test]
    public void Convert_TimeOnly_ParsesCorrectly() =>
        TypeUtil
            .Convert<TimeOnly>("14:30:00", null)
            .ShouldBe(new(14, 30, 0));

    [Test]
    public void Convert_Object_ReturnsSameStringValue() =>
        TypeUtil
            .Convert<object>("something", null)
            .ShouldBe("something");

    [Test]
    public void Convert_NullableInt_ParsesCorrectly() =>
        TypeUtil
            .Convert<int?>("42", null)
            .ShouldBe(42);

    [Test]
    public void Convert_NullableBool_ParsesCorrectly() =>
        TypeUtil
            .Convert<bool?>("true", null)
            .ShouldBe(true);

    [Test]
    public void Convert_NullableInt_InvalidInput_ThrowsException() =>
        Should.Throw<Exception>(() => TypeUtil.Convert<int?>("notanint", null));

    private enum Color
    {
        Red,
        Green,
        Blue,
    }

    [Test]
    public void Convert_Enum_ParsesCorrectly() =>
        TypeUtil
            .Convert<Color>("Green", null)
            .ShouldBe(Color.Green);

    [Test]
    public void Convert_Enum_CaseInsensitive_ParsesCorrectly() =>
        TypeUtil
            .Convert<Color>("blue", null)
            .ShouldBe(Color.Blue);

    [Test]
    public void Convert_Enum_InvalidValue_ThrowsException() =>
        // Enum.Parse throws; TypeDescriptor also throws for unknown values
        Should.Throw<Exception>(() => TypeUtil.Convert<Color>("Purple", null));

    [Test]
    public void Convert_StringArray_ParsesCommaSeparated()
    {
        var result = TypeUtil.Convert<string[]>("a,b,c", null);
        result.ShouldNotBeNull();
        result.ShouldBe(["a", "b", "c"]);
    }

    [Test]
    public void Convert_IntArray_ParsesCommaSeparated()
    {
        var result = TypeUtil.Convert<int[]>("1,2,3", null);
        result.ShouldNotBeNull();
        result.ShouldBe([1, 2, 3]);
    }

    [Test]
    public void Convert_SingleElementArray_ReturnsSingleElementArray()
    {
        var result = TypeUtil.Convert<string[]>("only", null);
        result.ShouldNotBeNull();
        result.Length.ShouldBe(1);

        result[0]
            .ShouldBe("only");
    }

    [Test]
    public void Convert_IReadOnlyListOfString_ParsesCommaSeparated()
    {
        var result = TypeUtil.Convert<IReadOnlyList<string>>("x,y,z", null);
        result.ShouldNotBeNull();
        result.ShouldBe(["x", "y", "z"]);
    }

    [Test]
    public void Convert_IReadOnlyListOfInt_ParsesCommaSeparated()
    {
        var result = TypeUtil.Convert<IReadOnlyList<int>>("10,20,30", null);
        result.ShouldNotBeNull();
        result.ShouldBe([10, 20, 30]);
    }

    [Test]
    public void Convert_UnsupportedGenericType_ThrowsNotSupportedException() =>
        Should.Throw<NotSupportedException>(() => TypeUtil.Convert<List<string>>("a,b", null));
}
