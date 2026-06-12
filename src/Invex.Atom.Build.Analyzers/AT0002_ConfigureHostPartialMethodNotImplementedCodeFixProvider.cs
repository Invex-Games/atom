using Microsoft.CodeAnalysis.Formatting;

namespace Invex.Atom.Build.Analyzers;

// ReSharper disable once InconsistentNaming
/// <summary>
///     Provides a code fix for interfaces decorated with [ConfigureHost] that are missing
///     the required partial method implementation, by inserting the method stub.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp,
    Name = nameof(AT0002_ConfigureHostPartialMethodNotImplementedCodeFixProvider))]
[Shared]
public class AT0002_ConfigureHostPartialMethodNotImplementedCodeFixProvider : CodeFixProvider
{
    private const string HostUsing = "Microsoft.Extensions.Hosting";

    /// <inheritdoc />
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
    [
        AT0002_ConfigureHostPartialMethodNotImplementedAnalyzer.DiagnosticId,
    ];

    /// <inheritdoc />
    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var methodName = diagnostic.Properties["methodName"] ?? "ConfigureHost";

        context.RegisterCodeFix(CodeAction.Create(string.Format(Resources.AT0002CodeFixTitle, methodName),
                c => AddMethodImplementationAsync(context.Document, diagnostic, c),
                nameof(Resources.AT0002CodeFixTitle)),
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

        var methodName = diagnostic.Properties["methodName"] ?? "ConfigureHost";

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan);

        // Find the interface declaration
        var interfaceDeclaration = node.FirstAncestorOrSelf<InterfaceDeclarationSyntax>();

        if (interfaceDeclaration == null)
            return document;

        // Create the method declaration:
        // protected static partial void ConfigureHostFrom{InterfaceName}(IHost host)
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
                    .Parameter(SyntaxFactory.Identifier("host"))
                    .WithType(SyntaxFactory.IdentifierName("IHost")),
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
            var eol = root
                          .DescendantTrivia()
                          .Where(t => t.IsKind(SyntaxKind.EndOfLineTrivia))
                          .Select(t => t.ToString())
                          .FirstOrDefault() ??
                      "\n";

            var usingDirective = SyntaxFactory
                .UsingDirective(SyntaxFactory.ParseName(HostUsing))
                .NormalizeWhitespace()
                .WithTrailingTrivia(SyntaxFactory.EndOfLine(eol));

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
