namespace DecSm.Atom.Tests.BuildTests.Params;

[TestFixture]
public class ParamTests
{
    [Test]
    public void Param_IsReadFromCommandLine()
    {
        // Arrange
        var host = CreateTestHost<ParamBuild>(commandLineArgs: new(true,
            [new CommandArg(nameof(IParamTarget1.ParamTarget1))]));

        var build = (ParamBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.ExecuteValue.ShouldBe("DefaultValue");
    }

    [Test]
    public void Param_FallsBackToDefault()
    {
        // Arrange
        var host = CreateTestHost<ParamBuild>(commandLineArgs: new(true,
        [
            new CommandArg(nameof(IParamTarget1.ParamTarget1)),
            new ParamArg("param-1", nameof(IParamTarget1.Param1), "TestValue"),
        ]));

        var build = (ParamBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        build.ExecuteValue.ShouldBe("TestValue");
    }

    [Test]
    public void Param_WhenRequiredAndNotSupplied_StopsAndReturnsError()
    {
        // Arrange
        var loggerProvider = new TestLoggerProvider();

        var host = CreateTestHost<ParamBuild>(
            commandLineArgs: new(true, [new CommandArg(nameof(IParamTarget2.ParamTarget2))]),
            configure: builder => builder.Logging.AddProvider(loggerProvider));

        // Act
        host.Run();

        // Assert
        Environment.ExitCode.ShouldBe(1);

        loggerProvider
            .Logger
            .LogContent
            .ToString()
            .ShouldContain("Missing required parameter 'param-2' for target ParamTarget2");
    }

    [Test]
    public void Param_WhenOptionalAndNotSupplied_UsesDefaultValue()
    {
        // Arrange
        var loggerProvider = new TestLoggerProvider();

        var host = CreateTestHost<OptionalParamBuild>(commandLineArgs: new(true,
            [
                new CommandArg(nameof(IOptionalParamTarget1.OptionalParamTarget1)),
                new ParamArg("param-1", nameof(IOptionalParamTarget1.Param1), "TestValue"),
            ]),
            configure: builder => builder.Logging.AddProvider(loggerProvider));

        var build = (OptionalParamBuild)host.Services.GetRequiredService<IBuildDefinition>();

        // Act
        host.Run();

        // Assert
        TestContext.Out.WriteLine(loggerProvider.Logger.LogContent.ToString());

        build.ExecuteValue1.ShouldBe("TestValue");
        build.ExecuteValue2.ShouldBeNull();
    }

    [Test]
    public void Param_WhenRequiredParamChainedAndNotSupplied_StopsAndReturnsError()
    {
        // Arrange
        var loggerProvider = new TestLoggerProvider();

        var host = CreateTestHost<ChainedParamBuild>(
            commandLineArgs: new(true,
            [
                new CommandArg(nameof(IChainedParamTarget.ChainedParamTarget1)),
                new ParamArg("param-2", nameof(IChainedParamTarget.Param2), "TestValue"),
            ]),
            configure: builder => builder.Logging.AddProvider(loggerProvider));

        // Act
        host.Run();

        // Assert
        Environment.ExitCode.ShouldBe(1);

        loggerProvider
            .Logger
            .LogContent
            .ToString()
            .ShouldContain("Missing required parameter 'param-1' for target ChainedParamTarget");
    }

    [Test]
    public void Param_WhenUsedParamChainedAndNotSupplied_IgnoresMissingParam()
    {
        // Arrange
        var loggerProvider = new TestLoggerProvider();

        var host = CreateTestHost<ChainedParamBuild>(
            commandLineArgs: new(true,
            [
                new CommandArg(nameof(IChainedParamTarget.ChainedParamTarget2)),
                new ParamArg("param-2", nameof(IChainedParamTarget.Param2), "TestValue"),
            ]),
            configure: builder => builder.Logging.AddProvider(loggerProvider));

        // Act
        host.Run();

        // Assert
        TestContext.Out.WriteLine(loggerProvider.Logger.LogContent.ToString());
    }
}
