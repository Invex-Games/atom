# Copilot Instructions

Guidance for AI agents working in **Atom** — an opinionated, type-safe build automation framework
for .NET. Build logic is defined in C# (debuggable like normal code), and Atom generates the CI/CD
YAML for GitHub Actions and Azure DevOps. Keep changes focused and defer to the linked docs for
detail.

## What's in the repo

| Project | Role |
|---------|------|
| `src/Invex.Atom.Build` | The core framework: build definitions, targets, params/secrets, artifacts, hosting, lifecycle hooks |
| `src/Invex.Atom.Build.Analyzers` | Roslyn analyzers shipped with the framework (e.g. `[PublicAPI]` enforcement) |
| `src/Invex.Atom.Build.SourceGenerators` | Source generators that discover targets/params and generate the entry point (`netstandard2.0`) |
| `src/Invex.Atom.Workflows` | Workflow definitions, triggers, and options on top of a base build |
| `src/Invex.Atom.Module.GithubWorkflows` | GitHub Actions YAML generation module |
| `src/Invex.Atom.Module.DevopsWorkflows` | Azure DevOps pipeline YAML generation module |
| `src/Invex.Atom.Module.Dotnet` | .NET build/test/pack targets module |
| `src/Invex.Atom.Module.GitVersion` | GitVersion-based build ID/version providers |
| `src/Invex.Atom.Module.AzureKeyVault` / `AzureStorage` | Azure secrets / artifact providers |
| `src/Invex.Atom.Tool` | The `atom` dotnet global tool (`Invex.Atom.Tool`) |
| `src/Invex.Atom.DotnetCliGenerator` | Generates the dotnet CLI model |
| `tests/*` | NUnit test suites, plus `Invex.Atom.TestUtils` (published test helpers) |
| `_atom/` | This repo's own Atom build definition (`IBuild.cs`) — Atom is dogfooded to build itself |
| `samples/` | Sample projects (Hello World, params, analyzer samples) |

The DocFX documentation site is configured by `docfx.json` with content in `docs/`, `api/`,
`index.md`, and `toc.yml`.

## Build & language specifics

- **.NET 10 SDK** is required. Library and test projects multi-target `net8.0;net9.0;net10.0`;
  `_atom` targets `net10.0`; source generators/analyzers target `netstandard2.0`.
- The solution file is `Invex.Atom.slnx` (slnx format).
- C# `LangVersion` 14, `ImplicitUsings` and `Nullable` enabled, `TreatWarningsAsErrors` on.
- Global usings live in each project's `_usings.cs` — add shared usings there, not per-file.
- `GenerateDocumentationFile` is on and `CS1591` is **enforced** in `src/` — every public type
  and member needs XML doc comments. (`_atom` and the test projects suppress `CS1591` locally; do
  not re-add it to the repo-wide `NoWarn` in `Directory.Build.props`.)

Build and test the whole solution:

```shell
dotnet build Invex.Atom.slnx
dotnet test Invex.Atom.slnx
```

Or use the repo's own Atom targets (run from the repo root):

```shell
atom PackProjects        # pack all NuGet packages
atom TestProjects        # run the test suites
atom BuildDocs           # build the DocFX site (atom ServeDocs to preview)
```

(`atom` is the `Invex.Atom.Tool` global tool; `dotnet run --project _atom -- <target>` is
equivalent.)

## Architecture overview

- A build is an `internal interface IBuild` annotated `[BuildDefinition]` and
  `[GenerateEntryPoint]`, extending `IBuildDefinition` (or `IWorkflowBuildDefinition` plus module
  interfaces). Source generators discover targets/params and generate the program entry point.
- **Targets** are `Target` properties built with a fluent API: `.DescribedAs(...)`,
  `.DependsOn(...)`, `.RequiresParam(...)` / `.UsesParam(...)`, `.ProducesArtifact(...)` /
  `.ConsumesArtifact(...)`, `.ConsumesVariable(...)`, `.Executes(...)`.
- **Params/secrets** are declared with `[ParamDefinition]` / `[SecretDefinition]` and resolved via
  `GetParam(() => Prop)`. Secrets are masked in logs.
- **Providers** are the extensibility points: `ISecretsProvider`, `IArtifactProvider`,
  `IVariableProvider`, `IBuildIdProvider`, `IBuildVersionProvider` (`SemVer Version`),
  `IBuildTimestampProvider`, `IPathProvider`, `IOutcomeReportWriter`. Modules plug in by
  registering these (see `docs/developer-guide/custom-providers.md` and `writing-a-module.md`).
- **Workflows** (`IWorkflowBuildDefinition.Workflows`) declare `WorkflowDefinition`s — triggers,
  targets, matrix dimensions, options, and types (`WorkflowTypes.Github.Action`,
  `WorkflowTypes.Devops.Pipeline`) — from which the CI/CD YAML is generated.

## This repo's workflows are generated

The YAML under `.github/workflows/` (`Validate.yml`, `Build.yml`,
`Dependabot Enable auto-merge.yml`), `.github/dependabot.yml`, and the DevOps test pipeline
(`.devops/workflows/Test_Devops_Build.yml`) are all **generated** from `_atom/IBuild.cs`.

Whenever you change anything that affects the workflows — targets, workflow definitions, triggers,
options, or params/secrets in `_atom/` — regenerate the YAML:

```shell
atom gen
```

Commit the regenerated files alongside your `_atom/` changes; never hand-edit the generated YAML.
A drift between `_atom/IBuild.cs` and the committed YAML should be treated as a missing
`atom gen` run.

## Conventions

- Annotate every new public type with `[PublicAPI]` — the in-repo analyzer flags anything missing,
  and warnings are errors.
- Add XML doc comments to all public types and members in `src/`. Match the existing `<summary>` /
  `<param>` / `<remarks>` / `<example>` style, and keep docs **accurate to the implementation**.
- Use Conventional Commits — the prefix drives versioning (see `GitVersion.yml`):

  | Prefix | Version bump |
  |--------|--------------|
  | `breaking:` / `major:` | Major |
  | `feat:` / `feature:` / `minor:` | Minor |
  | `fix:` / `patch:` | Patch |
  | `semver-none` / `semver-skip` | No bump |

- When adding user-facing features, update the relevant `docs/` page and `README.md`. The README
  is packed into the NuGet packages — keep links absolute where they must work outside the repo.

## Testing & the Verify workflow

- Tests use **NUnit** with **Shouldly**, **FakeItEasy**, and **Verify** (`Verify.NUnit`) for
  snapshot/approval testing. `Invex.Atom.TestUtils` provides shared helpers (and is itself a
  published package).
- Workflow-generation tests snapshot the generated YAML into `*.verified.txt` files. A snapshot
  test fails when its output differs; Verify writes a `*.received.txt` next to it.
- If the diff is unintended, fix the code. If the change is valid (expected new output), accept
  it and re-run:
  1. Overwrite the `*.verified.txt` with the contents of the matching `*.received.txt`.
  2. Delete the `*.received.txt`.
  3. Re-run `dotnet test` to confirm the suite is green.
- The Validate workflow's `CheckPrForBreakingChanges` target diffs **all**
  `tests/**/*.verified.txt` files against the latest release and requires a matching major/minor
  version bump for breaking changes. Snapshot changes must therefore be intentional, committed,
  and paired with an appropriate Conventional Commit prefix.

## Defer to the docs

For anything beyond the above, prefer these over duplicating detail:

- `README.md` — overview, quick-start examples, and the full docs index.
- `docs/getting-started/` — introduction, your first build, base vs workflow builds.
- `docs/core-concepts/` — build definitions, targets, parameters, secrets, artifacts, variables,
  file system, process runner, build info, build options, hosting, lifecycle hooks,
  logging & reports, file transformations.
- `docs/workflows/` — workflow definitions, triggers, options, variables, debugging.
- `docs/modules/` — the built-in modules (Dotnet, GithubWorkflows, DevopsWorkflows,
  AzureKeyVault, AzureStorage, GitVersion).
- `docs/built-in-targets/` — `SetupBuildInfo`, `ValidateBuild`, `GenerateWorkflowFiles`.
- `docs/developer-guide/` — writing a module, custom providers, source generators, testing.
- `docs/reference/cli.md` — the `atom` CLI and its arguments.


