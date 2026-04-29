namespace DecSm.Atom.Build.Tests.BuildTests.Core;

[BuildDefinition]
public partial class TestTargetAtomBuild : BuildDefinition, ITestTarget
{
    public string Description { get; set; } = "Test target";

    public Func<Task> Execute { get; set; } = () => Task.CompletedTask;
}

public interface ITestTarget
{
    string Description { get; }

    Func<Task> Execute { get; set; }

    Target TestTarget =>
        t => t
            .DescribedAs(Description)
            .Executes(Execute);
}
