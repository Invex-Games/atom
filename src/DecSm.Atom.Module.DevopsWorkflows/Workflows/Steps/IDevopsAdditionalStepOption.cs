namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Steps;

[PublicAPI]
public interface IDevopsAdditionalStepOption : IAdditionalStepOption
{
    Step Build();
}
