namespace Atom;

/// <summary>
///     This build definition is responsible for generating the C# code for the .NET CLI wrapper.
/// </summary>
/// <remarks>
///     It uses the `dotnet --cli-schema` command to obtain the latest .NET CLI command structure,
///     then parses this schema and generates strongly-typed C# interfaces, implementations, and
///     option classes for each `dotnet` command. The generated files are placed in the
///     `Invex.Atom.Module.Dotnet/Cli/Generated` directory.
/// </remarks>
[BuildDefinition]
[GenerateEntryPoint]
internal sealed partial class Build : BuildDefinition
{
    /// <summary>
    ///     Optional path to a file containing the output of <c>dotnet --cli-schema</c>.
    /// </summary>
    /// <remarks>
    ///     When not provided, the schema is obtained by running <c>dotnet --cli-schema</c> with the current SDK. Useful
    ///     when <c>dotnet --cli-schema</c> fails in the current environment (some SDK builds crash while resolving the
    ///     <c>completions</c> command's shell); a schema can then be assembled from per-command
    ///     <c>dotnet &lt;command&gt; --cli-schema</c> output.
    /// </remarks>
    [ParamDefinition("cli-schema-file", "Path to a file containing the output of 'dotnet --cli-schema'.")]
    private string? CliSchemaFile => GetParam(() => CliSchemaFile);

    /// <summary>
    ///     The target that generates the .NET CLI wrapper code.
    /// </summary>
    /// <remarks>
    ///     This target performs the following steps:
    ///     <list type="number">
    ///         <item>
    ///             <description>Executes `dotnet --cli-schema` to retrieve the .NET CLI's command schema in JSON format.</description>
    ///         </item>
    ///         <item>
    ///             <description>Parses the JSON schema into a structured command model.</description>
    ///         </item>
    ///         <item>
    ///             <description>Flattens the command hierarchy, prefixing subcommand names with their parent commands.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Filters out commands and options the schema marks as hidden (internal commands and legacy
    ///                 verb-first aliases such as "add package"), and specific commands (e.g., "completions script",
    ///                 "help") that should not have generated wrappers.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 For each valid command, it generates a C# interface, an implementation class, and an options
    ///                 record.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>Writes these generated C# files to the `Invex.Atom.Module.Dotnet/Cli/Generated` directory.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Deletes previously generated files in that directory that were not written by this run.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     This process ensures that the `Invex.Atom.Module.Dotnet` always provides an up-to-date and type-safe API for the
    ///     .NET CLI.
    /// </remarks>
    private Target GenerateDotnetCliCode =>
        t => t.Executes(async () =>
        {
            Logger.LogInformation("Generating .NET CLI wrapper code...");

            // Get the .NET CLI schema
            var schemaJson = CliSchemaFile is { Length: > 0 }
                ? await FileSystem.File.ReadAllTextAsync(CliSchemaFile)
                : (await ProcessRunner.RunAsync(new("dotnet", "--cli-schema"))).Output;

            var cliSchema = JsonDocument.Parse(schemaJson);

            // Parse the schema
            var rootCommand = DotnetCliParser.GetCommand("dotnet", cliSchema.RootElement);

            if (rootCommand is null)
                throw new InvalidOperationException("Failed to parse dotnet CLI schema.");

            var commands = DotnetCliGenerator.GetCommandsToGenerate(rootCommand);

            var generatedDirectory = FileSystem.AtomRootDirectory /
                                     "src" /
                                     "Invex.Atom.Module.Dotnet" /
                                     "Cli" /
                                     "Generated";

            if (!FileSystem.Directory.Exists(generatedDirectory))
                FileSystem.Directory.CreateDirectory(generatedDirectory);

            // Generate and write code for each command
            var writtenFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var command in commands)
            {
                var fileName = $"{CsharpWriter.ToPascalCase(command.Name)}.cs";

                await FileSystem.File.WriteAllTextAsync(generatedDirectory / fileName,
                    DotnetCliGenerator.GenerateFile(command));

                writtenFiles.Add(fileName);
            }

            // Delete stale generated files (e.g. commands that were removed or are now hidden)
            foreach (var file in FileSystem.Directory.GetFiles(generatedDirectory, "*.cs"))
            {
                if (writtenFiles.Contains(FileSystem.Path.GetFileName(file)))
                    continue;

                var content = await FileSystem.File.ReadAllTextAsync(file);

                if (!content.StartsWith(DotnetCliGenerator.AutoGeneratedHeader, StringComparison.Ordinal))
                    continue;

                Logger.LogInformation("Deleting stale generated file {File}", file);
                FileSystem.File.Delete(file);
            }

            Logger.LogInformation("Successfully generated .NET CLI wrapper code for {CommandCount} commands.",
                commands.Count);
        });
}
