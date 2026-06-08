namespace Invex.Atom.Build.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class DuplicateDependenciesBuild : BuildDefinition,
    IDuplicateDependenciesBase1,
    IDuplicateDependenciesBase2,
    IDuplicateDependenciesTarget;

public interface IDuplicateDependenciesBase1
{
    Target BaseDependency => t => t.Executes(() => Task.CompletedTask);

    Target BaseTarget1 =>
        t => t
            .DependsOn("BaseDependency")
            .Executes(() => Task.CompletedTask);
}

public interface IDuplicateDependenciesBase2
{
    Target BaseTarget2 =>
        t => t
            .DependsOn("BaseDependency")
            .Executes(() => Task.CompletedTask);
}

public interface IDuplicateDependenciesTarget
{
    Target DuplicateDependenciesTarget =>
        t => t
            .Extends<IDuplicateDependenciesBase1>(d => d.BaseTarget1)
            .Extends<IDuplicateDependenciesBase2>(d => d.BaseTarget2)
            .Executes(() => Task.CompletedTask);
}
