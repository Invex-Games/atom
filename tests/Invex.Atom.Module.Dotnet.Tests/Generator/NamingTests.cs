namespace Invex.Atom.Module.Dotnet.Tests.Generator;

[TestFixture]
public class NamingTests
{
    [TestCase("--configfile", "ConfigFile")]
    [TestCase("--nologo", "NoLogo")]
    [TestCase("--msbuild", "MsBuild")]
    [TestCase("--vbcscompiler", "VbcsCompiler")]
    [TestCase("vstest", "VsTest")]
    [TestCase("msbuild", "MsBuild")]
    [TestCase("--no-restore", "NoRestore")]
    [TestCase("--getProperty", "GetProperty")]
    [TestCase("build-server shutdown", "BuildServerShutdown")]
    [TestCase("PROJECT | SOLUTION | FILE", "ProjectOrSolutionOrFile")]
    public void ToPascalCase_HandlesCompoundWords(string input, string expected) =>
        CsharpWriter
            .ToPascalCase(input)
            .ShouldBe(expected);

    [Test]
    public void ToPascalCase_LowerFirstWord_LowersKnownWords() =>
        CsharpWriter
            .ToPascalCase("msbuild-args", true)
            .ShouldBe("msBuildArgs");

    [TestCase("build", "DotnetBuildOptions")]
    [TestCase("run", "DotnetRunOptions")]
    [TestCase("test", "DotnetTestOptions")]
    [TestCase("package add", "DotnetPackageAddOptions")]
    [TestCase("msbuild", "DotnetMsBuildOptions")]
    public void OptionsTypeName_HasDotnetPrefix(string commandName, string expected) =>
        DotnetCliGenerator
            .GetOptionsTypeName(new(commandName, null, [], [], []))
            .ShouldBe(expected);

    [Test]
    public void GeneratedOptions_DoNotCollideWithCoreBuildOptions()
    {
        typeof(DotnetBuildOptions).FullName.ShouldBe("Invex.Atom.Module.Dotnet.Cli.DotnetBuildOptions");

        typeof(IDotnetCli)
            .Assembly
            .GetExportedTypes()
            .Where(x => x.Name.EndsWith("Options", StringComparison.Ordinal) &&
                        x.Namespace is "Invex.Atom.Module.Dotnet.Cli" &&
                        x is { IsClass: true, IsSealed: true } &&
                        x.GetMethod("<Clone>$") is not null)
            .ShouldAllBe(x => x.Name.StartsWith("Dotnet", StringComparison.Ordinal));
    }

    [Test]
    public void GeneratedOptions_UsePascalCasedCompoundWords()
    {
        typeof(DotnetBuildOptions)
            .GetProperty(nameof(DotnetBuildOptions.NoLogo))
            .ShouldNotBeNull();

        typeof(DotnetBuildOptions)
            .GetProperty(nameof(DotnetBuildOptions.ConfigFile))
            .ShouldNotBeNull();

        typeof(DotnetBuildServerShutdownOptions)
            .GetProperty(nameof(DotnetBuildServerShutdownOptions.VbcsCompiler))
            .ShouldNotBeNull();

        typeof(IDotnetCli)
            .GetMethods()
            .Select(x => x.Name)
            .ShouldContain("MsBuild");
    }
}
