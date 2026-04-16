namespace DecSm.Atom.Help;

/// <summary>
///     Defines a service for displaying help information about the build system.
/// </summary>
[PublicAPI]
public interface IHelpService
{
    /// <summary>
    ///     Displays detailed help information for available commands, parameters, and options.
    /// </summary>
    void ShowHelp();
}

/// <summary>
///     Provides a service for displaying detailed help information about the build system, including commands, parameters,
///     and options.
/// </summary>
/// <param name="console">The console for writing output.</param>
/// <param name="args">The parsed command-line arguments.</param>
/// <param name="buildModel">The model representing the build structure.</param>
/// <param name="config">The application configuration.</param>
internal sealed class HelpService(
    IAnsiConsole console,
    CommandLineArgs args,
    BuildModel buildModel,
    IConfiguration config
) : IHelpService
{
    /// <summary>
    ///     Displays detailed help information for available commands, parameters, and options.
    /// </summary>
    public void ShowHelp()
    {
        console.WriteLine();
        console.Write(new Markup("[bold]Usage[/]\n"));
        console.WriteLine();
        console.Write(new Markup("atom [teal][[options]][/]\n"));
        console.Write(new Markup("atom [blue][[command/s]][/] [fuchsia][[parameters]][/] [teal][[options]][/]\n"));
        console.WriteLine();

        console.Write(new Markup("[bold]Options[/]\n"));
        console.WriteLine();

        console.Write(
            new Markup("  [dim]-h,  --help[/]        [dim]Show help for entire tool or a single command[/]\n"));

        console.Write(
            new Markup("  [dim]-i,  --interactive[/] [dim]Run in interactive mode (prompt for required params)[/]\n"));

        console.Write(new Markup("  [dim]-g,  --gen[/]         [dim]Generate build scripts[/]\n"));

        console.Write(new Markup(
            "  [dim]-s,  --skip[/]        [dim]Skip dependency execution (run only specified commands)[/]\n"));

        console.Write(new Markup(
            "  [dim]-hl, --headless[/]    [dim]Run in headless mode (no prompts or logins, used in CI)[/]\n"));

        console.Write(new Markup("  [dim]-v,  --verbose[/]     [dim]Show verbose output (extra logging)[/]\n"));
        console.WriteLine();

        var targets = args.HasVerbose
            ? buildModel.Targets
            : args.Commands.Count is 0
                ? buildModel
                    .Targets
                    .Where(x => !x.IsHidden)
                    .ToList()
                : buildModel
                    .Targets
                    .Where(x => args
                        .Commands
                        .Select(command => command.Name)
                        .Contains(x.Name))
                    .ToList();

        var atomAssembly = typeof(HelpService).Assembly;

        var projectAssembly = buildModel.DeclaringAssembly;

        var atomTargets = new List<TargetModel>(targets.Count);
        var libraryTargets = new List<TargetModel>(targets.Count);
        var projectTargets = new List<TargetModel>(targets.Count);

        foreach (var target in targets)
        {
            var assembly = target.DeclaringAssembly;

            if (assembly == atomAssembly)
                atomTargets.Add(target);
            else if (assembly == projectAssembly)
                projectTargets.Add(target);
            else
                libraryTargets.Add(target);
        }

        if (atomTargets.Count > 0)
        {
            console.Write(new Markup("[bold]Atom Commands[/]\n"));
            console.WriteLine();

            foreach (var target in atomTargets)
                WriteCommand(target);
        }

        if (libraryTargets.Count > 0)
        {
            console.Write(new Markup("[bold]Library Commands[/]\n"));
            console.WriteLine();

            foreach (var target in libraryTargets)
                WriteCommand(target);
        }

        // ReSharper disable once InvertIf
        if (projectTargets.Count > 0)
        {
            console.Write(new Markup("[bold]Project Commands[/]\n"));
            console.WriteLine();

            foreach (var target in projectTargets)
                WriteCommand(target);
        }
    }

    /// <summary>
    ///     Writes a detailed, formatted representation of a single command (target) to the console.
    /// </summary>
    /// <param name="target">The target model to display.</param>
    private void WriteCommand(TargetModel target)
    {
        var title = target.Description is { Length: > 0 }
            ? $"[bold navy]{target.Name.EscapeMarkup()}[/] [dim]| {target.Description.EscapeMarkup()}[/]"
            : $"[bold navy]{target.Name.EscapeMarkup()}[/]";

        var tree = new Tree(title);

        var dependencies = target.Dependencies;

        if (dependencies.Count > 0 && args.HasVerbose)
        {
            var depTree = tree.AddNode("[dim bold yellow]Depends on[/]");

            foreach (var dependency in dependencies)
                depTree.AddNode($"[dim]{dependency.Name}[/]");
        }

        var requiredParams = target
            .Params
            .Where(x => x is { Param.IsSecret: false, Required: true })
            .ToList();

        var optionalParams = target
            .Params
            .Where(x => x is { Param.IsSecret: false, Required: false })
            .ToList();

        var secrets = target
            .Params
            .Where(x => x is { Param.IsSecret: true })
            .ToList();

        if (requiredParams.Count > 0)
        {
            var nodes = new List<(string Name, string Value, bool IsSupplied)>(requiredParams.Count);

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var param in requiredParams)
            {
                var defaultValue = param.Param.DefaultValue ?? string.Empty;

                var configuredValue = config
                                          .GetSection("Params")[param.Param.ArgName] ??
                                      string.Empty;

                var suppliedDisplay = (defaultValue, configuredValue) switch
                {
                    ({ Length: > 0 }, { Length: > 0 }) when defaultValue == configuredValue =>
                        $"[dim] | [/][dim green][[✔ Default/Configured: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: > 0 }) =>
                        $"[dim] | [/][dim][[Default: {defaultValue.EscapeMarkup()}]] [dim green][[✔ Configured: {configuredValue.EscapeMarkup()}]][/][/]",
                    ({ Length: > 0 }, { Length: 0 }) =>
                        $"[dim] | [/][dim green][[✔ Default: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: 0 }, { Length: > 0 }) =>
                        $"[dim] | [/][dim green][[✔ Configured: {configuredValue.EscapeMarkup()}]][/]",
                    _ => "[dim] | [/][dim yellow][[⚠ None]][/]",
                };

                var descriptionDisplay = param.Param.Description is { Length: > 0 }
                    ? $"[dim] | {param.Param.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                var nameDisplay = $"--{param.Param.ArgName.EscapeMarkup()}";

                nodes.Add((param.Param.ArgName, $"{nameDisplay}{suppliedDisplay}{descriptionDisplay}",
                    defaultValue is { Length: > 0 } || configuredValue is { Length: > 0 }));
            }

            var requiredParamsTree = tree.AddNode("[bold red]Requires[/]");

            requiredParamsTree.AddNodes(nodes
                .Where(x => !x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));

            requiredParamsTree.AddNodes(nodes
                .Where(x => x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));
        }

        if (optionalParams.Count > 0)
        {
            var nodes = new List<(string Name, string Value, bool IsSupplied)>(optionalParams.Count);

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var param in optionalParams)
            {
                var defaultValue = param.Param.DefaultValue ?? string.Empty;

                var configuredValue = config
                                          .GetSection("Params")[param.Param.ArgName] ??
                                      string.Empty;

                var suppliedDisplay = (defaultValue, configuredValue) switch
                {
                    ({ Length: > 0 }, { Length: > 0 }) when defaultValue == configuredValue =>
                        $"[dim] | [/][dim green][[✔ Default/Configured: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: > 0 }) =>
                        $"[dim] | [/][dim green][[Default: {defaultValue.EscapeMarkup()}]][/][dim] [[✔ Configured: {configuredValue.EscapeMarkup()}]][/]",
                    ({ Length: > 0 }, { Length: 0 }) =>
                        $"[dim] | [/][dim green][[✔ Default: {defaultValue.EscapeMarkup()}]][/]",
                    ({ Length: 0 }, { Length: > 0 }) =>
                        $"[dim] | [/][dim green][[✔ Configured: {configuredValue.EscapeMarkup()}]][/]",
                    _ => "[dim] | [/][dim][[✔ None]][/]",
                };

                var descriptionDisplay = param.Param.Description is { Length: > 0 }
                    ? $"[dim] | {param.Param.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                var nameDisplay = $"--{param.Param.ArgName.EscapeMarkup()}";

                nodes.Add((param.Param.ArgName, $"{nameDisplay}{suppliedDisplay}{descriptionDisplay}",
                    defaultValue is { Length: > 0 } || configuredValue is { Length: > 0 }));
            }

            var optionalParamsTree = tree.AddNode("[bold green]Options[/]");

            optionalParamsTree.AddNodes(nodes
                .Where(x => !x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));

            optionalParamsTree.AddNodes(nodes
                .Where(x => x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));
        }

        if (secrets.Count > 0)
        {
            var nodes = new List<(string Name, string Value, bool IsSupplied)>(secrets.Count);

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var secret in secrets)
            {
                var defaultValue = secret.Param.DefaultValue ?? string.Empty;

                var configuredValue = config
                                          .GetSection("Params")[secret.Param.ArgName] ??
                                      string.Empty;

                var descriptionDisplay = secret.Param.Description is { Length: > 0 }
                    ? $"[dim] | {secret.Param.Description.EscapeMarkup()}[/]"
                    : string.Empty;

                var suppliedDisplay = (defaultValue, configuredValue) switch
                {
                    ({ Length: > 0 }, { Length: > 0 }) when defaultValue == configuredValue =>
                        "[dim] | [/][dim green][[✔ Default/Configured: ****]][/]",
                    ({ Length: > 0 }, { Length: > 0 }) =>
                        "[dim] | [/][dim green][[Default: ****]][/][dim][[✔ Configured: ****]][/]",
                    ({ Length: > 0 }, { Length: 0 }) => "[dim] | [/][dim green][[✔ Default: ****]][/]",
                    ({ Length: 0 }, { Length: > 0 }) => "[dim] | [/][dim green][[✔ Configured: ****]][/]",
                    _ when secret.Required => "[dim] | [/][dim blue][[? None]][/]",
                    _ => "[dim] | [/][dim][[✔ None]][/]",
                };

                var nameDisplay = $"--{secret.Param.ArgName.EscapeMarkup()}";

                nodes.Add((secret.Param.ArgName, $"{nameDisplay}{suppliedDisplay}{descriptionDisplay}",
                    defaultValue is { Length: > 0 } || configuredValue is { Length: > 0 }));
            }

            var secretsTree = tree.AddNode("[bold purple]Secrets[/]");

            secretsTree.AddNodes(nodes
                .Where(x => !x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));

            secretsTree.AddNodes(nodes
                .Where(x => x.IsSupplied)
                .OrderBy(x => x.Name)
                .Select(x => x.Value));
        }

        console.Write(tree);
        console.WriteLine();
    }
}
