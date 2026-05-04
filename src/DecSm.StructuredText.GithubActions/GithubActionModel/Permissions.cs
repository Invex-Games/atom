namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
[Union]
public partial record Permissions
{
    [PublicAPI]
    public sealed partial record All(PermissionsLevel Level);

    [PublicAPI]
    public sealed partial record Exact(PermissionsEvent Permissions);
}
