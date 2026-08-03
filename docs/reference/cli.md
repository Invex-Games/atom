# CLI Reference

Atom builds are invoked via `dotnet run` or the `atom` global tool. The repository currently
requires the .NET 10 SDK. Libraries may target `net8.0`, `net9.0`, and `net10.0`.

## Invocation

### Via `dotnet run` (project-based)

```shell
dotnet run --project _atom -- <targets> [options] [--param-name value ...]
```

For a project located in the current directory, `dotnet run -- <targets> ...` is also sufficient.

To run a file-based build:

```shell
dotnet run Build.cs <targets> [options] [--param-name value ...]
```

### Via the `atom` Global Tool

Install the tool:

```shell
dotnet tool install --global Invex.Atom.Tool
```

Then run:

```shell
atom <targets> [options] [--param-name value ...]
```

The tool searches the current directory and its parents for `_atom`, `_build`, `Atom`, or `Build`
projects/files. On case-sensitive systems it also checks lowercase `atom` and `build`.

## Targets

Specify one or more target names to execute:

```shell
dotnet run -- Compile Test Pack
```

Targets execute in dependency order. Duplicates are resolved automatically.

## Options

| Option             | Short       | Description                                           |
|--------------------|-------------|-------------------------------------------------------|
| `--help`           | `-h`        | Display help information                              |
| `--skip`           | `-s`        | Skip execution of dependent targets                   |
| `--headless`       | `-hl`       | Non-interactive mode (no prompts, plain output)       |
| `--verbose`        | `-v`        | Enable verbose (debug-level) logging                  |
| `--interactive`    | `-i`        | Prompt for missing required parameters                |
| `--project <name>` | `-p <name>` | Specify the Atom project or project name |
| `--file <path>`    | `-f <path>` | Specify a file-based C# build |
| `--no-restore-cache` |             | Force restore and build instead of using the Atom caches |

## Parameters

Pass parameter values with `--<param-name> <value>`:

```shell
dotnet run --project _atom -- Deploy --api-key example-key --environment production
```

Parameter names use kebab-case on the command line and are matched to `[ParamDefinition]`
attributes. Avoid passing real secrets on the command line because shell history and process
diagnostics may expose them; use a secret provider, user secrets, or CI secret injection instead.

## Examples

```shell
# Run a single target
dotnet run -- Compile

# Run multiple targets
dotnet run -- Compile Test

# Pass parameters (use a secret provider for real credentials)
dotnet run --project _atom -- Deploy --configuration Release --api-key example-key

# Interactive mode (prompt for missing params)
dotnet run -- Deploy -i

# Skip dependencies
dotnet run -- Pack -s

# Verbose output
dotnet run -- Compile -v

# Use a custom project with the global tool
atom --project MyBuildProject Compile

# Run a file-based build with the global tool
atom --file Build.cs SayHello

# Show help
dotnet run -- -h
```

## The `atom` Tool

The `atom` global tool (`Invex.Atom.Tool`) provides the same interface but discovers your build project automatically:

```shell
atom Compile Test --verbose
```

It searches the current directory and parent directories, and does not search arbitrary child
directories. Use `--project` or `--file` to select a specific build.

### Adding a NuGet source

The `nuget-add` command adds a package source to the user-level NuGet configuration:

```shell
atom nuget-add my-feed https://example.invalid/nuget/index.json
```

If `NUGET_TOKEN_MY_FEED` is set, its value is used as the feed password. This command writes
credentials to the user configuration; review the resulting NuGet configuration and protect it
appropriately.

### Restore & Build Caching

Because a consuming project's Atom build is distributed as source, the `atom` tool would normally trigger a
`dotnet restore` and `dotnet build` on every invocation. To avoid this, the tool caches hashes of the relevant
inputs and skips work that isn't needed:

- **Restore** is skipped (`--no-restore`) when the build project file plus any `Directory.Build.props`,
  `Directory.Build.targets`, `Directory.Packages.props`, `nuget.config` and `global.json` files (found while
  walking up to the project root) are unchanged.
- **Build** is skipped (`--no-build`) when, in addition to the restore inputs, every `.cs` source file under the
  build project (excluding `bin`/`obj`) is unchanged and a previous build output exists. `--no-build` implies
  `--no-restore`.

The hashes are stored in the build project's `obj/.atom-restore.hash` and `obj/.atom-build.hash` files, so they
are automatically invalidated by `dotnet clean` and never committed to source control.

> Note: source changes in projects referenced via `<ProjectReference>` are not tracked by the build cache. If your
> build project references other projects by source, force a full build with the opt-out below.

To force a full restore and build regardless of the caches, use either:

```shell
# CLI flag
atom Compile --no-restore-cache

# Environment variable (any value other than "0"/"false" enables the opt-out)
ATOM_NO_RESTORE_CACHE=1 atom Compile
```

