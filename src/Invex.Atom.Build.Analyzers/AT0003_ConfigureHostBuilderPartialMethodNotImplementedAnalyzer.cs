namespace Invex.Atom.Build.Analyzers;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Analyzer that reports when an interface decorated with [ConfigureHostBuilder] is missing
///     the required partial method implementation ConfigureBuilderFrom{InterfaceName}.
/// </summary>
#pragma warning disable RS1038
[DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1038
public class AT0003_ConfigureHostBuilderPartialMethodNotImplementedAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AT0003";

    private const string Category = "Usage";

    private const string ConfigureHostBuilderAttributeFull = "Invex.Atom.Build.Hosting.ConfigureHostBuilderAttribute";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AT0003Title),
        Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Resources.AT0003MessageFormat),
            Resources.ResourceManager,
            typeof(Resources));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Resources.AT0003Description),
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
            if (attr.AttributeClass?.ToDisplayString() == ConfigureHostBuilderAttributeFull)
            {
                hasAttribute = true;

                break;
            }

        if (!hasAttribute)
            return;

        var expectedMethodName = $"ConfigureBuilderFrom{interfaceSymbol.Name}";

        if (AT0002_ConfigureHostPartialMethodNotImplementedAnalyzer.HasMethodWithBody(interfaceSymbol,
                expectedMethodName,
                context.CancellationToken))
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
}
