namespace Invex.Atom.Module.DevOpsWorkflows.Workflows.Options;

[PublicAPI]
public sealed record DevOpsVariableGroup(string Name) : IBuildOption;
