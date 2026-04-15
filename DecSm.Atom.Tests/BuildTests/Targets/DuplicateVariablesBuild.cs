namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class DuplicateVariablesBuild : MinimalBuildDefinition,
    IDuplicateVariablesBase1,
    IDuplicateVariablesBase2,
    IDuplicateVariablesTarget;

public interface IDuplicateVariablesBase1
{
    Target ProducerTarget =>
        t => t
            .ProducesVariable("TestVariable")
            .Executes(() => Task.CompletedTask);

    Target BaseTarget1 =>
        t => t
            .ConsumesVariable("ProducerTarget", "TestVariable")
            .ProducesVariable("OutputVariable")
            .Executes(() => Task.CompletedTask);
}

public interface IDuplicateVariablesBase2
{
    Target BaseTarget2 =>
        t => t
            .ConsumesVariable("ProducerTarget", "TestVariable")
            .ProducesVariable("OutputVariable")
            .Executes(() => Task.CompletedTask);
}

public interface IDuplicateVariablesTarget
{
    Target DuplicateVariablesTarget =>
        t => t
            .Extends<IDuplicateVariablesBase1>(d => d.BaseTarget1)
            .Extends<IDuplicateVariablesBase2>(d => d.BaseTarget2)
            .Executes(() => Task.CompletedTask);
}
