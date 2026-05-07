namespace DecSm.Atom.Build.Args;

/// <summary>
///     Parses raw command-line arguments into a structured <see cref="CommandLineArgs" /> object.
/// </summary>
/// <remarks>
///     This parser identifies known options (e.g., <c>--help</c>), parameters, and commands (targets) by referencing an
///     <see cref="IBuildDefinition" />.
///     Unrecognized arguments will invalidate the result and trigger diagnostic messages.
/// </remarks>
internal sealed class CommandLineArgsParser(IBuildDefinition buildDefinition, IAnsiConsole console)
{
    // Needs lazy initialization to avoid circular dependency with IBuildDefinition
    // (which may reference CommandLineArgsParser for alias resolution)
    private IReadOnlyDictionary<string, string> AliasToTargetName =>
        field ??= buildDefinition
            .TargetDefinitions
            .Select(x =>
            {
                var definition = x.Value(new()
                {
                    Name = x.Key,
                });

                return (Name: x.Key, definition.Alias);
            })
            .Where(x => x.Alias is not null)
            .ToDictionary(x => x.Alias!, x => x.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Parses the provided command-line arguments into a <see cref="CommandLineArgs" /> object.
    /// </summary>
    /// <param name="rawArgs">A read-only list of string arguments from the application's entry point.</param>
    /// <returns>
    ///     A <see cref="CommandLineArgs" /> object representing the parsed arguments. Check the
    ///     <see cref="CommandLineArgs.IsValid" /> property to determine if parsing was successful.
    /// </returns>
    /// <exception cref="CommandLineException">
    ///     Thrown if an argument requiring a value is provided without one (e.g., <c>--project</c> or a defined parameter).
    /// </exception>
    /// <example>
    ///     <code>
    ///       var rawArgs = new[] { "Build", "--version", "1.0.0", "-s" };
    ///       var commandLineArgs = parser.Parse(rawArgs);
    ///       // commandLineArgs.Commands will contain "Build"
    ///       // commandLineArgs.Params will contain a ParamArg for "version" with value "1.0.0"
    ///       // commandLineArgs.HasSkip will be true
    ///     </code>
    /// </example>
    public CommandLineArgs Parse(IEnumerable<string> rawArgs)
    {
        var rawArgsArray = rawArgs.ToArray();

        List<IArg> args = [];

        var isValid = true;

        for (var i = 0; i < rawArgsArray.Length; i++)
        {
            var rawArg = rawArgsArray[i];

            if (TryParseOption(rawArg) is { } optionArg)
            {
                if (optionArg is ProjectArg)
                {
                    if (i == rawArgsArray.Length - 1)
                        throw new CommandLineException("Missing value for -[-p]roject option. Usage: --project <path>")
                        {
                            ArgumentName = "project",
                        };

                    optionArg = new ProjectArg(rawArgsArray[i + 1]);
                    i++;
                }

                args.Add(optionArg);

                continue;
            }

            // Params
            if (rawArg.StartsWith("--"))
            {
                var argParam = rawArg[2..];
                var matchedParam = false;

                foreach (var buildParam in buildDefinition.ParamDefinitions.Where(buildParam =>
                             string.Equals(argParam, buildParam.Value.ArgName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (i == rawArgsArray.Length - 1)
                        throw new CommandLineException(
                            $"Missing value for parameter '{argParam}'. Usage: --{argParam} <value>")
                        {
                            ArgumentName = argParam,
                        };

                    var nextArg = rawArgsArray[i + 1];

                    if (nextArg.StartsWith("--"))
                        throw new CommandLineException(
                            $"Missing value for parameter '{argParam}'. The next argument '{nextArg}' looks like another option. Usage: --{argParam} <value>")
                        {
                            ArgumentName = argParam,
                        };

                    args.Add(new ParamArg(buildParam.Value.ArgName, buildParam.Key, nextArg));
                    i++;
                    matchedParam = true;

                    break;
                }

                if (matchedParam)
                    continue;
            }

            if (TryParseCommand(buildDefinition.TargetDefinitions, AliasToTargetName, rawArg) is { } commandArg)
            {
                args.Add(commandArg);

                continue;
            }

            console.WriteLine();
            console.WriteLine($"Unknown argument '{rawArg}'", new(Color.Red));

            var commandMatches = buildDefinition
                .TargetDefinitions
                .Keys
                .Concat(AliasToTargetName.Keys)
                .OrderBy(targetDefinition => targetDefinition.GetLevenshteinDistance(rawArg))
                .Take(3)
                .ToList();

            if (commandMatches.Count > 0)
            {
                console.WriteLine();
                console.WriteLine("Similar commands", new(Color.Yellow));

                foreach (var possibleMatch in commandMatches)
                    console.WriteLine($"  {possibleMatch}");
            }

            var paramMatches = buildDefinition
                .ParamDefinitions
                .Values
                .Select(paramDefinition => paramDefinition.ArgName)
                .OrderBy(paramDefinition => paramDefinition.GetLevenshteinDistance(rawArg))
                .Take(3)
                .ToList();

            if (paramMatches.Count > 0)
            {
                console.WriteLine();
                console.WriteLine("Similar parameters", new(Color.Yellow));

                foreach (var possibleMatch in paramMatches)
                    console.WriteLine($"  --{possibleMatch}");
            }

            console.WriteLine();

            isValid = false;
        }

        return new(isValid, args.ToList());
    }

    /// <summary>
    ///     Attempts to parse a raw argument as a built-in option (e.g., <c>--help</c>, <c>--skip</c>).
    /// </summary>
    /// <param name="rawArg">The raw string argument.</param>
    /// <returns>An <see cref="IArg" /> representing the option if recognized; otherwise, <c>null</c>.</returns>
    private static IArg? TryParseOption(string rawArg) =>
        rawArg.ToLower() switch
        {
            "-h" or "--help" => new HelpArg(),
            "-s" or "--skip" => new SkipArg(),
            "-hl" or "--headless" => new HeadlessArg(),
            "-v" or "--verbose" => new VerboseArg(),
            "-p" or "--project" => new ProjectArg(string.Empty),
            "-i" or "--interactive" => new InteractiveArg(),
            _ => null,
        };

    /// <summary>
    ///     Attempts to parse a raw argument as a defined command (target) from the build definition.
    /// </summary>
    /// <param name="targetDefinitions">The available target definitions.</param>
    /// <param name="aliasToTargetName">A mapping from alias strings to target names.</param>
    /// <param name="rawArg">The raw string argument.</param>
    /// <returns>A <see cref="CommandArg" /> if the argument matches a known target or alias; otherwise, <c>null</c>.</returns>
    private static CommandArg? TryParseCommand(
        IReadOnlyDictionary<string, Target> targetDefinitions,
        IReadOnlyDictionary<string, string> aliasToTargetName,
        string rawArg)
    {
        // Match by target name
        var matchedByName = targetDefinitions
            .Where(buildTarget => string.Equals(rawArg, buildTarget.Key, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .FirstOrDefault();

        if (matchedByName is not null)
            return new(matchedByName);

        // Match by alias
        if (aliasToTargetName.TryGetValue(rawArg, out var targetName))
            return new(targetName);

        return null;
    }
}
