namespace DecSm.Atom.SourceGenerators.Tests.Tests;

[TestFixture]
public sealed class GenerateInterfaceMembersGeneratorTests
{
    [Test]
    public async Task MinimalBuildDefinition_WithGeneratedInterfaceMember_GeneratesSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;
                              using System.Collections.Generic;

                              namespace TestNamespace;

                              [BuildDefinition]
                              [GenerateInterfaceMembers]
                              public partial class MinimalTestDefinition : ISimpleMembers;

                              public interface ISimpleMembers
                              {
                                  int Property => 1;

                                  void ParameterlessMethod() { }

                                  void MethodWithParameters(int param1, string param2) { }

                                  void MethodWithCollectionParameters(IEnumerable<int> param1, IDictionary<string, string> param2) { }

                                  void MethodWithGenericParameters<T>(T param1) { }

                                  void MethodWithOptionalParameter(int param1 = 1, string? param2 = null, string param3 = nameof(param3), CancellationToken param4 = default) { }

                                  void MethodWithRefParameters(ref int param1, in string param2, out bool param3);

                                  void MethodWithAttributedParameters([Required] int param1, [Required] string param2);
                              }
                              """;

        // Act
        var generatedText =
            TestUtils.GetGeneratedSource<GenerateInterfaceMembersSourceGenerator>(source,
                typeof(MinimalBuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }
}
