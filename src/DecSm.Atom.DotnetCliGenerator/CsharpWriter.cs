namespace Atom;

/// <summary>
///     Represents a parameter for a C# method, including its type, name, default value, and XML description.
/// </summary>
/// <param name="Type">The C# type of the parameter (e.g., "string", "int?").</param>
/// <param name="Name">The name of the parameter (e.g., "projectName").</param>
/// <param name="DefaultValue">Optional. The default value for the parameter, if any (e.g., "null", "\"Release\"").</param>
/// <param name="XmlDescription">Optional. The XML documentation description for the parameter.</param>
[PublicAPI]
public sealed record MethodParam(string Type, string Name, string? DefaultValue = null, string? XmlDescription = null);

/// <summary>
///     Represents the getter part of a C# property.
/// </summary>
/// <param name="Body">
///     Optional. An action to write custom code for the getter's body. If null, a default `get;` is
///     written.
/// </param>
[PublicAPI]
public sealed record PropertyGetter(Action<CsharpWriter>? Body);

/// <summary>
///     Represents the setter part of a C# property.
/// </summary>
/// <param name="Body">
///     Optional. An action to write custom code for the setter's body. If null, a default `set;` or `init;`
///     is written.
/// </param>
/// <param name="InitOnly">A boolean indicating if the setter is `init` only.</param>
[PublicAPI]
public sealed record PropertySetter(Action<CsharpWriter>? Body, bool InitOnly);

/// <summary>
///     A utility class for generating C# code dynamically.
/// </summary>
/// <remarks>
///     This class provides methods to write C# code constructs like classes, methods, properties,
///     and manage indentation, making it suitable for source code generation tasks.
/// </remarks>
[PublicAPI]
public sealed partial class CsharpWriter
{
    private readonly StringBuilder _sb = new();

    private int _indent;

    /// <summary>
    ///     Gets an <see cref="IDisposable" /> that, when disposed, decreases the current indentation level.
    /// </summary>
    /// <remarks>
    ///     Use this in a `using` statement to automatically manage indentation for code blocks.
    /// </remarks>
    public IDisposable Indent
    {
        get
        {
            _indent++;

            return new ActionScope(() => _indent--);
        }
    }

    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex UppercaseRegex { get; }

    /// <summary>
    ///     Writes an opening curly brace, increases indentation, and returns an <see cref="IDisposable" />
    ///     that writes a closing curly brace and decreases indentation when disposed.
    /// </summary>
    /// <param name="text">Optional. Text to write before the opening curly brace.</param>
    /// <returns>An <see cref="IDisposable" /> to manage the code block.</returns>
    public IDisposable Block(string text = "")
    {
        if (text.Length > 0)
            WriteLine(text);

        WriteLine("{");

        _indent++;

        return new ActionScope(() =>
        {
            _indent--;
            WriteLine("}");
        });
    }

    /// <summary>
    ///     Writes a string to the internal buffer, optionally without indentation.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="noIndent">If <c>true</c>, the text is written without the current indentation.</param>
    public void Write(string text, bool noIndent = false) =>
        _sb.Append(noIndent
            ? text
            : $"{new(' ', _indent * 4)}{text}");

    /// <summary>
    ///     Writes a line of text to the internal buffer, followed by a new line, optionally without indentation.
    /// </summary>
    /// <param name="line">The text line to write.</param>
    /// <param name="noIndent">If <c>true</c>, the text is written without the current indentation.</param>
    public void WriteLine(string line, bool noIndent = false) =>
        _sb.AppendLine(noIndent
            ? line
            : $"{new(' ', _indent * 4)}{line}");

    /// <summary>
    ///     Returns the generated C# code as a single string.
    /// </summary>
    /// <returns>The generated C# code.</returns>
    public override string ToString() =>
        _sb.ToString();

    /// <summary>
    ///     Writes a C# property definition.
    /// </summary>
    /// <param name="access">The access modifier (e.g., "public", "internal").</param>
    /// <param name="type">The type of the property (e.g., "string", "int?").</param>
    /// <param name="name">The name of the property.</param>
    /// <param name="getter">Optional. Configuration for the property's getter.</param>
    /// <param name="setter">Optional. Configuration for the property's setter.</param>
    /// <param name="defaultValue">Optional. The default value for the property, if any.</param>
    /// <param name="xmlSummary">Optional. The XML documentation summary for the property.</param>
    public void WriteProperty(
        string access,
        string type,
        string name,
        PropertyGetter? getter = null,
        PropertySetter? setter = null,
        string? defaultValue = null,
        string? xmlSummary = null)
    {
        if (xmlSummary is not null)
        {
            WriteLine("/// <summary>");

            foreach (var line in xmlSummary
                         .Replace("<", "&lt;")
                         .Replace(">", "&gt;")
                         .Replace("\r", "")
                         .Split('\n'))
                WriteLine($"///     {line}");

            WriteLine("/// </summary>");
        }

        WriteLine(access.Length > 0
            ? $"{access} {type} {name}"
            : $"{type} {name}");

        using (Block())
        {
            if (getter is not null)
            {
                if (getter.Body is not null)
                    getter.Body(this);
                else
                    WriteLine("get;");
            }

            if (setter is not null)
            {
                if (setter.Body is not null)
                    using (Block(setter.InitOnly
                               ? "init"
                               : "set"))
                        setter.Body(this);
                else
                    WriteLine(setter.InitOnly
                        ? "init;"
                        : "set;");
            }
        }

        if (defaultValue is not null)
            WriteLine($"= {defaultValue};");

        WriteLine(string.Empty);
    }

    /// <summary>
    ///     Writes a C# method definition.
    /// </summary>
    /// <param name="access">The access modifier (e.g., "public", "internal").</param>
    /// <param name="returnType">The return type of the method (e.g., "void", "Task&lt;int&gt;").</param>
    /// <param name="name">The name of the method.</param>
    /// <param name="parameters">A collection of <see cref="MethodParam" /> defining the method's parameters.</param>
    /// <param name="body">
    ///     Optional. An action to write the custom code for the method's body. If null, an abstract or
    ///     interface method is written.
    /// </param>
    /// <param name="expressionBodied">If <c>true</c>, the method is written as an expression-bodied member.</param>
    /// <param name="xmlSummary">Optional. The XML documentation summary for the method.</param>
    public void WriteMethod(
        string access,
        string returnType,
        string name,
        IEnumerable<MethodParam?> parameters,
        Action<CsharpWriter>? body = null,
        bool expressionBodied = false,
        string? xmlSummary = null)
    {
        var validParameters = parameters
            .OfType<MethodParam>()
            .ToList();

        if (xmlSummary is not null)
        {
            WriteLine("/// <summary>");

            foreach (var line in xmlSummary
                         .Replace("<", "&lt;")
                         .Replace(">", "&gt;")
                         .Replace("\r", "")
                         .Split('\n'))
                WriteLine($"///     {line}");

            WriteLine("/// </summary>");

            foreach (var (paramName, paramDesc) in validParameters
                         .Select(x => (x.Name, x.XmlDescription))
                         .Where(x => x.XmlDescription is not null))
            {
                WriteLine($"/// <param name=\"{paramName.Replace("<", "&lt;").Replace(">", "&gt;")}\">");

                foreach (var line in paramDesc!
                             .Replace("<", "&lt;")
                             .Replace(">", "&gt;")
                             .Replace("\r", "")
                             .Split('\n'))
                    WriteLine($"///     {line}");

                WriteLine("/// </param>");
            }
        }

        if (body is null)
        {
            if (validParameters.Count != 0)
            {
                WriteLine(access.Length > 0
                    ? $"{access} {returnType} {name}("
                    : $"{returnType} {name}(");

                using (Indent)
                {
                    for (var i = 0; i < validParameters.Count; i++)
                    {
                        var (paramType, paramName, defaultValue, _) = validParameters[i];

                        Write(defaultValue is { Length: > 0 }
                            ? $"{paramType} {paramName} = {defaultValue}"
                            : $"{paramType} {paramName}");

                        WriteLine(i < validParameters.Count - 1
                                ? ","
                                : string.Empty,
                            true);
                    }
                }

                WriteLine(");");
            }
            else
            {
                WriteLine($"{access} {returnType} {name}();");
            }

            WriteLine(string.Empty);

            return;
        }

        if (validParameters.Count != 0)
        {
            WriteLine($"{access} {returnType} {name}(");

            using (Indent)
            {
                for (var i = 0; i < validParameters.Count; i++)
                {
                    var (paramType, paramName, defaultValue, _) = validParameters[i];

                    Write(defaultValue is { Length: > 0 }
                        ? $"{paramType} {paramName} = {defaultValue}"
                        : $"{paramType} {paramName}");

                    WriteLine(i < validParameters.Count - 1
                            ? ","
                            : string.Empty,
                        true);
                }
            }

            Write(")");
        }
        else
        {
            Write($"{access} {returnType} {name}()");
        }

        if (expressionBodied)
        {
            WriteLine(" =>", true);

            using (Indent)
                body(this);
        }
        else
        {
            using (Block())
                body(this);
        }

        WriteLine(string.Empty);
    }

    /// <summary>
    ///     Converts a string to PascalCase, optionally lower-casing the first word.
    /// </summary>
    /// <param name="text">The input string to convert.</param>
    /// <param name="lowerFirstWord">
    ///     If <c>true</c>, the first character of the resulting PascalCase string will be
    ///     lower-cased.
    /// </param>
    /// <returns>The PascalCase representation of the input string.</returns>
    /// <remarks>
    ///     This method handles various delimiters (hyphens, underscores, spaces, colons) and
    ///     attempts to convert all-uppercase words to title case before applying PascalCase.
    /// </remarks>
    public static string ToPascalCase(string text, bool lowerFirstWord = false)
    {
        // If all letters are uppercase, convert to lowercase
        if (text.All(x => char.IsUpper(x) || !char.IsLetter(x)))
            text = text.ToLowerInvariant();

        // Replace '|' with 'or'
        text = text.Replace("|", "or");

        // Add a space before each uppercase letter that is not at the start, and is not followed by another uppercase letter
        text = UppercaseRegex.Replace(text, " ");

        // Split by non-letter characters
        var parts = text.Split(['-', '_', ' ', ':'], StringSplitOptions.RemoveEmptyEntries);

        var pascalCase = string.Concat(parts.Select(part => char.ToUpperInvariant(part[0]) +
                                                            part[1..]
                                                                .ToLowerInvariant()));

        return lowerFirstWord
            ? char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..]
            : pascalCase;
    }
}
