using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DecSm.Atom.Analyzers.Tests;

[PublicAPI]
public sealed class ExtendedAnalyzerVerifier<TAnalyzer> : ExtendedAnalyzerVerifier<TAnalyzer,
    CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>, DefaultVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new();

public class ExtendedAnalyzerVerifier<TAnalyzer, TTest, TVerifier>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TTest : AnalyzerTest<TVerifier>, new()
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

    public static Task VerifyAnalyzerAsync(
        string source,
        Action<TTest>? configure = null,
        params DiagnosticResult[] expected)
    {
        var test = new TTest
        {
            TestCode = source,
        };

        test.ExpectedDiagnostics.AddRange(expected);
        test.CompilerDiagnostics = CompilerDiagnostics.None;
        configure?.Invoke(test);

        return test.RunAsync(CancellationToken.None);
    }
}
