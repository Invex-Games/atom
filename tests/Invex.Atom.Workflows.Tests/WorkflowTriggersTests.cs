namespace Invex.Atom.Workflows.Tests;

[TestFixture]
internal sealed class WorkflowTriggersTests
{
    [Test]
    public void Manual_ReturnsSingletonManualTrigger()
    {
        var t1 = WorkflowTriggers.Manual;
        var t2 = WorkflowTriggers.Manual;
        t1.ShouldBeSameAs(t2);
        t1.ShouldBeOfType<ManualTrigger>();
        t1.Inputs.ShouldBeNull();
    }

    [Test]
    public void ManualWithInputs_ReturnsManualTriggerWithInputs()
    {
        var input = new ManualStringInput("MyInput", "desc", false);
        var trigger = WorkflowTriggers.ManualWithInputs(input);
        trigger.Inputs.ShouldNotBeNull();
        trigger.Inputs!.ShouldHaveSingleItem();
    }

    [Test]
    public void PullIntoMain_ReturnsSingletonWithMainBranch()
    {
        var t = WorkflowTriggers.PullIntoMain;
        t.ShouldBeOfType<GitPullRequestTrigger>();
        t.IncludedBranches.ShouldContain("main");
    }

    [Test]
    public void PushToMain_ReturnsSingletonWithMainBranch()
    {
        var t = WorkflowTriggers.PushToMain;
        t.ShouldBeOfType<GitPushTrigger>();
        t.IncludedBranches.ShouldContain("main");
    }

    [Test]
    public void PullInto_ReturnsTriggerWithSpecifiedBranches()
    {
        var t = WorkflowTriggers.PullInto("main", "develop");
        t.IncludedBranches.ShouldBe(["main", "develop"]);
    }

    [Test]
    public void PushTo_ReturnsTriggerWithSpecifiedBranches()
    {
        var t = WorkflowTriggers.PushTo("release");
        t.IncludedBranches.ShouldBe(["release"]);
    }

    [Test]
    public void PullRequest_SetsAllProperties()
    {
        var t = WorkflowTriggers.PullRequest(["main"], ["dev"], ["src/**"], ["docs/**"], ["opened"]);

        t.IncludedBranches.ShouldContain("main");
        t.ExcludedBranches.ShouldContain("dev");
        t.IncludedPaths.ShouldContain("src/**");
        t.ExcludedPaths.ShouldContain("docs/**");
        t.Types.ShouldContain("opened");
    }

    [Test]
    public void Push_SetsAllProperties()
    {
        var t = WorkflowTriggers.Push(["main"], ["dev"], ["src/**"], ["docs/**"], ["v*"], ["v0*"]);

        t.IncludedBranches.ShouldContain("main");
        t.ExcludedBranches.ShouldContain("dev");
        t.IncludedPaths.ShouldContain("src/**");
        t.ExcludedPaths.ShouldContain("docs/**");
        t.IncludedTags.ShouldContain("v*");
        t.ExcludedTags.ShouldContain("v0*");
    }

    [Test]
    public void GitPushTrigger_Defaults_AreEmpty()
    {
        var t = new GitPushTrigger();
        t.IncludedBranches.ShouldBeEmpty();
        t.ExcludedBranches.ShouldBeEmpty();
        t.IncludedPaths.ShouldBeEmpty();
        t.ExcludedPaths.ShouldBeEmpty();
        t.IncludedTags.ShouldBeEmpty();
        t.ExcludedTags.ShouldBeEmpty();
    }

    [Test]
    public void GitPullRequestTrigger_Defaults_AreEmpty()
    {
        var t = new GitPullRequestTrigger();
        t.IncludedBranches.ShouldBeEmpty();
        t.ExcludedBranches.ShouldBeEmpty();
        t.IncludedPaths.ShouldBeEmpty();
        t.ExcludedPaths.ShouldBeEmpty();
        t.Types.ShouldBeEmpty();
    }

    [Test]
    public void ManualStringInput_InitializesCorrectly()
    {
        var i = new ManualStringInput("my-input", "My description", true);
        i.Name.ShouldBe("my-input");
        i.Description.ShouldBe("My description");
        i.Required.ShouldBe(true);
    }

    [Test]
    public void ManualBoolInput_InitializesCorrectly()
    {
        var i = new ManualBoolInput("flag", "A flag", false);
        i.Name.ShouldBe("flag");
        i.Required.ShouldBe(false);
    }

    [Test]
    public void ManualChoiceInput_InitializesCorrectly()
    {
        var i = new ManualChoiceInput("env", "Environment", null, ["dev", "staging", "prod"]);
        i.Name.ShouldBe("env");
        i.Choices.ShouldBe(["dev", "staging", "prod"]);
    }
}
