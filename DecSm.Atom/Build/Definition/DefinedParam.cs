namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents a parameter that is used by a target definition, specifying whether it is required.
/// </summary>
/// <param name="Param">The name of the parameter.</param>
/// <param name="Required">A value indicating whether this parameter is required.</param>
[PublicAPI]
public sealed record DefinedParam(string Param, bool Required);
