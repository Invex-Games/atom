using Verifier = Invex.Atom.Build.Analyzers.Tests.ExtendedCodeFixVerifier<
    Invex.Atom.Build.Analyzers.AT0003_ConfigureHostBuilderPartialMethodNotImplementedAnalyzer, Invex.Atom.Build.Analyzers.AT0003_ConfigureHostBuilderPartialMethodNotImplementedCodeFixProvider>;

namespace Invex.Atom.Build.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public sealed class AT0003_ConfigureHostBuilderPartialMethodNotImplementedCodeFixProviderTests
{
    private void Configure(
        CSharpCodeFixTest<AT0003_ConfigureHostBuilderPartialMethodNotImplementedAnalyzer,
            AT0003_ConfigureHostBuilderPartialMethodNotImplementedCodeFixProvider, DefaultVerifier> configuration)
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
    public async Task CodeFix_AddsPartialMethodImplementation()
    {
        const string text = """
                            using Invex.Atom.Build.Hosting;

                            [ConfigureHostBuilder]
                            public partial interface IMyBuilderConfigurator
                            {
                            }
                            """;

        const string fixedText = """
                                 using Invex.Atom.Build.Hosting;
                                 using Microsoft.Extensions.Hosting;

                                 [ConfigureHostBuilder]
                                 public partial interface IMyBuilderConfigurator
                                 {
                                     protected static partial void ConfigureBuilderFromIMyBuilderConfigurator(IHostApplicationBuilder builder)
                                     {
                                     }
                                 }
                                 """;

        var expected = Verifier
            .Diagnostic()
            .WithSpan(4, 26, 4, 48)
            .WithArguments("IMyBuilderConfigurator", "ConfigureBuilderFromIMyBuilderConfigurator");

        await Verifier.VerifyCodeFixAsync(text, expected, fixedText, Configure);
    }

    [Fact]
    public async Task CodeFix_AddsPartialMethodImplementationToEmptyType()
    {
        const string text = """
                            using Invex.Atom.Build.Hosting;

                            [ConfigureHostBuilder]
                            public partial interface IMyBuilderConfigurator;
                            """;

        const string fixedText = """
                                 using Invex.Atom.Build.Hosting;
                                 using Microsoft.Extensions.Hosting;

                                 [ConfigureHostBuilder]
                                 public partial interface IMyBuilderConfigurator
                                 {
                                     protected static partial void ConfigureBuilderFromIMyBuilderConfigurator(IHostApplicationBuilder builder)
                                     {
                                     }
                                 }
                                 """;

        var expected = Verifier
            .Diagnostic()
            .WithSpan(4, 26, 4, 48)
            .WithArguments("IMyBuilderConfigurator", "ConfigureBuilderFromIMyBuilderConfigurator");

        await Verifier.VerifyCodeFixAsync(text, expected, fixedText, Configure);
    }
}
