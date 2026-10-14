namespace Invex.Atom.Module.DevopsWorkflows.Workflows.Options;

[PublicAPI]
public sealed record DevopsVariableGroup(string Name) : IBuildOption;
