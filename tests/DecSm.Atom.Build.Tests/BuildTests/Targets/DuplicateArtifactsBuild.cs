namespace DecSm.Atom.Build.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class DuplicateArtifactsBuild : BuildDefinition,
    IDuplicateArtifactsBase1,
    IDuplicateArtifactsBase2,
    IDuplicateArtifactsTarget;

public interface IDuplicateArtifactsBase1
{
    Target ProducerTarget =>
        t => t
            .ProducesArtifact("TestArtifact")
            .Executes(() => Task.CompletedTask);

    Target BaseTarget1 =>
        t => t
            .ConsumesArtifact("ProducerTarget", "TestArtifact")
            .ProducesArtifact("OutputArtifact")
            .Executes(() => Task.CompletedTask);
}

public interface IDuplicateArtifactsBase2
{
    Target BaseTarget2 =>
        t => t
            .ConsumesArtifact("ProducerTarget", "TestArtifact")
            .ProducesArtifact("OutputArtifact")
            .Executes(() => Task.CompletedTask);
}

public interface IDuplicateArtifactsTarget
{
    Target DuplicateArtifactsTarget =>
        t => t
            .Extends<IDuplicateArtifactsBase1>(d => d.BaseTarget1)
            .Extends<IDuplicateArtifactsBase2>(d => d.BaseTarget2)
            .Executes(() => Task.CompletedTask);
}
