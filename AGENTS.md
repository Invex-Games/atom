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

The solution is `Invex.Atom.slnx`. The repository currently requires the .NET 10 SDK; libraries,
tests, and the build project all target `net10.0`.
`global.json` pins SDK `10.0.0` with `rollForward: latestMajor`, so any installed .NET 10 or later SDK is used.

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
- `CS1591` is enforced repository-wide. Only tests, samples, `_atom/`, and the generated-code module projects
  (`Invex.Atom.Module.Dotnet`, `.DevopsWorkflows`, `.GithubWorkflows`) suppress it in their own `.csproj`; do not
  broaden those suppressions to hide missing documentation.
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

## Documentation

Docs follow the shared Invex documentation standard used by every Invex library:

- **README.md** is also the NuGet readme. Keep the fixed section order (title, badges, pitch, Packages, Features,
  Installation, Quick start, Usage, Documentation, Development, License) and use absolute links only
  (`https://github.com/Invex-Games/atom/blob/main/...` or the docs site), so they work on nuget.org.
- **DocFX site** (`docfx.json`, `toc.yml`, `index.md`, `docs/`, `api/index.md`), published to
  https://invex-games.github.io/atom/. `docs/getting-started/introduction.md` and
  `docs/getting-started/your-first-build.md` come first; every page opens with one to three sentences on what it covers
  and ends with a "See also" or "Next steps" list. Link API types with `<xref:Namespace.Type>`, not file paths.
- **`api/index.md`** is the only hand-written file under `api/`; everything else there is generated. Put per-type
  detail in XML docs, not in hand-written API pages.
- **XML docs are enforced.** `CS1591` is not suppressed for shipped projects, so every public type and member needs
  `<summary>`, plus `<param>`, `<typeparam>`, `<returns>` and `<exception>` where they apply, `<remarks>` for
  non-obvious behaviour, and `<see cref>`/`<paramref>`/`<see langword>` for references.

## Change checklist

1. Follow existing patterns and make a precise, surgical change.
2. Add public API documentation and `[PublicAPI]` where applicable.
3. Build and test the solution.
4. Run `jb cleanupcode` and include its relevant formatting changes.
5. Update `README.md` or the relevant documentation for consumer-facing behavior.
6. Run `atom gen` whenever generated workflows are affected.

Do not commit secrets, generated planning notes, or unrelated formatting changes.
