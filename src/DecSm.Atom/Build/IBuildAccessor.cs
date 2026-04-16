namespace DecSm.Atom.Build;

/// <summary>
///     Provides base functionality for accessing services and parameters within the Atom build system.
/// </summary>
/// <remarks>
///     This interface serves as a foundation for target definitions and helpers, offering convenient access
///     to logging, file system operations, process execution, and parameter retrieval.
/// </remarks>
[PublicAPI]
public interface IBuildAccessor
{
    /// <summary>
    ///     Gets the service provider for resolving dependencies.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    ///     Gets a logger instance for the implementing type.
    /// </summary>
    protected ILogger Logger =>
        GetService<ILoggerFactory>()
            .CreateLogger(GetType());

    /// <summary>
    ///     Gets the Atom file system service for file and directory operations.
    /// </summary>
    protected IAtomFileSystem FileSystem => GetService<IAtomFileSystem>();

    /// <summary>
    ///     Gets the process runner service for executing external processes.
    /// </summary>
    protected IProcessRunner ProcessRunner => GetService<IProcessRunner>();

    /// <summary>
    ///     Gets a required service from the dependency injection container.
    /// </summary>
    /// <typeparam name="T">The type of service to retrieve.</typeparam>
    /// <returns>The service instance.</returns>
    /// <remarks>
    ///     If the requested type <typeparamref name="T" /> implements <see cref="IBuildDefinition" />, this method
    ///     returns the current instance cast to that type, rather than resolving from the service provider.
    /// </remarks>
    protected T GetService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : notnull =>
        typeof(IBuildDefinition).IsAssignableFrom(typeof(T))
            ? (T)this
            : Services.GetRequiredService<T>();

    /// <summary>
    ///     Retrieves all registered services of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of the services to retrieve.</typeparam>
    /// <returns>A collection of service instances.</returns>
    /// <remarks>
    ///     If the requested type <typeparamref name="T" /> implements <see cref="IBuildDefinition" />, this method
    ///     returns a collection containing only the current instance cast to that type.
    /// </remarks>
    protected IEnumerable<T> GetServices<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : notnull =>
        typeof(IBuildDefinition).IsAssignableFrom(typeof(T))
            ? [(T)this]
            : Services.GetServices<T>();

    /// <summary>
    ///     Retrieves a parameter value using a strongly-typed expression.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="parameterExpression">An expression identifying the parameter (e.g., <c>() => MyParam</c>).</param>
    /// <param name="defaultValue">The default value to return if the parameter is not found.</param>
    /// <param name="converter">An optional function to convert the parameter value from a string.</param>
    /// <returns>The resolved parameter value, or the default value if not found.</returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    protected T? GetParam<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        Expression<Func<T?>> parameterExpression,
        T? defaultValue = default,
        Func<string?, T?>? converter = null) =>
        Services
            .GetRequiredService<IParamService>()
            .GetParam(parameterExpression, defaultValue, converter);
}
