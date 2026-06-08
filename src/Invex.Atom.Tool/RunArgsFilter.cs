namespace Invex.Atom.Tool;

/// <summary>
///     A custom filter for the Invex.Atom.Tool console application to handle argument parsing.
/// </summary>
/// <remarks>
///     This filter modifies the <see cref="ConsoleAppContext" /> before command invocation,
///     specifically adjusting the <see cref="ConsoleAppContext.EscapeIndex" /> to correctly
///     parse arguments intended for the underlying Invex.Atom project.
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
        // Consume any leading tool-level options (the project/file selectors and the
        // restore-cache flag) so everything after them is forwarded to the Atom project.
        var escapeIndex = 0;
        var arguments = context.Arguments;

        while (escapeIndex < arguments.Length)
        {
            var current = arguments[escapeIndex];

            if (current is "-p" or "--project" or "-f" or "--file")
            {
                // These options take a value, so skip both the option and its value.
                if (escapeIndex + 1 >= arguments.Length)
                    break;

                escapeIndex += 2;
            }
            else if (current is "--no-restore-cache")
            {
                // Boolean flag, skip just the flag itself.
                escapeIndex += 1;
            }
            else
            {
                break;
            }
        }

        await Next.InvokeAsync(new(context.CommandName,
                context.Arguments,
                ReadOnlyMemory<string>.Empty, // Raw arguments are not directly used by the next layer after this filter
                context.State,
                null,
                context.CommandDepth,
                escapeIndex),
            cancellationToken);
    }
}
