namespace DecSm.Atom.Build.Tests.ClassTests.Build.Model;

[TestFixture]
public class BuildModelTests
{
    [Test]
    public void CurrentTarget_WhenNoTargets_ReturnsNull()
    {
        // Arrange
        var buildModel = new BuildModel
        {
            Targets = [],
            TargetStates = new Dictionary<TargetModel, TargetState>(),
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // Act
        var currentTarget = buildModel.CurrentTarget;

        // Assert
        currentTarget.ShouldBeNull();
    }

    private static TargetModel TestTargetModel =>
        new("TargetModel", null, false)
        {
            Tasks = [],
            Params = [],
            ConsumedArtifacts = [],
            ProducedArtifacts = [],
            ConsumedVariables = [],
            ProducedVariables = [],
            Dependencies = [],
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

    [Test]
    public void CurrentTarget_WhenNoRunningTargets_ReturnsNull()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    targetModel, new(targetModel.Name)
                    {
                        Status = TargetRunState.Uninitialized,
                    }
                },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // Act
        var currentTarget = buildModel.CurrentTarget;

        // Assert
        currentTarget.ShouldBeNull();
    }

    [Test]
    public void CurrentTarget_WhenRunningTarget_ReturnsTarget()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    targetModel, new(targetModel.Name)
                    {
                        Status = TargetRunState.Running,
                    }
                },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // Act
        var currentTarget = buildModel.CurrentTarget;

        // Assert
        currentTarget.ShouldBe(targetModel);
    }

    [Test]
    public void CurrentTarget_WhenMultipleRunningTargets_ReturnsFirstTarget()
    {
        // Arrange
        var targetModel1 = TestTargetModel;

        var targetModel2 = TestTargetModel with
        {
            Name = "TargetModel2",
        };

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel1,
                targetModel2,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    targetModel1, new(targetModel1.Name)
                    {
                        Status = TargetRunState.Running,
                    }
                },
                {
                    targetModel2, new(targetModel2.Name)
                    {
                        Status = TargetRunState.Running,
                    }
                },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // Act
        var currentTarget = buildModel.CurrentTarget;

        // Assert
        currentTarget.ShouldBe(targetModel1);
    }

    [Test]
    public void GetTarget_WhenTargetExists_ReturnsTarget()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>(),
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // Act
        var target = buildModel.GetTarget(targetModel.Name);

        // Assert
        target.ShouldBe(targetModel);
    }

    [Test]
    [SuppressMessage("ReSharper", "MoveLocalFunctionAfterJumpStatement")]
    public void GetTarget_WhenTargetDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>(),
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // Act
        void Act()
        {
            buildModel.GetTarget("NonExistentTarget");
        }

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }

    [Test]
    public void GetTargetState_WhenTargetExists_ReturnsState()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var targetState = new TargetState(targetModel.Name)
        {
            Status = TargetRunState.Succeeded,
        };

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                { targetModel, targetState },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // Act
        var state = buildModel.GetTargetState(targetModel);

        // Assert
        state.ShouldBe(targetState);
    }

    [Test]
    [SuppressMessage("ReSharper", "MoveLocalFunctionAfterJumpStatement")]
    public void GetTargetState_WhenTargetNotInStates_ThrowsInvalidOperationException()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var otherTarget = TestTargetModel with
        {
            Name = "OtherTarget",
        };

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>(),
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // Act
        void Act()
        {
            buildModel.GetTargetState(otherTarget);
        }

        // Assert
        Assert.Throws<InvalidOperationException>(Act);
    }
}
