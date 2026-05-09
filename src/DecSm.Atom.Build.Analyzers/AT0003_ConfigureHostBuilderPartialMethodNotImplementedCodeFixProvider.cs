using Microsoft.CodeAnalysis.Formatting;

namespace DecSm.Atom.Build.Analyzers;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Provides a code fix for interfaces decorated with [ConfigureHostBuilder] that are missing
///     the required partial method implementation, by inserting the method stub.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp,
    Name = nameof(AT0003_ConfigureHostBuilderPartialMethodNotImplementedCodeFixProvider))]
[Shared]
public class AT0003_ConfigureHostBuilderPartialMethodNotImplementedCodeFixProvider : CodeFixProvider
{
    private const string HostUsing = "Microsoft.Extensions.Hosting";

    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [
        AT0003_ConfigureHostBuilderPartialMethodNotImplementedAnalyzer.DiagnosticId,
    ];

    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var methodName = diagnostic.Properties["methodName"] ?? "ConfigureBuilder";

        context.RegisterCodeFix(CodeAction.Create(string.Format(Resources.AT0003CodeFixTitle, methodName),
                c => AddMethodImplementationAsync(context.Document, diagnostic, c),
                nameof(Resources.AT0003CodeFixTitle)),
            diagnostic);

        return Task.CompletedTask;
    }

    private static async Task<Document> AddMethodImplementationAsync(
        Document document,
        Diagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        var root = await document
            .GetSyntaxRootAsync(cancellationToken)
            .ConfigureAwait(false);

        if (root == null)
            return document;

        var methodName = diagnostic.Properties["methodName"] ?? "ConfigureBuilder";

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan);

        // Find the interface declaration
        var interfaceDeclaration = node.FirstAncestorOrSelf<InterfaceDeclarationSyntax>();

        if (interfaceDeclaration == null)
            return document;

        // Create the method declaration:
        // protected static partial void ConfigureBuilderFrom{InterfaceName}(IHostApplicationBuilder builder)
        // {
        // }
        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                SyntaxFactory.Identifier(methodName))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
            .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[]
            {
                SyntaxFactory
                    .Parameter(SyntaxFactory.Identifier("builder"))
                    .WithType(SyntaxFactory.IdentifierName("IHostApplicationBuilder")),
            })))
            .WithBody(SyntaxFactory.Block())
            .NormalizeWhitespace();

        // Convert semicolon-terminated empty type to brace-delimited type
        var originalInterfaceDeclaration = interfaceDeclaration;

        if (interfaceDeclaration.SemicolonToken != default)
            interfaceDeclaration = interfaceDeclaration
                .WithSemicolonToken(default)
                .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));

        // Add the method to the interface
        var newInterfaceDeclaration = interfaceDeclaration.AddMembers(method);
        var newRoot = root.ReplaceNode(originalInterfaceDeclaration, newInterfaceDeclaration);

        // Add using directive if not present
        if (newRoot is CompilationUnitSyntax compilationUnit &&
            compilationUnit.Usings.All(u => u.Name?.ToString() != HostUsing))
        {
            var usingDirective = SyntaxFactory
                .UsingDirective(SyntaxFactory.ParseName(HostUsing))
                .NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

            newRoot = compilationUnit.AddUsings(usingDirective);
        }

        var result = document.WithSyntaxRoot(newRoot);

        var resultRoot = await result
                             .GetSyntaxRootAsync(cancellationToken)
                             .ConfigureAwait(false) ??
                         throw new InvalidOperationException();

        var formattedRoot = Formatter.Format(resultRoot.WithAdditionalAnnotations(Formatter.Annotation),
            document.Project.Solution.Workspace);

        return result.WithSyntaxRoot(formattedRoot);
    }
}
