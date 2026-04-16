namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Represents a delegate that applies configuration to a <see cref="TargetDefinition" />.
/// </summary>
/// <param name="definition">The target definition to configure.</param>
/// <returns>The configured target definition.</returns>
[PublicAPI]
public delegate TargetDefinition Target(TargetDefinition definition);
