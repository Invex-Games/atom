using Invex.Atom.Build;
using Invex.Atom.Build.BuildInfo;
using Invex.Atom.Build.Definition;
using Invex.Atom.Workflows.WorkflowContext;
using Microsoft.Extensions.Logging;

namespace Atom;

/// <summary>
///     The per-product targets. Targets are fixed when the build compiles, so each product has its own.
/// </summary>
internal interface IProductTargets : IProductsDefinition, IBuildInfo, IReleaseTagHelper
{
    // Product targets in the validation workflow must depend directly on DetectAffectedProducts, or their jobs can't
    // read the affected products; atom gen checks this
    Target TestAppA =>
        t => t
            .DescribedAs("Tests AppA")
            .DependsOn(nameof(DetectAffectedProducts))
            .Executes(() => Logger.LogInformation("Testing {Product} {Version}", BuildName, BuildVersion));

    Target TestAppB =>
        t => t
            .DescribedAs("Tests AppB")
            .DependsOn(nameof(DetectAffectedProducts))
            .Executes(() => Logger.LogInformation("Testing {Product} {Version}", BuildName, BuildVersion));

    Target PackAppA =>
        t => t
            .DescribedAs("Packs AppA")
            .Executes(PackCurrentProduct);

    Target PackAppB =>
        t => t
            .DescribedAs("Packs AppB")
            .Executes(PackCurrentProduct);

    // The gate depends on every product target, and the workflow only runs it when one of them failed or the run was
    // cancelled. Locally its dependencies have already succeeded, so it does nothing.
    Target ValidateGate =>
        t => t
            .DescribedAs("Fails if any product job failed or was cancelled")
            .DependsOn(nameof(TestAppA))
            .DependsOn(nameof(TestAppB))
            .Executes(() =>
            {
                if (GetService<IWorkflowContext>().WorkflowType is not null)
                    throw new InvalidOperationException("A product job failed or was cancelled.");
            });

    private async Task PackCurrentProduct(CancellationToken cancellationToken)
    {
        // With --product AppA (which the AppA workflow injects), the name, version and tag are AppA's
        var commit = await GetReleaseTagCommit(cancellationToken);

        Logger.LogInformation("Packing {Product} {Version}; its release tag {Tag} goes on {Commit}",
            BuildName,
            BuildVersion,
            ReleaseTagFor(BuildVersion),
            commit);
    }
}
