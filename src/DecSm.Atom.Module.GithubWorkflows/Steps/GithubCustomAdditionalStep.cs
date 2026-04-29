namespace DecSm.Atom.Module.GithubWorkflows.Steps;

[PublicAPI]
public sealed record GithubCustomAdditionalStep : IGithubAdditionalStepOption
{
    public required Step Step { get; init; }

    public required int Order { get; init; }

    public Step Build() =>
        Step;
}
