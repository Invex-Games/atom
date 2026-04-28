namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
[Union]
public partial record Permissions
{
    public sealed partial record All(PermissionsLevel Level);

    public sealed partial record Exact(PermissionsEvent Permissions);
}
