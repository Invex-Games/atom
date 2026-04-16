namespace DecSm.Atom.Build.Definition;

/// <summary>
///     Triggers the source generation of a solution model based on a <c>.sln</c> or <c>.slnx</c> file.
/// </summary>
/// <remarks>
///     When this attribute is applied to a partial class, the source generator searches for a solution file in the
///     project's root directory. It then generates a C# model representing the solution's structure, including
///     projects and their dependencies, making it available for use within the build definition.
/// </remarks>
/// <example>
///     <code>
/// [BuildDefinition]
/// [GenerateSolutionModel]
/// public partial class MyBuild : BuildDefinition
/// {
///     // The generated solution model can now be accessed, e.g., `Solution.MyProject`.
/// }
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateSolutionModelAttribute : Attribute;
