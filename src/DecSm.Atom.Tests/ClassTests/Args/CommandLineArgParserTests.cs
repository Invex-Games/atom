namespace DecSm.Atom.Tests.ClassTests.Args;

[TestFixture]
public class CommandLineArgParserTests
{
    [Test]
    public void CommandLineArgsParser_Parses_NoArgs()
    {
        // Arrange
        string[] rawArgs = [];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldBeEmpty();
    }

    [TestCase("-h")]
    [TestCase("-H")]
    [TestCase("--help")]
    [TestCase("--HELP")]
    public void CommandLineArgsParser_Parses_HelpArg(string arg)
    {
        // Arrange
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<HelpArg>();
    }

    [TestCase("-g")]
    [TestCase("-G")]
    [TestCase("--gen")]
    [TestCase("--GEN")]
    public void CommandLineArgsParser_Parses_GenArg(string arg)
    {
        // Arrange
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<GenArg>();
    }

    [TestCase("-s")]
    [TestCase("-S")]
    [TestCase("--skip")]
    [TestCase("--SKIP")]
    public void CommandLineArgsParser_Parses_SkipArg(string arg)
    {
        // Arrange
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<SkipArg>();
    }

    [TestCase("-hl")]
    [TestCase("-HL")]
    [TestCase("--headless")]
    [TestCase("--HEADLESS")]
    public void CommandLineArgsParser_Parses_HeadlessArg(string arg)
    {
        // Arrange
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<HeadlessArg>();
    }

    [TestCase("-v")]
    [TestCase("-V")]
    [TestCase("--verbose")]
    [TestCase("--VERBOSE")]
    public void CommandLineArgsParser_Parses_VerboseArg(string arg)
    {
        // Arrange
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<VerboseArg>();
    }

    [TestCase("-p", "project-1")]
    [TestCase("-P", "project-1")]
    [TestCase("--project", "project-1")]
    [TestCase("--PROJECT", "project-1")]
    public void CommandLineArgsParser_Parses_ProjectArg(string arg, string value)
    {
        // Arrange
        string[] rawArgs = [arg, value];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<ProjectArg>()
            .ShouldSatisfyAllConditions(x => x.ProjectName.ShouldBe(value));
    }

    [TestCase("-p")]
    [TestCase("--project")]
    public void CommandLineArgsParser_ProjectArg_MissingValue_ThrowsException(string arg)
    {
        // Arrange
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();
        var parser = new CommandLineArgsParser(build, console);

        // Act / Assert
        var ex = Should.Throw<CommandLineException>(() => parser.Parse(rawArgs));
        ex.ArgumentName.ShouldBe("project");
        ex.Message.ShouldContain("Missing value");
    }

    [TestCase("--param1", "param1", "Param1")]
    [TestCase("--PARAM1", "param1", "Param1")]
    [TestCase("--param2", "param2", "Param2")]
    [TestCase("--PARAM2", "param2", "Param2")]
    public void Parse_Param_Arg(string arg, string argName, string paramName)
    {
        // Arrange
        string[] rawArgs = [arg, "value"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1")
                {
                    ArgName = "param1",
                    Description = "Param 1",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param2"] = new("Param2")
                {
                    ArgName = "param2",
                    Description = "Param 2",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param3"] = new("Param3")
                {
                    ArgName = "param3",
                    Description = "Param 3",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
            });

        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<ParamArg>()
            .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe(argName),
                x => x.ParamName.ShouldBe(paramName),
                x => x.ParamValue.ShouldBe("value"));
    }

    [Test]
    public void CommandLineArgsParser_Parses_ParamWithoutValue()
    {
        // Arrange
        string[] rawArgs = ["--param1", "--param2"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1")
                {
                    ArgName = "param1",
                    Description = "Param 1",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param2"] = new("Param2")
                {
                    ArgName = "param2",
                    Description = "Param 2",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param3"] = new("Param3")
                {
                    ArgName = "param3",
                    Description = "Param 3",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
            });

        var parser = new CommandLineArgsParser(build, console);

        // Act / Assert
        var ex = Should.Throw<CommandLineException>(() => parser.Parse(rawArgs));
        ex.ArgumentName.ShouldBe("param1");
        ex.Message.ShouldContain("Missing value for parameter");
    }

    [Test]
    public void CommandLineArgsParser_Parses_ParamAtEnd()
    {
        // Arrange
        string[] rawArgs = ["--param1"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1")
                {
                    ArgName = "param1",
                    Description = "Param 1",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param2"] = new("Param2")
                {
                    ArgName = "param2",
                    Description = "Param 2",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param3"] = new("Param3")
                {
                    ArgName = "param3",
                    Description = "Param 3",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
            });

        var parser = new CommandLineArgsParser(build, console);

        // Act / Assert
        var ex = Should.Throw<CommandLineException>(() => parser.Parse(rawArgs));
        ex.ArgumentName.ShouldBe("param1");
        ex.Message.ShouldContain("Missing value for parameter");
    }

    [TestCase("Command1", "Command1")]
    [TestCase("COMMAND1", "Command1")]
    [TestCase("Command2", "Command2")]
    [TestCase("COMMAND2", "Command2")]
    public void CommandLineArgsParser_Parses_CommandArg(string arg, string commandName)
    {
        // Arrange
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldHaveSingleItem();

        parsedArgs
            .Args[0]
            .ShouldBeOfType<CommandArg>()
            .ShouldSatisfyAllConditions(x => x.Name.ShouldBe(commandName));
    }

    [TestCase("Unknown1")]
    [TestCase("asdf")]
    [TestCase("wololo")]
    public void CommandLineArgsParser_Parses_UnknownCommandArg(string arg)
    {
        // Arrange
        string[] rawArgs = [arg];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>());

        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.IsValid.ShouldBeFalse();
    }

    [Test]
    public void CommandLineArgsParser_Parses_Complex_1()
    {
        // Arrange
        string[] rawArgs = ["-h", "--param1", "value1", "--param2", "value2", "Command1"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1")
                {
                    ArgName = "param1",
                    Description = "Param 1",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param2"] = new("Param2")
                {
                    ArgName = "param2",
                    Description = "Param 2",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param3"] = new("Param3")
                {
                    ArgName = "param3",
                    Description = "Param 3",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
            });

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldSatisfyAllConditions(args => args.Count.ShouldBe(4),
            args => args[0]
                .ShouldBeOfType<HelpArg>(),
            args => args[1]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param1"),
                    x => x.ParamName.ShouldBe("Param1"),
                    x => x.ParamValue.ShouldBe("value1")),
            args => args[2]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param2"),
                    x => x.ParamName.ShouldBe("Param2"),
                    x => x.ParamValue.ShouldBe("value2")),
            args => args[3]
                .ShouldBeOfType<CommandArg>()
                .ShouldSatisfyAllConditions(x => x.Name.ShouldBe("Command1")));
    }

    [Test]
    public void CommandLineArgsParser_Parses_Complex_2()
    {
        // Arrange
        string[] rawArgs = ["--param1", "value1", "--param2", "value2", "Command1", "--skip"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1")
                {
                    ArgName = "param1",
                    Description = "Param 1",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param2"] = new("Param2")
                {
                    ArgName = "param2",
                    Description = "Param 2",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param3"] = new("Param3")
                {
                    ArgName = "param3",
                    Description = "Param 3",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
            });

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldSatisfyAllConditions(args => args.Count.ShouldBe(4),
            args => args[0]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param1"),
                    x => x.ParamName.ShouldBe("Param1"),
                    x => x.ParamValue.ShouldBe("value1")),
            args => args[1]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param2"),
                    x => x.ParamName.ShouldBe("Param2"),
                    x => x.ParamValue.ShouldBe("value2")),
            args => args[2]
                .ShouldBeOfType<CommandArg>()
                .ShouldSatisfyAllConditions(x => x.Name.ShouldBe("Command1")),
            args => args[3]
                .ShouldBeOfType<SkipArg>());
    }

    [Test]
    public void CommandLineArgsParser_Parses_Complex_3()
    {
        // Arrange
        string[] rawArgs = ["--param1", "value1", "--param2", "value2", "Command1", "-s", "--param3", "value3"];
        var build = A.Fake<IBuildDefinition>();
        var console = new TestConsole();

        A
            .CallTo(() => build.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                ["Param1"] = new("Param1")
                {
                    ArgName = "param1",
                    Description = "Param 1",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param2"] = new("Param2")
                {
                    ArgName = "param2",
                    Description = "Param 2",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
                ["Param3"] = new("Param3")
                {
                    ArgName = "param3",
                    Description = "Param 3",
                    Sources = ParamSource.All,
                    IsSecret = false,
                    ChainedParams = [],
                },
            });

        A
            .CallTo(() => build.TargetDefinitions)
            .Returns(new Dictionary<string, Target>
            {
                ["Command1"] = definition => definition,
                ["Command2"] = definition => definition,
                ["Command3"] = definition => definition,
            });

        var parser = new CommandLineArgsParser(build, console);

        // Act
        var parsedArgs = parser.Parse(rawArgs);

        // Assert
        parsedArgs.Args.ShouldSatisfyAllConditions(args => args.Count.ShouldBe(5),
            args => args[0]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param1"),
                    x => x.ParamName.ShouldBe("Param1"),
                    x => x.ParamValue.ShouldBe("value1")),
            args => args[1]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param2"),
                    x => x.ParamName.ShouldBe("Param2"),
                    x => x.ParamValue.ShouldBe("value2")),
            args => args[2]
                .ShouldBeOfType<CommandArg>()
                .ShouldSatisfyAllConditions(x => x.Name.ShouldBe("Command1")),
            args => args[3]
                .ShouldBeOfType<SkipArg>(),
            args => args[4]
                .ShouldBeOfType<ParamArg>()
                .ShouldSatisfyAllConditions(x => x.ArgName.ShouldBe("param3"),
                    x => x.ParamName.ShouldBe("Param3"),
                    x => x.ParamValue.ShouldBe("value3")));
    }
}
