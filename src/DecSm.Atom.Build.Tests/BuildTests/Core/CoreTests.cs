namespace DecSm.Atom.Build.Tests.BuildTests.Core;

[TestFixture]
public class CoreTests
{
    [Test]
    public void DefaultBuildDefinition_IsEmpty()
    {
        // Arrange
        var host = CreateTestHost<DefaultAtomBuild>();

        // Act
        var buildModel = host.Services.GetRequiredService<BuildModel>();

        // Assert
        buildModel.ShouldSatisfyAllConditions(b => b.Targets.ShouldBeEmpty(),
            b => b.TargetStates.ShouldBeEmpty(),
            b => b.CurrentTarget.ShouldBeNull());
    }
}
