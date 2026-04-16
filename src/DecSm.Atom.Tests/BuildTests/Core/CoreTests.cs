namespace DecSm.Atom.Tests.BuildTests.Core;

[TestFixture]
public class CoreTests
{
    [Test]
    public void MinimalDefinition_IsEmpty()
    {
        // Arrange
        var host = CreateTestHost<MinimalAtomBuild>();

        // Act
        var buildModel = host.Services.GetRequiredService<BuildModel>();

        // Assert
        buildModel.ShouldSatisfyAllConditions(b => b.Targets.ShouldBeEmpty(),
            b => b.TargetStates.ShouldBeEmpty(),
            b => b.CurrentTarget.ShouldBeNull());
    }

    [Test]
    public async Task DefaultBuildDefinition_HasDefaultTargets()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>();

        // Act
        var buildModel = host.Services.GetRequiredService<BuildModel>();

        // Assert
        await Verify(buildModel);
    }
}
