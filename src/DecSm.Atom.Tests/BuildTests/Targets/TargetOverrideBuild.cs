namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class TargetOverrideBuild : MinimalBuildDefinition, IOverrideTarget
{
    public bool BaseOverrideTargetExecuted { get; set; }

    public bool OverrideOverrideTargetExecuted { get; set; }
}

public interface IBaseOverrideTarget
{
    bool BaseOverrideTargetExecuted { set; }

    Target OverrideTarget =>
        t => t.Executes(() =>
        {
            BaseOverrideTargetExecuted = true;

            return Task.CompletedTask;
        });
}

public interface IOverrideTarget : IBaseOverrideTarget
{
    bool OverrideOverrideTargetExecuted { set; }

    new Target OverrideTarget =>
        t => t.Executes(() =>
        {
            OverrideOverrideTargetExecuted = true;

            return Task.CompletedTask;
        });
}
