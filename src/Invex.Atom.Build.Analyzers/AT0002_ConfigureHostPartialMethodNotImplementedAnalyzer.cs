namespace Invex.Atom.Build.Analyzers;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Analyzer that reports when an interface decorated with [ConfigureHost] is missing
///     the required partial method implementation ConfigureHostFrom{InterfaceName}.
/// </summary>
#pragma warning disable RS1038
[DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1038
public class AT0002_ConfigureHostPartialMethodNotImplementedAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AT0002";

    private const string Category = "Usage";

    private const string ConfigureHostAttributeFull = "Invex.Atom.Build.Hosting.ConfigureHostAttribute";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AT0002Title),
        Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Resources.AT0002MessageFormat),
            Resources.ResourceManager,
            typeof(Resources));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Resources.AT0002Description),
            Resources.ResourceManager,
            typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        true,
        Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Interface } interfaceSymbol)
            return;

        var hasAttribute = false;

        foreach (var attr in interfaceSymbol.GetAttributes())
            if (attr.AttributeClass?.ToDisplayString() == ConfigureHostAttributeFull)
            {
                hasAttribute = true;

                break;
            }

        if (!hasAttribute)
            return;

        var expectedMethodName = $"ConfigureHostFrom{interfaceSymbol.Name}";

        if (HasMethodWithBody(interfaceSymbol, expectedMethodName, context.CancellationToken))
            return;

        var properties = new Dictionary<string, string?>
        {
            { "methodName", expectedMethodName },
        }.ToImmutableDictionary();

        var diagnostic = Diagnostic.Create(Rule,
            interfaceSymbol.Locations[0],
            properties,
            interfaceSymbol.Name,
            expectedMethodName);

        context.ReportDiagnostic(diagnostic);
    }

    internal static bool HasMethodWithBody(
        INamedTypeSymbol interfaceSymbol,
        string expectedMethodName,
        CancellationToken cancellationToken)
    {
        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol || methodSymbol.Name != expectedMethodName)
                continue;

            // Check the method's own declaring syntax references for a body
            foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
            {
                var syntax = syntaxRef.GetSyntax(cancellationToken);

                if (syntax is MethodDeclarationSyntax methodSyntax &&
                    (methodSyntax.Body != null || methodSyntax.ExpressionBody != null))
                    return true;
            }

            // For partial methods, also check the implementation part
            if (methodSymbol.PartialImplementationPart != null)
                foreach (var syntaxRef in methodSymbol.PartialImplementationPart.DeclaringSyntaxReferences)
                {
                    var syntax = syntaxRef.GetSyntax(cancellationToken);

                    if (syntax is MethodDeclarationSyntax methodSyntax &&
                        (methodSyntax.Body != null || methodSyntax.ExpressionBody != null))
                        return true;
                }
        }

        return false;
    }
}
