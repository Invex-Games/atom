using Verifier = DecSm.Atom.Analyzers.Tests.ExtendedAnalyzerVerifier<
    DecSm.Atom.Analyzers.AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer>;

namespace DecSm.Atom.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public class AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzerTests
{
    private void Configure(
        CSharpAnalyzerTest<AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer, DefaultVerifier>
            configuration)
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
            new("Microsoft.NETCore.App.Ref", "10.0.0-rc.1.25451.107"),
            Path.Combine("ref", "net10.0"));

        var assemblyReference = MetadataReference.CreateFromFile(typeof(MinimalBuildDefinition).Assembly.Location);
        configuration.TestState.AdditionalReferences.AddRange([assemblyReference]);
    }

    [Fact]
    public async Task TargetWithRequiresParamOfDirectParam_AlertDiagnostic()
    {
        const string text = """
                            using DecSm.Atom.Build;
                            using DecSm.Atom.Build.Definition;
                            using DecSm.Atom.Params;

                            public interface IMyTarget : IBuildAccessor
                            {
                                [ParamDefinition("my-param-1", "My Param 1")]
                                string MyParam1 => GetParam(() => MyParam1)!;

                                [ParamDefinition("my-param-2", "My Param 2")]
                                string MyParam2 => GetParam(() => MyParam2)!;

                                string NotParam1 { get; }
                                string NotParam2 { get; }

                                Target MyTarget => t => t.RequiresParam(MyParam1).RequiresParam(NotParam1).RequiresParam(MyParam2);
                                Target MyTarget2 => t => t.RequiresParam(NotParam1).RequiresParam(MyParam1).RequiresParam(NotParam2);
                            }
                            """;

        DiagnosticResult[] expected =
        [
            Verifier
                .Diagnostic()
                .WithSpan(16, 45, 16, 53)
                .WithArguments("MyParam1"),
            Verifier
                .Diagnostic()
                .WithSpan(16, 94, 16, 102)
                .WithArguments("MyParam2"),
            Verifier
                .Diagnostic()
                .WithSpan(17, 71, 17, 79)
                .WithArguments("MyParam1"),
        ];

        await Verifier.VerifyAnalyzerAsync(text, Configure, expected);
    }
}
