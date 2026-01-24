namespace DecSm.Atom.Experimental;

[UnstableAPI]
public sealed record AtomArguments(params string[] Arguments) : IWorkflowOption;
