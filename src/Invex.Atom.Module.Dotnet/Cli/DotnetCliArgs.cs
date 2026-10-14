namespace Invex.Atom.Module.Dotnet.Cli;

/// <summary>
///     Helpers used by the generated <see cref="IDotnetCli" /> implementation and options records to build
///     argument lists.
/// </summary>
/// <remarks>
///     Every value is added as its own list element, so no quoting is needed while building. The list is converted
///     to a command-line string once, by <see cref="CreateRunOptions" />, using <see cref="CommandLineArguments" />.
/// </remarks>
internal static class DotnetCliArgs
{
    /// <summary>
    ///     Creates the <see cref="ProcessRunOptions" /> that run <c>dotnet</c> with <paramref name="arguments" />.
    /// </summary>
    /// <param name="arguments">The individual arguments, starting with the command name(s).</param>
    /// <param name="processRunOptions">
    ///     Optional caller-supplied options. Their <see cref="ProcessRunOptions.Name" /> and
    ///     <see cref="ProcessRunOptions.Args" /> are replaced.
    /// </param>
    /// <returns>The options to pass to <see cref="IProcessRunner" />.</returns>
    public static ProcessRunOptions CreateRunOptions(IEnumerable<string> arguments, ProcessRunOptions? processRunOptions)
    {
        var args = CommandLineArguments.Join(arguments);

        return processRunOptions is null
            ? new("dotnet", args)
            : processRunOptions with
            {
                Name = "dotnet",
                Args = args,
            };
    }

    /// <summary>
    ///     Adds a positional argument, skipping <c>null</c> or empty values.
    /// </summary>
    public static void AddPositional(List<string> args, string? value)
    {
        if (value is { Length: > 0 })
            args.Add(value);
    }

    /// <summary>
    ///     Adds each value as a positional argument, skipping <c>null</c> or empty values.
    /// </summary>
    public static void AddPositional(List<string> args, IEnumerable<string?>? values)
    {
        if (values is null)
            return;

        foreach (var value in values)
            AddPositional(args, value);
    }

    /// <summary>
    ///     Adds a flag (an option that takes no value) when <paramref name="value" /> is <c>true</c>.
    /// </summary>
    public static void AddFlag(List<string> args, string name, bool? value)
    {
        if (value is true)
            args.Add(name);
    }

    /// <summary>
    ///     Adds a boolean option that takes a value, as <c>name true</c> or <c>name false</c>, when
    ///     <paramref name="value" /> is not <c>null</c>.
    /// </summary>
    public static void AddBool(List<string> args, string name, bool? value)
    {
        if (value is null)
            return;

        args.Add(name);

        args.Add(value.Value
            ? "true"
            : "false");
    }

    /// <summary>
    ///     Adds <c>name value</c> when <paramref name="value" /> is not <c>null</c>.
    /// </summary>
    public static void AddOption(List<string> args, string name, string? value)
    {
        if (value is null)
            return;

        args.Add(name);
        args.Add(value);
    }

    /// <summary>
    ///     Adds <c>name value</c>, formatted with the invariant culture, when <paramref name="value" /> is not
    ///     <c>null</c>.
    /// </summary>
    public static void AddOption(List<string> args, string name, int? value) =>
        AddOption(args, name, value?.ToString(System.Globalization.CultureInfo.InvariantCulture));

    /// <summary>
    ///     Adds <c>name value</c> using the lowercase CLI value (see <see cref="VerbosityOptionsExtensions" />) when
    ///     <paramref name="value" /> is not <c>null</c>.
    /// </summary>
    public static void AddOption(List<string> args, string name, VerbosityOptions? value) =>
        AddOption(args, name, value?.ToCliArg());

    /// <summary>
    ///     Adds <c>name path</c> when <paramref name="value" /> is not <c>null</c>.
    /// </summary>
    public static void AddOption(List<string> args, string name, FileSystemInfo? value) =>
        AddOption(args, name, value?.ToString());

    /// <summary>
    ///     Adds <c>name value</c> once for each value when <paramref name="values" /> is not <c>null</c>.
    /// </summary>
    public static void AddOption(List<string> args, string name, IEnumerable<string>? values)
    {
        if (values is null)
            return;

        foreach (var value in values)
            AddOption(args, name, value);
    }

    /// <summary>
    ///     Adds <c>name key=value</c> once for each entry when <paramref name="values" /> is not <c>null</c>.
    /// </summary>
    public static void AddOption(List<string> args, string name, IReadOnlyDictionary<string, string>? values)
    {
        if (values is null)
            return;

        foreach (var (key, value) in values)
            AddOption(args, name, $"{key}={value}");
    }
}
