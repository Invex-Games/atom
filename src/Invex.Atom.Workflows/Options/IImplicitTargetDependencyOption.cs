namespace Invex.Atom.Workflows.Options;

/// <summary>
///     A build option that relies on other targets, for example by reading their outputs.
/// </summary>
/// <remarks>
///     The targets named in <see cref="TargetNames" /> are not added as dependencies automatically. During
///     workflow resolution, Atom logs a warning for each one that is not a declared dependency of the target
///     (e.g., when a <see cref="TargetCondition" /> references the output of a target it does not depend on).
/// </remarks>
[PublicAPI]
public interface IImplicitTargetDependencyOption
{
    /// <summary>
    ///     Gets the names of targets that the option relies on and that should be declared as dependencies.
    /// </summary>
    IEnumerable<string> TargetNames { get; }
}
