namespace DecSm.Atom.Tests;

[TestFixture]
public class ExceptionTests
{
    [Test]
    public void AtomException_DefaultConstructor_Creates()
    {
        var ex = new AtomException();
        ex.ShouldNotBeNull();
    }

    [Test]
    public void AtomException_MessageConstructor_SetsMessage()
    {
        const string message = "Test message";
        var ex = new AtomException(message);
        ex.Message.ShouldBe(message);
    }

    [Test]
    public void AtomException_MessageAndInnerExceptionConstructor_SetsBoth()
    {
        const string message = "Test message";
        var innerException = new InvalidOperationException("Inner");
        var ex = new AtomException(message, innerException);
        ex.Message.ShouldBe(message);
        ex.InnerException.ShouldBe(innerException);
    }

    [Test]
    public void BuildConfigurationException_InheritsFromAtomException()
    {
        var ex = new BuildConfigurationException("Test");
        ex.ShouldBeAssignableTo<AtomException>();
        ex.ShouldBeAssignableTo<Exception>();
    }

    [Test]
    public void BuildConfigurationException_WithReportData_SetsProperty()
    {
        var reportData = new ListReportData(new[] { "Error 1", "Error 2" });

        var ex = new BuildConfigurationException("Test")
        {
            ReportData = reportData,
        };

        ex.ReportData.ShouldBe(reportData);
    }

    [Test]
    public void BuildConfigurationException_WithoutReportData_PropertyIsNull()
    {
        var ex = new BuildConfigurationException("Test");
        ex.ReportData.ShouldBeNull();
    }

    [Test]
    public void WorkflowOutdatedException_InheritsFromAtomException()
    {
        var ex = new WorkflowOutdatedException("Test");
        ex.ShouldBeAssignableTo<AtomException>();
        ex.ShouldBeAssignableTo<Exception>();
    }

    [Test]
    public void WorkflowOutdatedException_MessageConstructor_SetsMessage()
    {
        const string message = "Workflows are outdated";
        var ex = new WorkflowOutdatedException(message);
        ex.Message.ShouldBe(message);
    }

    [Test]
    public void CommandLineException_InheritsFromAtomException()
    {
        var ex = new CommandLineException("Test");
        ex.ShouldBeAssignableTo<AtomException>();
        ex.ShouldBeAssignableTo<Exception>();
    }

    [Test]
    public void CommandLineException_WithArgumentName_SetsProperty()
    {
        const string argumentName = "project";

        var ex = new CommandLineException("Missing value")
        {
            ArgumentName = argumentName,
        };

        ex.ArgumentName.ShouldBe(argumentName);
    }

    [Test]
    public void CommandLineException_WithoutArgumentName_PropertyIsNull()
    {
        var ex = new CommandLineException("Test");
        ex.ArgumentName.ShouldBeNull();
    }

    [Test]
    public void StepFailedException_InheritsFromAtomException()
    {
        var ex = new StepFailedException("Test");
        ex.ShouldBeAssignableTo<AtomException>();
        ex.ShouldBeAssignableTo<Exception>();
    }

    [Test]
    public void AllAtomExceptions_CanBeCaughtAsAtomException()
    {
        var exceptions = new Exception[]
        {
            new BuildConfigurationException("Test"),
            new CommandLineException("Test"),
            new WorkflowOutdatedException("Test"),
            new StepFailedException("Test"),
        };

        foreach (var ex in exceptions)
            ex.ShouldBeAssignableTo<AtomException>();
    }
}
