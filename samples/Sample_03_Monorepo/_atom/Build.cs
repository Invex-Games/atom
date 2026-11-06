// A monorepo with two independently versioned products, AppA and AppB, that share the code in src/Shared.
//
// Run these from the _atom folder: Atom finds the repository root (here the folder with Sample_03_Monorepo.slnx) from
// the current directory. The project isn't called _atom, so each command names it with --project.
//
// To regenerate the workflows in .github/workflows and the products' GitVersion.yml files:
// dotnet run -- gen --project Sample_03_Monorepo
//
// The sample sits in a subfolder of the Atom repository, so the generated GitVersion.yml files anchor their ignored
// paths below samples/Sample_03_Monorepo, where GitVersion sees them. In a real monorepo the Atom root is the
// repository root. GitHub only reads workflows from the repository root, so the sample's workflows never run, and their
// path filters are relative to the sample folder, not the repository root.
//
// To see which products a branch affects compared with main:
// GITHUB_BASE_REF=main dotnet run -- DetectAffectedProducts --project Sample_03_Monorepo
//
// To pack one product with its own name, version and release tag:
// dotnet run -- PackAppA --product AppA --project Sample_03_Monorepo

// These usings are automatically globally included when using Invex.Atom from a nuget package

using Invex.Atom.Build;
using Invex.Atom.Build.BuildInfo;
using Invex.Atom.Build.BuildOptions;
using Invex.Atom.Build.Definition;
using Invex.Atom.Build.Hosting;
using Invex.Atom.Module.GitHubWorkflows.Helpers;
using Invex.Atom.Module.GitHubWorkflows.Extensions;
using Invex.Atom.Module.GitVersion;
using Invex.Atom.Module.GitVersion.Options;
using Invex.Atom.Workflows;
using Invex.Atom.Workflows.Definition;
using Invex.Atom.Workflows.Definition.Triggers;

namespace Atom;

/// <summary>
///     The monorepo build: it declares the products and generates one validation workflow for all of them, plus one
///     build workflow per app.
/// </summary>
[BuildDefinition]
[GenerateEntryPoint]
internal interface IBuild : IWorkflowBuildDefinition, IGitHubWorkflows, IGitVersion, IProductTargets
{
    IReadOnlyList<IBuildOption> IBuildDefinition.Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId,
        BuildOptions.GitVersion.ProvideBuildVersion,
    ];

    IReadOnlyList<Product> IProductsDefinition.Products =>
    [
        // A path group: it has no pipelines of its own, but a change to it affects the products that depend on it
        new("Shared")
        {
            Paths = ["src/Shared/"],
        },
        new("AppA")
        {
            Paths = ["src/AppA/"],
            DependsOn = ["Shared"],
            GitVersionConfig = "src/AppA/GitVersion.yml",
        },
        new("AppB")
        {
            Paths = ["src/AppB/"],
            DependsOn = ["Shared"],
            GitVersionConfig = "src/AppB/GitVersion.yml",
        },
    ];

    IReadOnlyList<WorkflowDefinition> IWorkflowBuildDefinition.Workflows =>
    [
        // Pull requests run only the affected products' tests; require only ValidateGate in branch protection
        WorkflowPresets.ProductValidate(this,
            nameof(ValidateGate),
            [
                new("AppA", nameof(TestAppA)),
                new("AppB", nameof(TestAppB)),
            ]) with
        {
            Types = [WorkflowTypes.GitHub.Action],
        },

        // Each app builds when its paths (or Shared, or a global path) change on main, and when one of its own
        // releases (tagged appa/v1.2.0 and so on) is published
        ProductBuild("AppA", nameof(PackAppA)),
        ProductBuild("AppB", nameof(PackAppB)),
    ];

    private WorkflowDefinition ProductBuild(string product, string target) =>
        WorkflowDefinition
            .ForProduct(this, product, "Build")
            .WithTriggers(WorkflowTriggers.Manual, WorkflowTriggers.GitHub.OnRelease()) with
            {
                Targets = [new(target)],
                Types = [WorkflowTypes.GitHub.Action],
            };
}
