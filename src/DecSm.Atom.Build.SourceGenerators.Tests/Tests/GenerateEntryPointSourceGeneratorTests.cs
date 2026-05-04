namespace DecSm.Atom.Build.SourceGenerators.Tests.Tests;

internal sealed class GenerateEntryPointSourceGeneratorTests
{
    [Test]
    public async Task EmptyDefinition_GeneratesDefaultSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;
                              using DecSm.Atom.Build.Hosting;

                              namespace TestNamespace;

                              [GenerateEntryPoint]
                              [BuildDefinition]
                              public partial class TestBuildDefinition : BuildDefinition;
                              """;

        // Act
        var generatedText =
            TestUtils.GetGeneratedSource<GenerateEntryPointSourceGenerator>(source, typeof(BuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }
}
