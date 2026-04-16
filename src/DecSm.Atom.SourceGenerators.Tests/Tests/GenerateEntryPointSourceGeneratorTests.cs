namespace DecSm.Atom.SourceGenerators.Tests.Tests;

public class GenerateEntryPointSourceGeneratorTests
{
    [Test]
    public async Task EmptyDefinition_GeneratesDefaultSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;
                              using DecSm.Atom.Hosting;

                              namespace TestNamespace;

                              [GenerateEntryPoint]
                              [BuildDefinition]
                              public partial class TestBuildDefinition : BuildDefinition;
                              """;

        // Act
        var generatedText =
            TestUtils.GetGeneratedSource<GenerateEntryPointSourceGenerator>(source,
                typeof(MinimalBuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }
}
