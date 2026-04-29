namespace DecSm.Atom.Build.Tests.ClassTests.Build.Definition;

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

    [Test]
    public void IsHidden_DefaultIsFalse()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.Hidden.ShouldBeFalse();
    }

    [Test]
    public void IsHidden_SetsHiddenTrue()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.IsHidden();
        targetDefinition.Hidden.ShouldBeTrue();
    }

    [Test]
    public void IsHidden_ExplicitFalse_SetsHiddenFalse()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.IsHidden(false);
        targetDefinition.Hidden.ShouldBeFalse();
    }

    [Test]
    public void UsesParam_AddsOptionalParam()
    {
        const string paramName = "ParamName";

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.UsesParam(paramName);

        targetDefinition.Params.ShouldSatisfyAllConditions(x => x.Count.ShouldBe(1),
            x => x[0]
                .Param
                .ShouldBe(paramName),
            x => x[0]
                .Required
                .ShouldBeFalse());
    }

    [Test]
    public void UsesParam_MultipleParams_AddsAll()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.UsesParam("Param1", "Param2");
        targetDefinition.Params.Count.ShouldBe(2);
        targetDefinition.Params.ShouldAllBe(p => !p.Required);
    }

    [Test]
    public void RequiresParam_SetsRequired()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.RequiresParam("RequiredParam");

        targetDefinition
            .Params
            .Single()
            .Required
            .ShouldBeTrue();
    }

    [Test]
    public async Task Executes_SynchronousAction_IsInvoked()
    {
        var called = false;

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.Executes(() => called = true);

        await targetDefinition
            .Tasks
            .Single()(CancellationToken.None);

        called.ShouldBeTrue();
    }

    [Test]
    public async Task Executes_FuncWithCancellationToken_PassesToken()
    {
        CancellationToken capturedToken = default;
        using var cts = new CancellationTokenSource();

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.Executes(ct =>
        {
            capturedToken = ct;

            return Task.CompletedTask;
        });

        await targetDefinition
            .Tasks
            .Single()(cts.Token);

        capturedToken.ShouldBe(cts.Token);
    }

    [Test]
    public void DependsOn_NullOrWhiteSpaceString_ThrowsArgumentException()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        Should.Throw<ArgumentException>(() => targetDefinition.DependsOn((string)null!));
        Should.Throw<ArgumentException>(() => targetDefinition.DependsOn("   "));
    }

    [Test]
    public void DependsOn_TargetDefinitionOverload_AddsByName()
    {
        var dep = new TargetDefinition
        {
            Name = "DepTarget",
        };

        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.DependsOn(dep);

        targetDefinition.Dependencies.ShouldHaveSingleItem();

        targetDefinition
            .Dependencies[0]
            .ShouldBe("DepTarget");
    }

    [Test]
    public void ProducesArtifacts_AddsMultipleArtifacts()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.ProducesArtifacts(["Art1", "Art2"]);

        targetDefinition.ProducedArtifacts.Count.ShouldBe(2);

        targetDefinition
            .ProducedArtifacts
            .Select(a => a.ArtifactName)
            .ShouldBe(["Art1", "Art2"]);
    }

    [Test]
    public void ProducesArtifact_WithBuildSlice_SetsBuildSlice()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.ProducesArtifact("Art1", "linux-x64");

        targetDefinition
            .ProducedArtifacts[0]
            .BuildSlice
            .ShouldBe("linux-x64");
    }

    [Test]
    public void ConsumesArtifacts_MultipleNames_AddsAll()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.ConsumesArtifacts("Producer", ["Artifact1", "Artifact2"]);

        targetDefinition.ConsumedArtifacts.Count.ShouldBe(2);
        targetDefinition.ConsumedArtifacts.ShouldAllBe(a => a.TargetName == "Producer");
    }

    [Test]
    public void ConsumesArtifact_MultipleSlices_AddsOnePerSlice()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.ConsumesArtifact("Producer", "Artifact1", ["linux-x64", "win-x64"]);

        targetDefinition.ConsumedArtifacts.Count.ShouldBe(2);

        targetDefinition
            .ConsumedArtifacts
            .Select(a => a.BuildSlice)
            .ShouldBe(["linux-x64", "win-x64"]);
    }

    [Test]
    public void ConsumesArtifacts_NamesAndSlices_AddsCrossProduct()
    {
        var targetDefinition = new TargetDefinition
        {
            Name = "name",
        };

        targetDefinition.ConsumesArtifacts("Producer", ["Art1", "Art2"], ["linux-x64", "win-x64"]);

        // 2 artifacts × 2 slices = 4 entries
        targetDefinition.ConsumedArtifacts.Count.ShouldBe(4);

        targetDefinition
            .ConsumedArtifacts
            .Select(a => a.ArtifactName)
            .Distinct()
            .ShouldBe(["Art1", "Art2"], true);

        targetDefinition
            .ConsumedArtifacts
            .Select(a => a.BuildSlice)
            .Distinct()
            .ShouldBe(["linux-x64", "win-x64"], true);
    }
}
