namespace Invex.Atom.Build.Tests.ClassTests.Args;

[TestFixture]
internal sealed class CommandLineArgsTests
{
    [Test]
    public void HasHelp_WhenHelpArgIsPresent_ShouldReturnTrue()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasHelp;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasHelp_WhenHelpArgIsNotPresent_ShouldReturnFalse()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new ParamArg("param-1", "Param1", "1"),
            });

        // Act
        var result = args.HasHelp;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasSkip_WhenSkipArgIsPresent_ShouldReturnTrue()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new SkipArg(),
            });

        // Act
        var result = args.HasSkip;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasSkip_WhenSkipArgIsNotPresent_ShouldReturnFalse()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasSkip;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasHeadless_WhenHeadlessArgIsPresent_ShouldReturnTrue()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HeadlessArg(),
            });

        // Act
        var result = args.HasHeadless;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasHeadless_WhenHeadlessArgIsNotPresent_ShouldReturnFalse()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasHeadless;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasVerbose_WhenVerboseArgIsPresent_ShouldReturnTrue()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new VerboseArg(),
            });

        // Act
        var result = args.HasVerbose;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void HasVerbose_WhenVerboseArgIsNotPresent_ShouldReturnFalse()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new HelpArg(),
            });

        // Act
        var result = args.HasVerbose;

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    public void HasProject_WhenProjectArgIsPresent_ShouldReturnTrue()
    {
        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                new ProjectArg("project-1"),
            });

        // Act
        var result = args.HasProject;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void Params_ShouldReturnAllParamArgs()
    {
        var paramArg1 = new ParamArg("param-1", "Param1", "1");
        var paramArg2 = new ParamArg("param-2", "Param2", "2");

        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                paramArg1,
                paramArg2,
                new HelpArg(),
            });

        // Act
        var result = args.Params;

        // Assert
        result.ShouldSatisfyAllConditions(() => result.ShouldNotBeNull(),
            () => result.Count.ShouldBe(2),
            () => result[0]
                .ShouldBeEquivalentTo(paramArg1),
            () => result[1]
                .ShouldBeEquivalentTo(paramArg2));
    }

    [Test]
    public void Commands_ShouldReturnAllCommandArgs()
    {
        var commandArg1 = new CommandArg("command-1");
        var commandArg2 = new CommandArg("command-2");

        // Arrange
        var args = new CommandLineArgs(true,
            new List<IArg>
            {
                commandArg1,
                commandArg2,
                new HelpArg(),
            });

        // Act
        var result = args.Commands;

        // Assert
        result.ShouldSatisfyAllConditions(() => result.ShouldNotBeNull(),
            () => result.Count.ShouldBe(2),
            () => result[0]
                .ShouldBeEquivalentTo(commandArg1),
            () => result[1]
                .ShouldBeEquivalentTo(commandArg2));
    }

    [Test]
    public void HasInteractive_WhenInteractiveArgIsPresent_ShouldReturnTrue()
    {
        var args = new CommandLineArgs(true, [new InteractiveArg()]);
        args.HasInteractive.ShouldBeTrue();
    }

    [Test]
    public void HasInteractive_WhenInteractiveArgIsNotPresent_ShouldReturnFalse()
    {
        var args = new CommandLineArgs(true, [new HelpArg()]);
        args.HasInteractive.ShouldBeFalse();
    }

    [Test]
    public void HasProject_WhenProjectArgIsNotPresent_ShouldReturnFalse()
    {
        var args = new CommandLineArgs(true, [new HelpArg()]);
        args.HasProject.ShouldBeFalse();
    }

    [Test]
    public void ProjectName_WhenProjectArgIsPresent_ReturnsProjectName()
    {
        var args = new CommandLineArgs(true, [new ProjectArg("MyProject")]);
        args.ProjectName.ShouldBe("MyProject");
    }

    [Test]
    public void ProjectName_WhenProjectArgIsNotPresent_ReturnsDefaultAtom()
    {
        var args = new CommandLineArgs(true, []);
        args.ProjectName.ShouldBe("_atom");
    }

    [Test]
    public void GetValidationErrors_WhenIsValidFalse_ContainsParseError()
    {
        var args = new CommandLineArgs(false, []);
        var errors = args.GetValidationErrors();
        errors.ShouldContain(e => e.Contains("could not be parsed"));
    }

    [Test]
    public void GetValidationErrors_WhenCommandHasEmptyName_ContainsError()
    {
        var args = new CommandLineArgs(true, [new CommandArg("")]);
        var errors = args.GetValidationErrors();
        errors.ShouldContain(e => e.Contains("Target name cannot be empty"));
    }

    [Test]
    public void GetValidationErrors_WhenParamHasEmptyName_ContainsError()
    {
        var args = new CommandLineArgs(true, [new ParamArg("", "", "someValue")]);
        var errors = args.GetValidationErrors();
        errors.ShouldContain(e => e.Contains("Parameter name cannot be empty"));
    }

    [Test]
    public void GetValidationErrors_WhenArgsAreValid_ReturnsEmpty()
    {
        var args = new CommandLineArgs(true, [new CommandArg("Build"), new ParamArg("version", "Version", "1.0.0")]);

        args
            .GetValidationErrors()
            .ShouldBeEmpty();
    }

    [Test]
    public void GetValidationErrors_WhenMultipleErrors_ReturnsAll()
    {
        var args = new CommandLineArgs(false, [new CommandArg(""), new CommandArg("   "), new ParamArg("", "", "val")]);

        var errors = args.GetValidationErrors();
        errors.Count.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Test]
    public void Errors_WhenPresent_ForcesIsValidFalse()
    {
        var args = new CommandLineArgs(true, [], [new("Unknown argument 'asdf'")]);

        args.IsValid.ShouldBeFalse();
        args.Errors.ShouldHaveSingleItem();
    }

    [Test]
    public void GetValidationErrors_WhenParseErrorsPresent_ContainsTheirMessages()
    {
        var args = new CommandLineArgs(false,
            [],
            [
                new("Unknown argument 'asdf'"),
                new("Missing value for parameter 'param1'. Usage: --param1 <value>")
                {
                    ArgumentName = "param1",
                },
            ]);

        var errors = args.GetValidationErrors();

        errors.ShouldContain("Unknown argument 'asdf'");
        errors.ShouldContain(e => e.Contains("Missing value for parameter 'param1'"));
        errors.ShouldNotContain(e => e.Contains("could not be parsed"));
    }
}
