namespace DecSm.Atom.Build.Model;

/// <summary>
///     Represents a variable that is consumed by a target.
/// </summary>
/// <param name="TargetName">The name of the target that produced the variable.</param>
/// <param name="VariableName">The name of the variable being consumed.</param>
[PublicAPI]
public sealed record ConsumedVariable(string TargetName, string VariableName);
