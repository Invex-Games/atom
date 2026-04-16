namespace Atom;

/// <summary>
///     Provides functionality to generate C# code for .NET CLI commands based on a parsed schema.
/// </summary>
/// <remarks>
///     This static class is used by the <see cref="Build" /> definition in <c>DecSm.Atom.DotnetCliGenerator</c>
///     to create strongly-typed interfaces, implementations, and option classes for `dotnet` commands.
/// </remarks>
public static class DotnetCliGenerator
{
    /// <summary>
    ///     Generates the C# interface code for a given .NET CLI command.
    /// </summary>
    /// <param name="writer">The <see cref="CsharpWriter" /> to write the code to.</param>
    /// <param name="command">The <see cref="Command" /> object representing the .NET CLI command.</param>
    /// <remarks>
    ///     This method generates a method signature within the `IDotnetCli` interface for the command,
    ///     including overloads for single arguments and path-like arguments where applicable.
    /// </remarks>
    public static void GenerateInterfaceCode(CsharpWriter writer, Command command)
    {
        using (writer.Block("public partial interface IDotnetCli"))
        {
            GenerateCommandMethod(writer, command, true);

            if (command.Arguments.Count is 0)
                return;

            var singleArg = command.Arguments[0];

            // Generate overload for single argument
            if (singleArg.ValueType.EndsWith("[]", StringComparison.Ordinal))
                GenerateCommandMethod(writer,
                    command with
                    {
                        Arguments =
                        [
                            command.Arguments[0] with
                            {
                                ValueType = singleArg.ValueType.Replace("[]", string.Empty),
                            },
                        ],
                    },
                    true);

            // Generate overload for a single argument that is a path
            if (singleArg.ValueType is "System.String" or "System.String[]" &&
                (singleArg.Name.Contains("path", StringComparison.OrdinalIgnoreCase) ||
                 (singleArg.Description ?? "").Contains("path", StringComparison.OrdinalIgnoreCase) ||
                 singleArg.Name.Contains("file", StringComparison.OrdinalIgnoreCase) ||
                 (singleArg.Description ?? "").Contains("file", StringComparison.OrdinalIgnoreCase)))
                GenerateCommandMethod(writer,
                    command with
                    {
                        Arguments =
                        [
                            command.Arguments[0] with
                            {
                                ValueType = "DecSm.Atom.Paths.RootedPath",
                            },
                        ],
                    },
                    true);
        }

        writer.WriteLine(string.Empty);
    }

    /// <summary>
    ///     Generates the C# implementation code for a given .NET CLI command.
    /// </summary>
    /// <param name="writer">The <see cref="CsharpWriter" /> to write the code to.</param>
    /// <param name="command">The <see cref="Command" /> object representing the .NET CLI command.</param>
    /// <remarks>
    ///     This method generates the actual implementation of the command within the `DotnetCli` class,
    ///     including overloads for single arguments and path-like arguments where applicable.
    ///     It constructs the command-line arguments and invokes the `dotnet` process.
    /// </remarks>
    public static void GenerateImplementationCode(CsharpWriter writer, Command command)
    {
        using (writer.Block("internal partial class DotnetCli"))
        {
            GenerateCommandMethod(writer, command, false);

            if (command.Arguments.Count is 0)
                return;

            var singleArg = command.Arguments[0];

            // Generate overload for single argument
            if (singleArg.ValueType.EndsWith("[]", StringComparison.Ordinal))
                GenerateCommandMethod(writer,
                    command with
                    {
                        Arguments =
                        [
                            command.Arguments[0] with
                            {
                                ValueType = singleArg.ValueType.Replace("[]", string.Empty),
                            },
                        ],
                    },
                    false);

            // Generate overload for a single argument that is a path
            if (singleArg.ValueType is "System.String" or "System.String[]" &&
                (singleArg.Name.Contains("path", StringComparison.OrdinalIgnoreCase) ||
                 (singleArg.Description ?? "").Contains("path", StringComparison.OrdinalIgnoreCase) ||
                 singleArg.Name.Contains("file", StringComparison.OrdinalIgnoreCase) ||
                 (singleArg.Description ?? "").Contains("file", StringComparison.OrdinalIgnoreCase)))
                GenerateCommandMethod(writer,
                    command with
                    {
                        Arguments =
                        [
                            command.Arguments[0] with
                            {
                                ValueType = "DecSm.Atom.Paths.RootedPath",
                            },
                        ],
                    },
                    false);
        }

        writer.WriteLine(string.Empty);
    }

    /// <summary>
    ///     Generates the C# options record for a given .NET CLI command.
    /// </summary>
    /// <param name="writer">The <see cref="CsharpWriter" /> to write the code to.</param>
    /// <param name="command">The <see cref="Command" /> object representing the .NET CLI command.</param>
    /// <remarks>
    ///     This method creates a `sealed record` named after the command (e.g., `BuildOptions`)
    ///     that contains properties for each of the command's options. It also includes an
    ///     overridden `ToString()` method to correctly format these options into command-line arguments.
    /// </remarks>
    public static void GenerateOptionsCode(CsharpWriter writer, Command command)
    {
        if (command.Options.Count is 0)
            return;

        writer.WriteLine("[PublicAPI]");

        using (writer.Block($"public sealed record {CsharpWriter.ToPascalCase(command.Name)}Options"))
        {
            foreach (var option in command.Options.Where(x => x.ValueType is not (null or "System.Void")))
                writer.WriteProperty("public",
                    IsKnownAotFriendlyType(option.ValueType)
                        ? $"{FormatType(option.ValueType)}?"
                        : "string?",
                    CsharpWriter.ToPascalCase(option.Name),
                    new(null),
                    new(null, true),
                    null,
                    option.Description);

            writer.WriteMethod("public override",
                "string",
                "ToString",
                Array.Empty<MethodParam>(),
                bodyWriter =>
                {
                    bodyWriter.WriteLine("return string.Join(' ',");

                    using (bodyWriter.Block("new[]"))
                    {
                        foreach (var option in command.Options.Where(x => x.ValueType is not (null or "System.Void")))
                        {
                            bodyWriter.WriteLine(option.ValueType is "System.Boolean"
                                ? $"{CsharpWriter.ToPascalCase(option.Name)} is true"
                                : $"{CsharpWriter.ToPascalCase(option.Name)} is not null");

                            using (bodyWriter.Indent)
                            {
                                bodyWriter.WriteLine(
                                    $"? {FormatToPrint(option.ValueType, option.Name, CsharpWriter.ToPascalCase(option.Name))}");

                                bodyWriter.WriteLine(": null,");
                            }
                        }
                    }

                    bodyWriter.WriteLine(".Where(x => x is { Length: > 0 }));");
                });
        }

        writer.WriteLine(string.Empty);
    }

    /// <summary>
    ///     Generates a C# method for a given .NET CLI command, either as an interface stub or an implementation.
    /// </summary>
    /// <param name="writer">The <see cref="CsharpWriter" /> to write the code to.</param>
    /// <param name="command">The <see cref="Command" /> object representing the .NET CLI command.</param>
    /// <param name="isStub">If <c>true</c>, generates an interface method signature; otherwise, generates an implementation.</param>
    private static void GenerateCommandMethod(CsharpWriter writer, Command command, bool isStub) =>
        writer.WriteMethod("public",
            "Task<ProcessRunResult>",
            CsharpWriter.ToPascalCase(command.Name),
            command
                .Arguments
                .Select(arg => new MethodParam(IsKnownAotFriendlyType(arg.ValueType)
                        ? FormatType(arg.ValueType)
                        : arg.ValueType is "DecSm.Atom.Paths.RootedPath"
                            ? "DecSm.Atom.Paths.RootedPath"
                            : "System.String",
                    CsharpWriter.ToPascalCase(arg.Name, true),
                    XmlDescription: arg.Description))
                .Append(command.Options.Count > 0
                    ? new MethodParam($"{CsharpWriter.ToPascalCase(command.Name)}Options?",
                        "options",
                        "null",
                        "Options")
                    : null)
                .Append(new("ProcessRunOptions?",
                    "processRunOptions",
                    "null",
                    "Process run options. NOTE: The command name will be overridden by the appropriate dotnet command"))
                .Append(new("CancellationToken", "cancellationToken", "default", "Cancellation token"))
                .ToArray(),
            isStub
                ? null
                : bodyWriter =>
                {
                    switch (command)
                    {
                        case { Arguments.Count: 0, Options.Count: 0 }:
                        {
                            bodyWriter.WriteLine("return processRunner.RunAsync((processRunOptions is null");

                            using (bodyWriter.Indent)
                            {
                                bodyWriter.WriteLine("? new(\"dotnet\", string.Empty)");

                                bodyWriter.WriteLine(
                                    ": processRunOptions with { Name = \"dotnet\" }), cancellationToken);");
                            }

                            return;
                        }

                        case { Arguments.Count: 0, Options.Count: > 0 }:
                        {
                            bodyWriter.WriteLine("return processRunner.RunAsync((processRunOptions is null");

                            using (bodyWriter.Indent)
                            {
                                bodyWriter.WriteLine("? new(\"dotnet\", (options ?? new()).ToString())");

                                bodyWriter.WriteLine(
                                    ": processRunOptions with { Name = \"dotnet\", Args = (options ?? new()).ToString() }), cancellationToken);");
                            }

                            return;
                        }

                        default:
                        {
                            bodyWriter.WriteLine("var argsString = string.Join(' ', new string[] {");

                            using (bodyWriter.Indent)
                            {
                                bodyWriter.WriteLine($"\"{command.Name}\",");

                                foreach (var arg in command.Arguments)
                                    bodyWriter.WriteLine($"{FormatToPrint(
                                        arg.ValueType,
                                        command.Name,
                                        CsharpWriter.ToPascalCase(arg.Name, true), false)},");

                                if (command.Options.Count > 0)
                                    bodyWriter.WriteLine("(options ?? new()).ToString(),");
                            }

                            bodyWriter.WriteLine("}.Where(x => x is { Length: > 0 }));");

                            bodyWriter.WriteLine(string.Empty);

                            bodyWriter.WriteLine("return processRunner.RunAsync((processRunOptions is null");

                            using (bodyWriter.Indent)
                            {
                                bodyWriter.WriteLine("? new(\"dotnet\", argsString)");

                                bodyWriter.WriteLine(
                                    ": processRunOptions with { Name = \"dotnet\", Args = argsString }), cancellationToken);");
                            }

                            return;
                        }
                    }
                },
            command is { Arguments.Count: 0 },
            isStub
                ? command.Description
                : null);

    /// <summary>
    ///     Formats a .NET type name for use in generated C# code, handling nullable types and specific replacements.
    /// </summary>
    /// <param name="type">The original type name from the CLI schema.</param>
    /// <returns>The formatted C# type name.</returns>
    private static string FormatType(string type)
    {
        // ReSharper disable once InvertIf
        if (type.StartsWith("System.Nullable<", StringComparison.Ordinal))
        {
            var closingBracketIndex = type.LastIndexOf('>');
            var innerType = type["System.Nullable<".Length..closingBracketIndex];
            type = innerType;
        }

        return type
            .Replace("System.Collections.ObjectModel.ReadOnlyDictionary",
                "System.Collections.Generic.IReadOnlyDictionary")
            .Replace("Microsoft.DotNet.Cli.PackageIdentityWithRange", "string")
            .Replace("Microsoft.DotNet.Cli.Utils.VerbosityOptions", "VerbosityOptions")
            .Replace("NuGet.CommandLine.XPlat.Commands.VerbosityEnum", "VerbosityOptions")
            .Replace("NuGet.CommandLine.XPlat.Commands.Package.Update.Package", "string");
    }

    /// <summary>
    ///     Formats a command-line argument or option value for printing in the generated `ToString()` method of options
    ///     records.
    /// </summary>
    /// <param name="type">The .NET type of the argument/option value.</param>
    /// <param name="argName">The original command-line argument name (e.g., "--configuration").</param>
    /// <param name="propertyName">The C# property name corresponding to the argument/option.</param>
    /// <param name="includeArgName">
    ///     If <c>true</c>, the argument name is included in the formatted string (e.g., "--arg
    ///     value").
    /// </param>
    /// <returns>A string representing the formatted argument/option for command-line usage.</returns>
    private static string FormatToPrint(string type, string argName, string propertyName, bool includeArgName = true)
    {
        if (type.StartsWith("System.Nullable<", StringComparison.Ordinal))
        {
            var closingBracketIndex = type.LastIndexOf('>');
            var innerType = type["System.Nullable<".Length..closingBracketIndex];
            type = innerType;
        }

        if (type is "System.Boolean")
            return $"\"{argName}\"";

        if (type.StartsWith("System.Collections.ObjectModel.ReadOnlyDictionary<") ||
            type.StartsWith("System.Collections.Generic.IReadOnlyDictionary<"))
            return $$"""
                     $"{{argName}} {string.Join(" {{argName}} ", {{propertyName}}.Select(kv => $"{kv.Key}=\"{kv.Value}\""))}"
                     """;

        if (type.EndsWith("[]", StringComparison.Ordinal) ||
            type.StartsWith("System.Collections.Generic.IEnumerable<") ||
            type.StartsWith("System.Collections.Generic.IReadOnlyList<"))
            return includeArgName
                ? $$"""
                    $"{{argName}} {string.Join(" {{argName}} ", {{propertyName}})}"
                    """
                : $$"""
                    $"{string.Join(" ", {{propertyName}})}"
                    """;

        return includeArgName
            ? $$"""
                $"{{argName}} {{{propertyName}}}"
                """
            : propertyName;
    }

    /// <summary>
    ///     Determines if a given type name is considered "AOT-friendly" (i.e., can be used directly without causing
    ///     AOT/trimming issues).
    /// </summary>
    /// <param name="typeName">The name of the type to check.</param>
    /// <returns><c>true</c> if the type is considered AOT-friendly; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     This method uses simple string checks to classify types. It explicitly allows
    ///     `DecSm.Atom.Paths.RootedPath`, `Microsoft.DotNet.Cli.Utils.VerbosityOptions`,
    ///     and any type starting with "System." (assuming BCL types are generally AOT-friendly).
    /// </remarks>
    private static bool IsKnownAotFriendlyType(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return false;

        // Exclude void explicitly
        if (typeName is "void" or "System.Void")
            return false;

        // Handle single-dimensional arrays by checking the element type
        if (typeName.EndsWith("[]", StringComparison.Ordinal))
            typeName = typeName[..^2];

        if (typeName.StartsWith("System.Nullable`", StringComparison.Ordinal))
            typeName = typeName[(typeName.IndexOf('`') + 1)..];

        // Explicitly allow our known custom types and consider all BCL types under System.* as known
        return typeName is "DecSm.Atom.Paths.RootedPath" or "Microsoft.DotNet.Cli.Utils.VerbosityOptions" ||
               typeName.StartsWith("System.", StringComparison.Ordinal);
    }
}
