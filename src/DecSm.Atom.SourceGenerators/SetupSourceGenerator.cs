using DeclarationResult =
    (Microsoft.CodeAnalysis.CSharp.Syntax.InterfaceDeclarationSyntax Declaration, bool HasConfigureBuilder, bool
    HasConfigureHost);

namespace DecSm.Atom.SourceGenerators;

[Generator]
public class SetupSourceGenerator : IIncrementalGenerator
{
    private const string ConfigureHostBuilderAttributeFull = "DecSm.Atom.Hosting.ConfigureHostBuilderAttribute";
    private const string ConfigureHostAttributeFull = "DecSm.Atom.Hosting.ConfigureHostAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterSourceOutput(context.CompilationProvider.Combine(context
                .SyntaxProvider
                .CreateSyntaxProvider(static (syntaxNode, _) => syntaxNode is InterfaceDeclarationSyntax,
                    static (context, _) => GetInterfaceDeclaration(context))
                .Where(static declarationResult =>
                    declarationResult.HasConfigureBuilder || declarationResult.HasConfigureHost)
                .Select(static (declarationResult, _) => declarationResult.Declaration)
                .Collect()),
            GenerateCode);

    private static DeclarationResult GetInterfaceDeclaration(GeneratorSyntaxContext context)
    {
        var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;

        var hasConfigureBuilder = false;
        var hasConfigureHost = false;

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var attributeListSyntax in interfaceDeclarationSyntax.AttributeLists)
        foreach (var attributeSyntax in attributeListSyntax.Attributes)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);

            if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                continue;

            var attributeName = attributeSymbol.ContainingType.ToDisplayString();

            switch (attributeName)
            {
                case ConfigureHostBuilderAttributeFull:
                    hasConfigureBuilder = true;

                    break;
                case ConfigureHostAttributeFull:
                    hasConfigureHost = true;

                    break;
            }

            // If both attributes are found, we can stop checking further.
            if (hasConfigureBuilder && hasConfigureHost)
                break;
        }

        return (interfaceDeclarationSyntax, hasConfigureBuilder, hasConfigureHost);
    }

    private static void GenerateCode(
        SourceProductionContext context,
        (Compilation Compilation, ImmutableArray<InterfaceDeclarationSyntax> ClassDeclarations)
            compilationWithClassDeclarations)
    {
        foreach (var interfaceDeclarationSyntax in compilationWithClassDeclarations.ClassDeclarations)
            if (compilationWithClassDeclarations
                    .Compilation
                    .GetSemanticModel(interfaceDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(interfaceDeclarationSyntax) is INamedTypeSymbol classSymbol)
                GeneratePartial(context, classSymbol, interfaceDeclarationSyntax);
    }

    private static void GeneratePartial(
        SourceProductionContext context,
        INamedTypeSymbol interfaceSymbol,
        InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        var @namespace = interfaceSymbol.ContainingNamespace.ToDisplayString();

        var namespaceLine = @namespace is "<global namespace>"
            ? string.Empty
            : $"namespace {@namespace};";

        var @interface = interfaceDeclarationSyntax.Identifier.Text;

        var hasConfigureBuilder = interfaceSymbol
            .GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == ConfigureHostBuilderAttributeFull);

        var hasConfigureHost = interfaceSymbol
            .GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == ConfigureHostAttributeFull);

        var inheritsConfigureBuilder = interfaceSymbol.AllInterfaces.Any(i => i
            .GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == ConfigureHostBuilderAttributeFull));

        var inheritsConfigureHost = interfaceSymbol.AllInterfaces.Any(i => i
            .GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == ConfigureHostAttributeFull));

        var setupBuilderLine = hasConfigureBuilder
            ? $"""
                   [JetBrains.Annotations.UsedImplicitly]
                   protected {(inheritsConfigureBuilder ? "new " : string.Empty)}static partial void ConfigureBuilder(IHostApplicationBuilder builder);
               """
            : string.Empty;

        var setupHostLine = hasConfigureHost
            ? $"""
                   [JetBrains.Annotations.UsedImplicitly]
                   protected {(inheritsConfigureHost ? "new " : string.Empty)}static partial void ConfigureHost(IHost host);
               """
            : string.Empty;

        // Build up the source code
        var code = $$"""
                     // <auto-generated/>

                     #nullable enable

                     using Microsoft.Extensions.Hosting;

                     {{namespaceLine}}

                     partial interface {{@interface}}
                     {
                     {{setupBuilderLine}}
                     {{setupHostLine}}
                     }

                     """;

        // Add the source code to the compilation.
        context.AddSource($"{@interface}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}
