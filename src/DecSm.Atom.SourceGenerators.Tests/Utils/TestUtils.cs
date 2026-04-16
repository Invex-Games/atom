namespace DecSm.Atom.SourceGenerators.Tests.Utils;

public static class TestUtils
{
    public static string GetGeneratedSource<TSourceGenerator>(string source, params Assembly[] additionalAssemblies)
        where TSourceGenerator : IIncrementalGenerator, new()
    {
        var compilation = CreateCompilation(source, additionalAssemblies);
        var driver = CreateDriver<TSourceGenerator>();

        driver = (CSharpGeneratorDriver)driver.RunGenerators(compilation);

        var generatedText = driver
            .GetRunResult()
            .Results
            .Single()
            .GeneratedSources
            .Single()
            .SourceText
            .ToString()
            .Replace("\r\n", "\n");

        return generatedText;
    }

    private static CSharpCompilation CreateCompilation(string source, Assembly[] additionalAssemblies)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references =
            Net100.References.All.Concat(additionalAssemblies.Select(a =>
                MetadataReference.CreateFromFile(a.Location)));

        var compilation = CSharpCompilation.Create("Tests", [syntaxTree], references);

        return compilation;
    }

    private static CSharpGeneratorDriver CreateDriver<TSourceGenerator>()
        where TSourceGenerator : IIncrementalGenerator, new() =>
        CSharpGeneratorDriver.Create(new TSourceGenerator());
}
