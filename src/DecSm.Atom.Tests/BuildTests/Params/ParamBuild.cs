namespace DecSm.Atom.Tests.BuildTests.Params;

[BuildDefinition]
public partial class ParamBuild : MinimalBuildDefinition, IParamTarget1, IParamTarget2
{
    public string? ExecuteValue { get; set; }
}

public interface IParamTarget1 : IBuildAccessor
{
    [ParamDefinition("param-1", "Param 1")]
    string Param1 => GetParam(() => Param1, "DefaultValue");

    string? ExecuteValue { set; }

    Target ParamTarget1 =>
        t => t.Executes(() =>
        {
            ExecuteValue = Param1;

            return Task.CompletedTask;
        });
}

public interface IParamTarget2 : IBuildAccessor
{
    [ParamDefinition("param-2", "Param 2")]
    string Param2 => GetParam(() => Param2)!;

    string? ExecuteValue { set; }

    Target ParamTarget2 =>
        t => t
            .RequiresParam(nameof(Param2))
            .Executes(() =>
            {
                ExecuteValue = Param2;

                return Task.CompletedTask;
            });
}
