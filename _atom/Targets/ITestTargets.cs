namespace Atom.Targets;

internal interface ITestTargets : IDotnetTestHelper
{
    static readonly string[] ProjectsToTest =
    [
        Projects.Invex_Atom_Build_Tests.Name,
        Projects.Invex_Atom_Build_Analyzers_Tests.Name,
        Projects.Invex_Atom_Build_SourceGenerators_Tests.Name,
        Projects.Invex_Atom_Module_DevopsWorkflows_Tests.Name,
        Projects.Invex_Atom_Module_Dotnet_Tests.Name,
        Projects.Invex_Atom_Module_GithubWorkflows_Tests.Name,
        Projects.Invex_Atom_Workflows_Tests.Name,
        Projects.Invex_Atom_Tool_Tests.Name,
    ];

    [ParamDefinition("test-framework", "Test framework to use for unit tests")]
    string TestFramework => GetParam(() => TestFramework, "net10.0");

    Target TestProjects =>
        t => t
            .DescribedAs("Runs all unit tests for the Atom projects")
            .RequiresParam(nameof(TestFramework))
            .ProducesArtifacts(ProjectsToTest)
            .Executes(async cancellationToken =>
            {
                var exitCode = 0;

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var project in ProjectsToTest)
                    exitCode += await DotnetTestAndStage(project,
                        new()
                        {
                            TestOptions = new()
                            {
                                Framework = TestFramework,
                            },
                            IncludeCoverage = !project.Contains("Analyzers") && !project.Contains("SourceGenerators"),
                        },
                        cancellationToken);

                if (exitCode != 0)
                    throw new StepFailedException("One or more unit tests failed");
            });
}
