using Verifier = Invex.Atom.Build.Analyzers.Tests.ExtendedAnalyzerVerifier<
    Invex.Atom.Build.Analyzers.AT0003_ConfigureHostBuilderPartialMethodNotImplementedAnalyzer>;

namespace Invex.Atom.Build.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public sealed class AT0003_ConfigureHostBuilderPartialMethodNotImplementedAnalyzerTests
{
    private void Configure(
        CSharpAnalyzerTest<AT0003_ConfigureHostBuilderPartialMethodNotImplementedAnalyzer, DefaultVerifier>
            configuration)
    {
        configuration.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId);

            if (project == null)
                return solution;

            var parseOptions = (CSharpParseOptions)project.ParseOptions!;
            var updatedParseOptions = parseOptions.WithLanguageVersion(LanguageVersion.CSharp14);

            return solution.WithProjectParseOptions(projectId, updatedParseOptions);
        });

        configuration.ReferenceAssemblies = new("net10.0",
            new("Microsoft.NETCore.App.Ref", "10.0.8"),
            Path.Combine("ref", "net10.0"));

        var assemblyReference = MetadataReference.CreateFromFile(typeof(BuildDefinition).Assembly.Location);
        configuration.TestState.AdditionalReferences.AddRange([assemblyReference]);
    }

    [Fact]
    public async Task InterfaceWithConfigureHostBuilder_MissingPartialMethod_AlertDiagnostic()
    {
        const string text = """
                            using Invex.Atom.Build.Hosting;

                            [ConfigureHostBuilder]
                            public partial interface IMyBuilderConfigurator
                            {
                            }
                            """;

        DiagnosticResult[] expected =
        [
            Verifier
                .Diagnostic()
                .WithSpan(4, 26, 4, 48)
                .WithArguments("IMyBuilderConfigurator", "ConfigureBuilderFromIMyBuilderConfigurator"),
        ];

        await Verifier.VerifyAnalyzerAsync(text, Configure, expected);
    }

    [Fact]
    public async Task InterfaceWithConfigureHostBuilder_HasPartialMethodImplementation_NoDiagnostic()
    {
        const string text = """
                            using Invex.Atom.Build.Hosting;
                            using Microsoft.Extensions.Hosting;

                            [ConfigureHostBuilder]
                            public partial interface IMyBuilderConfigurator
                            {
                                protected static partial void ConfigureBuilderFromIMyBuilderConfigurator(IHostApplicationBuilder builder);
                            }

                            public partial interface IMyBuilderConfigurator
                            {
                                protected static partial void ConfigureBuilderFromIMyBuilderConfigurator(IHostApplicationBuilder builder)
                                {
                                }
                            }
                            """;

        await Verifier.VerifyAnalyzerAsync(text, Configure);
    }

    [Fact]
    public async Task InterfaceWithoutConfigureHostBuilder_NoDiagnostic()
    {
        const string text = """
                            public partial interface INotConfigured
                            {
                            }
                            """;

        await Verifier.VerifyAnalyzerAsync(text, Configure);
    }
}
