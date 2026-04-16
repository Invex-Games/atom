namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Group configuration for dependencies.
/// </summary>
[UnstableAPI]
[Union]
public partial record DependabotGroup
{
    /// <summary>
    ///     Use to specify whether the rules in the group apply to version updates or security updates.
    /// </summary>
    public GroupAppliesTo? AppliesTo { get; init; }

    /// <summary>
    ///     Specify a dependency type to be included in the group.
    /// </summary>
    public virtual GroupDependencyType? DependencyType { get; init; }

    /// <summary>
    ///     Define strings of characters that match with a dependency name to include those dependencies in the group.
    /// </summary>
    public virtual IReadOnlyList<string> Patterns { get; init; } = [];

    /// <summary>
    ///     Exclude certain dependencies from the group.
    /// </summary>
    public virtual IReadOnlyList<string> ExcludePatterns { get; init; } = [];

    /// <summary>
    ///     Specify the semantic versioning level to include in the group.
    /// </summary>
    public virtual IReadOnlyList<GroupUpdateType> UpdateTypes { get; init; } = [];

    /// <summary>
    ///     Configure how dependencies are grouped within this group.
    /// </summary>
    public virtual GroupBy? GroupBy { get; init; }

    public sealed partial record FromType
    {
        public override required GroupDependencyType? DependencyType { get; init; }
    }

    public sealed partial record FromPatterns
    {
        public override required IReadOnlyList<string> Patterns { get; init; }
    }

    public sealed partial record FromExcludePatterns
    {
        public override required IReadOnlyList<string> ExcludePatterns { get; init; }
    }

    public sealed partial record FromUpdateTypes
    {
        public override required IReadOnlyList<GroupUpdateType> UpdateTypes { get; init; }
    }

    public sealed partial record FromGroupBy
    {
        public override required GroupBy? GroupBy { get; init; }
    }
}

/// <summary>
///     Specifies whether the rules in the group apply to version updates or security updates.
/// </summary>
[UnstableAPI]
public enum GroupAppliesTo
{
    VersionUpdates,
    SecurityUpdates,
}

/// <summary>
///     Dependency type for groups.
/// </summary>
[UnstableAPI]
public enum GroupDependencyType
{
    Development,
    Production,
}

/// <summary>
///     Semantic versioning update type.
/// </summary>
[UnstableAPI]
public enum GroupUpdateType
{
    Major,
    Minor,
    Patch,
}

/// <summary>
///     Configure how dependencies are grouped.
/// </summary>
[UnstableAPI]
public enum GroupBy
{
    DependencyName,
}
