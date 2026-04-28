namespace DecSm.StructuredText.GithubActions.DependabotConfigModel.Model;

/// <summary>
///     Private package registry configuration.
/// </summary>
[PublicAPI]
public sealed record DependabotRegistry
{
    /// <summary>
    ///     Identifies the type of registry.
    /// </summary>
    public required RegistryType Type { get; init; }

    /// <summary>
    ///     The URL to use to access the dependencies in this registry.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    ///     The username that Dependabot uses to access the registry.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    ///     A reference to a Dependabot secret containing the password for the specified user.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    ///     A reference to a Dependabot secret containing an access key for this registry.
    /// </summary>
    public string? Key { get; init; }

    /// <summary>
    ///     A reference to a Dependabot secret containing an access token for this registry.
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    ///     For registries with type: python-index, if true, pip resolves dependencies by using the specified URL
    ///     rather than the base URL of the Python Package Index.
    /// </summary>
    public bool? ReplacesBase { get; init; }

    /// <summary>
    ///     The organization for hex-organization registries.
    /// </summary>
    public string? Organization { get; init; }

    /// <summary>
    ///     The repository for hex-repository registries.
    /// </summary>
    public string? Repo { get; init; }

    /// <summary>
    ///     The auth key for hex-repository registries.
    /// </summary>
    public string? AuthKey { get; init; }

    /// <summary>
    ///     The public key fingerprint for hex-repository registries.
    /// </summary>
    public string? PublicKeyFingerprint { get; init; }

    /// <summary>
    ///     The name of the cargo registry.
    /// </summary>
    public string? Registry { get; init; }

    /// <summary>
    ///     The tenant ID for Azure OIDC authentication.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    ///     The client ID for Azure OIDC authentication.
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    ///     The JFrog OIDC provider name for authentication.
    /// </summary>
    public string? JfrogOidcProviderName { get; init; }

    /// <summary>
    ///     The identity mapping name for JFrog OIDC authentication.
    /// </summary>
    public string? IdentityMappingName { get; init; }

    /// <summary>
    ///     The audience for OIDC or AWS authentication.
    /// </summary>
    public string? Audience { get; init; }

    /// <summary>
    ///     The AWS region for AWS CodeArtifact authentication.
    /// </summary>
    public string? AwsRegion { get; init; }

    /// <summary>
    ///     The AWS account ID for AWS CodeArtifact authentication.
    /// </summary>
    public string? AccountId { get; init; }

    /// <summary>
    ///     The AWS role name for AWS CodeArtifact authentication.
    /// </summary>
    public string? RoleName { get; init; }

    /// <summary>
    ///     The domain for AWS CodeArtifact authentication.
    /// </summary>
    public string? Domain { get; init; }

    /// <summary>
    ///     The domain owner for AWS CodeArtifact authentication.
    /// </summary>
    public string? DomainOwner { get; init; }
}

/// <summary>
///     Registry type for Dependabot.
/// </summary>
[PublicAPI]
public enum RegistryType
{
    CargoRegistry,
    ComposerRepository,
    DockerRegistry,
    Git,
    GoproxyServer,
    HexOrganization,
    HexRepository,
    HelmRegistry,
    MavenRepository,
    NpmRegistry,
    NugetFeed,
    PubRepository,
    PythonIndex,
    RubygemsServer,
    TerraformRegistry,
}
