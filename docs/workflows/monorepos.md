# Monorepos

This page shows how to build several independently versioned products from one repository with Atom: one always-running
validation workflow whose product jobs run only when their product changed, one GitVersion configuration per product,
and per-product release tags. Everything here is written by hand with the building blocks Atom has today.

The examples use a repository with three folders: `src/AppA/` (a web API), `src/AppB/` (a mobile app) and `src/Shared/`
(code used by both).

## When to use it

Use this pattern when one repository holds several products that ship on their own schedule and should have their own
version numbers and tags, for example `appa/v1.4.0` and `appb/v2.0.1`. Pull requests should only build and test the
products they touch, while branch protection still has a single check to require.

If everything in the repository is released together under one version, you do not need any of this: one workflow, the
default `GitVersion.yml` and `v1.2.0` tags are simpler.

> [!NOTE]
> A product model that generates most of this page's code (product paths, the affected-products target, the gate, the
> per-product GitVersion configuration and per-product pipelines) is planned but not available yet. Until then, the
> pattern below is written by hand.

## The validation workflow with a gate

GitHub branch protection needs a fixed list of required checks, but product jobs that only run when their product
changed are skipped on most pull requests. (An Azure DevOps build validation policy requires a whole pipeline run
instead, see [below](#the-workflow).) The pattern that reconciles the two is:

1. **One always-running Validate workflow.** It has no path filters, so it runs for every pull request.
2. **`DetectAffectedProducts`.** The first job compares the pull request with its base branch and publishes the
   affected products as a variable, for example `,appa,shared,`.
3. **Per-product targets.** `PackAppA`, `TestAppA`, `PackAppB` and so on are separate targets. Each one runs only when
   its product is in the list.
4. **`ValidateGate`.** One job that depends on every product job and runs only if one of them failed or the run was
   cancelled. It is the only required check.

On GitHub the gate is **skipped** when every product job succeeded or was skipped, and GitHub treats a skipped required
check as passed. It **runs and fails** when any product job failed or the run was cancelled. Branch protection
therefore only needs to require `ValidateGate`.

### The targets

The affected products are a [variable](variables-in-workflows.md), so they need a backing parameter. The list is
comma-wrapped so that a condition can match `,appa,` without also matching a product called `appa2`:

```csharp
internal interface IProductTargets : IBuildInfo, IVariablesHelper, IReleaseTagHelper
{
    static string[] SharedPaths => ["src/Shared/"];
    static string[] AppAPaths => ["src/AppA/", .. SharedPaths];
    static string[] AppBPaths => ["src/AppB/", .. SharedPaths];
    static string[] GlobalPaths =>
    [
        "global.json", "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props",
        "Directory.Packages.targets", "_atom/", ".github/workflows/",
    ];

    [ParamDefinition("affected-products", "Comma-wrapped list of affected products, e.g. ',appa,shared,'")]
    string? AffectedProducts => GetParam(() => AffectedProducts);

    Target DetectAffectedProducts =>
        t => t
            .DescribedAs("Works out which products the change affects")
            .ProducesVariable(nameof(AffectedProducts))
            .Executes(async cancellationToken =>
            {
                // Empty for pushes, manual runs and local runs: treat everything as affected
                var baseRef = GitHub.Variables.BaseRef;

                if (baseRef.Length is 0)
                {
                    await WriteVariable(nameof(AffectedProducts), ",appa,appb,shared,", cancellationToken);

                    return;
                }

                // --no-renames lists both sides of a move (a file moved out of AppA still affects AppA), and -z
                // writes raw, NUL-separated names, so Git doesn't quote unusual file names. The process runner
                // reads output line by line, so also split on the line break it appends.
                var diff = await ProcessRunner.RunAsync(
                    new("git", $"diff --name-only --no-renames -z origin/{baseRef}...HEAD")
                    {
                        WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                        OutputLogLevel = LogLevel.Debug,
                    },
                    cancellationToken);

                var changedFiles = diff.Output.Split(['\0', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

                bool Touches(string[] paths) =>
                    changedFiles.Any(file => paths
                        .Concat(GlobalPaths)
                        .Any(path => file.StartsWith(path, StringComparison.Ordinal)));

                var affected = new List<string>();

                if (Touches(AppAPaths))
                    affected.Add("appa");

                if (Touches(AppBPaths))
                    affected.Add("appb");

                if (Touches(SharedPaths))
                    affected.Add("shared");

                await WriteVariable(nameof(AffectedProducts), $",{string.Join(',', affected)},", cancellationToken);
            });

    Target PackAppA =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts)) // must be a direct dependency, see below
            .Executes(() => { /* pack AppA */ });

    Target PackAppB =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts))
            .Executes(() => { /* pack AppB */ });

    Target ValidateGate =>
        t => t
            .DescribedAs("Fails if any product job failed or was cancelled")
            .DependsOn(nameof(PackAppA))
            .DependsOn(nameof(PackAppB))
            .Executes(() =>
            {
                // In CI this job only runs after a failure. Locally, the dependencies have already succeeded.
                if (GetService<IWorkflowContext>().WorkflowType is not null)
                    throw new InvalidOperationException("A product job failed or was cancelled.");
            });
}
```

On Azure DevOps, read the base branch from `DevOps.Variables.SystemPullRequestTargetBranchName` instead of
`GitHub.Variables.BaseRef`.

`DependsOn` takes one target name, so chain one call per dependency. List each product target the gate stands for
(`TestAppA`, `TestAppB` and so on in a real build).

### The workflow

`BuildOptions.Target.RunIfWorkflowCondition` gates each product job on the variable, and
`BuildOptions.Target.RunIfAnyDependencyFailed` makes the gate run only after a failure or a cancelled run. Reference
the variable with `Tex.ParamOutput`, using the C# names of the producing target and the parameter:

```csharp
[BuildDefinition]
[GenerateEntryPoint]
internal partial class Build : WorkflowBuildDefinition, IGitHubWorkflows, IGitVersion, IProductTargets
{
    public IReadOnlyList<IBuildOption> Options =>
    [
        BuildOptions.GitVersion.ProvideBuildId,
        BuildOptions.GitVersion.ProvideBuildVersion,
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Validate")
        {
            // No path filters, so the workflow (and the gate) runs for every pull request
            Triggers = [WorkflowTriggers.PullIntoMain, WorkflowTriggers.Manual],
            Targets =
            [
                new(nameof(IProductTargets.DetectAffectedProducts)),
                new(nameof(IProductTargets.PackAppA)) { Options = [AffectedIf("appa"), AppAConfig] },
                new(nameof(IProductTargets.PackAppB)) { Options = [AffectedIf("appb"), AppBConfig] },
                new(nameof(IProductTargets.ValidateGate)) { Options = [BuildOptions.Target.RunIfAnyDependencyFailed] },
            ],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];

    private static WorkflowParamInjection AppAConfig =>
        BuildOptions.Inject.Param(nameof(IGitVersion.GitVersionConfig), "src/AppA/GitVersion.yml");

    private static WorkflowParamInjection AppBConfig =>
        BuildOptions.Inject.Param(nameof(IGitVersion.GitVersionConfig), "src/AppB/GitVersion.yml");

    private TargetCondition AffectedIf(string product) =>
        BuildOptions.Target.RunIfWorkflowCondition(Tex
            .ParamOutput(this, nameof(IProductTargets.DetectAffectedProducts), nameof(IProductTargets.AffectedProducts))
            .Contains($",{product},"));
}
```

`Tex.ParamOutput` is in `Invex.Atom.Workflows.Extensions`, and `Tex` itself in `Invex.StructuredText.Expressions`.

On GitHub Actions this generates, among others:

```yaml
  PackAppA:
    needs: [ DetectAffectedProducts ]
    if: contains(needs.DetectAffectedProducts.outputs.affected-products, ',appa,')

  ValidateGate:
    needs: [ PackAppA, PackAppB ]
    if: failure() || cancelled()
```

On Azure DevOps a custom condition **replaces** the default `succeeded()` condition, so add `Tex.Success()` to the
product conditions there, or a product job would still run after an earlier job failed:

```csharp
private TargetCondition AffectedIf(string product) =>
    BuildOptions.Target.RunIfWorkflowCondition(Tex.Success() &&
        Tex
            .ParamOutput(this, nameof(IProductTargets.DetectAffectedProducts), nameof(IProductTargets.AffectedProducts))
            .Contains($",{product},"));
```

The gate renders as `or(failed(), canceled())` on Azure DevOps. A branch policy there already fails when any job fails,
so the gate matters less than on GitHub, but it keeps the two platforms' workflows alike.

`BuildOptions.Target.RunAlways` (rendered as `always()`) is the counterpart for jobs that must run whatever their
dependencies did, such as reporting. Several `TargetCondition` options on one target are combined with AND, so add
`RunIfAnyDependencyFailed` or `RunAlways` on its own rather than next to another condition.

## Per-product versions with GitVersion

Each product gets its own GitVersion configuration file, selected with the
[`gitversion-config` parameter](../modules/git-version.md#choosing-a-configuration-file) of the GitVersion module.
In the workflow above, `AppAConfig` injects it into AppA's targets, so `BuildVersion` and `BuildId` in those jobs come
from AppA's configuration (unless the target consumes them as variables from `SetupBuildInfo`, see the gotchas below).
The project needs `BuildOptions.GitVersion.ProvideBuildId` and `ProvideBuildVersion`, as in the example.

A product's configuration does two things that the repository-wide one does not:

- **`tag-prefix`** makes GitVersion read only the product's tags, for example `appa/v1.2.0`.
- **`ignore.paths`** makes GitVersion skip commits that did not touch the product, so a change to AppB does not bump
  AppA's version.

```yaml
# src/AppA/GitVersion.yml
tag-prefix: 'appa/v'

ignore:
  paths:
    # Ignore every commit whose changed files are all outside AppA, Shared and the repository-wide build files
    - '^(?!(src/AppA/|src/Shared/|_atom/|\.github/workflows/|global\.json$|Directory\.(Build|Packages)\.(props|targets)$)).*'

# Copy the rest of the repository's GitVersion.yml here (mode, bump messages, branches and so on):
# a file passed with /config replaces GitVersion's own lookup, it is not merged with the root GitVersion.yml.
```

The single anchored negative lookahead matches every path that is **not** under one of the product's paths. The paths
are relative to the repository root and have **no leading slash**. GitVersion's older documentation shows paths with a
leading slash; in this regex that would never recognise the product's own files, so every commit would be ignored.

Keep the paths in the regex, the paths `DetectAffectedProducts` checks and the paths you pass to
`GetReleaseTagCommit` (below) the same.

`ignore.paths` needs GitVersion 6.4.0 or later, and path matching became case-insensitive in 6.8.0. Atom requires
`GitVersion.Tool` 6.8.0 or later and installs the latest 6.x release when no suitable version is installed, so CI
does not silently pick up a later major version with a different configuration format.

To compute a product's version locally:

```shell
atom PackAppA --gitversion-config src/AppA/GitVersion.yml
```

## Release tags

Product tags use the product's prefix: `appa/v1.2.0`, `appb/v2.0.1`. In a targets interface derived from
<xref:Invex.Atom.Build.BuildInfo.IReleaseTagHelper>, `ReleaseTagFor`, `TryParseReleaseTag` and `ReleaseTagGlob` use
the build's release tag prefix, `v`. For a product's tags, the <xref:Invex.Atom.Build.BuildInfo.ReleaseTag> helpers in
`Invex.Atom.Build` take the prefix explicitly:

```csharp
ReleaseTagFor(BuildVersion);                             // "v1.2.0"
ReleaseTag.For(BuildVersion, "appa/v");                  // "appa/v1.2.0"
ReleaseTag.TryParse("appa/v1.2.0", "appa/v", out var v); // true, v is 1.2.0
ReleaseTag.TryParse("appa/v1.2.0", "v", out _);          // false: the prefix does not match
ReleaseTag.Glob("appa/v");                               // "appa/v*", for git tag --list
```

`ReleaseTag.Glob` is broader than a release tag (`v*` also matches `vnext`), so filter listed tags with
`ReleaseTag.TryParse` using the same prefix.

### Tag the latest commit that touched the product

GitVersion ignores a tag that sits on a commit excluded by `ignore.paths`. If `appa/v1.2.0` is put on a commit that only
touched AppB, AppA cannot see the tag and keeps versioning from an older one. **Put a product's tag on the latest commit
that touched the product's paths**, not on `HEAD`.

<xref:Invex.Atom.Build.BuildInfo.IReleaseTagHelper> finds that commit. Derive your targets interface from it and call
`GetReleaseTagCommit` with the same paths as the product's `ignore.paths` regex:

```csharp
Target TagAppA =>
    t => t
        .DescribedAs("Tags the AppA release")
        .Executes(async cancellationToken =>
        {
            var commit = await GetReleaseTagCommit([.. AppAPaths, .. GlobalPaths], cancellationToken);
            var tag = ReleaseTag.For(BuildVersion, "appa/v");

            await ProcessRunner.RunAsync(new("git", $"tag {tag} {commit}")
                {
                    WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                },
                cancellationToken);

            await ProcessRunner.RunAsync(new("git", $"push origin {tag}")
                {
                    WorkingDirectory = RootedFileSystem.AtomRootDirectory,
                },
                cancellationToken);
        });
```

When you create a GitHub release instead, set its target commit to the returned hash.

`GetReleaseTagCommit` runs `git log -1 --format=%H -- <paths>`, or returns `HEAD` when no paths are given. It needs the
full history: in a shallow clone the oldest fetched commit appears to touch every file, so it fails instead of returning
a wrong commit. Atom's generated GitHub checkout steps fetch the full history (`fetch-depth: 0`) by default.

## Gotchas

- **Conditions don't add dependencies.** Every product target must `DependsOn(nameof(DetectAffectedProducts))`
  **directly**. A condition that references the variable does not add the dependency, so without it the job has no
  `needs:` on the producing job, the output is empty and the job is always skipped. Atom only logs a warning.
- **`Inject.Param` key.** Use the C# property name (`nameof(IGitVersion.GitVersionConfig)`), not the command-line name
  (`gitversion-config`). A key that does not match is dropped with only a warning when the workflow is generated.
- **Azure DevOps conditions.** A custom condition replaces `succeeded()`, so add `Tex.Success()` to product conditions
  on Azure DevOps.
- **Auto-added dependency jobs.** A dependency that is not listed in the workflow's `Targets` is still added as a job,
  but without its target options (so without its condition) and with no `needs:`. List every product target in the
  workflow explicitly.
- **One version per workflow.** `SetupBuildInfo` produces one version for the whole workflow. In the combined Validate
  workflow, inject `gitversion-config` per target instead, or add a per-product setup target (for example
  `SetupBuildInfoAppA`) that writes the product's version to its own variable. `StoreArtifact` and `RetrieveArtifact`
  still use the workflow-wide `BuildId`, which is fine for validation.
- **`ignore.paths` semantics.** A commit is ignored only if **all** of its changed paths (compared with its first
  parent) match. Paths have no leading slash. Ignored commits can still change the build metadata part of `FullSemVer`.
- **Tagging.** Tag `appa/vX` on the latest commit that touched AppA's paths (`GetReleaseTagCommit`), or GitVersion
  cannot see the tag.
- **Local runs.** `atom PackAppA` runs `DetectAffectedProducts` first, so it must succeed with no base branch (treat
  everything as affected). Make the gate a no-op outside CI. One process computes one product's version, so build one
  product per run.

## Migration for an existing repo

1. **Baseline tags.** For each product, create a tag `appa/v<current version>` on the latest commit that touched the
   product's paths (use `GetReleaseTagCommit`, or `git log -1 --format=%H -- <paths>`). Existing `v*` tags no longer
   match the product's `tag-prefix`, so without a baseline the product's version starts again from the beginning.
2. **Product configurations.** Add a GitVersion configuration per product, such as `src/AppA/GitVersion.yml`, with its
   `tag-prefix` and `ignore.paths`, and inject it into the product's targets with `gitversion-config`.
3. **Validate workflow.** Add `DetectAffectedProducts`, the per-product targets and `ValidateGate`, then run `atom gen`.
4. **Branch protection.** Require only the `ValidateGate` check, and remove the per-product checks.

## See also

- [GitVersion](../modules/git-version.md): the `gitversion-config` parameter and caching.
- [Workflow Options](workflow-options.md): conditions, injections and other per-target options.
- [Variables in Workflows](variables-in-workflows.md): passing `AffectedProducts` between jobs.
- [Triggers](triggers.md): pull request, push and manual triggers.
- <xref:Invex.Atom.Build.BuildInfo.ReleaseTag>
- <xref:Invex.Atom.Build.BuildInfo.IReleaseTagHelper>
- <xref:Invex.Atom.Module.GitVersion.IGitVersion>
- <xref:Invex.Atom.Workflows.Options.TargetCondition>
