namespace Invex.Atom.Workflows.Options;

[PublicAPI]
public interface IImplicitTargetDependencyOption
{
    IEnumerable<string> TargetNames { get; }
}
