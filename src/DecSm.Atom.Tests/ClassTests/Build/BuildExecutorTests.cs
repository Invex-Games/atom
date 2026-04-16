namespace DecSm.Atom.Tests.ClassTests.Build;

[TestFixture]
public class BuildExecutorTests
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
        _workflowVariableService = A.Fake<IWorkflowVariableService>();
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

    private IWorkflowVariableService _workflowVariableService;

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
            _workflowVariableService,
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
            _paramService,
            _workflowVariableService,
            _outcomeReporters,
            _console,
            _reportService,
            _logger);

        // Act
        await buildExecutor.Execute(CancellationToken.None);

        // Assert
        testVal.Value.ShouldBe("Test");
    }
}
