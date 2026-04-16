namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     GitHub Dependabot v2 configuration.
/// </summary>
[UnstableAPI]
public sealed record DependabotConfig
{
    // We deviate from the schema and add a default value of 2 for Version
    /// <summary>
    ///     Dependabot configuration files require this key, and its value must be 2.
    /// </summary>
    public int Version { get; init; } = 2;

    /// <summary>
    ///     Enable ecosystems that have beta-level support.
    /// </summary>
    public bool? EnableBetaEcosystems { get; init; }

    /// <summary>
    ///     Element for each package manager that you want GitHub Dependabot to monitor for new versions.
    /// </summary>
    public required IReadOnlyList<DependabotUpdate> Updates { get; init; }

    /// <summary>
    ///     Authentication details that Dependabot can use to access private package registries.
    /// </summary>
    public IReadOnlyDictionary<string, DependabotRegistry>? Registries { get; init; }

    /// <summary>
    ///     Groups that span multiple package ecosystems, allowing consolidated pull requests across different ecosystems.
    /// </summary>
    public IReadOnlyDictionary<string, DependabotMultiEcosystemGroup>? MultiEcosystemGroups { get; init; }
}
