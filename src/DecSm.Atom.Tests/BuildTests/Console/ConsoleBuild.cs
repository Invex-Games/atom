namespace DecSm.Atom.Tests.BuildTests.Console;

[BuildDefinition]
public partial class ConsoleBuild : BuildDefinition, IConsoleTarget;

public interface IConsoleTarget : IBuildAccessor
{
    [ParamDefinition("required-param", "Required param")]
    string RequiredParam => GetParam(() => RequiredParam)!;

    [ParamDefinition("default-param", "Default param")]
    string DefaultParam => GetParam(() => DefaultParam, "default-value");

    [SecretDefinition("secret-param", "Secret param")]
    string SecretParam => GetParam(() => SecretParam)!;

    Target ConsoleTarget =>
        t => t
            .DescribedAs("Console target")
            .RequiresParam(nameof(RequiredParam))
            .RequiresParam(nameof(DefaultParam))
            .RequiresParam(nameof(SecretParam))
            .Executes(() => Task.CompletedTask);
}
