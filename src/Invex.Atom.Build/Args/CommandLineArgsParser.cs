namespace Invex.Atom.Build.Args;

/// <summary>
///     Parses raw command-line arguments into a structured <see cref="CommandLineArgs" /> object.
/// </summary>
/// <remarks>
///     This parser identifies known options (e.g., <c>--help</c>), parameters, and commands (targets) by referencing an
///     <see cref="IBuildDefinition" />.
///     Parsing never throws or writes output; any problems (unknown arguments, missing values) are collected as
///     <see cref="ParseError" /> objects on the returned <see cref="CommandLineArgs" />, to be reported by the
///     application once the host is fully constructed.
/// </remarks>
internal sealed class CommandLineArgsParser(IBuildDefinition buildDefinition)
{
    /// <summary>
    ///     Parses the provided command-line arguments into a <see cref="CommandLineArgs" /> object.
    /// </summary>
    /// <param name="rawArgs">A read-only list of string arguments from the application's entry point.</param>
    /// <returns>
    ///     A <see cref="CommandLineArgs" /> object representing the parsed arguments. Check the
    ///     <see cref="CommandLineArgs.IsValid" /> property to determine if parsing was successful; when it is false,
    ///     <see cref="CommandLineArgs.Errors" /> describes each problem encountered.
    /// </returns>
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
        List<ParseError> errors = [];

        for (var i = 0; i < rawArgsArray.Length; i++)
        {
            var rawArg = rawArgsArray[i];

            if (TryParseOption(rawArg) is { } optionArg)
            {
                if (optionArg is ProjectArg)
                {
                    if (i == rawArgsArray.Length - 1)
                    {
                        errors.Add(new("Missing value for -[-p]roject option. Usage: --project <path>")
                        {
                            ArgumentName = "project",
                        });

                        continue;
                    }

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

                var buildParam = buildDefinition.ParamDefinitions.FirstOrDefault(buildParam =>
                    string.Equals(argParam, buildParam.Value.ArgName, StringComparison.OrdinalIgnoreCase));

                if (buildParam.Key is not null)
                {
                    if (i == rawArgsArray.Length - 1)
                    {
                        errors.Add(new($"Missing value for parameter '{argParam}'. Usage: --{argParam} <value>")
                        {
                            ArgumentName = argParam,
                        });

                        continue;
                    }

                    var nextArg = rawArgsArray[i + 1];

                    if (nextArg.StartsWith("--"))
                    {
                        errors.Add(new(
                            $"Missing value for parameter '{argParam}'. The next argument '{nextArg}' looks like another option. Usage: --{argParam} <value>")
                        {
                            ArgumentName = argParam,
                        });

                        continue;
                    }

                    args.Add(new ParamArg(buildParam.Value.ArgName, buildParam.Key, nextArg));
                    i++;

                    continue;
                }
            }

            if (TryParseCommand(buildDefinition.TargetDefinitions, rawArg) is { } commandArg)
            {
                args.Add(commandArg);

                continue;
            }

            var commandMatches = buildDefinition
                .TargetDefinitions
                .Keys
                .OrderBy(targetDefinition => targetDefinition.GetLevenshteinDistance(rawArg))
                .Take(3)
                .ToList();

            var paramMatches = buildDefinition
                .ParamDefinitions
                .Values
                .Select(paramDefinition => paramDefinition.ArgName)
                .OrderBy(paramDefinition => paramDefinition.GetLevenshteinDistance(rawArg))
                .Take(3)
                .ToList();

            errors.Add(new($"Unknown argument '{rawArg}'")
            {
                ArgumentName = rawArg,
                SimilarCommands = commandMatches,
                SimilarParams = paramMatches,
            });
        }

        return new(errors.Count == 0, args, errors);
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
    /// <param name="rawArg">The raw string argument.</param>
    /// <returns>A <see cref="CommandArg" /> if the argument matches a known target or alias; otherwise, <c>null</c>.</returns>
    private static CommandArg? TryParseCommand(IReadOnlyDictionary<string, Target> targetDefinitions, string rawArg)
    {
        // Match by target name
        var matchedByName = targetDefinitions
            .Where(buildTarget => string.Equals(rawArg, buildTarget.Key, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Key)
            .FirstOrDefault();

        return matchedByName is not null
            ? new(matchedByName)
            : null;
    }
}
