namespace DecSm.Atom.Args;

/// <summary>
///     Defines the base interface for a parsed command-line argument.
/// </summary>
/// <remarks>
///     This marker interface is implemented by all specific argument types, such as commands, parameters, and options.
/// </remarks>
[PublicAPI]
public interface IArg;

/// <summary>
///     Represents a command argument, which corresponds to a specific target to be executed.
/// </summary>
/// <param name="Name">The name of the build target to execute. This is case-insensitive.</param>
/// <remarks>
///     The <paramref name="Name" /> must correspond to a target defined in the
///     <see cref="DecSm.Atom.Build.Definition.IBuildDefinition" />.
/// </remarks>
/// <example>For the command <c>atom MyTarget</c>, a <see cref="CommandArg" /> with `Name = "MyTarget"` is created.</example>
[PublicAPI]
public sealed record CommandArg(string Name) : IArg;

/// <summary>
///     Represents the argument to generate or regenerate workflow files (e.g., for CI/CD systems).
/// </summary>
/// <remarks>
///     This is triggered by the <c>-g</c> or <c>--gen</c> flag.
/// </remarks>
/// <example>For the command <c>atom --gen</c>, a <see cref="GenArg" /> is created.</example>
[PublicAPI]
public sealed record GenArg : IArg;

/// <summary>
///     Represents the argument to display help information.
/// </summary>
/// <remarks>
///     When present, the application should show available commands, options, and parameters.
///     This is triggered by the <c>-h</c> or <c>--help</c> flag.
/// </remarks>
/// <example>For the command <c>atom --help</c>, a <see cref="HelpArg" /> is created.</example>
[PublicAPI]
public sealed record HelpArg : IArg;

/// <summary>
///     Represents a parameter argument, which provides a configurable value to a build target.
/// </summary>
/// <param name="ArgName">The name of the argument as specified on the command line (e.g., "--version").</param>
/// <param name="ParamName">The canonical name of the parameter as defined in the build (e.g., "Version").</param>
/// <param name="ParamValue">The value provided for the parameter.</param>
/// <remarks>
///     The <paramref name="ParamName" /> must match a parameter defined in the
///     <see cref="DecSm.Atom.Build.Definition.IBuildDefinition" />.
/// </remarks>
/// <example>
///     For the command <c>atom Build --configuration Release</c>, a <see cref="ParamArg" /> would be created where
///     <c>ArgName</c> is "--configuration", <c>ParamName</c> is "Configuration", and <c>ParamValue</c> is "Release".
/// </example>
[PublicAPI]
public sealed record ParamArg(string ArgName, string ParamName, string ParamValue) : IArg;

/// <summary>
///     Represents the argument to skip the execution of dependent targets.
/// </summary>
/// <remarks>
///     When present, only explicitly specified targets will run; their dependencies will be ignored.
///     This is triggered by the <c>-s</c> or <c>--skip</c> flag.
/// </remarks>
/// <example>For the command <c>atom Deploy -s</c>, a <see cref="SkipArg" /> is created.</example>
[PublicAPI]
public sealed record SkipArg : IArg;

/// <summary>
///     Represents the argument to run in headless mode, disabling interactive features.
/// </summary>
/// <remarks>
///     Headless mode is intended for non-interactive environments like CI/CD pipelines.
///     This is triggered by the <c>-hl</c> or <c>--headless</c> flag.
/// </remarks>
/// <example>For the command <c>atom Build --headless</c>, a <see cref="HeadlessArg" /> is created.</example>
[PublicAPI]
public sealed record HeadlessArg : IArg;

/// <summary>
///     Represents the argument to run in verbose mode, enabling detailed logging.
/// </summary>
/// <remarks>
///     Verbose mode provides more detailed output, which is useful for debugging.
///     This is triggered by the <c>-v</c> or <c>--verbose</c> flag.
/// </remarks>
/// <example>For the command <c>atom Test --verbose</c>, a <see cref="VerboseArg" /> is created.</example>
[PublicAPI]
public sealed record VerboseArg : IArg;

/// <summary>
///     Represents the argument that specifies a custom project name for the build.
/// </summary>
/// <param name="ProjectName">The name of the project to use for the build.</param>
/// <remarks>
///     If not specified, the build defaults to the project named "_atom".
///     This is triggered by the <c>-p</c> or <c>--project</c> flag.
/// </remarks>
/// <example>
///     For the command <c>atom Build -p MyProject</c>, a <see cref="ProjectArg" /> with `ProjectName = "MyProject"` is
///     created.
/// </example>
[PublicAPI]
public sealed record ProjectArg(string ProjectName) : IArg;

/// <summary>
///     Represents the argument to run in interactive mode, allowing user prompts.
/// </summary>
/// <remarks>
///     This mode allows the build to prompt for input if needed. It is the default behavior unless <c>--headless</c> is
///     specified.
///     This is triggered by the <c>-i</c> or <c>--interactive</c> flag.
/// </remarks>
/// <example>For the command <c>atom Configure --interactive</c>, an <see cref="InteractiveArg" /> is created.</example>
[PublicAPI]
public sealed record InteractiveArg : IArg;
