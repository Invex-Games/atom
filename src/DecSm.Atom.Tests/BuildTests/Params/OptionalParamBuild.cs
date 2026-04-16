namespace DecSm.Atom.Tests.BuildTests.Params;

[BuildDefinition]
public partial class OptionalParamBuild : MinimalBuildDefinition, IOptionalParamTarget1
{
    public string? ExecuteValue1 { get; set; }

    public string? ExecuteValue2 { get; set; }
}

public interface IOptionalParamTarget1 : IBuildAccessor
{
    [ParamDefinition("param-1", "Param 1")]
    string? Param1 => GetParam(() => Param1);

    [ParamDefinition("param-2", "Param 2")]
    string? Param2 => GetParam(() => Param2);

    string? ExecuteValue1 { set; }

    string? ExecuteValue2 { set; }

    Target OptionalParamTarget1 =>
        t => t
            .RequiresParam(nameof(Param1))
            .UsesParam(nameof(Param2))
            .Executes(() =>
            {
                ExecuteValue1 = Param1;
                ExecuteValue2 = Param2;

                return Task.CompletedTask;
            });
}
