using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DecSm.Atom.Analyzers.Tests;

[PublicAPI]
public class ExtendedCodeFixVerifier<TAnalyzer, TCodeFix> : ExtendedCodeFixVerifier<TAnalyzer,
    CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new();

[PublicAPI]
public class ExtendedCodeFixVerifier<TAnalyzer, TTest, TVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TTest : CodeFixTest<TVerifier>, new()
    where TVerifier : IVerifier, new()
{
    public static DiagnosticResult Diagnostic()
    {
        var analyzer = new TAnalyzer();

        try
        {
            return Diagnostic(analyzer.SupportedDiagnostics.Single());
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"'{nameof(Diagnostic)}()' can only be used when the analyzer has a single supported diagnostic. Use the '{nameof(Diagnostic)}(DiagnosticDescriptor)' overload to specify the descriptor from which to create the expected result.",
                ex);
        }
    }

    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        var analyzer = new TAnalyzer();

        try
        {
            return Diagnostic(analyzer.SupportedDiagnostics.Single(i => i.Id == diagnosticId));
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"'{nameof(Diagnostic)}(string)' can only be used when the analyzer has a single supported diagnostic with the specified ID. Use the '{nameof(Diagnostic)}(DiagnosticDescriptor)' overload to specify the descriptor from which to create the expected result.",
                ex);
        }
    }

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) =>
        new(descriptor);

    public static Task VerifyCodeFixAsync(
        string source,
        DiagnosticResult expected,
        string fixedSource,
        Action<TTest>? configure = null) =>
        VerifyCodeFixAsync(source, [expected], fixedSource, configure);

    public static Task VerifyCodeFixAsync(
        string source,
        DiagnosticResult[] expected,
        string fixedSource,
        Action<TTest>? configure = null)
    {
        var test = new TTest
        {
            TestCode = source,
            FixedCode = fixedSource,
        };

        test.ExpectedDiagnostics.AddRange(expected);
        test.CompilerDiagnostics = CompilerDiagnostics.None;
        configure?.Invoke(test);

        return test.RunAsync(CancellationToken.None);
    }
}
