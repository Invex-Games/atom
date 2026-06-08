using CodeFixVerifier = Invex.Atom.Build.Analyzers.Tests.ExtendedCodeFixVerifier<
    Invex.Atom.Build.Analyzers.AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer, Invex.Atom.Build.Analyzers.AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamCodeFixProvider>;

namespace Invex.Atom.Build.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public sealed class AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamCodeFixProviderTests
{
    private void Configure(
        CSharpCodeFixTest<AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer,
            AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamCodeFixProvider, DefaultVerifier> configuration)
    {
        configuration.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId);

            if (project == null)
                return solution; // Should not happen in normal test execution

            // Get the existing parse options and update the language version
            var parseOptions = (CSharpParseOptions)project.ParseOptions!;

            var updatedParseOptions = parseOptions.WithLanguageVersion(LanguageVersion.CSharp14);

            // Return the solution with the updated parse options for the project
            return solution.WithProjectParseOptions(projectId, updatedParseOptions);
        });

        // TODO: Use standard .NET 10.0 reference assemblies when available
        // configuration.ReferenceAssemblies = ReferenceAssemblies.Net.Net100;
        configuration.ReferenceAssemblies = new("net10.0",
            new("Microsoft.NETCore.App.Ref", "10.0.8"),
            Path.Combine("ref", "net10.0"));

        var assemblyReference = MetadataReference.CreateFromFile(typeof(BuildDefinition).Assembly.Location);
        configuration.TestState.AdditionalReferences.AddRange([assemblyReference]);
    }

    [Fact]
    public async Task RequiresParamWithDirectParam_ReplacesWithNameof()
    {
        const string sourceCode = """
                                  using Invex.Atom.Build;
                                  using Invex.Atom.Build.Definition;
                                  using Invex.Atom.Build.Params;

                                  public interface IMyTarget : IBuildAccessor
                                  {
                                      [ParamDefinition("my-param", "My Param")]
                                      string MyParam => GetParam(() => MyParam)!;

                                      Target MyTarget => t => t.RequiresParam(MyParam);
                                  }
                                  """;

        const string fixedCode = """
                                 using Invex.Atom.Build;
                                 using Invex.Atom.Build.Definition;
                                 using Invex.Atom.Build.Params;

                                 public interface IMyTarget : IBuildAccessor
                                 {
                                     [ParamDefinition("my-param", "My Param")]
                                     string MyParam => GetParam(() => MyParam)!;

                                     Target MyTarget => t => t.RequiresParam(nameof(MyParam));
                                 }
                                 """;

        var expected = CodeFixVerifier
            .Diagnostic(AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer.DiagnosticId)
            .WithSpan(10, 45, 10, 52)
            .WithArguments("MyParam");

        await CodeFixVerifier.VerifyCodeFixAsync(sourceCode, [expected], fixedCode, Configure);
    }
}
