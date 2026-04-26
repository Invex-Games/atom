namespace DecSm.Atom.Build.SourceGenerators.Tests.Utils;

public static class Initializer
{
    [ModuleInitializer]
    public static void Init() =>
        DiffTools.UseOrder(DiffTool.Rider, DiffTool.VisualStudioCode, DiffTool.VisualStudio, DiffTool.BeyondCompare);
}
