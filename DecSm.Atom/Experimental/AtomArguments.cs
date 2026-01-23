namespace DecSm.Atom.Experimental;

[PublicAPI]
public sealed record AtomArguments(params string[] Arguments) : IWorkflowOption;
