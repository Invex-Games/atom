namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Dependency type for allow/ignore rules.
/// </summary>
[PublicAPI]
public enum DependencyType
{
    /// <summary>
    ///     All explicitly defined dependencies.
    /// </summary>
    Direct,

    /// <summary>
    ///     Dependencies of direct dependencies (also known as sub-dependencies, or transient dependencies).
    /// </summary>
    Indirect,

    /// <summary>
    ///     All explicitly defined dependencies. For bundler, pip, composer, cargo, also the dependencies of direct
    ///     dependencies.
    /// </summary>
    All,

    /// <summary>
    ///     Only dependencies in the 'Production dependency group'.
    /// </summary>
    Production,

    /// <summary>
    ///     Only dependencies in the 'Development dependency group'.
    /// </summary>
    Development,
}
