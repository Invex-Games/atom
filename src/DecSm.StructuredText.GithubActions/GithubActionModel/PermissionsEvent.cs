namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record PermissionsEvent
{
    public PermissionsLevel? Actions { get; init; }

    public PermissionsLevel? Attestations { get; init; }

    public PermissionsLevel? Checks { get; init; }

    public PermissionsLevel? Contents { get; init; }

    public PermissionsLevel? Deployments { get; init; }

    public PermissionsLevel? IdTokens { get; init; }

    public PermissionsLevel? Issues { get; init; }

    public PermissionsLevel? Discussions { get; init; }

    public PermissionsLevel? Packages { get; init; }

    public PermissionsLevel? Pages { get; init; }

    public PermissionsLevel? PullRequests { get; init; }

    public PermissionsLevel? RepositoryProjects { get; init; }

    public PermissionsLevel? SecurityEvents { get; init; }

    public PermissionsLevel? Statuses { get; init; }

    public bool IsAll(PermissionsLevel permission) =>
        Actions == permission &&
        Attestations == permission &&
        Checks == permission &&
        Contents == permission &&
        Deployments == permission &&
        IdTokens == permission &&
        Issues == permission &&
        Discussions == permission &&
        Packages == permission &&
        Pages == permission &&
        PullRequests == permission &&
        RepositoryProjects == permission &&
        SecurityEvents == permission &&
        Statuses == permission;
}
