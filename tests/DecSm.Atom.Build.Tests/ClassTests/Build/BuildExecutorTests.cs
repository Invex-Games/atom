namespace DecSm.Atom.Build.Tests.ClassTests.Build;

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

        _paramService = A.Fake<IParamService>();
        _variableService = A.Fake<IVariableService>();
        _outcomeReporters = [];
        _console = new();
        _reportService = new();
        _logger = A.Fake<ILogger<BuildExecutor>>();
    }

    [TearDown]
    public void TearDown() =>
        _console.Dispose();

    private CommandLineArgs _commandLineArgs;
    private BuildModel _buildModel;

    private IParamService _paramService;

    private IVariableService _variableService;

    private IReadOnlyList<IOutcomeReportWriter> _outcomeReporters;
    private TestConsole _console;

    private ReportService _reportService;

    private ILogger<BuildExecutor> _logger;

    [Test]
    public async Task Execute_WithNoCommand_SucceedsAndLogs()
    {
        // Arrange
        var buildExecutor = new BuildExecutor(_commandLineArgs,
            _buildModel,
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

        var target = new TargetModel("Test", null, false, null)
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

        var target = new TargetModel("Test", null, false, null)
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
}
