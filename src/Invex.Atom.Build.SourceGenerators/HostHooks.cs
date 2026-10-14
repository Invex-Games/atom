// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator - Perf
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - Perf

namespace Invex.Atom.Build.SourceGenerators;

/// <summary>
///     Emits the static <c>IConfigureHost</c> implementation shared by the class and interface build definition
///     generators.
/// </summary>
internal static class HostHooks
{
    public const string ConfigureHostInterface = "Invex.Atom.Build.Hosting.IConfigureHost";

    private const string IHost = "Microsoft.Extensions.Hosting.IHost";
    private const string IHostApplicationBuilder = "Microsoft.Extensions.Hosting.IHostApplicationBuilder";

    /// <summary>
    ///     Builds the members implementing <c>IConfigureHost</c> and the optional user hooks.
    /// </summary>
    /// <param name="hookInterfaces">
    ///     The interfaces to scan for <c>[ConfigureHost]</c> and <c>[ConfigureHostBuilder]</c>.
    /// </param>
    /// <param name="registeredInterfaces">
    ///     The interfaces to scan for build interfaces (deriving from <c>IBuildDefinition</c> or <c>IBuildAccessor</c>)
    ///     that are registered in the service collection.
    /// </param>
    public static string[] GetMembers(
        IEnumerable<INamedTypeSymbol> hookInterfaces,
        ImmutableArray<INamedTypeSymbol> registeredInterfaces) =>
    [
        GetConfigureHostBuilderMethod(hookInterfaces, registeredInterfaces),
        GetConfigureHostMethod(hookInterfaces),
        $"    static partial void OnConfigureHostBuilder({IHostApplicationBuilder} builder);",
        $"    static partial void OnConfigureHost({IHost} host);",
    ];

    private static string GetConfigureHostMethod(IEnumerable<INamedTypeSymbol> hookInterfaces)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"    public static void ConfigureHost({IHost} host)");
        sb.AppendLine("    {");

        foreach (var interfaceSymbol in hookInterfaces)
            if (HasAttribute(interfaceSymbol, ConfigureHostAttribute))
                sb.AppendLine(
                    $"        {interfaceSymbol.ToDisplayString()}.ConfigureHostFrom{interfaceSymbol.Name}(host);");

        sb.AppendLine("        OnConfigureHost(host);");
        sb.AppendLine("    }");

        return sb.ToString();
    }

    private static string GetConfigureHostBuilderMethod(
        IEnumerable<INamedTypeSymbol> hookInterfaces,
        ImmutableArray<INamedTypeSymbol> registeredInterfaces)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"    public static void ConfigureHostBuilder({IHostApplicationBuilder} builder)");
        sb.AppendLine("    {");

        foreach (var interfaceSymbol in registeredInterfaces)
        foreach (var subInterfaceSymbol in interfaceSymbol.AllInterfaces)
        {
            if (subInterfaceSymbol.ToDisplayString() is not IBuildDefinition and not IBuildAccessor)
                continue;

            var interfaceFullName = interfaceSymbol.ToDisplayString();

            sb.AppendLine(
                $"        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton<{interfaceFullName}>(builder.Services, static p => ({interfaceFullName})p.GetRequiredService<{IBuildDefinition}>());");

            break;
        }

        foreach (var interfaceSymbol in hookInterfaces)
            if (HasAttribute(interfaceSymbol, ConfigureHostBuilderAttribute))
                sb.AppendLine(
                    $"        {interfaceSymbol.ToDisplayString()}.ConfigureHostBuilderFrom{interfaceSymbol.Name}(builder);");

        sb.AppendLine("        OnConfigureHostBuilder(builder);");
        sb.AppendLine("    }");

        return sb.ToString();
    }

    private static bool HasAttribute(INamedTypeSymbol symbol, string attributeName)
    {
        foreach (var attributeData in symbol.GetAttributes())
            if (attributeData.AttributeClass?.ToDisplayString() == attributeName)
                return true;

        return false;
    }
}
