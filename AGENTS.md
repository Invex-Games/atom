# Agent Instructions

Guidance for AI agents working in **Atom**, an opinionated, type-safe build automation framework
for .NET. Keep changes focused; `README.md` and `docs/` describe consumer-facing usage.

## Repository map

| Area | Purpose |
|------|---------|
| `src/Invex.Atom.Build` | Core build definitions, targets, parameters, secrets, artifacts, hosting, and lifecycle hooks |
| `src/Invex.Atom.Workflows` | Workflow definitions, triggers, and options |
| `src/Invex.Atom.Module.*` | Built-in modules for .NET, GitHub Actions, Azure DevOps, GitVersion, and Azure services |
| `src/Invex.Atom.Build.Analyzers` / `SourceGenerators` | Roslyn analyzers and generated build entry points |
| `src/Invex.Atom.Tool` | The `Invex.Atom.Tool` global tool |
| `tests/` | NUnit tests, snapshots, and the published `Invex.Atom.TestUtils` helpers |
| `_atom/` | This repository's own Atom build definition |
| `samples/` | Small consumer examples |
| `docs/`, `api/` | DocFX documentation source and generated API content |

The solution is `Invex.Atom.slnx`. The repository currently requires the .NET 10 SDK; libraries
and tests target `net8.0`, `net9.0`, and `net10.0`, while the build project targets `net10.0`.
There is no `global.json`, so use the installed .NET 10 SDK selected by the environment.

## Build, test, and cleanup

Run commands from the repository root:

```shell
dotnet build Invex.Atom.slnx
dotnet test Invex.Atom.slnx
```

The repository's own build targets can also be used:

```shell
atom PackProjects
atom TestProjects
atom BuildDocs
atom ServeDocs
```

`atom` is the `Invex.Atom.Tool` global tool. The equivalent form is
`dotnet run --project _atom -- <target>`.

After making C# changes, run ReSharper cleanup over the solution:

```powershell
$sdk = dotnet --version
jb cleanupcode Invex.Atom.slnx --include="**.cs" --toolset-path="C:\Program Files\dotnet\sdk\$sdk\MSBuild.dll"
```

If `jb` is unavailable, install it with
`dotnet tool install --global JetBrains.ReSharper.GlobalTools`. The explicit `--toolset-path`
ensures cleanup uses the selected .NET SDK's MSBuild rather than an incompatible Visual Studio
BuildTools installation. Cleanup honors `.editorconfig` and `Invex.Atom.sln.DotSettings`.

## Build and language conventions

- `ImplicitUsings`, nullable reference types, documentation generation, and
  `TreatWarningsAsErrors` are enabled in `Directory.Build.props`.
- Global usings belong in each project's `_usings.cs`, not in individual source files.
- Add XML documentation to every new public type and member in `src/`, matching the existing
  style and accurately describing nullability, return values, retries, and caching.
- Add `[PublicAPI]` to new public types where the public API analyzer applies.
- Some generated-code projects and tests explicitly suppress `CS1591`; do not broaden the
  repository-wide warning suppressions to hide missing documentation.
- Keep cross-member implementation helpers `internal`; expose only the intended consumer API.
- The IDE can report false errors for `[GeneratedRegex]` partial members; `dotnet build` is the
  source of truth for those members.

## Atom architecture

- A build is an `internal interface IBuild` annotated with `[BuildDefinition]` and
  `[GenerateEntryPoint]`, extending `IBuildDefinition` or
  `IWorkflowBuildDefinition` plus module interfaces.
- Targets use the fluent API for descriptions, dependencies, parameters, artifacts, variables,
  and execution.
- Parameters and secrets use `[ParamDefinition]` / `[SecretDefinition]` and
  `GetParam(() => Property)`. Secrets are masked in logs.
- Providers are the extension points for secrets, artifacts, variables, build identity/version,
  timestamps, paths, and outcome reports. See `docs/developer-guide/`.
- Workflow definitions describe triggers, targets, matrices, options, and GitHub Actions or
  Azure DevOps output types.

## Generated workflows

The committed workflow files under `.github/workflows/`, `.github/dependabot.yml`, and
`.devops/workflows/` are generated from `_atom/IBuild.cs`. When changing targets, workflow
definitions, triggers, options, or parameters/secrets that affect them, run:

```shell
atom gen
```

Commit the generated files with the source change. Never hand-edit generated workflow files.

## Versioning and commits

Use Conventional Commits; `GitVersion.yml` maps prefixes as follows:

| Prefix | Version bump |
|--------|--------------|
| `breaking:` / `major:` | Major |
| `feat:` / `feature:` / `minor:` | Minor |
| `fix:` / `patch:` | Patch |
| `semver-none` / `semver-skip` | No bump |

## Testing and Verify snapshots

Tests use NUnit, Shouldly, FakeItEasy, and Verify (`Verify.NUnit`). Workflow and source-generator
tests compare output with committed `*.verified.txt` files. A mismatch creates a corresponding
`*.received.txt` file:

1. Fix the implementation if the output is unintended.
2. If the output is intentional, replace the matching `.verified.txt` with the received output,
   remove `.received.txt`, and rerun `dotnet test`.
3. Keep approved snapshot changes intentional because PR validation checks the snapshots for
   breaking API changes.

## Change checklist

1. Follow existing patterns and make a precise, surgical change.
2. Add public API documentation and `[PublicAPI]` where applicable.
3. Build and test the solution.
4. Run `jb cleanupcode` and include its relevant formatting changes.
5. Update `README.md` or the relevant documentation for consumer-facing behavior.
6. Run `atom gen` whenever generated workflows are affected.

Do not commit secrets, generated planning notes, or unrelated formatting changes.
