namespace DecSm.Atom.Hosting;

/// <summary>
///     Triggers the source generation of the application's entry point (<c>Main</c> method).
/// </summary>
/// <remarks>
///     When this attribute is applied to a class, the source generator creates a <c>Program.cs</c> file
///     containing a <c>Main</c> method that calls <see cref="AtomHost.Run{T}" /> with the decorated class
///     as the build definition. This automates the setup of the application's entry point.
/// </remarks>
/// <example>
///     <code>
/// [GenerateEntryPoint]
/// [BuildDefinition]
/// public partial class MyBuild : IMyTargets;
///     </code>
/// </example>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class GenerateEntryPointAttribute : Attribute;
