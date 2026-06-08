namespace Invex.Atom.Build.Tests.ClassTests.Build;

[TestFixture]
internal sealed class BuildResolverTests
{
    [SetUp]
    public void Setup() =>
        _services = new ServiceCollection()
            .AddSingleton<IServiceProvider>(_ => _services)
            .BuildServiceProvider();

    [TearDown]
    public void TearDown() =>
        _services.Dispose();

    private class TestBuildDefinition(IServiceProvider services) : BuildDefinition(services)
    {
        public IReadOnlyDictionary<string, Target> ManualTargetDefinitions { get; init; } =
            new Dictionary<string, Target>();

        private IReadOnlyDictionary<string, ParamDefinition> ManualParamDefinitions { get; } =
            new Dictionary<string, ParamDefinition>();

        public override IReadOnlyDictionary<string, Target> TargetDefinitions => ManualTargetDefinitions;

        public override IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions => ManualParamDefinitions;

        public override Func<object?> AccessParam(string paramName) =>
            throw new NotImplementedException();
    }

    private ServiceProvider _services = null!;

    [Test]
    public void Resolve_WithNoTargets_ReturnsEmptyModel()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services);
        var paramService = A.Fake<IParamService>();
        var commandLineArgs = new CommandLineArgs(true, []);
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.ShouldBeEmpty(), x => x.TargetStates.ShouldBeEmpty());
    }

    [Test]
    public void Resolve_WithTargetNotSpecified_ReturnsModelWithTarget()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, []);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.ShouldHaveSingleItem(),
            x => x
                .Targets[0]
                .Name
                .ShouldBe("Target1"));
    }

    [Test]
    public void Resolve_WithCircularDependency_ThrowsException()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t.DependsOn("Target2"),
                ["Target2"] = t => t.DependsOn("Target1"),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, []);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act & Assert
        var ex = Assert.Throws<BuildConfigurationException>(() => buildResolver.Resolve());
        ex.Message.ShouldContain("Circular dependency detected");
        ex.ReportData.ShouldNotBeNull();
    }

    [Test]
    public void Resolve_WithSingleTarget_ReturnsModelWithSingleTarget()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.ShouldHaveSingleItem(),
            x => x
                .Targets[0]
                .Name
                .ShouldBe("Target1"),
            x => x
                .TargetStates[x.Targets[0]]
                .Status
                .ShouldBe(TargetRunState.PendingRun));
    }

    [Test]
    public void Resolve_WithMultipleTargets_ReturnsModelWithAllTargets()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t.Executes(() => Task.CompletedTask),
                ["Target2"] = t => t.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1"), new CommandArg("Target2")]);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.Count.ShouldBe(2),
            x => x
                .Targets
                .Any(t => t.Name == "Target1")
                .ShouldBeTrue(),
            x => x
                .Targets
                .Any(t => t.Name == "Target2")
                .ShouldBeTrue(),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target1")]
                .Status
                .ShouldBe(TargetRunState.PendingRun),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target2")]
                .Status
                .ShouldBe(TargetRunState.PendingRun));
    }

    [Test]
    public void Resolve_WithDependencies_ReturnsModelWithResolvedDependencies()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t
                    .DependsOn("Target2")
                    .Executes(() => Task.CompletedTask),
                ["Target2"] = t => t.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.Count.ShouldBe(2),
            x => x
                .Targets
                .Any(t => t.Name == "Target1")
                .ShouldBeTrue(),
            x => x
                .Targets
                .Any(t => t.Name == "Target2")
                .ShouldBeTrue(),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target1")]
                .Status
                .ShouldBe(TargetRunState.PendingRun),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target2")]
                .Status
                .ShouldBe(TargetRunState.PendingRun));
    }

    [Test]
    public void Resolve_WithMissingDependency_ThrowsException()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t
                    .DependsOn("Target2")
                    .Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act & Assert
        var ex = Should.Throw<BuildConfigurationException>(buildResolver.Resolve);
        ex.Message.ShouldContain("depends on target 'Target2' which does not exist");
    }

    [Test]
    public void Resolve_WithComplexCircularDependency_ThrowsException()
    {
        // Arrange - Three node cycle: A -> B -> C -> A
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["TargetA"] = t => t.DependsOn("TargetB"),
                ["TargetB"] = t => t.DependsOn("TargetC"),
                ["TargetC"] = t => t.DependsOn("TargetA"),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, []);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act & Assert
        var ex = Should.Throw<BuildConfigurationException>(buildResolver.Resolve);
        ex.Message.ShouldContain("Circular dependency detected");
        ex.ReportData.ShouldNotBeNull();
        ex.ReportData.ShouldBeOfType<TextReportData>();
    }

    [Test]
    public void Resolve_WithSkippedTargets_ReturnsModelWithSkippedTargets()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t.Executes(() => Task.CompletedTask),
                ["Target2"] = t => t.Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(false, [new CommandArg("Target1")]);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.Count.ShouldBe(2),
            x => x
                .Targets
                .Any(t => t.Name == "Target1")
                .ShouldBeTrue(),
            x => x
                .Targets
                .Any(t => t.Name == "Target2")
                .ShouldBeTrue(),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target1")]
                .Status
                .ShouldBe(TargetRunState.PendingRun),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target2")]
                .Status
                .ShouldBe(TargetRunState.Skipped));
    }

    [Test]
    public void Resolve_WithArtifactDependencies_ReturnsModelWithResolvedArtifactDependencies()
    {
        // Arrange
        var buildDefinition = new TestBuildDefinition(_services)
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t
                    .ConsumesArtifact("Target2", "Artifact1")
                    .Executes(() => Task.CompletedTask),
                ["Target2"] = t => t
                    .ProducesArtifact("Artifact1")
                    .Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act
        var buildModel = buildResolver.Resolve();

        // Assert
        buildModel.ShouldSatisfyAllConditions(x => x.Targets.Count.ShouldBe(2),
            x => x
                .Targets
                .Any(t => t.Name == "Target1")
                .ShouldBeTrue(),
            x => x
                .Targets
                .FirstOrDefault(t => t.Name == "Target1")
                .ShouldNotBeNull()
                .ShouldSatisfyAllConditions(t => t.ConsumedArtifacts.Count.ShouldBe(1),
                    t => t
                        .ConsumedArtifacts[0]
                        .TargetName
                        .ShouldBe("Target2"),
                    t => t
                        .ConsumedArtifacts[0]
                        .ArtifactName
                        .ShouldBe("Artifact1")),
            x => x
                .Targets
                .Any(t => t.Name == "Target2")
                .ShouldBeTrue(),
            x => x
                .Targets
                .First(t => t.Name == "Target2")
                .ProducedArtifacts
                .Count
                .ShouldBe(1),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target1")]
                .Status
                .ShouldBe(TargetRunState.PendingRun),
            x => x
                .TargetStates[x.Targets.First(t => t.Name == "Target2")]
                .Status
                .ShouldBe(TargetRunState.PendingRun));
    }

    [Test]
    public void Resolve_WithCircularChainedParams_DoesNotStackOverflow()
    {
        // Arrange — Param1 chains to Param2, Param2 chains back to Param1
        var buildDefinition = new TestBuildDefinitionWithParams(_services,
            new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1")
                {
                    ArgName = "param-1",
                    Description = "Param 1",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = ["Param2"],
                },
                ["Param2"] = new("Param2")
                {
                    ArgName = "param-2",
                    Description = "Param 2",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = ["Param1"],
                },
            })
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t
                    .RequiresParam("Param1")
                    .Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act — should not throw StackOverflowException
        var buildModel = buildResolver.Resolve();

        // Assert — Target1 should have both params resolved (each appearing once)
        var target = buildModel.Targets.First(t => t.Name == "Target1");
        target.Params.Count.ShouldBe(2);
        target.Params.ShouldContain(p => p.Param.Name == "Param1");
        target.Params.ShouldContain(p => p.Param.Name == "Param2");
    }

    [Test]
    public void Resolve_WithSelfReferencingChainedParam_DoesNotStackOverflow()
    {
        // Arrange — Param1 chains to itself
        var buildDefinition = new TestBuildDefinitionWithParams(_services,
            new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1")
                {
                    ArgName = "param-1",
                    Description = "Param 1",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = ["Param1"],
                },
            })
        {
            ManualTargetDefinitions = new Dictionary<string, Target>
            {
                ["Target1"] = t => t
                    .RequiresParam("Param1")
                    .Executes(() => Task.CompletedTask),
            },
        };

        var commandLineArgs = new CommandLineArgs(true, [new CommandArg("Target1")]);
        var paramService = A.Fake<IParamService>();
        var logger = A.Fake<ILogger<BuildResolver>>();
        var buildResolver = new BuildResolver(buildDefinition, paramService, commandLineArgs, logger);

        // Act — should not throw StackOverflowException
        var buildModel = buildResolver.Resolve();

        // Assert — Target1 should have Param1 resolved once (not infinite)
        var target = buildModel.Targets.First(t => t.Name == "Target1");
        target.Params.ShouldHaveSingleItem();

        target
            .Params[0]
            .Param
            .Name
            .ShouldBe("Param1");
    }

    private class TestBuildDefinitionWithParams(
        IServiceProvider services,
        IReadOnlyDictionary<string, ParamDefinition> paramDefinitions
    ) : BuildDefinition(services)
    {
        public IReadOnlyDictionary<string, Target> ManualTargetDefinitions { get; init; } =
            new Dictionary<string, Target>();

        public override IReadOnlyDictionary<string, Target> TargetDefinitions => ManualTargetDefinitions;

        public override IReadOnlyDictionary<string, ParamDefinition> ParamDefinitions => paramDefinitions;

        public override object? AccessParam(string paramName) =>
            null;
    }
}
