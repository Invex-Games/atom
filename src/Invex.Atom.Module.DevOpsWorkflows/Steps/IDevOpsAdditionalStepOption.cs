namespace Invex.Atom.Module.DevOpsWorkflows.Steps;

[PublicAPI]
public interface IDevOpsAdditionalStepOption : IAdditionalStepOption
{
    Step Build();
}
