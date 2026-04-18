namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
[Union]
public partial record Permissions
{
    partial record All(PermissionsLevel Level);

    partial record Exact(PermissionsEvent Permissions);

    public Permissions Shrink() =>
        this switch
        {
            All all => all,
            Exact exact => exact.Permissions.IsAll(PermissionsLevel.Write)
                ? new All(PermissionsLevel.Write)
                : exact.Permissions.IsAll(PermissionsLevel.Read)
                    ? new All(PermissionsLevel.Read)
                    : exact.Permissions.IsAll(PermissionsLevel.None)
                        ? new All(PermissionsLevel.None)
                        : exact.Permissions,
        };
}
