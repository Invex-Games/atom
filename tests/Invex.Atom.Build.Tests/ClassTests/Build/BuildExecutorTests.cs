namespace Invex.Atom.Build.Tests.ClassTests.Build;

[TestFixture]
internal sealed class BuildExecutorTests
{
    [SetUp]
    public void SetUp()
    {
        _commandLineArgs = new(true, []);

        _buildModel = new()
        {
            Targets = [],
            TargetStates = new Dictionary<TargetModel, TargetState>(),
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        _buildDefinition = A.Fake<IBuildDefinition>();

        A
            .CallTo(() => _buildDefinition.AccessParam(A<string>._))
            .Returns(null);

        _paramService = A.Fake<IParamService>();
        _variableService = A.Fake<IVariableService>();
        _outcomeReporters = [];
        _console = new();
        _reportService = new ReportService();
        _logger = A.Fake<ILogger<BuildExecutor>>();
    }

    [TearDown]
    public void TearDown() =>
        _console.Dispose();

    private CommandLineArgs _commandLineArgs;
    private BuildModel _buildModel;

    private IBuildDefinition _buildDefinition;

    private IParamService _paramService;

    private IVariableService _variableService;

    private IReadOnlyList<IOutcomeReportWriter> _outcomeReporters;
    private TestConsole _console;

    private IReportService _reportService;

    private ILogger<BuildExecutor> _logger;

    [Test]
    public async Task Execute_WithNoCommand_SucceedsAndLogs()
    {
        // Arrange
        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _buildDefinition,
            _paramService,
            _variableService,
            _outcomeReporters,
            _console,
            _reportService,
            _logger);

        // Act
        await buildExecutor.Execute(CancellationToken.None);

        // Assert
        // TODO: Verify logs
    }

    private class TestVal
    {
        public string? Value { get; set; }
    }

    [Test]
    public async Task Execute_WhenBuildIsValid_SucceedsAndLogs()
    {
        // Arrange
        var testVal = new TestVal();

        _commandLineArgs = new(true, [new CommandArg("Test")]);

        var target = new TargetModel("Test", null, false)
        {
            Tasks =
            [
                _ =>
                {
                    testVal.Value = "Test";

                    return Task.CompletedTask;
                },
            ],
            Params = [],
            ConsumedArtifacts = [],
            ProducedArtifacts = [],
            ConsumedVariables = [],
            ProducedVariables = [],
            Dependencies = [],
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        _buildModel = new()
        {
            Targets = [target],
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    target, new(target.Name)
                    {
                        Status = TargetRunState.PendingRun,
                        RunDuration = null,
                    }
                },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _buildDefinition,
            _paramService,
            _variableService,
            _outcomeReporters,
            _console,
            _reportService,
            _logger);

        // Act
        await buildExecutor.Execute(CancellationToken.None);

        // Assert
        testVal.Value.ShouldBe("Test");
    }

    [Test]
    public async Task Execute_WithMultipleMissingRequiredParams_ReportsAllMissingParams()
    {
        // Arrange
        var param1 = new ParamModel("Param1")
        {
            ArgName = "param-1",
            Description = "Param 1",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        var param2 = new ParamModel("Param2")
        {
            ArgName = "param-2",
            Description = "Param 2",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _commandLineArgs = new(true, [new CommandArg("Test")]);

        var target = new TargetModel("Test", null, false)
        {
            Tasks = [_ => Task.CompletedTask],
            Params = [new(param1, true), new(param2, true)],
            ConsumedArtifacts = [],
            ProducedArtifacts = [],
            ConsumedVariables = [],
            ProducedVariables = [],
            Dependencies = [],
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        _buildModel = new()
        {
            Targets = [target],
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    target, new(target.Name)
                    {
                        Status = TargetRunState.PendingRun,
                    }
                },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        // ParamService returns null/empty for both params
        A
            .CallTo(() => _paramService.GetParam(A<string>._, A<string?>.Ignored))
            .Returns(null);

        A
            .CallTo(() => _paramService.CreateNoCacheScope())
            .Returns(new ActionScope());

        var testLoggerProvider = new TestLoggerProvider();
        var loggerFactory = LoggerFactory.Create(b => b.AddProvider(testLoggerProvider));
        var testLogger = loggerFactory.CreateLogger<BuildExecutor>();

        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _buildDefinition,
            _paramService,
            _variableService,
            _outcomeReporters,
            _console,
            _reportService,
            testLogger);

        // Act
        await Should.ThrowAsync<StepFailedException>(buildExecutor.Execute(CancellationToken.None));

        // Assert — both params should be reported in logs
        var logOutput = testLoggerProvider.Logger.LogContent.ToString();
        logOutput.ShouldContain("param-1");
        logOutput.ShouldContain("param-2");
    }

    [Test]
    public async Task Execute_WithRequiredParamWithoutStoredDefault_ReadsParamFromBuildDefinition()
    {
        // Arrange: the param has no stored default (it could not be computed during resolution), but the build
        // definition can compute it now
        var executed = false;
        CreateSingleParamTarget(() => executed = true);

        A
            .CallTo(() => _paramService.GetParam(A<string>._, A<string?>.Ignored))
            .Returns(null);

        A
            .CallTo(() => _paramService.CreateNoCacheScope())
            .Returns(new ActionScope());

        A
            .CallTo(() => _buildDefinition.AccessParam("Param1"))
            .Returns("1.2.3");

        var buildExecutor = CreateExecutor();

        // Act
        await buildExecutor.Execute(CancellationToken.None);

        // Assert
        executed.ShouldBeTrue();
    }

    [Test]
    public async Task Execute_WithRequiredParamWhoseBuildDefinitionValueThrows_ReportsMissingParam()
    {
        // Arrange
        var executed = false;
        CreateSingleParamTarget(() => executed = true);

        A
            .CallTo(() => _paramService.GetParam(A<string>._, A<string?>.Ignored))
            .Returns(null);

        A
            .CallTo(() => _paramService.CreateNoCacheScope())
            .Returns(new ActionScope());

        A
            .CallTo(() => _buildDefinition.AccessParam("Param1"))
            .Throws(new InvalidOperationException("Default unavailable"));

        var testLoggerProvider = new TestLoggerProvider();
        var loggerFactory = LoggerFactory.Create(b => b.AddProvider(testLoggerProvider));
        var buildExecutor = CreateExecutor(loggerFactory.CreateLogger<BuildExecutor>());

        // Act
        await Should.ThrowAsync<StepFailedException>(buildExecutor.Execute(CancellationToken.None));

        // Assert
        executed.ShouldBeFalse();

        var logOutput = testLoggerProvider.Logger.LogContent.ToString();
        logOutput.ShouldContain("Could not determine the value of parameter 'param-1'");
        logOutput.ShouldContain("Missing required parameter 'param-1'");
    }

    private void CreateSingleParamTarget(Action onExecute)
    {
        var param = new ParamModel("Param1")
        {
            ArgName = "param-1",
            Description = "Param 1",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
            ChainedParams = [],
        };

        _commandLineArgs = new(true, [new CommandArg("Test")]);

        var target = new TargetModel("Test", null, false)
        {
            Tasks =
            [
                _ =>
                {
                    onExecute();

                    return Task.CompletedTask;
                },
            ],
            Params = [new(param, true)],
            ConsumedArtifacts = [],
            ProducedArtifacts = [],
            ConsumedVariables = [],
            ProducedVariables = [],
            Dependencies = [],
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        _buildModel = new()
        {
            Targets = [target],
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    target, new(target.Name)
                    {
                        Status = TargetRunState.PendingRun,
                    }
                },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };
    }

    private BuildExecutor CreateExecutor(ILogger<BuildExecutor>? logger = null) =>
        new(_commandLineArgs,
            _buildModel,
            _buildDefinition,
            _paramService,
            _variableService,
            _outcomeReporters,
            _console,
            _reportService,
            logger ?? _logger);

    [Test]
    public async Task Execute_WithSecretParams_PrefetchesSecretsOfPendingTargetsWithBuildToken()
    {
        // Arrange
        ParamModel CreateParam(string name, bool isSecret) =>
            new(name)
            {
                ArgName = name.ToLowerInvariant(),
                Description = name,
                DefaultValue = null,
                Sources = ParamSource.All,
                IsSecret = isSecret,
                ChainedParams = [],
            };

        TargetModel CreateTarget(string name, params UsedParam[] usedParams) =>
            new(name, null, false)
            {
                Tasks = [_ => Task.CompletedTask],
                Params = usedParams,
                ConsumedArtifacts = [],
                ProducedArtifacts = [],
                ConsumedVariables = [],
                ProducedVariables = [],
                Dependencies = [],
                DeclaringAssembly = Assembly.GetExecutingAssembly(),
            };

        _commandLineArgs = new(true, [new CommandArg("Run")]);

        var runTarget = CreateTarget("Run",
            new(CreateParam("RunSecret", true), false),
            new(CreateParam("RunPlain", false), false));

        var skippedTarget = CreateTarget("Skipped", new UsedParam(CreateParam("SkippedSecret", true), false));

        _buildModel = new()
        {
            Targets = [runTarget, skippedTarget],
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    runTarget, new(runTarget.Name)
                    {
                        Status = TargetRunState.PendingRun,
                    }
                },
                {
                    skippedTarget, new(skippedTarget.Name)
                    {
                        Status = TargetRunState.Skipped,
                    }
                },
            },
            DeclaringAssembly = Assembly.GetExecutingAssembly(),
        };

        using var cts = new CancellationTokenSource();

        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
            _buildDefinition,
            _paramService,
            _variableService,
            _outcomeReporters,
            _console,
            _reportService,
            _logger);

        // Act
        await buildExecutor.Execute(cts.Token);

        // Assert
        A
            .CallTo(() => _paramService.PrefetchSecretsAsync(
                A<IEnumerable<string>>.That.Matches(names => names.SequenceEqual(new[] { "RunSecret" })),
                cts.Token))
            .MustHaveHappenedOnceExactly();
    }
}
