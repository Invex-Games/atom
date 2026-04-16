namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
internal partial class UnspecifiedTargetsBuild : MinimalBuildDefinition,
    IUnspecifiedTarget1,
    IUnspecifiedTarget2,
    IUnspecifiedTarget3;

internal interface IUnspecifiedTarget1
{
    Target UnspecifiedTarget1 => t => t.DescribedAs("Unspecified target 1");
}

internal interface IUnspecifiedTarget2
{
    Target UnspecifiedTarget2 =>
        t => t
            .DependsOn(nameof(IUnspecifiedTarget3.UnspecifiedTarget3))
            .DescribedAs("Unspecified target 2");
}

internal interface IUnspecifiedTarget3
{
    Target UnspecifiedTarget3 => t => t.DescribedAs("Unspecified target 3");
}
