namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class DependencyTargetBuild : MinimalBuildDefinition,
    IDependencyTarget1,
    IDependencyTarget2,
    IDependencyFailTarget1,
    IDependencyFailTarget2
{
    public bool DependencyFailTarget1Executed { get; set; }

    public bool DependencyFailTarget2Executed { get; set; }

    public bool DependencyTarget1Executed { get; set; }

    public bool DependencyTarget2Executed { get; set; }
}

public interface IDependencyTarget1
{
    bool DependencyTarget1Executed { set; }

    Target DependencyTarget1 =>
        t => t.Executes(() =>
        {
            DependencyTarget1Executed = true;

            return Task.CompletedTask;
        });
}

public interface IDependencyTarget2
{
    bool DependencyTarget2Executed { set; }

    Target DependencyTarget2 =>
        t => t
            .DependsOn(nameof(IDependencyTarget1.DependencyTarget1))
            .Executes(() =>
            {
                DependencyTarget2Executed = true;

                return Task.CompletedTask;
            });
}

public interface IDependencyFailTarget1
{
    bool DependencyFailTarget1Executed { set; }

    Target DependencyFailTarget1 =>
        t => t.Executes(() =>
        {
            DependencyFailTarget1Executed = true;

            throw new("TestFailTarget1 failed");
        });
}

public interface IDependencyFailTarget2
{
    bool DependencyFailTarget2Executed { set; }

    Target DependencyFailTarget2 =>
        t => t
            .DependsOn(nameof(IDependencyFailTarget1.DependencyFailTarget1))
            .Executes(() =>
            {
                DependencyFailTarget2Executed = true;

                return Task.CompletedTask;
            });
}
