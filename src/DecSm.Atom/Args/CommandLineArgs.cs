namespace DecSm.Atom.Args;

/// <summary>
///     Contains the parsed command-line arguments for an Atom build execution, providing a structured representation of
///     user input.
/// </summary>
/// <param name="IsValid">
///     A value indicating whether the command-line arguments were parsed successfully and are valid. If
///     false, the Atom application will typically terminate or display help.
/// </param>
/// <param name="Args">
///     A read-only list of <see cref="IArg" /> objects representing the individual arguments parsed from the command
///     line. This list provides access to raw argument objects like <see cref="HelpArg" />, <see cref="CommandArg" />, or
///     <see cref="ParamArg" />.
/// </param>
/// <remarks>
///     An instance of this record is created by the <see cref="CommandLineArgsParser" /> after parsing the raw string
///     arguments provided to the Atom application.
/// </remarks>
[PublicAPI]
public sealed record CommandLineArgs(bool IsValid, IReadOnlyList<IArg> Args)
{
    /// <summary>
    ///     Gets a list of validation errors for the current command-line arguments.
    /// </summary>
    /// <returns>A read-only list of error messages. An empty list indicates no validation errors.</returns>
    public IReadOnlyList<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (!IsValid)
            errors.Add("One or more arguments could not be parsed");

        errors.AddRange(Commands
            .Where(command => string.IsNullOrWhiteSpace(command.Name))
            .Select(_ => "Target name cannot be empty"));

        errors.AddRange(Params
            .Where(param => string.IsNullOrWhiteSpace(param.ParamName))
            .Select(param => $"Parameter name cannot be empty for value '{param.ParamValue}'"));

        return errors;
    }

    /// <summary>
    ///     Gets a value indicating whether the help argument (<c>-h</c> or <c>--help</c>) was provided.
    /// </summary>
    /// <value>true if the help argument is present; otherwise, false.</value>
    /// <remarks>
    ///     When this property is true, the Atom application should display help information and exit.
    ///     Running <c>atom --help</c> or <c>atom -h</c> will result in this property being true.
    /// </remarks>
    /// <seealso cref="HelpArg" />
    /// <seealso cref="DecSm.Atom.Help.IHelpService" />
    public bool HasHelp => Args.Any(arg => arg is HelpArg);

    /// <summary>
    ///     Gets a value indicating whether the generate argument (<c>-g</c> or <c>--gen</c>) was provided.
    /// </summary>
    /// <value>true if the generate argument is present; otherwise, false.</value>
    /// <remarks>
    ///     This flag instructs the Atom framework to (re)generate workflow files (e.g., for CI/CD systems).
    ///     Running <c>atom --gen</c> or <c>atom -g</c> will result in this property being true.
    /// </remarks>
    /// <seealso cref="GenArg" />
    /// <seealso cref="DecSm.Atom.Workflows.WorkflowGenerator" />
    public bool HasGen => Args.Any(arg => arg is GenArg);

    /// <summary>
    ///     Gets a value indicating whether the skip argument (<c>-s</c> or <c>--skip</c>) was provided.
    /// </summary>
    /// <value>true if the skip argument is present; otherwise, false.</value>
    /// <remarks>
    ///     This flag instructs the build executor to skip the execution of dependent targets.
    ///     Running <c>atom MyTarget --skip</c> or <c>atom MyTarget -s</c> will result in this property being true.
    /// </remarks>
    /// <seealso cref="SkipArg" />
    /// <seealso cref="DecSm.Atom.Build.BuildExecutor" />
    public bool HasSkip => Args.Any(arg => arg is SkipArg);

    /// <summary>
    ///     Gets a value indicating whether the headless argument (<c>-hl</c> or <c>--headless</c>) was provided.
    /// </summary>
    /// <value>true if the headless argument is present; otherwise, false.</value>
    /// <remarks>
    ///     This mode is used for non-interactive environments like CI/CD pipelines, where user prompts should be disabled.
    ///     Running <c>atom MyCommand --headless</c> or <c>atom MyCommand -hl</c> will result in this property being true.
    /// </remarks>
    /// <seealso cref="HeadlessArg" />
    public bool HasHeadless => Args.Any(arg => arg is HeadlessArg);

    /// <summary>
    ///     Gets a value indicating whether the verbose argument (<c>-v</c> or <c>--verbose</c>) was provided.
    /// </summary>
    /// <value>true if the verbose argument is present; otherwise, false.</value>
    /// <remarks>
    ///     When true, the Atom application should output more detailed logging information for debugging or diagnostics.
    ///     Running <c>atom MyCommand --verbose</c> or <c>atom MyCommand -v</c> will result in this property being true.
    /// </remarks>
    /// <seealso cref="VerboseArg" />
    /// <seealso cref="DecSm.Atom.Logging.LogOptions.IsVerboseEnabled" />
    public bool HasVerbose => Args.Any(arg => arg is VerboseArg);

    /// <summary>
    ///     Gets a value indicating whether the project argument (<c>-p</c> or <c>--project</c>) was provided.
    /// </summary>
    /// <value>true if the project argument is present; otherwise, false.</value>
    /// <remarks>
    ///     This argument allows specifying a custom Atom project directory, overriding the default.
    ///     Running <c>atom -p MyAtomProject</c> will result in this property being true.
    /// </remarks>
    /// <seealso cref="ProjectArg" />
    /// <seealso cref="ProjectName" />
    public bool HasProject => Args.Any(arg => arg is ProjectArg);

    /// <summary>
    ///     Gets a value indicating whether the interactive argument (<c>-i</c> or <c>--interactive</c>) was provided.
    /// </summary>
    /// <value>true if the interactive argument is present; otherwise, false.</value>
    /// <remarks>
    ///     This flag suggests that the Atom application may prompt the user for input if needed for certain parameters.
    ///     Running <c>atom MyCommand -i</c> will result in this property being true.
    /// </remarks>
    /// <seealso cref="InteractiveArg" />
    /// <seealso cref="DecSm.Atom.Params.ParamService" />
    public bool HasInteractive => Args.Any(arg => arg is InteractiveArg);

    /// <summary>
    ///     Gets a read-only list of parameter arguments (<see cref="ParamArg" />) from the command line.
    /// </summary>
    /// <value>A read-only list of <see cref="ParamArg" /> objects.</value>
    /// <remarks>
    ///     Each <see cref="ParamArg" /> represents a key-value pair (e.g., <c>--my-param value</c>).
    ///     For <c>atom --param1 value1</c>, this list would contain one <see cref="ParamArg" /> object.
    /// </remarks>
    /// <seealso cref="ParamArg" />
    public IReadOnlyList<ParamArg> Params =>
        Args
            .OfType<ParamArg>()
            .ToList();

    /// <summary>
    ///     Gets a read-only list of command arguments (<see cref="CommandArg" />) from the command line.
    /// </summary>
    /// <value>A read-only list of <see cref="CommandArg" /> objects.</value>
    /// <remarks>
    ///     Each <see cref="CommandArg" /> represents a target to be executed.
    ///     For <c>atom Clean Build</c>, this list would contain "Clean" and "Build".
    /// </remarks>
    /// <seealso cref="CommandArg" />
    public IReadOnlyList<CommandArg> Commands =>
        Args
            .OfType<CommandArg>()
            .ToList();

    /// <summary>
    ///     Gets the project name specified by the project argument (<c>-p</c> or <c>--project</c>).
    /// </summary>
    /// <value>The specified project name, or "_atom" if not provided.</value>
    /// <remarks>
    ///     If the project argument is not provided, this defaults to "_atom".
    ///     The Atom project typically resides in a directory with this name.
    ///     If <c>atom -p MyBuildScripts</c> is run, this property will be "MyBuildScripts".
    /// </remarks>
    /// <seealso cref="ProjectArg" />
    /// <seealso cref="HasProject" />
    public string ProjectName =>
        Args
            .OfType<ProjectArg>()
            .Select(arg => arg.ProjectName)
            .FirstOrDefault() ??
        "_atom";
}
