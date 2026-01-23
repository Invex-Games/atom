namespace DecSm.Atom.Util;

/// <summary>
///     Provides static access to a service instance registered in the dependency injection container.
/// </summary>
/// <typeparam name="T">The type of the service to access.</typeparam>
/// <remarks>
///     This class implements the service locator pattern, allowing access to services without explicit
///     constructor injection. It should be used sparingly, as it can make code harder to test.
///     The <see cref="Service" /> property is populated by the extension methods in
///     <see cref="ServiceAccessorExtensions" />.
///     <para>
///         <b>Warning:</b> The service is only available after it has been resolved from the DI container.
///     </para>
/// </remarks>
/// <example>
///     <code>
/// // 1. Register the service
/// services.AddSingletonWithStaticAccessor&lt;IMyService, MyService&gt;();
/// // 2. Access the service statically from anywhere
/// ServiceStaticAccessor&lt;IMyService&gt;.Service?.DoSomething();
///     </code>
/// </example>
/// <seealso cref="ServiceAccessorExtensions" />
[PublicAPI]
public static class ServiceStaticAccessor<T>
    where T : notnull
{
    /// <summary>
    ///     Gets or sets the service instance. This property is populated by the DI container.
    /// </summary>
    public static T? Service { get; set; }
}

/// <summary>
///     Provides extension methods for registering services with static accessor functionality.
/// </summary>
internal static class ServiceAccessorExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers a singleton service that can be accessed statically via <see cref="ServiceStaticAccessor{T}" />.
        /// </summary>
        /// <typeparam name="TImplementation">The implementation type of the service.</typeparam>
        public void AddSingletonWithStaticAccessor<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TImplementation : class =>
            services
                .AddKeyedSingleton<TImplementation>("StaticAccess")
                .AddSingleton<TImplementation>(x =>
                    ServiceStaticAccessor<TImplementation>.Service =
                        x.GetRequiredKeyedService<TImplementation>("StaticAccess"));

        /// <summary>
        ///     Registers a singleton service with a separate interface and implementation that can be accessed statically.
        /// </summary>
        /// <typeparam name="TService">The service interface type.</typeparam>
        /// <typeparam name="TImplementation">The service implementation type.</typeparam>
        public void AddSingletonWithStaticAccessor<TService,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TService : class
            where TImplementation : class, TService =>
            services
                .AddKeyedSingleton<TService, TImplementation>("StaticAccess")
                .AddSingleton<TService>(x =>
                    ServiceStaticAccessor<TService>.Service = x.GetRequiredKeyedService<TService>("StaticAccess"));

        /// <summary>
        ///     Registers a singleton service using a factory function that can be accessed statically.
        /// </summary>
        /// <typeparam name="TService">The type of the service to register.</typeparam>
        /// <param name="implementationFactory">A factory function that creates the service instance.</param>
        /// <returns>The <see cref="IServiceCollection" /> for method chaining.</returns>
        public IServiceCollection AddSingletonWithStaticAccessor<TService>(
            Func<IServiceProvider, object?, TService> implementationFactory)
            where TService : class =>
            services
                .AddKeyedSingleton("StaticAccess", implementationFactory)
                .AddSingleton<TService>(x =>
                    ServiceStaticAccessor<TService>.Service = x.GetRequiredKeyedService<TService>("StaticAccess"));
    }
}
