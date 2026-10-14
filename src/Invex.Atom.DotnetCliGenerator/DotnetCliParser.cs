namespace Atom;

/// <summary>
///     Represents a .NET CLI command, including its name, description, arguments, options, and subcommands.
/// </summary>
/// <param name="Name">The name of the command (e.g., "build", "test").</param>
/// <param name="Description">A brief description of the command's purpose.</param>
/// <param name="Arguments">A list of arguments that the command accepts.</param>
/// <param name="Options">A list of options that the command accepts.</param>
/// <param name="SubCommands">A list of subcommands nested under this command.</param>
/// <param name="Hidden">
///     Whether the schema marks the command as hidden (internal or legacy). Hidden commands, and every command
///     nested under them, are not generated.
/// </param>
public sealed record Command(
    string Name,
    string? Description,
    List<Argument> Arguments,
    List<Option> Options,
    List<Command> SubCommands,
    bool Hidden = false
);

/// <summary>
///     Represents a command-line argument for a .NET CLI command.
/// </summary>
/// <param name="Name">The name of the argument (e.g., "PROJECT | SOLUTION | FILE").</param>
/// <param name="Description">A brief description of the argument.</param>
/// <param name="ValueType">The expected .NET type of the argument's value (e.g., "System.String", "System.String[]").</param>
public sealed record Argument(string Name, string? Description, string ValueType);

/// <summary>
///     Represents a command-line option for a .NET CLI command.
/// </summary>
/// <param name="Name">The name of the option (e.g., "--configuration", "-c").</param>
/// <param name="Description">A brief description of the option.</param>
/// <param name="ValueType">The expected .NET type of the option's value (e.g., "System.String", "System.Boolean").</param>
/// <param name="Hidden">Whether the schema marks the option as hidden.</param>
/// <param name="AcceptsValue">
///     Whether the option accepts a value (its maximum arity is greater than zero). A boolean option that does not
///     accept a value is a pure flag.
/// </param>
public sealed record Option(
    string Name,
    string? Description,
    string ValueType,
    bool Hidden = false,
    bool AcceptsValue = true
);

/// <summary>
///     Provides functionality to parse the .NET CLI schema (obtained via `dotnet --cli-schema`)
///     into a structured object model.
/// </summary>
public static class DotnetCliParser
{
    /// <summary>
    ///     Hidden options that are still generated.
    /// </summary>
    /// <remarks>
    ///     The .NET CLI hides these options from its help output to reduce clutter, but they are supported, documented
    ///     options that are forwarded to MSBuild or to the implicit restore (e.g. <c>--property</c>, <c>--source</c>).
    ///     All other hidden options (for example the <c>dotnet new --debug:*</c> options) are internal or deprecated and
    ///     are not generated.
    /// </remarks>
    public static IReadOnlySet<string> RetainedHiddenOptions { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "--configfile",
        "--disable-parallel",
        "--force",
        "--getItem",
        "--getProperty",
        "--getResultOutputFile",
        "--getTargetResult",
        "--ignore-failed-sources",
        "--no-dependencies",
        "--no-http-cache",
        "--packages",
        "--property",
        "--restoreProperty",
        "--source",
        "--target",
    };

    /// <summary>
    ///     Parses a JSON element representing a .NET CLI command into a <see cref="Command" /> object.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="element">The <see cref="JsonElement" /> containing the command's schema.</param>
    /// <returns>A <see cref="Command" /> object if parsing is successful, otherwise <c>null</c>.</returns>
    public static Command? GetCommand(string name, JsonElement element)
    {
        if (element.ValueKind is not JsonValueKind.Object)
            return null;

        var description = element.TryGetProperty("description", out var descElement)
            ? descElement.GetString()
            : null;

        var arguments = new List<Argument>();

        if (element.TryGetProperty("arguments", out var argsElement))
            arguments.AddRange(argsElement
                .EnumerateObject()
                .Select(static arg =>
                {
                    var argName = arg.Name;
                    var argValue = arg.Value;

                    if (argValue.ValueKind is not JsonValueKind.Object)
                        return null;

                    var argDescription = argValue.TryGetProperty("description", out var desc)
                        ? desc.GetString()
                        : null;

                    var argValueType = argValue.TryGetProperty("valueType", out var valType)
                        ? valType.GetString()
                        : null;

                    // Special handling for "arguments" which is often a string array
                    if (argValueType is null)
                        return argName is "arguments"
                            ? new(argName, argDescription, "System.String[]")
                            : null;

                    return new Argument(argName, argDescription, argValueType);
                })
                .OfType<Argument>());

        var options = new List<Option>();

        if (element.TryGetProperty("options", out var optsElement))
            options.AddRange(optsElement
                .EnumerateObject()
                .Select(static opt =>
                {
                    var optName = opt.Name;
                    var optValue = opt.Value;

                    if (optValue.ValueKind is not JsonValueKind.Object)
                        return null;

                    var optDescription = optValue.TryGetProperty("description", out var desc)
                        ? desc.GetString()
                        : null;

                    var optValueType = optValue.TryGetProperty("valueType", out var valType)
                        ? valType.GetString()
                        : null;

                    if (optValueType is null)
                        return null;

                    var acceptsValue = !optValue.TryGetProperty("arity", out var arity) ||
                                       !arity.TryGetProperty("maximum", out var maximum) ||
                                       maximum.GetInt32() > 0;

                    return new Option(optName, optDescription, optValueType, IsHidden(optValue), acceptsValue);
                })
                .OfType<Option>());

        var subCommands = new List<Command>();

        if (element.TryGetProperty("subcommands", out var subCmdsElement))
            subCommands.AddRange(subCmdsElement
                .EnumerateObject()
                .Select(static subCmd => GetCommand(subCmd.Name, subCmd.Value))
                .OfType<Command>());

        // If there is a framework option and not a project or solution argument, add one
        // This is a heuristic to ensure commands like 'dotnet build' have a target argument
        if (options.Any(static opt => opt.Name is "--framework") &&
            options.Any(static opt => opt.Name is "--configuration") &&
            !arguments.Any(static arg => arg.Name is "PROJECT | SOLUTION | FILE"))
            arguments.Insert(0,
                new("PROJECT | SOLUTION | FILE", "The project, solution, or file to operate on.", "System.String[]"));

        return new(name, description, arguments, options, subCommands, IsHidden(element));
    }

    /// <summary>
    ///     Determines whether a schema element is marked as hidden.
    /// </summary>
    /// <param name="element">The command or option schema element.</param>
    /// <returns><c>true</c> if the element has a <c>hidden</c> property set to <c>true</c>; otherwise <c>false</c>.</returns>
    private static bool IsHidden(JsonElement element) =>
        element.TryGetProperty("hidden", out var hidden) && hidden.ValueKind is JsonValueKind.True;

    /// <summary>
    ///     Recursively flattens a hierarchy of commands into a single list,
    ///     prefixing subcommand names with their parent command names.
    /// </summary>
    /// <param name="prefix">The current prefix for command names (e.g., "dotnet build").</param>
    /// <param name="subCommand">The subcommand to flatten.</param>
    /// <param name="flattenedCommands">The list to which flattened commands are added.</param>
    /// <remarks>
    ///     Commands the schema marks as hidden (internal commands such as <c>complete</c> or <c>parse</c>, and legacy
    ///     verb-first forms such as <c>add package</c>) are skipped together with all of their subcommands. Hidden
    ///     options are removed, except those listed in <see cref="RetainedHiddenOptions" />.
    /// </remarks>
    public static void FlattenCommands(string prefix, Command subCommand, List<Command> flattenedCommands)
    {
        if (subCommand.Hidden)
            return;

        var prefixedName = prefix.Length > 0
            ? $"{prefix} {subCommand.Name}"
            : subCommand.Name;

        flattenedCommands.Add(subCommand with
        {
            Name = $"{prefixedName}",
            Options = subCommand
                .Options
                .Where(static opt => !opt.Hidden || RetainedHiddenOptions.Contains(opt.Name))
                .ToList(),
        });

        foreach (var subSubCommand in subCommand.SubCommands)
            FlattenCommands($"{prefixedName}", subSubCommand, flattenedCommands);
    }
}
