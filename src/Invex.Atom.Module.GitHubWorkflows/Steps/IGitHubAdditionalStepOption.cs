namespace Invex.Atom.Module.GitHubWorkflows.Steps;

[PublicAPI]
public interface IGitHubAdditionalStepOption : IAdditionalStepOption
{
    Step Build();
}
