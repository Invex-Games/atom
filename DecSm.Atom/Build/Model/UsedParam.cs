namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents a parameter used by a build target, including its definition and whether it is required.
/// </summary>
/// <param name="Param">The <see cref="ParamModel" /> defining the parameter's metadata.</param>
/// <param name="Required">A value indicating whether this parameter is required by the target.</param>
[PublicAPI]
public sealed record UsedParam(ParamModel Param, bool Required);
