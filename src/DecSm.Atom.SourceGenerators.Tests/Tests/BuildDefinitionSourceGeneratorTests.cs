namespace DecSm.Atom.SourceGenerators.Tests.Tests;

[TestFixture]
public class BuildDefinitionSourceGeneratorTests
{
    [Test]
    public async Task MinimalBuildDefinition_WithoutBaseType_GeneratesSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;

                              namespace TestNamespace;

                              [BuildDefinition]
                              public partial class MinimalTestDefinition;
                              """;

        // Act
        var generatedText =
            TestUtils.GetGeneratedSource<BuildDefinitionSourceGenerator>(source,
                typeof(MinimalBuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }

    [Test]
    public async Task MinimalBuildDefinition_WithBaseType_GeneratesSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;

                              namespace TestNamespace;

                              [BuildDefinition]
                              public partial class MinimalTestDefinition : MinimalBuildDefinition;
                              """;

        // Act
        var generatedText =
            TestUtils.GetGeneratedSource<BuildDefinitionSourceGenerator>(source,
                typeof(MinimalBuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }

    [Test]
    public async Task BuildDefinition_GeneratesSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;

                              namespace TestNamespace;

                              [BuildDefinition]
                              public partial class DefaultTestDefinition : BuildDefinition;
                              """;

        // Act
        var generatedText =
            TestUtils.GetGeneratedSource<BuildDefinitionSourceGenerator>(source,
                typeof(MinimalBuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }

    [Test]
    public async Task DefinitionWithChainedParam_GeneratesSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;
                              using DecSm.Atom.Params;

                              namespace TestNamespace;

                              [BuildDefinition]
                              public partial class ChainedParamBuild : IChainedParamTarget;

                              public interface IChainedParamTarget : IBuildAccessor
                              {
                                  [ParamDefinition("param-1", "Param 1")]
                                  string Param1 => GetParam(() => Param1)!;

                                  [ParamDefinition("param-2", "Param 2", ParamSource.All, [nameof(Param1)])]
                                  string Param2 => GetParam(() => Param2, "DefaultValue")!;

                                  Target ChainedParamTarget =>
                                      t => t
                                          .RequiresParam(nameof(Param2))
                                          .Executes(() => Task.CompletedTask);
                              }
                              """;

        // Act
        var generatedText =
            TestUtils.GetGeneratedSource<BuildDefinitionSourceGenerator>(source,
                typeof(MinimalBuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
        await TestContext.Out.WriteLineAsync(generatedText);
    }
}
