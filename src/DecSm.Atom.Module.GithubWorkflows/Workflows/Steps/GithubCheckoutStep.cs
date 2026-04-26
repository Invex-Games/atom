namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Steps;

[PublicAPI]
public sealed record GithubCheckoutStep : CheckoutStep, IGithubAdditionalStepOption
{
    /// <summary>
    ///     Repository name with owner. For example, actions/checkout.
    /// </summary>
    /// <remarks>Default: ${{ github.repository }}</remarks>
    public TextExpression? Repository { get; init; }

    /// <summary>
    ///     The branch, tag or SHA to checkout. When checking out the repository that
    ///     triggered a workflow, this defaults to the reference or SHA for that event.
    ///     Otherwise, uses the default branch.
    /// </summary>
    public TextExpression? Branch { get; init; }

    /// <summary>
    ///     Personal access token (PAT) used to fetch the repository. The PAT is configured
    ///     with the local git config, which enables your scripts to run authenticated git
    ///     commands. The post-job step removes the PAT.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         We recommend using a service account with the least permissions necessary. Also
    ///         when generating a new PAT, select the least scopes necessary.
    ///     </para>
    ///     <para>
    ///         <see
    ///             href="https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets">
    ///             Learn
    ///             more about creating and using encrypted secrets
    ///         </see>
    ///     </para>
    ///     <para>Default: ${{ github.token }}</para>
    /// </remarks>
    public TextExpression? Token { get; init; }

    /// <summary>
    ///     SSH key used to fetch the repository. The SSH key is configured with the local
    ///     git config, which enables your scripts to run authenticated git commands. The
    ///     post-job step removes the SSH key.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         We recommend using a service account with the least permissions necessary.
    ///     </para>
    ///     <para>
    ///         <see
    ///             href="https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets">
    ///             Learn
    ///             more about creating and using encrypted secrets
    ///         </see>
    ///     </para>
    /// </remarks>
    public TextExpression? SshKey { get; init; }

    /// <summary>
    ///     Known hosts in addition to the user and global host key database. The public SSH
    ///     keys for a host may be obtained using the utility <c>ssh-keyscan</c>. For example,
    ///     <c>ssh-keyscan github.com</c>. The public key for github.com is always implicitly
    ///     added.
    /// </summary>
    public TextExpression? SshKnownHosts { get; init; }

    /// <summary>
    ///     Whether to perform strict host key checking. When true, adds the options
    ///     <c>StrictHostKeyChecking=yes</c> and <c>CheckHostIP=no</c> to the SSH command line. Use
    ///     the input <c>ssh-known-hosts</c> to configure additional hosts.
    /// </summary>
    /// <remarks>Default: true</remarks>
    public TextExpression? SshStrict { get; init; }

    /// <summary>
    ///     The user to use when connecting to the remote SSH host. By default 'git' is used.
    /// </summary>
    /// <remarks>Default: git</remarks>
    public TextExpression? SshUser { get; init; }

    /// <summary>
    ///     Whether to configure the token or SSH key with the local git config.
    /// </summary>
    /// <remarks>Default: true</remarks>
    public TextExpression? PersistCredentials { get; init; }

    /// <summary>
    ///     Relative path under $GITHUB_WORKSPACE to place the repository.
    /// </summary>
    public TextExpression? Path { get; init; }

    /// <summary>
    ///     Whether to execute <c>git clean -ffdx &amp;&amp; git reset --hard HEAD</c> before fetching.
    /// </summary>
    /// <remarks>Default: true</remarks>
    public TextExpression? Clean { get; init; }

    /// <summary>
    ///     Partially clone against a given filter. Overrides sparse-checkout if set.
    /// </summary>
    /// <remarks>Default: null</remarks>
    public TextExpression? Filter { get; init; }

    /// <summary>
    ///     Do a sparse checkout on given patterns. Each pattern should be separated with new lines.
    /// </summary>
    /// <remarks>Default: null</remarks>
    public TextExpression? SparseCheckout { get; init; }

    /// <summary>
    ///     Specifies whether to use cone-mode when doing a sparse checkout.
    /// </summary>
    /// <remarks>Default: true</remarks>
    public TextExpression? SparseCheckoutConeMode { get; init; }

    /// <summary>
    ///     Number of commits to fetch. 0 indicates all history for all branches and tags.
    /// </summary>
    /// <remarks>Default: 1</remarks>
    public TextExpression? FetchDepth { get; init; }

    /// <summary>
    ///     Whether to fetch tags, even if fetch-depth > 0.
    /// </summary>
    /// <remarks>Default: false</remarks>
    public TextExpression? FetchTags { get; init; }

    /// <summary>
    ///     Whether to show progress status output when fetching.
    /// </summary>
    /// <remarks>Default: true</remarks>
    public TextExpression? ShowProgress { get; init; }

    /// <summary>
    ///     Whether to download Git-LFS files.
    /// </summary>
    /// <remarks>Default: false</remarks>
    public TextExpression? Lfs { get; init; }

    /// <summary>
    ///     Whether to checkout submodules: <c>true</c> to checkout submodules or <c>recursive</c> to
    ///     recursively checkout submodules.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When the <c>ssh-key</c> input is not provided, SSH URLs beginning with
    ///         <c>git@github.com:</c> are converted to HTTPS.
    ///     </para>
    ///     <para>Default: false</para>
    /// </remarks>
    public TextExpression? Submodules { get; init; }

    /// <summary>
    ///     Add repository path as safe.directory for Git global config by running
    ///     <c>git config --global --add safe.directory &lt;path&gt;</c>.
    /// </summary>
    /// <remarks>Default: true</remarks>
    public TextExpression? SetSafeDirectory { get; init; }

    /// <summary>
    ///     The base URL for the GitHub instance that you are trying to clone from, will use
    ///     environment defaults to fetch from the same instance that the workflow is
    ///     running from unless specified. Example URLs are https://github.com or
    ///     https://my-ghes-server.example.com.
    /// </summary>
    public TextExpression? GithubServerUrl { get; init; }

    public Step Build()
    {
        var with = new Dictionary<string, TextExpressionCollection>();

        if (Repository is not null)
            with["repository"] = [Repository];

        if (Branch is not null)
            with["ref"] = [Branch];

        if (Token is not null)
            with["token"] = [Token];

        if (SshKey is not null)
            with["ssh-key"] = [SshKey];

        if (SshKnownHosts is not null)
            with["ssh-known-hosts"] = [SshKnownHosts];

        if (SshStrict is not null)
            with["ssh-strict"] = [SshStrict];

        if (SshUser is not null)
            with["ssh-user"] = [SshUser];

        if (PersistCredentials is not null)
            with["persist-credentials"] = [PersistCredentials];

        if (Path is not null)
            with["path"] = [Path];

        if (Clean is not null)
            with["clean"] = [Clean];

        if (Filter is not null)
            with["filter"] = [Filter];

        if (SparseCheckout is not null)
            with["sparse-checkout"] = [SparseCheckout];

        if (SparseCheckoutConeMode is not null)
            with["sparse-checkout-cone-mode"] = [SparseCheckoutConeMode];

        if (FetchDepth is not null)
            with["fetch-depth"] = [FetchDepth];

        if (FetchTags is not null)
            with["fetch-tags"] = [FetchTags];

        if (ShowProgress is not null)
            with["show-progress"] = [ShowProgress];

        if (Lfs is not null)
            with["lfs"] = [Lfs];

        if (Submodules is not null)
            with["submodules"] = [Submodules];

        if (SetSafeDirectory is not null)
            with["set-safe-directory"] = [SetSafeDirectory];

        if (GithubServerUrl is not null)
            with["github-server-url"] = [GithubServerUrl];

        return new Step.UsesStep
        {
            Name = "Checkout",
            With = with,
            Uses = "actions/checkout@v6",
        };
    }
}
