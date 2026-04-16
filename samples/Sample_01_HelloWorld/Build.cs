// Minimal build definition that prints a hello world message.
// In the console, run the following command to execute the build:
// dotnet run -- HelloWorld

// This is automatically globally included when using DecSm.Atom from a nuget package

using DecSm.Atom.Build.Definition;
using DecSm.Atom.Hosting;

namespace Atom;

/// <summary>
///     This build definition provides a minimal example of how to create a build process using DecSm.Atom.
///     It defines a single target, <see cref="HelloWorld" />, which prints a "Hello, World!" message to the console.
/// </summary>
/// <remarks>
///     To execute this build, navigate to the project directory in your terminal and run:
///     <code>dotnet run -- HelloWorld</code>
/// </remarks>
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition
{
    /// <summary>
    ///     The "HelloWorld" target prints a simple greeting message to the console.
    /// </summary>
    /// <remarks>
    ///     This target demonstrates the basic structure of a build target and how to use the
    ///     built-in logger to output information during the build process.
    /// </remarks>
    private Target HelloWorld =>
        t => t
            .DescribedAs("Prints a hello world message to the console")
            .Executes(() =>
            {
                // The Logger is automatically injected into the build context,
                // allowing for structured logging of information, warnings, and errors.
                Logger.LogInformation("Hello, World!");
            });
}
