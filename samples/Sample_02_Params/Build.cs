// Build definition that prints a hello message based on the provided parameter.

// In the console, run the following command to execute the build:
// dotnet run -- Hello

// This will present the following error message:
// Missing required parameter 'my-name' for target Hello

// Run the following command to execute the build with a parameter:
// dotnet run -- Hello --my-name World

// Run one of the following commands to have atom interactively prompt for the required params
// dotnet run -- Hello --interactive
// or
// dotnet run -- Hello -i

// These are automatically globally included when using DecSm.Atom from a nuget package

using DecSm.Atom.Build.Definition;
using DecSm.Atom.Hosting;
using DecSm.Atom.Params;

namespace Atom;

/// <summary>
///     This build definition demonstrates how to define and use parameters within a DecSm.Atom build process.
///     It showcases required parameters, parameters with default values, and how to retrieve configuration from
///     `appsettings.json`.
/// </summary>
/// <remarks>
///     To execute this build, navigate to the project directory in your terminal.
///     <para>
///         To see the effect of a missing required parameter, run:
///         <code>dotnet run -- Hello</code>
///         This will result in an error indicating the missing 'my-name' parameter.
///     </para>
///     <para>
///         To provide a parameter value via the command line, run:
///         <code>dotnet run -- Hello --my-name World</code>
///     </para>
///     <para>
///         To have DecSm.Atom interactively prompt for required parameters, use the `--interactive` or `-i` flag:
///         <code>dotnet run -- Hello --interactive</code>
///         or
///         <code>dotnet run -- Hello -i</code>
///     </para>
/// </remarks>
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : BuildDefinition
{
    /// <summary>
    ///     Defines a required parameter named "my-name".
    /// </summary>
    /// <remarks>
    ///     This parameter is used to personalize the greeting message. Since it's marked as required
    ///     by the <see cref="Hello" /> target, the build will fail if this parameter is not provided.
    /// </remarks>
    [ParamDefinition("my-name", "Name to greet")]
    private string? MyName => GetParam(() => MyName);

    /// <summary>
    ///     Defines a parameter named "config-item-1" which is populated from `appsettings.json`.
    /// </summary>
    /// <remarks>
    ///     This property demonstrates how DecSm.Atom can automatically bind configuration values
    ///     from `appsettings.json` to build parameters. The name "config-item-1" in the `ParamDefinition`
    ///     attribute corresponds to a key in the `appsettings.json` file.
    /// </remarks>
    [ParamDefinition("config-item-1", "Configuration item 1")]
    private string? ConfigItem1 => GetParam(() => ConfigItem1);

    /// <summary>
    ///     Defines a parameter named "config-item-2" with a default value.
    /// </summary>
    /// <remarks>
    ///     If this parameter is not explicitly provided via the command line or `appsettings.json`,
    ///     its value will default to "Default Value". This is useful for optional parameters.
    /// </remarks>
    [ParamDefinition("config-item-2", "Configuration item 2")]
    private string ConfigItem2 => GetParam(() => ConfigItem2, "Default Value");

    /// <summary>
    ///     The "Hello" target demonstrates the usage of defined parameters.
    /// </summary>
    /// <remarks>
    ///     This target requires <see cref="MyName" />, <see cref="ConfigItem1" />, and <see cref="ConfigItem2" />
    ///     to be provided. It then logs their values to the console.
    ///     The <see cref="TargetDefinition.RequiresParam(IEnumerable{string})" /> method ensures that the specified
    ///     parameters are available before the target's execution.
    /// </remarks>
    private Target Hello =>
        t => t
            .DescribedAs("Prints some messages based on provided parameters")
            .RequiresParam(nameof(MyName))
            .RequiresParam(nameof(ConfigItem1))
            .RequiresParam(nameof(ConfigItem2))
            .Executes(() =>
            {
                // The Logger is automatically injected into the TargetDefinition context,
                // allowing for structured logging of information, warnings, and errors.
                Logger.LogInformation("Hello, {Name}!", MyName);
                Logger.LogInformation("Config Item 1: {ConfigItem1}", ConfigItem1);
                Logger.LogInformation("Config Item 2: {ConfigItem2}", ConfigItem2);

                return Task.CompletedTask;
            });
}
