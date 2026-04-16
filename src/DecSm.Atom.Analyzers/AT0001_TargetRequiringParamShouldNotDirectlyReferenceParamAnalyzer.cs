namespace DecSm.Atom.Analyzers;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Analyzer that reports direct parameter references in RequiresParam calls that should use nameof instead.
/// </summary>
#pragma warning disable RS1038
[DiagnosticAnalyzer(LanguageNames.CSharp)]
#pragma warning restore RS1038
public class AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "AT0001";

    // The category of the diagnostic (Design, Naming etc.).
    private const string Category = "Usage";

    // Feel free to use raw strings if you don't need localization.
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AT0001Title),
        Resources.ResourceManager,
        typeof(Resources));

    // The message that will be displayed to the user.
    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Resources.AT0001MessageFormat),
            Resources.ResourceManager,
            typeof(Resources));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Resources.AT0001Description),
            Resources.ResourceManager,
            typeof(Resources));

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        true,
        Description);

    // Keep in mind: you have to list your rules here.
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // You must call this method to avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // You must call this method to enable the Concurrent Execution.
        context.EnableConcurrentExecution();

        // Register for invocation expressions (method calls)
        context.RegisterOperationAction(AnalyzeOperation, OperationKind.AnonymousFunction);
    }

    private static void AnalyzeOperation(OperationAnalysisContext context)
    {
        if (context.Operation is not IAnonymousFunctionOperation anonFuncOperation ||
            context.Operation.Syntax is not SimpleLambdaExpressionSyntax)
            return;

        if (anonFuncOperation.Body.Operations.Length is 0)
            return;

        foreach (var bodyOperation in anonFuncOperation.Body.Operations.OfType<IReturnOperation>())
        {
            if (bodyOperation.ReturnedValue is not IInvocationOperation returnedValue)
                return;

            AnalyzeInvocationOperation(context, returnedValue);
        }
    }

    private static void AnalyzeInvocationOperation(
        OperationAnalysisContext context,
        IInvocationOperation invocationOperation)
    {
        if (invocationOperation.ChildOperations.Count <= 1)
            return;

        foreach (var childInvocationOperation in invocationOperation.ChildOperations.OfType<IInvocationOperation>())
            AnalyzeInvocationOperation(context, childInvocationOperation);

        var argOperations = invocationOperation.ChildOperations.OfType<IArgumentOperation>();

        foreach (var argumentOperation in argOperations)
        {
            if (argumentOperation.Value is not ICollectionExpressionOperation collectionExpressionOperation ||
                collectionExpressionOperation.Elements.First() is not IPropertyReferenceOperation
                    propertyReferenceOperation)
                return;

            // Check that propertyReferenceOperation is a property with the [ParamDefinition] attribute
            if (!propertyReferenceOperation
                    .Property
                    .GetAttributes()
                    .Any(attr => attr.AttributeClass?.Name == "ParamDefinitionAttribute"))
                return;

            var properties = new Dictionary<string, string?>
            {
                { "paramName", propertyReferenceOperation.Property.Name },
            }.ToImmutableDictionary();

            var diagnostic = Diagnostic.Create(Rule,
                propertyReferenceOperation.Syntax.GetLocation(),
                properties,
                propertyReferenceOperation.Property.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
