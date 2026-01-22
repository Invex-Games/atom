namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public sealed record GithubTokenPermissionsOption : IWorkflowOption
{
    public GithubTokenPermission? Actions { get; init; }

    public GithubTokenPermission? Attestations { get; init; }

    public GithubTokenPermission? Checks { get; init; }

    public GithubTokenPermission? Contents { get; init; }

    public GithubTokenPermission? Deployments { get; init; }

    public GithubTokenPermission? IdToken { get; init; }

    public GithubTokenPermission? Issues { get; init; }

    public GithubTokenPermission? Discussions { get; init; }

    public GithubTokenPermission? Packages { get; init; }

    public GithubTokenPermission? Pages { get; init; }

    public GithubTokenPermission? PullRequests { get; init; }

    public GithubTokenPermission? SecurityEvents { get; init; }

    public GithubTokenPermission? Statuses { get; init; }

    public List<(string, string)> GetStrings =>
        new List<(string, string?)>
            {
                ("actions", GetTokenPermissionString(Actions)),
                ("attestations", GetTokenPermissionString(Attestations)),
                ("checks", GetTokenPermissionString(Checks)),
                ("contents", GetTokenPermissionString(Contents)),
                ("deployments", GetTokenPermissionString(Deployments)),
                ("id-token", GetTokenPermissionString(IdToken)),
                ("issues", GetTokenPermissionString(Issues)),
                ("discussions", GetTokenPermissionString(Discussions)),
                ("packages", GetTokenPermissionString(Packages)),
                ("pages", GetTokenPermissionString(Pages)),
                ("pull-requests", GetTokenPermissionString(PullRequests)),
                ("security-events", GetTokenPermissionString(SecurityEvents)),
                ("statuses", GetTokenPermissionString(Statuses)),
            }
            .Where(x => x.Item2 is not null)
            .Select(x => (x.Item1, x.Item2!))
            .ToList();

    private static string? GetTokenPermissionString(GithubTokenPermission? permission) =>
        permission switch
        {
            GithubTokenPermission.None => "none",
            GithubTokenPermission.Read => "read",
            GithubTokenPermission.Write => "write",
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(permission), permission, null),
        };
}

[PublicAPI]
public enum GithubTokenPermission
{
    None,
    Read,
    Write,
}
