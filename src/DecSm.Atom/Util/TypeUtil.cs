namespace DecSm.Atom.Util;

/// <summary>
///     Provides flexible type conversion utilities, handling primitives, arrays, and generic collections.
/// </summary>
/// <remarks>
///     This utility class is designed to handle complex type conversions beyond standard .NET capabilities,
///     with special handling for arrays (from comma-separated strings) and generic collections. It supports
///     custom converters for specialized types and is optimized for performance-sensitive scenarios like
///     parameter resolution.
/// </remarks>
[PublicAPI]
public static class TypeUtil
{
    /// <summary>
    ///     Converts a string value to a specified target type, with optional custom conversion logic.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="stringValue">The string value to convert. For array types, a comma-separated string is expected.</param>
    /// <param name="converter">An optional custom conversion function that takes precedence over built-in logic.</param>
    /// <returns>The converted value, or the default value for the type if conversion is not possible.</returns>
    /// <remarks>
    ///     The method uses a prioritized strategy: custom converter, array conversion, generic collection conversion,
    ///     and finally, standard <see cref="TypeDescriptor" /> conversion. It is designed to be AOT-friendly by
    ///     attempting simple, reflection-free conversions first.
    /// </remarks>
    [UnconditionalSuppressMessage("Trimming",
        "IL2026",
        Justification =
            "May fall back to the TypeDescriptor-based Convert(string, Type) helper (marked RequiresUnreferencedCode) when no custom converter, array/generic path, or AOT-safe TryConvert path can handle the input. This is a last-resort conversion; in trimmed/AOT builds callers should prefer passing explicit converters for unsupported types.")]
    public static T? Convert<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string? stringValue,
        Func<string?, T?>? converter) =>
        stringValue is null
            ? default
            : converter is not null
                ? converter(stringValue)
                : typeof(T).IsArray
                    ? ConvertArray<T>(stringValue)
                    : typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() != typeof(Nullable<>)
                        ? ConvertGeneric<T>(stringValue, typeof(T))
                        : TryConvert(stringValue, typeof(T), out var simple) && simple is T simpleT
                            ? simpleT
                            : Convert(stringValue, typeof(T)) is T tValue
                                ? tValue
                                : default;

    /// <summary>
    ///     Converts a string to a specified type using <see cref="TypeDescriptor" />. This method is not trim-safe.
    /// </summary>
    [RequiresUnreferencedCode(
        "TypeDescriptor.GetConverter(Type) is not trimming-safe. Converters for some types may be removed during trimming.")]
    [UnconditionalSuppressMessage("Trimming",
        "IL2067",
        Justification =
            "TypeDescriptor.GetConverter(Type) uses runtime-discovered converters that can be trimmed away. The 'type' parameter is annotated with DynamicallyAccessedMembers(All), but the linker cannot guarantee presence of external converter types, so IL2067 can still be produced. This method is an explicit best-effort fallback for non-AOT-friendly conversions; AOT callers should avoid it or provide explicit converters.")]
    private static object? Convert(
        string value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type) =>
        TypeDescriptor
            .GetConverter(type)
            .ConvertFromInvariantString(value);

    /// <summary>
    ///     Attempts to convert a string to a specified type using AOT-friendly methods.
    /// </summary>
    private static bool TryConvert(string value, Type type, out object? result)
    {
        // Handle Nullable<T> by unwrapping the underlying type
        if (IsNullableValueType(type, out var underlying))
        {
            if (TryConvert(value, underlying!, out var inner))
            {
                result = inner;

                return true;
            }

            result = null;

            return false;
        }

        if (TryConvertSimple(value, type, out result))
            return true;

        // Enums (non-flag) and flags
        if (type.IsEnum)
        {
            try
            {
                result = Enum.Parse(type, value, true);

                return true;
            }
            catch
            {
                // ignored
            }
        }

        // If type is just 'object', we can just return the string value
        if (type == typeof(object))
        {
            result = value;

            return true;
        }

        result = null;

        return false;
    }

    /// <summary>
    ///     Checks if a type is a nullable value type and returns its underlying type.
    /// </summary>
    private static bool IsNullableValueType(Type type, out Type? underlying)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            underlying = Nullable.GetUnderlyingType(type);

            return true;
        }

        underlying = null;

        return false;
    }

    /// <summary>
    ///     Attempts to convert a string to common simple types without using reflection-heavy APIs.
    /// </summary>
    private static bool TryConvertSimple(string value, Type type, out object? result)
    {
        // String is already simple
        if (type == typeof(string))
        {
            result = value;

            return true;
        }

        // Char
        if (type == typeof(char))
        {
            if (value.Length == 1)
            {
                result = value[0];

                return true;
            }

            result = null;

            return false;
        }

        const NumberStyles styleInteger = NumberStyles.Integer;
        const NumberStyles styleFloat = NumberStyles.Float | NumberStyles.AllowThousands;
        var ci = CultureInfo.InvariantCulture;

        // Boolean
        if (type == typeof(bool))
        {
            if (bool.TryParse(value, out var b))
            {
                result = b;

                return true;
            }

            result = null;

            return false;
        }

        // Integral types
        if (type == typeof(int))
        {
            if (int.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(long))
        {
            if (long.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(short))
        {
            if (short.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(byte))
        {
            if (byte.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(sbyte))
        {
            if (sbyte.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(uint))
        {
            if (uint.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(ulong))
        {
            if (ulong.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(ushort))
        {
            if (ushort.TryParse(value, styleInteger, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        // Floating point / decimal
        if (type == typeof(float))
        {
            if (float.TryParse(value, styleFloat, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(double))
        {
            if (double.TryParse(value, styleFloat, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(decimal))
        {
            if (decimal.TryParse(value, styleFloat, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        // Guid
        if (type == typeof(Guid))
        {
            if (Guid.TryParse(value, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        // Date/time types
        if (type == typeof(DateTime))
        {
            if (DateTime.TryParse(value, ci, DateTimeStyles.None, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(TimeSpan))
        {
            if (TimeSpan.TryParse(value, ci, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        #if NET6_0_OR_GREATER
        if (type == typeof(DateOnly))
        {
            if (DateOnly.TryParse(value, ci, DateTimeStyles.None, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }

        if (type == typeof(TimeOnly))
        {
            if (TimeOnly.TryParse(value, ci, DateTimeStyles.None, out var v))
            {
                result = v;

                return true;
            }

            result = null;

            return false;
        }
        #endif

        result = null;

        return false;
    }

    /// <summary>
    ///     Converts a comma-separated string into an array of the specified type.
    /// </summary>
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL2026:RequiresUnreferencedCode",
        Justification =
            "ConvertArray<T> may fall back to Convert(string, Type), which is marked RequiresUnreferencedCode. We first attempt AOT-safe TryConvert for each element and only use the TypeDescriptor-based fallback if necessary. Generic parameter T is annotated with DynamicallyAccessedMembers(All), improving linker preservation of required members; nonetheless callers targeting AOT should prefer element types supported by TryConvert or supply explicit converters.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL2072:UnrecognizedReflectionPattern",
        Justification =
            "The analyzer cannot prove at compile time the safety of using typeof(T).GetElementType() and Array.CreateInstance for arbitrary T. This method only operates on array types (T is TElement[]), and the reflection usage is limited to creating an array and passing the element Type into narrowly-scoped conversion helpers. The remaining risky path is the explicit RUC-marked fallback, which is documented above.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL3050:RequiresUnreferencedCode",
        Justification =
            "May fall back to the TypeDescriptor-based Convert(string, Type) helper when no AOT-safe TryConvert path can handle the input. This is a last-resort conversion; in trimmed/AOT builds callers should prefer passing explicit converters for unsupported types.")]
    private static T ConvertArray<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string value)
    {
        var elementType = typeof(T).GetElementType()!;
        var values = value.Split(',');

        var array = Array.CreateInstance(elementType, values.Length);

        for (var i = 0; i < values.Length; i++)
        {
            array.SetValue(TryConvert(values[i], elementType, out var elem)
                    ? elem
                    : Convert(values[i], elementType),
                i);
        }

        return (T)(object)array;
    }

    /// <summary>
    ///     Converts a comma-separated string into a supported generic collection type.
    /// </summary>
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL2026:RequiresUnreferencedCode",
        Justification =
            "ConvertGeneric<T> can fall back to Convert(string, Type) for each element if AOT-safe TryConvert fails. That helper is intentionally marked RequiresUnreferencedCode. Here we only support IReadOnlyList<TElement> and implement it by constructing a TElement[]; all conversions attempt AOT-safe parsing first.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL2062:UnrecognizedReflectionPattern",
        Justification =
            "This method inspects the generic type (GetGenericTypeDefinition/GetGenericArguments) and creates an array via Array.CreateInstance. The usage is bounded to supported shape IReadOnlyList<TElement>. The generic parameter T is annotated with DynamicallyAccessedMembers(All) at the call site, improving member preservation. Any remaining reflection risk exists only on the documented RUC-marked fallback path.")]
    [UnconditionalSuppressMessage("AssemblyLoadTrimming",
        "IL3050:RequiresUnreferencedCode",
        Justification =
            "May fall back to the TypeDescriptor-based Convert(string, Type) helper when no AOT-safe TryConvert path can handle the input. This is a last-resort conversion; in trimmed/AOT builds callers should prefer passing explicit converters for unsupported types.")]
    private static T ConvertGeneric<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string value,
        Type type)
    {
        var genericType = type.GetGenericTypeDefinition();

        var genericArgument = type
            .GetGenericArguments()[0];

        if (genericType == typeof(IReadOnlyList<>))
        {
            var values = value.Split(',');

            // Build a TElement[] which implements IReadOnlyList<TElement>
            var array = Array.CreateInstance(genericArgument, values.Length);

            for (var i = 0; i < values.Length; i++)
                array.SetValue(TryConvert(values[i], genericArgument, out var elem)
                        ? elem
                        : Convert(values[i], genericArgument),
                    i);

            return (T)(object)array;
        }

        throw new NotSupportedException($"Generic type '{type}' is not supported.");
    }
}
