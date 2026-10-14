namespace Invex.Atom.Module.GitHubWorkflows.Steps;

[PublicAPI]
public sealed record GitHubCustomAdditionalStep : IGitHubAdditionalStepOption
{
    public required Step Step { get; init; }

    public bool Enabled { get; init; } = true;

    public required int Order { get; init; }

    public Step Build() =>
        Step;
}
