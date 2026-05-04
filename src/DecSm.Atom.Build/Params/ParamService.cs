namespace DecSm.Atom.Build.Params;

/// <summary>
///     Defines a service for resolving and managing build parameters from various sources.
/// </summary>
/// <remarks>
///     <para>
///         The service resolves parameters from the following sources in order of precedence:
///     </para>
///     <list type="number">
///         <item>
///             <description>Cache (if enabled)</description>
///         </item>
///         <item>
///             <description>Command-line arguments</description>
///         </item>
///         <item>
///             <description>Environment variables</description>
///         </item>
///         <item>
///             <description>Configuration files (e.g., <c>appsettings.json</c>)</description>
///         </item>
///         <item>
///             <description>Secret providers</description>
///         </item>
///         <item>
///             <description>Interactive console prompts</description>
///         </item>
///         <item>
///             <description>Default value</description>
///         </item>
///     </list>
///     <para>
///         This service is central to the framework's parameter handling, providing features for type conversion,
///         secret masking, and caching.
///     </para>
/// </remarks>
[PublicAPI]
public interface IParamService
{
    /// <summary>
    ///     Retrieves a parameter value using a lambda expression to identify the parameter.
    /// </summary>
    /// <typeparam name="T">The target type of the parameter.</typeparam>
    /// <param name="paramExpression">A lambda expression identifying the parameter (e.g., <c>() => MyParam</c>).</param>
    /// <param name="defaultValue">The value to return if the parameter cannot be resolved.</param>
    /// <param name="converter">An optional function to convert the resolved string value to the target type.</param>
    /// <returns>The resolved parameter value, or the default value if not found.</returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        Expression<Func<T?>> paramExpression,
        T? defaultValue = default,
        Func<string?, T?>? converter = null);

    /// <summary>
    ///     Retrieves a parameter value by its name.
    /// </summary>
    /// <typeparam name="T">The target type of the parameter.</typeparam>
    /// <param name="paramName">The name of the parameter to retrieve.</param>
    /// <param name="defaultValue">The value to return if the parameter cannot be resolved.</param>
    /// <param name="converter">An optional function to convert the resolved string value to the target type.</param>
    /// <returns>The resolved parameter value, or the default value if not found.</returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    T? GetParam<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string paramName,
        T? defaultValue = default,
        Func<string?, T?>? converter = null);

    /// <summary>
    ///     Retrieves a string parameter value by its name.
    /// </summary>
    /// <param name="paramName">The name of the parameter to retrieve.</param>
    /// <param name="defaultValue">The value to return if the parameter cannot be resolved.</param>
    /// <returns>The resolved string parameter value, or the default value if not found.</returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    string? GetParam(string paramName, string? defaultValue = null) =>
        GetParam<string>(paramName, defaultValue);

    /// <summary>
    ///     Replaces known secret values in the provided text with a mask (e.g. "*****").
    /// </summary>
    /// <param name="text">The text to scan and mask.</param>
    /// <returns>The text with any resolved secret values replaced by a mask.</returns>
    string MaskMatchingSecrets(string text);

    /// <summary>
    ///     Creates a disposable scope within which parameter caching is temporarily disabled.
    /// </summary>
    /// <returns>An <see cref="IDisposable" /> that restores the previous cache state upon disposal.</returns>
    IDisposable CreateNoCacheScope();

    /// <summary>
    ///     Creates a disposable scope that temporarily causes parameter resolution to be suppressed, returning only default
    ///     values (when they exist).
    /// </summary>
    /// <returns>An <see cref="IDisposable" /> that restores the previous state upon disposal.</returns>
    IDisposable CreateDefaultValuesOnlyScope();

    /// <summary>
    ///     Creates a disposable scope that temporarily overrides parameter sources.
    /// </summary>
    /// <param name="sources">The parameter sources to use within the scope.</param>
    /// <returns>An <see cref="IDisposable" /> that restores the previous state upon disposal.</returns>
    IDisposable CreateOverrideSourcesScope(ParamSource sources);
}

/// <summary>
///     An internal implementation of <see cref="IParamService" /> that resolves parameters from various sources.
/// </summary>
/// <param name="buildDefinition">The build definition containing parameter metadata.</param>
/// <param name="args">The parsed command-line arguments.</param>
/// <param name="console">The console for interactive prompts.</param>
/// <param name="config">The application configuration.</param>
/// <param name="logger">The logger for diagnostics.</param>
/// <param name="secretsProviders">The registered secret providers.</param>
internal sealed class ParamService(
    IBuildDefinition buildDefinition,
    CommandLineArgs args,
    IAnsiConsole console,
    IConfiguration config,
    ILogger<ParamService> logger,
    IEnumerable<ISecretsProvider> secretsProviders
) : IParamService
{
    private readonly Dictionary<string, object?> _cache = [];

    /// <summary>
    ///     Gets or sets a value indicating whether parameter resolution is currently suppressed.
    /// </summary>
    private readonly AsyncLocal<bool> _defaultValuesOnly = new();

    private readonly List<string> _knownSecrets = [];

    /// <summary>
    ///     Gets or sets a value indicating whether parameter caching is disabled.
    /// </summary>
    private readonly AsyncLocal<bool> _noCache = new();

    /// <summary>
    ///     Gets or sets an optional override for parameter sources to use when resolving parameters.
    /// </summary>
    private readonly AsyncLocal<ParamSource?> _overrideSources = new();

    private readonly ISecretsProvider[] _secretsProviders = secretsProviders
        .OrderByDescending(p => p.Priority)
        .ToArray();

    /// <inheritdoc />
    public IDisposable CreateNoCacheScope() =>
        new NoCacheScope(this);

    /// <inheritdoc />
    public IDisposable CreateDefaultValuesOnlyScope() =>
        new DefaultValuesOnlyScope(this);

    /// <inheritdoc />
    public IDisposable CreateOverrideSourcesScope(ParamSource sources) =>
        new OverrideSourcesScope(this, sources);

    /// <inheritdoc />
    public string MaskMatchingSecrets(string text) =>
        text.SanitizeSecrets(_knownSecrets);

    /// <inheritdoc />
    public T? GetParam<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        Expression<Func<T?>> paramExpression,
        T? defaultValue = default,
        Func<string?, T?>? converter = null)
    {
        var paramName = paramExpression switch
        {
            LambdaExpression lambdaExpression => lambdaExpression.Body switch
            {
                MemberExpression memberExpression => memberExpression.Member.Name,
                _ => throw new ArgumentException("Invalid expression type."),
            },
            _ => throw new ArgumentException("Invalid expression type."),
        };

        return GetParam(paramName, defaultValue, converter);
    }

    /// <inheritdoc />
    public T? GetParam<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string paramName,
        T? defaultValue = default,
        Func<string?, T?>? converter = null)
    {
        var paramDefinition = buildDefinition.ParamDefinitions.GetValueOrDefault(paramName);

        return paramDefinition is null
            ? throw new InvalidOperationException($"Parameter '{paramName}' not found.")
            : GetParam(paramDefinition, defaultValue, converter);
    }

    /// <summary>
    ///     The core method for resolving a parameter value based on its definition and configured sources.
    /// </summary>
    private T? GetParam<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        ParamDefinition paramDefinition,
        T? defaultValue = default,
        Func<string?, T?>? converter = null)
    {
        if (_defaultValuesOnly.Value)
            return defaultValue;

        var source = _overrideSources.Value ?? paramDefinition.Sources;

        T? result;

        if (source.HasFlag(ParamSource.Cache) &&
            TryGetParamFromCache(paramDefinition, _cache, converter) is (true, { } cacheValue))
        {
            result = cacheValue;
            logger.LogDebug("Resolved param {ParamName} from cache", paramDefinition.Name);
        }
        else if (source.HasFlag(ParamSource.CommandLineArgs) &&
                 TryGetParamFromArgs(paramDefinition, args, converter) is (true, { } argsValue))
        {
            result = argsValue;
            logger.LogDebug("Resolved param {ParamName} from command line args", paramDefinition.Name);
        }
        else if (source.HasFlag(ParamSource.EnvironmentVariables) &&
                 TryGetParamFromEnvironmentVariables(paramDefinition, converter) is (true, { } envVarValue))
        {
            result = envVarValue;
            logger.LogDebug("Resolved param {ParamName} from environment variables", paramDefinition.Name);
        }
        else if (source.HasFlag(ParamSource.Configuration) &&
                 TryGetParamFromConfig(paramDefinition, config, converter) is (true, { } configValue))
        {
            result = configValue;
            logger.LogDebug("Resolved param {ParamName} from configuration", paramDefinition.Name);
        }
        else if (source.HasFlag(ParamSource.Secrets) &&
                 paramDefinition.IsSecret &&
                 TryGetParamFromSecrets(paramDefinition, _secretsProviders, converter) is (true, { } secretValue))
        {
            result = secretValue;
            logger.LogDebug("Resolved param {ParamName} from secrets", paramDefinition.Name);
        }
        else if (args is { HasHeadless: false, HasInteractive: true, HasHelp: false } &&
                 TryGetParamFromConsole(paramDefinition, console, defaultValue, _cache, converter)
                     is (true, { } userValue))
        {
            result = userValue;
            logger.LogDebug("Resolved param {ParamName} from user input", paramDefinition.Name);
        }
        else
        {
            result = defaultValue;

            logger.LogDebug("Parameter '{ParamName}' not resolved, using default value '{DefaultValue}'",
                paramDefinition.Name,
                result is null
                    ? "(null)"
                    : result is ""
                        ? "(empty string)"
                        : paramDefinition.IsSecret
                            ? "*****"
                            : result.ToString());
        }

        if (!_noCache.Value)
            _cache[paramDefinition.Name] = result;

        if (paramDefinition.IsSecret)
            _knownSecrets.Add(result?.ToString() ?? string.Empty);

        logger.LogDebug("Parameter '{ParamName}' resolved to '{Value}'",
            paramDefinition.Name,
            result is null
                ? "(null)"
                : result is ""
                    ? "(empty string)"
                    : paramDefinition.IsSecret
                        ? "*****"
                        : result.ToString());

        return result;
    }

    /// <summary>
    ///     Attempts to resolve a parameter from the cache.
    /// </summary>
    private static (bool HasValue, T? Value)
        TryGetParamFromCache<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
            ParamDefinition paramDefinition,
            Dictionary<string, object?> cache,
            Func<string?, T?>? converter)
    {
        if (cache.TryGetValue(paramDefinition.Name, out var value))
            return (true, value switch
            {
                T valueAsT => valueAsT,
                string valueAsString => TypeUtil.Convert(valueAsString, converter),
                _ => default,
            });

        return (false, default);
    }

    /// <summary>
    ///     Attempts to resolve a parameter from the command-line arguments.
    /// </summary>
    private static (bool HasValue, T? Value)
        TryGetParamFromArgs<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
            ParamDefinition paramDefinition,
            CommandLineArgs commandLineArgs,
            Func<string?, T?>? converter)
    {
        var matchingArg = commandLineArgs.Params.FirstOrDefault(x => x.ParamName == paramDefinition.Name);

        if (matchingArg is null)
            return (false, default);

        var convertedMatchingArg = TypeUtil.Convert(matchingArg.ParamValue, converter);

        return (true, convertedMatchingArg);
    }

    /// <summary>
    ///     Attempts to resolve a parameter from environment variables.
    /// </summary>
    private static (bool HasValue, T? Value) TryGetParamFromEnvironmentVariables<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        ParamDefinition paramDefinition,
        Func<string?, T?>? converter)
    {
        var envVar = Environment.GetEnvironmentVariable(paramDefinition.Name);

        if (string.IsNullOrEmpty(envVar))
            envVar = Environment.GetEnvironmentVariable(paramDefinition.ArgName);

        if (string.IsNullOrEmpty(envVar))
            envVar = Environment.GetEnvironmentVariable(paramDefinition.EnvVarName);

        if (string.IsNullOrEmpty(envVar))
            return (false, default);

        var convertedEnvVar = TypeUtil.Convert(envVar, converter);

        return (true, convertedEnvVar);
    }

    /// <summary>
    ///     Attempts to resolve a parameter from configuration files.
    /// </summary>
    private static (bool HasValue, T? Value)
        TryGetParamFromConfig<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
            ParamDefinition paramDefinition,
            IConfiguration configuration,
            Func<string?, T?>? converter)
    {
        var configSection = configuration
            .GetSection("Params")
            .GetSection(paramDefinition.ArgName);

        var configValue = configSection.Exists()

            // Avoid using ConfigurationBinder generic APIs to be compatible with trimming/AOT (SYSLIB1104)
            ? TypeUtil.Convert(configSection.Value, converter)
            : default;

        return (configSection.Exists(), configValue);
    }

    /// <summary>
    ///     Attempts to resolve a parameter from registered secret providers.
    /// </summary>
    private static (bool HasValue, T? Value)
        TryGetParamFromSecrets<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
            ParamDefinition paramDefinition,
            ISecretsProvider[] vaultProviders,
            Func<string?, T?>? converter)
    {
        foreach (var vaultProvider in vaultProviders)
        {
            var vaultValue = vaultProvider.GetSecret(paramDefinition.ArgName);

            if (vaultValue is null)
                continue;

            var convertedVaultValue = TypeUtil.Convert(vaultValue, converter);

            return (true, convertedVaultValue);
        }

        return (false, default);
    }

    /// <summary>
    ///     Attempts to resolve a parameter by prompting the user in the console.
    /// </summary>
    private static (bool HasValue, T? Value)
        TryGetParamFromConsole<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
            ParamDefinition paramDefinition,
            IAnsiConsole ansiConsole,
            T? defaultValue,
            Dictionary<string, object?> cache,
            Func<string?, T?>? converter)
    {
        var result = ansiConsole.Prompt(
            new TextPrompt<string>($"Enter value for parameter '{paramDefinition.ArgName}': ")
            {
                IsSecret = paramDefinition.IsSecret,
                Mask = '*',
                ShowDefaultValue = defaultValue is not null,
                AllowEmpty = defaultValue is not null,
            });

        if (result is not { Length: > 0 })
            return (false, default);

        var convertedResult = TypeUtil.Convert(result, converter);

        // Force cache as it is a user input
        cache[paramDefinition.Name] = convertedResult;

        return (true, convertedResult);
    }

    /// <summary>
    ///     A disposable struct that temporarily disables caching in the <see cref="ParamService" />.
    /// </summary>
    private readonly record struct NoCacheScope : IDisposable
    {
        private readonly ParamService _paramService;

        public NoCacheScope(ParamService paramService)
        {
            _paramService = paramService;
            paramService._noCache.Value = true;
        }

        public void Dispose() =>
            _paramService._noCache.Value = false;
    }

    /// <summary>
    ///     A disposable struct that temporarily causes parameter resolution to be suppressed, returning only default values
    ///     (when they exist).
    /// </summary>
    private readonly record struct DefaultValuesOnlyScope : IDisposable
    {
        private readonly ParamService _paramService;

        public DefaultValuesOnlyScope(ParamService paramService)
        {
            _paramService = paramService;
            paramService._defaultValuesOnly.Value = true;
        }

        public void Dispose() =>
            _paramService._defaultValuesOnly.Value = false;
    }

    /// <summary>
    ///     A disposable struct that temporarily overrides parameter sources.
    /// </summary>
    private readonly record struct OverrideSourcesScope : IDisposable
    {
        private readonly ParamService _paramService;
        private readonly ParamSource? _previousSources;

        public OverrideSourcesScope(ParamService paramService, ParamSource sources)
        {
            _paramService = paramService;
            _previousSources = paramService._overrideSources.Value;
            paramService._overrideSources.Value = sources;
        }

        public void Dispose() =>
            _paramService._overrideSources.Value = _previousSources;
    }
}
