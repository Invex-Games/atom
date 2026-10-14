namespace Invex.Atom.Module.DevOpsWorkflows.Workflows.Steps;

[PublicAPI]
public interface IDevOpsAdditionalStepOption : IAdditionalStepOption
{
    Step Build();
}
