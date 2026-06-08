using Verifier = Invex.Atom.Build.Analyzers.Tests.ExtendedAnalyzerVerifier<
    Invex.Atom.Build.Analyzers.AT0002_ConfigureHostPartialMethodNotImplementedAnalyzer>;

namespace Invex.Atom.Build.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public sealed class AT0002_ConfigureHostPartialMethodNotImplementedAnalyzerTests
{
    private void Configure(
        CSharpAnalyzerTest<AT0002_ConfigureHostPartialMethodNotImplementedAnalyzer, DefaultVerifier> configuration)
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
            new("Microsoft.NETCore.App.Ref", "10.0.0-rc.1.25451.107"),
            Path.Combine("ref", "net10.0"));

        var assemblyReference = MetadataReference.CreateFromFile(typeof(BuildDefinition).Assembly.Location);
        configuration.TestState.AdditionalReferences.AddRange([assemblyReference]);
    }

    [Fact]
    public async Task InterfaceWithConfigureHost_MissingPartialMethod_AlertDiagnostic()
    {
        const string text = """
                            using Invex.Atom.Build.Hosting;

                            [ConfigureHost]
                            public partial interface IMyHostConfigurator
                            {
                            }
                            """;

        DiagnosticResult[] expected =
        [
            Verifier
                .Diagnostic()
                .WithSpan(4, 26, 4, 45)
                .WithArguments("IMyHostConfigurator", "ConfigureHostFromIMyHostConfigurator"),
        ];

        await Verifier.VerifyAnalyzerAsync(text, Configure, expected);
    }

    [Fact]
    public async Task InterfaceWithConfigureHost_HasPartialMethodImplementation_NoDiagnostic()
    {
        const string text = """
                            using Invex.Atom.Build.Hosting;
                            using Microsoft.Extensions.Hosting;

                            [ConfigureHost]
                            public partial interface IMyHostConfigurator
                            {
                                protected static partial void ConfigureHostFromIMyHostConfigurator(IHost host);
                            }

                            public partial interface IMyHostConfigurator
                            {
                                protected static partial void ConfigureHostFromIMyHostConfigurator(IHost host)
                                {
                                }
                            }
                            """;

        await Verifier.VerifyAnalyzerAsync(text, Configure);
    }

    [Fact]
    public async Task InterfaceWithoutConfigureHost_NoDiagnostic()
    {
        const string text = """
                            public partial interface INotConfigured
                            {
                            }
                            """;

        await Verifier.VerifyAnalyzerAsync(text, Configure);
    }
}
