namespace DecSm.Atom.Tool;

/// <summary>
///     A custom filter for the DecSm.Atom.Tool console application to handle argument parsing.
/// </summary>
/// <remarks>
///     This filter modifies the <see cref="ConsoleAppContext" /> before command invocation,
///     specifically adjusting the <see cref="ConsoleAppContext.EscapeIndex" /> to correctly
///     parse arguments intended for the underlying DecSm.Atom project.
///     It ensures that arguments following the main command (or project flag) are
///     passed directly to the Atom build process.
/// </remarks>
internal class RunArgsFilter(ConsoleAppFilter next) : ConsoleAppFilter(next)
{
    /// <summary>
    ///     Invokes the filter logic.
    /// </summary>
    /// <param name="context">The console app context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public override async Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        // If no arguments are provided or the command is "nuget-add", proceed with default parsing.
        if (context.Arguments.Length is 0 || context.Arguments[0] is "nuget-add")
        {
            await Next.InvokeAsync(context, cancellationToken);

            return;
        }

        // For other commands, especially the root command that runs an Atom project,
        // adjust the EscapeIndex to correctly pass arguments to the Atom project.
        // If the first argument is "-p" or "--project", then the project name is the second argument,
        // and subsequent arguments are for the Atom project. Otherwise, all arguments are for Atom.
        await Next.InvokeAsync(new(context.CommandName,
                context.Arguments,
                ReadOnlyMemory<string>.Empty, // Raw arguments are not directly used by the next layer after this filter
                context.State,
                null,
                context.CommandDepth,
                context.Arguments[0] is "-p" or "--project" or "-f" or "--file" && context.Arguments.Length >= 2
                    ? 2 // Skip "-p" or "--project" or "-f" or "--file" and the project/file name
                    : 0), // All arguments are for the Atom project
            cancellationToken);
    }
}
