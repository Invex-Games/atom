namespace Invex.Atom.Build.Analyzers.Tests;

/// <summary>
///     The reference assemblies the analyzer tests compile against. They match the framework the tests run on, because
///     the Atom assembly added to each test compilation is the build for that framework.
/// </summary>
internal static class TestReferenceAssemblies
{
#if NET11_0_OR_GREATER
    public static ReferenceAssemblies Current { get; } = new("net11.0",
        new("Microsoft.NETCore.App.Ref", "11.0.0"),
        Path.Combine("ref", "net11.0"));
#else
    public static ReferenceAssemblies Current { get; } = new("net10.0",
        new("Microsoft.NETCore.App.Ref", "10.0.8"),
        Path.Combine("ref", "net10.0"));
#endif
}
