namespace DecSm.Atom.SourceGenerators;

[Generator]
public class GenerateSolutionModelSourceGenerator : IIncrementalGenerator
{
    private static readonly Regex SlnProjectLineRegex = new("""
                                                            ^Project\s*\(\s*"\{[A-F0-9\-]+\}"\s*\)\s*=\s*"([^"]+)",\s*"([^"]+)"
                                                            """,
        RegexOptions.Multiline | RegexOptions.Compiled);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classSymbolsProvider = context
            .SyntaxProvider
            .ForAttributeWithMetadataName(GenerateSolutionModelAttribute,
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol)
            .WithTrackingName(nameof(GenerateSolutionModelSourceGenerator))
            .Collect();

        var projectDirProvider = context.AnalyzerConfigOptionsProvider.Select((options, _) =>
            options.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var path)
                ? path
                : null);

        var combinedProvider = classSymbolsProvider.Combine(projectDirProvider);

        context.RegisterSourceOutput(combinedProvider, (spc, source) => GenerateCode(spc, source.Left, source.Right));
    }

    private static void GenerateCode(
        SourceProductionContext context,
        ImmutableArray<INamedTypeSymbol> classSymbols,
        string? projectDir)
    {
        if (classSymbols.IsDefaultOrEmpty || string.IsNullOrEmpty(projectDir))
            return;

        var solutionFilePath = FindSolutionFile(projectDir!);

        if (solutionFilePath is null)
            return;

        try
        {
#pragma warning disable RS1035
            var solutionContent = File.ReadAllText(solutionFilePath);
#pragma warning restore RS1035
            var projects = ParseSolution(solutionContent, solutionFilePath);

            if (projects.IsEmpty)
                return;

            foreach (var classSymbol in classSymbols)
                GeneratePartial(context, classSymbol, solutionFilePath, projects);
        }
        catch
        {
            // Ignored
        }
    }

    private static string? FindSolutionFile(string projectDir)
    {
        var currentDir = new DirectoryInfo(projectDir);

        while (currentDir is not null)
        {
            var solutionFile = currentDir
                                   .EnumerateFiles("*.slnx", SearchOption.TopDirectoryOnly)
                                   .FirstOrDefault() ??
                               currentDir
                                   .EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly)
                                   .FirstOrDefault();

            if (solutionFile != null)
                return solutionFile.FullName;

            currentDir = currentDir.Parent;
        }

        return null;
    }

    private static ImmutableDictionary<string, string> ParseSolution(string solutionContent, string solutionPath) =>
        solutionPath.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase)
            ? ParseSlnx(solutionContent, solutionPath)
            : ParseSln(solutionContent, solutionPath);

    private static ImmutableDictionary<string, string> ParseSlnx(string slnxContent, string slnxPath)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        var slnxDir = Path.GetDirectoryName(slnxPath);

        if (slnxDir is null)
            return builder.ToImmutable();

        try
        {
            var doc = XDocument.Parse(slnxContent);

            foreach (var projectElement in doc.Descendants("Project"))
            {
                var projectPath = projectElement.Attribute("Path")
                    ?.Value;

                if (string.IsNullOrEmpty(projectPath))
                    continue;

                var fullPath = Path.GetFullPath(Path.Combine(slnxDir, projectPath));
                var projectName = Path.GetFileNameWithoutExtension(projectPath);

                if (!string.IsNullOrEmpty(projectName))
                    builder[projectName] = fullPath;
            }
        }
        catch (XmlException)
        {
            // Ignored
        }

        return builder.ToImmutable();
    }

    private static ImmutableDictionary<string, string> ParseSln(string slnContent, string slnPath)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        var slnDir = Path.GetDirectoryName(slnPath);

        if (slnDir is null)
            return builder.ToImmutable();

        foreach (Match match in SlnProjectLineRegex.Matches(slnContent))
        {
            var name = match.Groups[1].Value;
            var path = match.Groups[2].Value;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
                continue;

            var fullPath = Path.GetFullPath(Path.Combine(slnDir, path));
            builder[name] = fullPath;
        }

        return builder.ToImmutable();
    }

    private static void GeneratePartial(
        SourceProductionContext context,
        INamedTypeSymbol classSymbol,
        string solutionPath,
        ImmutableDictionary<string, string> projects)
    {
        var @namespace = classSymbol.ContainingNamespace.ToDisplayString();

        var namespaceLine = @namespace is "<global namespace>"
            ? string.Empty
            : $"namespace {@namespace};";

        var projectFileTypeLines = string.Join("\n\n",
            projects.Select(kvp =>
            {
                var validIdentifier = kvp
                    .Key
                    .Replace('.', '_')
                    .Replace(' ', '_')
                    .Replace("\\", "/")
                    .Split('/')
                    .Last();

                var projectPath = kvp.Value.Replace("\\", "/");

                return $$"""
                             /// <summary>
                             ///    {{projectPath}}
                             /// </summary>
                             public interface {{validIdentifier}} : IPathLocator
                             {
                                 public const string Name = @"{{kvp.Key}}";

                                 static RootedPath IPathLocator.Path(IAtomFileSystem fileSystem) =>
                                     fileSystem.CreateRootedPath(@"{{projectPath}}");

                                 new static RootedPath Path(IAtomFileSystem fileSystem) =>
                                     fileSystem.CreateRootedPath(@"{{projectPath}}");
                             }
                         """;
            }));

        var solutionName = Path.GetFileNameWithoutExtension(solutionPath);
        var solutionPathNormalized = solutionPath.Replace("\\", "/");

        var code = $$"""
                     // <auto-generated/>

                     #nullable enable

                     using DecSm.Atom.Paths;

                     {{namespaceLine}}

                     /// <summary>
                     ///    {{solutionPathNormalized}}
                     /// </summary>
                     public interface Solution : IPathLocator
                     {
                         public const string Name = @"{{solutionName}}";

                         static RootedPath IPathLocator.Path(IAtomFileSystem fileSystem) =>
                            fileSystem.CreateRootedPath(@"{{solutionPathNormalized}}");
                     }

                     public static class Projects
                     {
                     {{projectFileTypeLines}}
                     }
                     """;

        context.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}
