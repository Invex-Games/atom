namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ArtifactBuild : MinimalBuildDefinition,
    IDevopsWorkflows,
    IArtifactTarget1,
    IArtifactTarget2,
    IArtifactTarget3,
    IArtifactTarget4
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("artifact-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets =
            [
                WorkflowTargets.ArtifactTarget1,
                WorkflowTargets.ArtifactTarget2,
                WorkflowTargets.ArtifactTarget3,
                WorkflowTargets.ArtifactTarget4.WithMatrixDimensions(
                    new MatrixDimension(nameof(IArtifactSliceTarget1.Slice))
                    {
                        Values = [IArtifactTarget2.Slice1, IArtifactTarget2.Slice2],
                    }),
            ],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

[BuildDefinition]
public partial class CustomArtifactBuild : MinimalBuildDefinition,
    IDevopsWorkflows,
    IStoreArtifact,
    IRetrieveArtifact,
    IArtifactTarget1,
    IArtifactTarget2,
    IArtifactTarget3,
    IArtifactTarget4
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("custom-artifact-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets =
            [
                WorkflowTargets.SetupBuildInfo,
                WorkflowTargets.ArtifactTarget1,
                WorkflowTargets.ArtifactTarget2,
                WorkflowTargets.ArtifactTarget3,
                WorkflowTargets.ArtifactTarget4.WithMatrixDimensions(
                    new MatrixDimension(nameof(IArtifactSliceTarget1.Slice))
                    {
                        Values = [IArtifactTarget2.Slice1, IArtifactTarget2.Slice2],
                    }),
            ],
            WorkflowTypes = [Devops.WorkflowType],
            Options = [WorkflowOptions.Artifacts.UseCustomProvider],
        },
    ];
}

public interface IArtifactSliceTarget1 : IBuildAccessor
{
    const string Slice1 = "Slice1";
    const string Slice2 = "Slice2";

    [ParamDefinition("slice", "Slice")]
    string Slice => GetParam(() => Slice)!;
}

public interface IArtifactTarget1
{
    const string Artifact1 = "TestArtifact1";

    Target ArtifactTarget1 =>
        t => t
            .DescribedAs("Artifact Target 1")
            .ProducesArtifact(Artifact1);
}

public interface IArtifactTarget2 : IArtifactSliceTarget1
{
    const string Artifact2 = "TestArtifact2";

    Target ArtifactTarget2 =>
        t => t
            .DescribedAs("Artifact Target 2")
            .ConsumesArtifact(nameof(IArtifactTarget1.ArtifactTarget1), IArtifactTarget1.Artifact1)
            .ProducesArtifact(Artifact2, Slice1)
            .ProducesArtifact(Artifact2, Slice2);
}

public interface IArtifactTarget3 : IArtifactSliceTarget1
{
    Target ArtifactTarget3 =>
        t => t
            .DescribedAs("Artifact Target 3")
            .ConsumesArtifact(nameof(IArtifactTarget1.ArtifactTarget1), IArtifactTarget1.Artifact1)
            .ConsumesArtifact(nameof(IArtifactTarget2.ArtifactTarget2), IArtifactTarget2.Artifact2, Slice1)
            .ConsumesArtifact(nameof(IArtifactTarget2.ArtifactTarget2), IArtifactTarget2.Artifact2, Slice2);
}

public interface IArtifactTarget4
{
    Target ArtifactTarget4 =>
        t => t
            .DescribedAs("Artifact Target 4")
            .ConsumesArtifact(nameof(IArtifactTarget2.ArtifactTarget2), IArtifactTarget2.Artifact2);
}
