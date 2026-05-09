using Verifier = DecSm.Atom.Build.Analyzers.Tests.ExtendedCodeFixVerifier<
    DecSm.Atom.Build.Analyzers.AT0002_ConfigureHostPartialMethodNotImplementedAnalyzer, DecSm.Atom.Build.Analyzers.AT0002_ConfigureHostPartialMethodNotImplementedCodeFixProvider>;

namespace DecSm.Atom.Build.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public sealed class AT0002_ConfigureHostPartialMethodNotImplementedCodeFixProviderTests
{
    private void Configure(
        CSharpCodeFixTest<AT0002_ConfigureHostPartialMethodNotImplementedAnalyzer,
            AT0002_ConfigureHostPartialMethodNotImplementedCodeFixProvider, DefaultVerifier> configuration)
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
    public async Task CodeFix_AddsPartialMethodImplementation()
    {
        const string text = """
                            using DecSm.Atom.Build.Hosting;

                            [ConfigureHost]
                            public partial interface IMyHostConfigurator
                            {
                            }
                            """;

        const string fixedText = """
                                 using DecSm.Atom.Build.Hosting;
                                 using Microsoft.Extensions.Hosting;

                                 [ConfigureHost]
                                 public partial interface IMyHostConfigurator
                                 {
                                     protected static partial void ConfigureHostFromIMyHostConfigurator(IHost host)
                                     {
                                     }
                                 }
                                 """;

        var expected = Verifier
            .Diagnostic()
            .WithSpan(4, 26, 4, 45)
            .WithArguments("IMyHostConfigurator", "ConfigureHostFromIMyHostConfigurator");

        await Verifier.VerifyCodeFixAsync(text, expected, fixedText, Configure);
    }

    [Fact]
    public async Task CodeFix_AddsPartialMethodImplementationToEmptyType()
    {
        const string text = """
                            using DecSm.Atom.Build.Hosting;

                            [ConfigureHost]
                            public partial interface IMyHostConfigurator;
                            """;

        const string fixedText = """
                                 using DecSm.Atom.Build.Hosting;
                                 using Microsoft.Extensions.Hosting;

                                 [ConfigureHost]
                                 public partial interface IMyHostConfigurator
                                 {
                                     protected static partial void ConfigureHostFromIMyHostConfigurator(IHost host)
                                     {
                                     }
                                 }
                                 """;

        var expected = Verifier
            .Diagnostic()
            .WithSpan(4, 26, 4, 45)
            .WithArguments("IMyHostConfigurator", "ConfigureHostFromIMyHostConfigurator");

        await Verifier.VerifyCodeFixAsync(text, expected, fixedText, Configure);
    }
}
