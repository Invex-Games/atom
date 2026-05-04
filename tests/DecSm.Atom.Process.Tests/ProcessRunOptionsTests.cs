namespace DecSm.Atom.Process.Tests;

[TestFixture]
internal sealed class ProcessRunOptionsTests
{
    [Test]
    public void Constructor_WithStringArgs_StoresNameAndArgs()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "build --configuration Release");

        // Assert
        options.ShouldSatisfyAllConditions(() => options.Name.ShouldBe("dotnet"),
            () => options.Args.ShouldBe("build --configuration Release"));
    }

    [Test]
    public void Constructor_WithArrayArgs_JoinsArgsWithSpaces()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", ["build", "--configuration", "Release"]);

        // Assert
        options.Args.ShouldBe("build --configuration Release");
    }

    [Test]
    public void Constructor_WithArrayArgs_FiltersEmptyAndWhitespaceEntries()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", ["build", "", "  ", "--configuration", "Release"]);

        // Assert
        options.Args.ShouldBe("build --configuration Release");
    }

    [Test]
    public void Constructor_WithArrayArgs_AllEmptyEntries_ProducesEmptyArgs()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", ["", "   ", ""]);

        // Assert
        options.Args.ShouldBe(string.Empty);
    }

    [Test]
    public void InvocationLogLevel_DefaultsToInformation()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version");

        // Assert
        options.InvocationLogLevel.ShouldBe(LogLevel.Information);
    }

    [Test]
    public void OutputLogLevel_DefaultsToDebug()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version");

        // Assert
        options.OutputLogLevel.ShouldBe(LogLevel.Debug);
    }

    [Test]
    public void ErrorLogLevel_DefaultsToWarning()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version");

        // Assert
        options.ErrorLogLevel.ShouldBe(LogLevel.Warning);
    }

    [Test]
    public void AllowFailedResult_DefaultsToFalse()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version");

        // Assert
        options.AllowFailedResult.ShouldBeFalse();
    }

    [Test]
    public void WorkingDirectory_DefaultsToNull()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version");

        // Assert
        options.WorkingDirectory.ShouldBeNull();
    }

    [Test]
    public void EnvironmentVariables_DefaultsToEmpty()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version");

        // Assert
        options.EnvironmentVariables.ShouldBeEmpty();
    }

    [Test]
    public void TransformOutput_DefaultsToNull()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version");

        // Assert
        options.TransformOutput.ShouldBeNull();
    }

    [Test]
    public void TransformError_DefaultsToNull()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version");

        // Assert
        options.TransformError.ShouldBeNull();
    }

    [Test]
    public void Properties_CanBeSetViaInit()
    {
        // Arrange / Act
        var options = new ProcessRunOptions("dotnet", "--version")
        {
            WorkingDirectory = "/tmp",
            InvocationLogLevel = LogLevel.Debug,
            OutputLogLevel = LogLevel.Trace,
            ErrorLogLevel = LogLevel.Error,
            AllowFailedResult = true,
            EnvironmentVariables = new()
            {
                ["KEY"] = "VALUE",
            },
            TransformOutput = line => $"[OUT] {line}",
            TransformError = line => $"[ERR] {line}",
        };

        // Assert
        options.ShouldSatisfyAllConditions(() => options.WorkingDirectory.ShouldBe("/tmp"),
            () => options.InvocationLogLevel.ShouldBe(LogLevel.Debug),
            () => options.OutputLogLevel.ShouldBe(LogLevel.Trace),
            () => options.ErrorLogLevel.ShouldBe(LogLevel.Error),
            () => options.AllowFailedResult.ShouldBeTrue(),
            () => options
                .EnvironmentVariables["KEY"]
                .ShouldBe("VALUE"),
            () => options.TransformOutput.ShouldNotBeNull(),
            () => options.TransformError.ShouldNotBeNull());
    }
}
