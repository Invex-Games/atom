namespace DecSm.Atom.Module.GithubWorkflows.Steps;

[PublicAPI]
public interface IGithubAdditionalStepOption : IAdditionalStepOption
{
    Step Build();
}
