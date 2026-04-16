namespace DecSm.Atom.Tests.BuildTests.Params;

[BuildDefinition]
public partial class ChainedParamBuild : MinimalBuildDefinition, IChainedParamTarget;

public interface IChainedParamTarget : IBuildAccessor
{
    [ParamDefinition("param-1", "Param 1")]
    string Param1 => GetParam(() => Param1)!;

    [ParamDefinition("param-2", "Param 2", ParamSource.All, [nameof(Param1)])]
    string Param2 => GetParam(() => Param2)!;

    Target ChainedParamTarget1 =>
        t => t
            .RequiresParam(nameof(Param2))
            .Executes(() => Task.CompletedTask);

    Target ChainedParamTarget2 =>
        t => t
            .UsesParam(nameof(Param2))
            .Executes(() => Task.CompletedTask);
}
