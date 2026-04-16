namespace DecSm.Atom.Tests.BuildTests.Secrets;

[BuildDefinition]
public partial class UserSecretsBuild : MinimalBuildDefinition, IUserSecretsTarget, IDotnetUserSecrets
{
    public string? ExecutionValue { get; set; }
}

public interface IUserSecretsTarget : IBuildAccessor
{
    [SecretDefinition("secret-1", "Secret 1")]
    string? Secret1 => GetParam(() => Secret1);

    string? ExecutionValue { set; }

    Target UserSecretsTarget =>
        t => t
            .DescribedAs("User secrets target")
            .RequiresParam(nameof(Secret1))
            .Executes(() =>
            {
                ExecutionValue = Secret1;

                return Task.CompletedTask;
            });
}
