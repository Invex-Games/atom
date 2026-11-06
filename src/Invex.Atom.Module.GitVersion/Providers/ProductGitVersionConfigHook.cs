namespace Invex.Atom.Module.GitVersion.Providers;

/// <summary>
///     Keeps the monorepo products' GitVersion configuration files up to date.
/// </summary>
/// <remarks>
///     Like workflow generation, the files are written before the build runs when the <c>Gen</c> target is invoked or
///     the run is not headless. A headless run (CI) instead fails if a file is missing or out of date.
/// </remarks>
internal sealed class ProductGitVersionConfigHook(
    CommandLineArgs args,
    ProductGitVersionConfigGenerator generator,
    ILogger<ProductGitVersionConfigHook> logger
) : IAtomLifecycleHook
{
    /// <summary>
    ///     The name of the target that generates files (<c>IGen.Gen</c> in <c>Invex.Atom.Workflows</c>).
    /// </summary>
    private const string GenTargetName = "Gen";

    /// <inheritdoc />
    public async Task BeforeExecute(CancellationToken cancellationToken)
    {
        var errors = generator.GetProductErrors();
        var generate = args.Commands.Any(x => x.Name is GenTargetName) || !args.HasHeadless;

        if (errors.Count > 0)
        {
            var message =
                $"The products are invalid, so their GitVersion configuration cannot be generated: {string.Join(" ", errors)}";

            // Let a local run continue, so that ValidateBuild can report every problem
            if (!generate)
                throw new AtomException(message);

            logger.LogWarning("{Message}", message);

            return;
        }

        if (generate)
        {
            logger.LogDebug("Generating product GitVersion configuration files");
            await generator.Generate(cancellationToken);

            return;
        }

        var outdated = await generator.GetOutdatedConfigs(cancellationToken);

        if (outdated.Count > 0)
            throw new AtomException(
                $"The GitVersion configuration of one or more products is out of date: '{string.Join("', '", outdated)}'. " +
                "To regenerate it, run the 'Gen' target.");
    }
}
