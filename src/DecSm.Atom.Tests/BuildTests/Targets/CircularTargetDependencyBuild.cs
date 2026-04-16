namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class CircularTargetDependencyBuild : MinimalBuildDefinition, ICircularTarget1, ICircularTarget2
{
    public bool CircularTarget1Executed { get; set; }

    public bool CircularTarget2Executed { get; set; }
}

public interface ICircularTarget1
{
    bool CircularTarget1Executed { set; }

    Target CircularTarget1 =>
        t => t
            .DependsOn(nameof(ICircularTarget1))
            .Executes(() =>
            {
                CircularTarget1Executed = true;

                return Task.CompletedTask;
            });
}

public interface ICircularTarget2
{
    bool CircularTarget2Executed { set; }

    Target CircularTarget2 =>
        t => t
            .DependsOn(nameof(ICircularTarget2))
            .Executes(() =>
            {
                CircularTarget2Executed = true;

                return Task.CompletedTask;
            });
}

[BuildDefinition]
public partial class CircularTargetDependencyBuild2 : MinimalBuildDefinition,
    ITestCircularTarget3,
    ITestCircularTarget4,
    ITestCircularTarget5
{
    public bool CircularTarget3Executed { get; set; }

    public bool CircularTarget4Executed { get; set; }

    public bool CircularTarget5Executed { get; set; }
}

public interface ITestCircularTarget3
{
    bool CircularTarget3Executed { set; }

    Target TestCircularTarget3 =>
        t => t
            .DependsOn(nameof(ITestCircularTarget4))
            .Executes(() =>
            {
                CircularTarget3Executed = true;

                return Task.CompletedTask;
            });
}

public interface ITestCircularTarget4
{
    bool CircularTarget4Executed { set; }

    Target TestCircularTarget4 =>
        t => t
            .DependsOn(nameof(ITestCircularTarget5))
            .Executes(() =>
            {
                CircularTarget4Executed = true;

                return Task.CompletedTask;
            });
}

public interface ITestCircularTarget5
{
    bool CircularTarget5Executed { set; }

    Target TestCircularTarget5 =>
        t => t
            .DependsOn(nameof(ITestCircularTarget3))
            .Executes(() =>
            {
                CircularTarget5Executed = true;

                return Task.CompletedTask;
            });
}
