namespace Invex.Atom.Build.Tests.ClassTests.Build.Definition;

[TestFixture]
[SuppressMessage("ReSharper", "ConvertToLocalFunction")]
internal sealed class TargetDefinitionExtensionsTests
{
    private static TargetDefinition MakeTarget(string name = "Test") =>
        new()
        {
            Name = name,
        };

    [Test]
    public void DependsOn_TwoTargets_AddsBothDependencies()
    {
        var t = MakeTarget();
        Target target1 = x => x;
        Target target2 = x => x;

        t.DependsOn(target1, target2);

        t.Dependencies.Count.ShouldBe(2);
        t.Dependencies.ShouldContain("target1");
        t.Dependencies.ShouldContain("target2");
    }

    [Test]
    public void DependsOn_ThreeTargets_AddsAllDependencies()
    {
        var t = MakeTarget();
        Target alpha = x => x;
        Target beta = x => x;
        Target gamma = x => x;

        t.DependsOn(alpha, beta, gamma);

        t.Dependencies.Count.ShouldBe(3);
        t.Dependencies.ShouldContain("alpha");
        t.Dependencies.ShouldContain("beta");
        t.Dependencies.ShouldContain("gamma");
    }

    [Test]
    public void DependsOn_FourTargets_AddsAllDependencies()
    {
        var t = MakeTarget();
        Target step1 = x => x;
        Target step2 = x => x;
        Target step3 = x => x;
        Target step4 = x => x;

        t.DependsOn(step1, step2, step3, step4);

        t.Dependencies.Count.ShouldBe(4);
        t.Dependencies.ShouldContain("step1");
        t.Dependencies.ShouldContain("step4");
    }

    [Test]
    public void DependsOn_FiveTargets_AddsAllDependencies()
    {
        var t = MakeTarget();
        Target job1 = x => x;
        Target job2 = x => x;
        Target job3 = x => x;
        Target job4 = x => x;
        Target job5 = x => x;

        t.DependsOn(job1, job2, job3, job4, job5);

        t.Dependencies.Count.ShouldBe(5);
        t.Dependencies.ShouldContain("job1");
        t.Dependencies.ShouldContain("job5");
    }

    [Test]
    public void DependsOn_TwoTargets_ReturnsSameInstance()
    {
        var t = MakeTarget();
        Target a = x => x;
        Target b = x => x;

        var result = t.DependsOn(a, b);

        result.ShouldBeSameAs(t);
    }
}
