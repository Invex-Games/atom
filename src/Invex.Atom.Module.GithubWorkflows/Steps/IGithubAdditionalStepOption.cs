namespace Invex.Atom.Module.GithubWorkflows.Steps;

[PublicAPI]
public interface IGithubAdditionalStepOption : IAdditionalStepOption
{
    Step Build();
}
