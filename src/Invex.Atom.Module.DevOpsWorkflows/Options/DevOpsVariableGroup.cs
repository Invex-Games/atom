namespace Invex.Atom.Module.DevOpsWorkflows.Options;

[PublicAPI]
public sealed record DevOpsVariableGroup(string Name) : IBuildOption;
