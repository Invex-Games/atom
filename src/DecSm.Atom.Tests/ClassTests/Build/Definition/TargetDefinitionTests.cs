namespace DecSm.Atom.Tests.ClassTests.Build.Definition;

[TestFixture]
public class TargetDefinitionTests
{
    [Test]
    public void WithDescription_SetsDescription()
    {
        // Arrange
        const string description = "description";

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition.DescribedAs(description);

        // Assert
        targetDefinition.Description.ShouldBe(description);
    }

    [Test]
    public void Executes_SetsSingleTask()
    {
        // Arrange
        var task = Task.CompletedTask;

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition.Executes(() => task);

        // Assert
        targetDefinition.Tasks.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0](CancellationToken.None)
                .ShouldBe(task));
    }

    [Test]
    public void Executes_SetsMultipleTasks()
    {
        // Arrange
        var task1 = Task.CompletedTask;
        var task2 = Task.Delay(1);

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition
            .Executes(() => task1)
            .Executes(() => task2);

        // Assert
        targetDefinition.Tasks.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(2),
            x => x[0](CancellationToken.None)
                .ShouldBe(task1),
            x => x[1](CancellationToken.None)
                .ShouldBe(task2));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private interface ITestTarget : IBuildDefinition
    {
        // ReSharper disable once UnusedMember.Local
#pragma warning disable CA1822
        Target TestTarget => x => x;
#pragma warning restore CA1822
    }

    [Test]
    public void DependsOn_AddsDependency()
    {
        // Arrange
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition.DependsOn(nameof(ITestTarget.TestTarget));

        // Assert
        targetDefinition.Dependencies.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ShouldBe(nameof(ITestTarget.TestTarget)));
    }

    [Test]
    public void RequiresParam_AddsRequiredParam()
    {
        // Arrange
        const string paramName = "ParamName";

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition.RequiresParam(paramName);

        // Assert
        targetDefinition.Params.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .Param
                .ShouldBe(paramName));
    }

    [Test]
    public void ProducesArtifact_AddsProducedArtifact()
    {
        // Arrange
        const string artifactName = "ArtifactName";

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition.ProducesArtifact(artifactName);

        // Assert
        targetDefinition.ProducedArtifacts.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ArtifactName
                .ShouldBe(artifactName));
    }

    [Test]
    public void ConsumesArtifact_AddsConsumedArtifact()
    {
        // Arrange
        const string artifactName = "ArtifactName";

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition.ConsumesArtifact(nameof(ITestTarget.TestTarget), artifactName);

        // Assert
        targetDefinition.ConsumedArtifacts.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ArtifactName
                .ShouldBe(artifactName),
            x => x[0]
                .TargetName
                .ShouldBe(nameof(ITestTarget.TestTarget)));
    }

    [Test]
    public void ProducesVariable_AddsProducedVariable()
    {
        // Arrange
        const string variableName = "VariableName";

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition.ProducesVariable(variableName);

        // Assert
        targetDefinition.ProducedVariables.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ShouldBe(variableName));
    }

    [Test]
    public void ConsumesVariable_AddsConsumedVariable()
    {
        // Arrange
        const string variableName = "VariableName";

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        // Act
        targetDefinition.ConsumesVariable(nameof(ITestTarget.TestTarget), variableName);

        // Assert
        targetDefinition.ConsumedVariables.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .VariableName
                .ShouldBe(variableName),
            x => x[0]
                .TargetName
                .ShouldBe(nameof(ITestTarget.TestTarget)));
    }
}
