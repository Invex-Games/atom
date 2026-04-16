namespace DecSm.Atom.Analyzers;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Provides a code fix for direct parameter references in RequiresParam by replacing them with nameof expressions.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp,
    Name = nameof(AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamCodeFixProvider))]
[Shared]
public class AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamCodeFixProvider : CodeFixProvider
{
    // Specify the diagnostic IDs that this provider can fix
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [
        AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer.DiagnosticId,
    ];

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // Get the diagnostic being fixed
        var diagnostic = context.Diagnostics.First();

        // Get the location and identifier text from the diagnostic
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var paramName = diagnostic.Properties["paramName"] ?? diagnostic.GetMessage();

        // Register the code fix
        context.RegisterCodeFix(CodeAction.Create(string.Format(Resources.AT0001CodeFixTitle, paramName),
                c => ReplaceWithNameofAsync(context.Document, diagnosticSpan, c),
                nameof(Resources.AT0001CodeFixTitle)),
            diagnostic);

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Replaces a direct parameter reference with a nameof expression.
    /// </summary>
    private static async Task<Document> ReplaceWithNameofAsync(
        Document document,
        TextSpan diagnosticSpan,
        CancellationToken cancellationToken)
    {
        // Get the syntax root
        var root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root == null)
            return document;

        // Find the identifier node to replace
        var identifierNode = root.FindNode(diagnosticSpan);

        if (identifierNode is not ArgumentSyntax identifierName)
            return document;

        // Create the nameof expression: nameof(paramName)
        var nameofExpression = SyntaxFactory.Argument(identifierName.NameColon,
            identifierName.RefKindKeyword,
            SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("nameof"),
                SyntaxFactory.ArgumentList(SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                    SyntaxFactory.SeparatedList<ArgumentSyntax>([identifierName]),
                    SyntaxFactory.Token(SyntaxKind.CloseParenToken))));

        // Replace the old identifier with the nameof expression
        var newRoot = root.ReplaceNode(identifierNode, nameofExpression);

        // Return the updated document
        return document.WithSyntaxRoot(newRoot);
    }
}
