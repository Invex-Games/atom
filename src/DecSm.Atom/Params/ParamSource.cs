namespace DecSm.Atom.Params;

/// <summary>
///     Specifies the sources from which a parameter's value can be resolved.
/// </summary>
[PublicAPI]
[Flags]
public enum ParamSource
{
    /// <summary>
    ///     No source is specified.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The parameter can be resolved from a local cache.
    /// </summary>
    Cache = 1,

    /// <summary>
    ///     The parameter can be resolved from command-line arguments.
    /// </summary>
    CommandLineArgs = 2,

    /// <summary>
    ///     The parameter can be resolved from environment variables.
    /// </summary>
    EnvironmentVariables = 4,

    /// <summary>
    ///     The parameter can be resolved from configuration files (e.g., appsettings.json).
    /// </summary>
    Configuration = 8,

    /// <summary>
    ///     The parameter can be resolved from a secret management system.
    /// </summary>
    Secrets = 32,

    /// <summary>
    ///     The parameter can be resolved from any of the available sources.
    /// </summary>
    All = Cache | CommandLineArgs | EnvironmentVariables | Configuration | Secrets,
}
