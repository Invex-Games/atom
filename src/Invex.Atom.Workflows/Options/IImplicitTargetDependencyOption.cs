namespace Invex.Atom.Workflows.Options;

/// <summary>
///     A build option that implicitly introduces dependencies on other targets.
/// </summary>
/// <remarks>
///     When such an option is applied to a target, the targets named in <see cref="TargetNames" /> are
///     added as dependencies during workflow resolution (e.g., when a <see cref="TargetCondition" />
///     references another target's output).
/// </remarks>
[PublicAPI]
public interface IImplicitTargetDependencyOption
{
    /// <summary>
    ///     Gets the names of targets that should be implicitly added as dependencies.
    /// </summary>
    IEnumerable<string> TargetNames { get; }
}
