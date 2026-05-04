namespace DecSm.Atom.Workflows.Tests;

[TestFixture]
internal sealed class WorkflowTargetDefinitionTests
{
    [Test]
    public void CreateModel_WithNoOptions_ReturnsModelWithWorkflowOptions()
    {
        var target = new WorkflowTargetDefinition("MyTarget");
        IBuildOption[] workflowOptions = [new TestWorkflowOption("wf")];

        var model = target.CreateModel(workflowOptions);

        model.Name.ShouldBe("MyTarget");
        model.MatrixDimensions.ShouldBeEmpty();

        model
            .Options
            .OfType<TestWorkflowOption>()
            .ShouldContain(o => o.Value == "wf");
    }

    [Test]
    public void CreateModel_WithTargetOptions_MergesWorkflowAndTargetOptions()
    {
        var target = new WorkflowTargetDefinition("MyTarget")
        {
            Options = [new TestWorkflowOption("target")],
        };

        var workflowOptions = new IBuildOption[] { new TestWorkflowOption("workflow") };
        var model = target.CreateModel(workflowOptions);

        model.Options.Count.ShouldBe(2);

        model
            .Options
            .OfType<TestWorkflowOption>()
            .ShouldContain(o => o.Value == "workflow");

        model
            .Options
            .OfType<TestWorkflowOption>()
            .ShouldContain(o => o.Value == "target");
    }

    [Test]
    public void CreateModel_PreservesMatrixDimensions()
    {
        var dim = new MatrixDimension("os")
        {
            Values = ["linux", "windows"],
        };

        var target = new WorkflowTargetDefinition("MyTarget")
        {
            MatrixDimensions = [dim],
        };

        var model = target.CreateModel([]);

        model.MatrixDimensions.ShouldHaveSingleItem();

        model
            .MatrixDimensions[0]
            .Name
            .ShouldBe("os");
    }

    [Test]
    public void WithMatrixDimensions_AddsDimensions()
    {
        var dim1 = new MatrixDimension("os")
        {
            Values = ["ubuntu-latest"],
        };

        var dim2 = new MatrixDimension("dotnet")
        {
            Values = ["8.0", "9.0"],
        };

        var target = new WorkflowTargetDefinition("T").WithMatrixDimensions(dim1, dim2);

        target.MatrixDimensions.Count.ShouldBe(2);
        target.MatrixDimensions.ShouldContain(d => d.Name == "os");
        target.MatrixDimensions.ShouldContain(d => d.Name == "dotnet");
    }

    [Test]
    public void WithMatrixDimensions_PreservesExistingDimensions()
    {
        var existing = new MatrixDimension("existing");

        var target = new WorkflowTargetDefinition("T")
        {
            MatrixDimensions = [existing],
        };

        var newDim = new MatrixDimension("new");

        var updated = target.WithMatrixDimensions(newDim);

        updated.MatrixDimensions.Count.ShouldBe(2);
        updated.MatrixDimensions.ShouldContain(d => d.Name == "existing");
        updated.MatrixDimensions.ShouldContain(d => d.Name == "new");
    }

    [Test]
    public void WithMatrixDimensions_ReturnsNewInstance()
    {
        var target = new WorkflowTargetDefinition("T");
        var updated = target.WithMatrixDimensions(new MatrixDimension("os"));
        updated.ShouldNotBeSameAs(target);
    }

    [Test]
    public void WithOptions_AddsOptions()
    {
        var target = new WorkflowTargetDefinition("T");
        var withOpts = target.WithOptions(new TestWorkflowOption("val"));

        withOpts.Options.ShouldHaveSingleItem();
        ((TestWorkflowOption)withOpts.Options[0]).Value.ShouldBe("val");
    }

    [Test]
    public void WithOptions_PreservesExistingOptions()
    {
        var target = new WorkflowTargetDefinition("T")
        {
            Options = [new TestWorkflowOption("existing")],
        };

        var updated = target.WithOptions(new TestWorkflowOption("new"));

        updated.Options.Count.ShouldBe(2);
    }

    [Test]
    public void WithOptions_ReturnsNewInstance()
    {
        var target = new WorkflowTargetDefinition("T");
        var updated = target.WithOptions(new TestWorkflowOption("x"));
        updated.ShouldNotBeSameAs(target);
    }
}
