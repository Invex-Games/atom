namespace Invex.Atom.Module.AzureKeyVault.Tests.Workflows;

[BuildDefinition]
public partial class GitHubKeyVaultBuild : WorkflowBuildDefinition, IGitHubWorkflows, IAzureKeyVault, IVaultTarget
{
    public IReadOnlyList<IBuildOption> Options => [BuildOptions.AzureKeyVault.UseAzureKeyVault];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("github-key-vault-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets =
            [
                new(nameof(IVaultTarget.VaultTarget))
                {
                    // Allows the job to request an OIDC token for workload identity federation
                    Options =
                    [
                        new GitHubTokenPermissionsOption(new PermissionsEvent
                        {
                            IdTokens = PermissionsLevel.Write,
                        }),
                    ],
                },
            ],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}

[BuildDefinition]
public partial class DevOpsKeyVaultBuild : WorkflowBuildDefinition, IDevOpsWorkflows, IAzureKeyVault, IVaultTarget
{
    public IReadOnlyList<IBuildOption> Options => [BuildOptions.AzureKeyVault.UseAzureKeyVault];

    public AzureKeyVaultValueInjections AzureKeyVaultValueInjections =>
        new(ServiceConnectionId: AzureKeyVaultValueInjectionType.EnvironmentVariable);

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("devops-key-vault-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets =
            [
                new(nameof(IVaultTarget.VaultTarget))
                {
                    // Required by AzurePipelinesCredential for workload identity federation
                    Options =
                    [
                        BuildOptions.Inject.EnvironmentVariable("SYSTEM_ACCESSTOKEN",
                            new DevopsMacroExpression(TextExpressions.Raw("System.AccessToken"))),
                    ],
                },
            ],
            Types = [WorkflowTypes.DevOps.Pipeline],
        },
    ];
}

public interface IVaultTarget : IBuildAccessor
{
    [SecretDefinition("vault-secret", "A secret resolved from Azure Key Vault")]
    string? VaultSecret => GetParam(() => VaultSecret);

    Target VaultTarget =>
        t => t
            .DescribedAs("Uses a secret from Azure Key Vault")
            .RequiresParam(nameof(VaultSecret));
}
