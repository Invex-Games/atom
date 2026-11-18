# Monorepos

This page shows how to build several independently versioned products from one repository with Atom: one always-running
validation workflow whose product jobs run only when their product changed, one GitVersion configuration per product,
per-product release tags and, optionally, per-product build and release pipelines.

The examples use a repository with three folders: `src/AppA/` (a web API), `src/AppB/` (a mobile app) and `src/Shared/`
(code used by both). [`samples/Sample_03_Monorepo`](https://github.com/Invex-Games/atom/tree/main/samples/Sample_03_Monorepo)
is a complete build with this layout, including the files `atom gen` generates.

## When to use it

Use this pattern when one repository holds several products that ship on their own schedule and should have their own
version numbers and tags, for example `appa/v1.4.0` and `appb/v2.0.1`. Pull requests should only build and test the
products they touch, while branch protection still has a single check to require.

If everything in the repository is released together under one version, you do not need any of this: one workflow, the
default `GitVersion.yml` and `v1.2.0` tags are simpler.

## Declare the products

Implement <xref:Invex.Atom.Build.IProductsDefinition> on the build and list the products:

```csharp
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
        // A path group: no pipelines of its own, but a change to it affects the products that depend on it
        new("Shared") { Paths = ["src/Shared/"] },
        new("AppA")
        {
            Paths = ["src/AppA/"],
            DependsOn = ["Shared"],
            GitVersionConfig = RootedFileSystem.AtomRootDirectory / "src/AppA/GitVersion.yml",
        },
        new("AppB")
        {
            Paths = ["src/AppB/"],
            DependsOn = ["Shared"],
            GitVersionConfig = RootedFileSystem.AtomRootDirectory / "src/AppB/GitVersion.yml",
        },
    ];
}
```

A <xref:Invex.Atom.Build.BuildInfo.Product> has:

| Property           | Meaning                                                                                                            |
|--------------------|--------------------------------------------------------------------------------------------------------------------|
| `Name`             | The product's name, such as `AppA`. Letters, digits, `.`, `-` and `_`.                                             |
| `Id`               | The lowercase name, such as `appa`, used in the affected-products list and the default tag prefix.                 |
| `Paths`            | The product's own paths, as globs relative to the Atom root (normally the repository root).                        |
| `DependsOn`        | Products whose changes also affect this one, followed transitively.                                                |
| `Directory`        | Optional. The product's folder, a `RootedPath` such as `RootedFileSystem.AtomRootDirectory / "src/AppA"`, whose `Directory.Build.props` the default version provider reads first. |
| `GitVersionConfig` | The product's GitVersion configuration file, a `RootedPath` below the Atom root, which `atom gen` generates (see [below](#per-product-versions)). Needed for every product built with GitVersion. |
| `TagPrefix`        | The release tag prefix, `<id>/v` by default (for example `appa/v`).                                                |

**Paths** use a small glob dialect that GitHub, Azure DevOps, git and GitVersion all understand: `**` as a whole path
segment matches any number of folders, and `*` matches any characters except `/`. A path ending in `/` means everything
below that folder, so `src/AppA/` is the same as `src/AppA/**`. A path with no wildcard and no trailing `/` means one
file, such as `global.json`. Use `/` as the separator, with no leading `/` or `./` and no `.` or `..` segments.
Matching is case-sensitive, except in GitVersion's `ignore.paths`, which ignores case from GitVersion 6.8, so avoid
paths that differ only in case.

**`Directory` and `GitVersionConfig`** are locations on disk, not globs, so they are `RootedPath` values built from the
Atom root directory. Declare the products in a property getter, as above, because the file system is not available in
an initializer. An interface build uses `RootedFileSystem.AtomRootDirectory`; a build class uses its generated
`FileSystem` property:

```csharp
[BuildDefinition]
internal partial class Build : BuildDefinition, IProductsDefinition
{
    IReadOnlyList<Product> IProductsDefinition.Products =>
    [
        new("AppA")
        {
            Paths = ["src/AppA/"],
            Directory = FileSystem.AtomRootDirectory / "src/AppA",
            GitVersionConfig = FileSystem.AtomRootDirectory / "src/AppA/GitVersion.yml",
        },
    ];
}
```

The platforms disagree on the rest of glob syntax, so it is rejected: `?` and `+` (GitHub treats them as quantifiers of
the preceding character, so `src/C++/` would not match), `[...]`, `{...}` and a leading `!`. They also disagree on a
folder written without a trailing `/`, such as `src/AppA`: git and Azure DevOps match everything below it, but GitHub
and GitVersion only match the path itself, so `ValidateBuild` reports a path like that when the folder exists. A git
submodule or a symbolic link is a single entry to git, so its path is written without a trailing `/`.

**Global paths** affect every product. By default they are `global.json`, the root `Directory.Build.*` and
`Directory.Packages.*` files, `_atom/**`, `.github/workflows/**` and `.devops/workflows/**`. Override
`IProductsDefinition.GlobalPaths` to change them, for example when the build project is not in `_atom`:

```csharp
IReadOnlyList<string> IProductsDefinition.GlobalPaths => [.. IProductsDefinition.DefaultGlobalPaths, "build/**"];
```

[`ValidateBuild`](../built-in-targets/validate-build.md) checks the declarations: names, empty or unsupported paths,
folders without a trailing `/`, unknown or cyclic dependencies, tag prefixes that equal or start another product's
(compared ignoring case), GitVersion configurations shared by two products, and a product GitVersion configuration that
is not below the Atom root directory or is the root `GitVersion.yml`.

### The current product

The `product` parameter (`IProductsDefinition.ProductName`) selects the product being built. The generated product jobs
set it, and locally you pass it yourself:

```shell
atom PackAppA --product AppA
```

When a product is selected:

- `BuildName` defaults to the product's name.
- The GitVersion module uses the product's `GitVersionConfig` (unless `--gitversion-config` is given).
- Without GitVersion, the default version provider reads the `Directory.Build.props` in the product's `Directory` before
  the root one.
- `ReleaseTagFor`, `TryParseReleaseTag` and `ReleaseTagGlob` use the product's tag prefix, and `GetReleaseTagCommit()`
  finds the commit for the product's tag (see [Release tags](#release-tags)).

Without the parameter, everything behaves exactly as in a build without products. Build code can read
`CurrentProduct`, and services can inject <xref:Invex.Atom.Build.BuildInfo.IProductContext>.

Like any parameter, `product` can also come from an environment variable named `ProductName`, `product` or `PRODUCT`
(on Windows, in any case). Check that your CI environment does not set one for another purpose. In an interactive run
(`-i`), the prompt for `product` accepts an empty answer, which selects no product.

## The validation workflow with a gate

GitHub branch protection needs a fixed list of required checks, but product jobs that only run when their product
changed are skipped on most pull requests. (An Azure DevOps build validation policy requires a whole pipeline run
instead.) The pattern that reconciles the two is:

1. **One always-running Validate workflow.** It has no path filters, so it runs for every pull request.
2. **[`DetectAffectedProducts`](../built-in-targets/detect-affected-products.md).** The first job compares the pull
   request with its base branch and publishes the affected products as a variable, for example `,appa,shared,`.
3. **Per-product targets.** `TestAppA`, `TestAppB` and so on are separate targets. Each one runs only when its product
   is in the list.
4. **A gate.** One job that depends on every product job and runs only if one of them failed or the run was cancelled.
   It is the only required check.

On GitHub the gate is **skipped** when every product job succeeded or was skipped, and GitHub treats a skipped required
check as passed. It **runs and fails** when any product job failed or the run was cancelled.

### The targets

Targets are fixed when the build compiles, so each product has its own. Each product target must depend **directly** on
`DetectAffectedProducts`, and the gate on every product target:

```csharp
internal interface IProductTargets : IProductsDefinition, IBuildInfo, IReleaseTagHelper
{
    Target TestAppA =>
        t => t
            .DescribedAs("Tests AppA")
            .DependsOn(nameof(DetectAffectedProducts))
            .Executes(() => { /* test AppA */ });

    Target TestAppB =>
        t => t
            .DescribedAs("Tests AppB")
            .DependsOn(nameof(DetectAffectedProducts))
            .Executes(() => { /* test AppB */ });

    Target ValidateGate =>
        t => t
            .DescribedAs("Fails if any product job failed or was cancelled")
            .DependsOn(nameof(TestAppA))
            .DependsOn(nameof(TestAppB))
            .Executes(() =>
            {
                // In CI this job only runs after a failure. Locally, the dependencies have already succeeded.
                if (GetService<IWorkflowContext>().WorkflowType is not null)
                    throw new InvalidOperationException("A product job failed or was cancelled.");
            });
}
```

`DependsOn` takes one target name, so chain one call per dependency. `IWorkflowContext` is in
`Invex.Atom.Workflows.WorkflowContext`.

### The workflow

<xref:Invex.Atom.Workflows.Definition.WorkflowPresets.ProductValidate*> builds the workflow from the gate and the
product targets:

```csharp
IReadOnlyList<WorkflowDefinition> IWorkflowBuildDefinition.Workflows =>
[
    WorkflowPresets.ProductValidate(this,
        nameof(ValidateGate),
        [
            new("AppA", nameof(TestAppA)),
            new("AppB", nameof(TestAppB)),
        ]) with
    {
        Types = [WorkflowTypes.GitHub.Action],
    },
];
```

It is named `Validate` and triggered by pull requests into `main` and manual runs; change either with the `name`
argument or a `with` expression. Add options with `WithOptions(...)`, not `with { Options = [...] }`: replacing the
options removes the wiring checks below. It:

- runs `DetectAffectedProducts` first;
- gives each product target the condition `Tex.Success() && Tex.ProductAffected(product)` for its product, and the
  `product` parameter, so the job builds with the product's name, version and GitVersion configuration;
- makes the gate run only if a product job failed or the run was cancelled
  (`BuildOptions.Target.RunIfAnyDependencyFailed`);
- makes `atom gen` fail if a product target does not depend directly on `DetectAffectedProducts`, a product target or
  the gate depends on a target that is not a product target of the same product or a shared target, a shared target
  depends on a target that is not a shared target or `DetectAffectedProducts`, the gate does not depend directly on
  every product target, or the gate does not depend on every shared target (directly or through a product target).
  Each of these mistakes would otherwise skip a job or hide a failure silently. A dependency on another product's target is skipped whenever only this product is affected (affected
  products only flow from a product to the products that depend on it), which skips this job too. A dependency on an
  unlisted target would be added as a job without a condition, the product or `needs:`.

A target that does not depend on the product, such as a solution-wide compile or restore, can be a **shared target**
that product targets depend on. Pass it in `sharedTargets`: its job runs on every pull request, without a condition or
the product. If it fails, the product jobs that depend on it are skipped and the gate fails. A shared target can only
depend on other shared targets (and `DetectAffectedProducts`), because a product target it waited for would be skipped
whenever that product is not affected. A shared check that no product target needs, such as a solution-wide lint,
must be a dependency of the gate, or its failure would not fail the gate.

```csharp
WorkflowPresets.ProductValidate(this,
    nameof(ValidateGate),
    [new("AppA", nameof(TestAppA)), new("AppB", nameof(TestAppB))],
    sharedTargets: [new(nameof(Compile))])
```

A target that reads the build name or version (such as `SetupBuildInfo`, or a pack target) gets the repository's values
there, not the product's, so make it a product target instead.

A product can have several targets: list each one, for example `new("AppA", nameof(TestAppA))` and
`new("AppA", nameof(LintAppA))`, and make the gate depend on all of them. `LintAppA` may depend on `TestAppA`, because
both are AppA's. A target can only be listed once, so give each product its own targets. To give a target other options,
pass a `WorkflowTargetDefinition`: `new("AppA", new WorkflowTargetDefinition(nameof(TestAppA)) { Options = [...] })`.

To get the same checks in a validation workflow you build yourself, add a
<xref:Invex.Atom.Workflows.Options.ProductValidateOption> to its options:
`new ProductValidateOption(nameof(ValidateGate), [new("AppA", nameof(TestAppA)), new("AppB", nameof(TestAppB))])`.
It also checks that the product targets, shared targets and gate are in the workflow's `Targets`, but not their
conditions or `product` injections, which you then write yourself.

On GitHub Actions this generates, among others:

```yaml
  TestAppA:
    needs: [ DetectAffectedProducts ]
    if: success() && contains(needs.DetectAffectedProducts.outputs.affected-products, ',appa,')
    steps:
      # ...
      - name: TestAppA
        run: dotnet run --project _atom/_atom.csproj -- TestAppA --skip --headless
        env:
          product: AppA

  ValidateGate:
    needs: [ TestAppA, TestAppB ]
    if: failure() || cancelled()
```

On Azure DevOps the product conditions render as `and(succeeded(), contains(...))`: a custom condition there replaces
the default `succeeded()`, which is why the preset adds `Tex.Success()`. The gate renders as `or(failed(), canceled())`.
A branch policy there already fails when any job fails, so the gate matters less than on GitHub, but it keeps the two
platforms' workflows alike.

Branch protection then only needs to require `ValidateGate`.

## Per-product pipelines

<xref:Invex.Atom.Workflows.Definition.WorkflowDefinition.ForProduct*> creates a product's own workflow, such as its build
or release pipeline:

```csharp
WorkflowDefinition
    .ForProduct(this, "AppA", "Build")
    .WithTriggers(WorkflowTriggers.Manual, WorkflowTriggers.GitHub.OnRelease()) with
    {
        Targets = [new(nameof(PackAppA))],
        Types = [WorkflowTypes.GitHub.Action],
    },
```

The workflow is named `AppA - Build` (and written to `AppA - Build.yml`). Add options with `WithOptions(...)`: replacing
`Options` in a `with` expression would remove the product parameter and the release guard. It has:

- a single push trigger for `main` (or the `branches` you pass), filtered to AppA's paths, Shared's paths and the global
  paths, so a change to AppB does not run it;
- the `product` parameter on every job, so the name, version, GitVersion configuration and release tags are AppA's;
- on GitHub Actions with a release trigger, the job condition
  `github.event_name != 'release' || startsWith(github.event.release.tag_name, 'appa/v')` on every job, so publishing
  AppB's release does not run AppA's jobs. Pushes and manual runs are unaffected. Because the guard compares prefixes,
  `ValidateBuild` rejects a tag prefix that starts another product's, such as `v` and `vision/v`.

Azure DevOps has no release trigger. Pass `releaseTagTrigger: true` to add the product's tags (`appa/v*`) to the push
trigger instead. Do not use it on GitHub together with the release trigger: creating a release also pushes its tag, so
the workflow would run twice.

Use `ForProduct` for a product's own pipelines only. Pull request validation for every product belongs in the one
Validate workflow above, so that branch protection has a single check.

## Per-product versions

Each product with a `GitVersionConfig` gets its own GitVersion configuration file, generated by `atom gen` (and by any
local run, like the workflow files). The file is the repository's root GitVersion configuration (the first of
`GitVersion.yml`, `GitVersion.yaml`, `.GitVersion.yml` and `.GitVersion.yaml`) with two settings replaced:

- **`tag-prefix`** is the product's tag prefix, so GitVersion reads only the product's tags, for example `appa/v1.2.0`.
- **`ignore.paths`** makes GitVersion skip commits that touched none of the product's paths, its dependencies' paths
  or the global paths, so a change to AppB does not bump AppA's version (but see
  [the gotcha about an ignored `HEAD`](#gotchas)).

```yaml
# Generated by Atom for the product 'AppA'. Do not edit: run 'atom gen' to update it.
# Settings: the root GitVersion.yml, with this product's tag prefix and ignored paths.

mode: ContinuousDelivery

branches:
  main:
    regex: ^main$
    # ...

tag-prefix: 'appa/v'
ignore:
  paths:
    - '^(?!(?:src[/\\]AppA[/\\].*|src[/\\]Shared[/\\].*|global\.json|Directory\.Build\.[^/\\]*|Directory\.Packages\.[^/\\]*|_atom[/\\].*|\.github[/\\]workflows[/\\].*|\.devops[/\\]workflows[/\\].*)$).*'
```

The single anchored negative lookahead matches every path that is **not** one of the product's. Separators match both
`/` and `\`, because GitVersion sees paths with the platform's separator. GitVersion matches paths from the git
repository root, so when the Atom root is a subfolder of the repository (such as `samples/Monorepo`), the paths are
anchored below that folder: `^(?!samples[/\\]Monorepo[/\\](?:...)$).*`. Any `tag-prefix` and `ignore` settings in the
root configuration are replaced, including `ignore.sha` and `ignore.commits-before`.

Every product that is built with GitVersion needs a `GitVersionConfig`. Without one, GitVersion uses the root
configuration, so it counts every commit and reads the root tag prefix, while the release tag helpers use the product's
prefix: the product's own tags would never be read. Atom logs a warning when that happens. A path group such as
`Shared`, which is never built on its own, does not need one.

Atom owns these files. A headless run (CI) fails if one is missing or has been edited, and asks you to run `atom gen`,
in the same way as for workflow files. To change shared settings such as `branches`, edit the root configuration and
run `atom gen`.

When a product is selected, the GitVersion module uses its file automatically. To compute a product's version locally:

```shell
atom SetupBuildInfo --product AppA
```

`ignore.paths` needs GitVersion 6.4.0 or later, and path matching became case-insensitive in 6.8.0. Atom requires
`GitVersion.Tool` 6.8.0 or later and installs the latest 6.x release when no suitable version is installed.

## Release tags

When a product is selected, the <xref:Invex.Atom.Build.BuildInfo.IReleaseTagHelper> members use its tag prefix:

```csharp
ReleaseTagPrefix;                    // "appa/v" (or "v" with no product)
ReleaseTagFor(BuildVersion);         // "appa/v1.2.0"
TryParseReleaseTag("appa/v1.2.0", out var v); // true, v is 1.2.0
ReleaseTagGlob;                      // "appa/v*", for git tag --list
```

For another product's tags, the <xref:Invex.Atom.Build.BuildInfo.ReleaseTag> helpers take the prefix explicitly, for
example `ReleaseTag.For(version, "appb/v")`. `ReleaseTagGlob` is broader than a release tag (`appa/v*` also matches
`appa/vnext`), so filter listed tags with `TryParseReleaseTag`.

### Tag the latest commit that touched the product

GitVersion ignores a tag that sits on a commit excluded by `ignore.paths`. If `appa/v1.2.0` is put on a commit that only
touched AppB, AppA cannot see the tag and keeps versioning from an older one. **Put a product's tag on the latest commit
that touched the product's paths**, not on `HEAD`. `GetReleaseTagCommit()` finds it, using the same paths as the
generated `ignore.paths`:

```csharp
Target TagAppA =>
    t => t
        .DescribedAs("Tags the AppA release")
        .Executes(async cancellationToken =>
        {
            // Run with --product AppA, which the AppA workflows inject
            var commit = await GetReleaseTagCommit(cancellationToken);
            var tag = ReleaseTagFor(BuildVersion);

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

One history defeats any choice of commit: a merged branch whose own commits touched the product but whose merge, compared
with its first parent, did not (the branch's changes cancelled out, or the merge kept the base's version of the files).
GitVersion counts the branch's commits, which are not on the first-parent line, so they still count after the tag.
Squash merges avoid this.

`GetReleaseTagCommit()` runs `git log -1 --first-parent --format=%H -- <paths>` with the current product's paths (with
its dependencies' and the global paths), matched as globs, or returns `HEAD` when no product is selected.
`--first-parent` compares each commit with its first parent only, as `ignore.paths` does, so in a repository that
merges with merge commits it finds the merge commit rather than the merged commit behind it. Two overloads take the
paths from elsewhere:

- `GetReleaseTagCommit(product)` uses another product's paths, the same way, so a build can find the commit for a
  product other than the one selected with `--product`, for example to list every product's next release commit:

  ```csharp
  foreach (var product in Products.Where(x => x.GitVersionConfig is not null))
  {
      var commit = await GetReleaseTagCommit(product, cancellationToken);
      Logger.LogInformation("{Product}: tag {Commit} as {Prefix}<version>", product.Name, commit, product.TagPrefix);
  }
  ```

  It only picks the commit. `BuildVersion` is still the selected product's version, so get the other product's version
  from a run with `--product` set to it before formatting its tag.

- `GetReleaseTagCommit(paths)` takes files and folders below the Atom root as `RootedPath` values, such as
  `[RootedFileSystem.AtomRootDirectory / "docs"]`. They are matched literally (a folder includes everything below it),
  not as globs, and a path outside the Atom root is rejected.

All of them need the full history: in a shallow clone the oldest fetched commit appears to touch every file, so they
fail instead of returning a wrong commit. Atom's generated checkout steps fetch the full history by default.

## Gotchas

- **Direct dependencies.** Conditions don't add dependencies. Every product target in the Validate workflow must
  `DependsOn(nameof(DetectAffectedProducts))` **directly**, and the gate must depend directly on every product target.
  `ProductValidate` makes `atom gen` fail when they don't. In a workflow you build by hand, Atom only warns about a
  product job that does not depend on `DetectAffectedProducts`; a gate that misses a product job gets no warning, so
  add a `ProductValidateOption` (see [the workflow](#the-workflow)).
- **Auto-added dependency jobs.** A dependency that is not listed in the workflow's `Targets` is still added as a job,
  but without its target options (so without its condition or product) and with no `needs:`. `ProductValidate` rejects
  them for product targets and the gate; in a workflow you build by hand, list every target explicitly or add a
  `ProductValidateOption`.
- **Custom artifact providers.** With `BuildOptions.Artifacts.UseCustomProvider`, every job that stores or retrieves
  artifacts also depends on `SetupBuildInfo`. In the Validate workflow, list `SetupBuildInfo` as a shared target:
  `ProductValidate` checks these added dependencies like the others, so it fails if `SetupBuildInfo` is missing or
  belongs to a product.
- **One version per process.** Parameters are resolved once per run, so one run builds one product. `SetupBuildInfo`
  produces one version for the whole workflow: in the Validate workflow each product job computes its own version from
  its `product` parameter instead. `StoreArtifact` and `RetrieveArtifact` still use the workflow-wide `BuildId`, which
  is fine for validation.
- **`ignore.paths` semantics.** A commit is ignored only if **all** of its changed paths (compared with its first
  parent) match. Ignored commits can still change the build metadata part of `FullSemVer`.
- **Repository root.** Product paths are relative to the Atom root directory, which should be the git repository root.
  The generated `ignore.paths` are anchored below the Atom root when it is a subfolder, but GitHub and Azure DevOps
  path filters are always relative to the repository root, so they would not match. GitHub also only reads workflows
  from the repository root. A headless run outside a git repository (such as a source export) skips the check of the
  product GitVersion files, because their contents depend on where the repository root is.
- **An ignored `HEAD`.** When the latest commit touched only other products, GitVersion versions a product from a
  commit it ignores, and can report a different version than at the product's own latest commit. With one counted
  commit since the product's tag, GitVersion 6.8 reported the tagged version itself. A push whose last commit touched
  only other products usually doesn't run the product's pipeline, because of its path filters. But a push of several
  commits (a rebase merge, or a direct push) runs it when any of them touched the product, with `HEAD` at the last one.
  Squash or merge-commit merges avoid this; otherwise run the release from the product's latest commit.
- **Release builds of a tag below the branch tip.** A release run checks out the tag, detached. GitVersion attaches it
  to a branch only when a branch's tip is that commit, which is rarely true of a product's tag: it goes on the
  product's latest commit, often below the tip of `main`. GitVersion then versions the checkout with the branch
  configuration whose `regex` matches anything. If that configuration sets
  `prevent-increment.when-current-commit-tagged: false`, the tagged commit gets a pre-release version such as
  `1.0.0-alpha.-no-branch-.0`, not `1.0.0`, so the release build doesn't match its tag. Set it to `true` on that
  catch-all branch configuration in the root `GitVersion.yml`, and run `atom gen`.
- **Local runs.** `DetectAffectedProducts` treats every product as affected when there is no base branch, so local runs
  of product targets work. Make the gate a no-op outside CI, as above.
- **Azure DevOps tag triggers.** Check whether path filters also apply to tag pushes in your Azure DevOps organisation
  before relying on `releaseTagTrigger` together with path filters.

## Migration for an existing repo

1. **Baseline tags.** For each product, create a tag `appa/v<current version>` on the latest commit that touched the
   product's paths (`GetReleaseTagCommit()` with `--product AppA`, `GetReleaseTagCommit(GetProduct("AppA"))`, or
   `git log -1 --first-parent --format=%H -- ':(glob)<path>' ...` with the product's, its dependencies' and the global
   paths). Existing
   `v*` tags no longer match the product's tag prefix, so without a baseline the product's version starts again.
2. **Products.** Implement `IProductsDefinition`, declare the products with their `GitVersionConfig`, and run
   `atom gen` to generate the product configurations.
3. **Validate workflow.** Add the per-product targets and the gate, replace the Validate workflow with
   `WorkflowPresets.ProductValidate`, and run `atom gen`.
4. **Branch protection.** Require only the gate check, and remove the per-product checks.

If you wrote this pattern by hand with an earlier version of Atom, delete your own `AffectedProducts` parameter,
`DetectAffectedProducts` target and path lists when you implement `IProductsDefinition`: parameter and target names must
be unique across the build. Replace hand-written `gitversion-config` injections and `ReleaseTag.For(version, "appa/v")`
calls with the `product` parameter and the `IReleaseTagHelper` members.

## See also

- [DetectAffectedProducts](../built-in-targets/detect-affected-products.md): how the affected products are worked out.
- [GitVersion](../modules/git-version.md): the `gitversion-config` parameter and caching.
- [Workflow Options](workflow-options.md): conditions, injections and other per-target options.
- [Variables in Workflows](variables-in-workflows.md): passing `AffectedProducts` between jobs.
- <xref:Invex.Atom.Build.IProductsDefinition>
- <xref:Invex.Atom.Build.BuildInfo.Product>
- <xref:Invex.Atom.Build.BuildInfo.IReleaseTagHelper>
- <xref:Invex.Atom.Workflows.Definition.WorkflowPresets>
- <xref:Invex.Atom.Workflows.Options.ProductWorkflowOption>
