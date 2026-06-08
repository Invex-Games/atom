# CLI Reference

Atom builds are invoked via `dotnet run` or the `atom` global tool.

## Invocation

### Via `dotnet run`

```shell
dotnet run -- <targets> [options] [--param-name value ...]
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

The tool discovers your Atom project (defaults to the `_atom` directory) and runs it.

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
| `--project <name>` | `-p <name>` | Specify the Atom project directory (default: `_atom`) |

## Parameters

Pass parameter values with `--<param-name> <value>`:

```shell
dotnet run -- Deploy --api-key sk-123 --environment production
```

Parameter names use kebab-case on the command line and are matched to `[ParamDefinition]` attributes.

## Examples

```shell
# Run a single target
dotnet run -- Compile

# Run multiple targets
dotnet run -- Compile Test

# Pass parameters
dotnet run -- Deploy --configuration Release --api-key sk-123

# Interactive mode (prompt for missing params)
dotnet run -- Deploy -i

# Skip dependencies
dotnet run -- Pack -s

# Verbose output
dotnet run -- Compile -v

# Use a custom project directory
dotnet run -- Compile -p MyBuildProject

# Show help
dotnet run -- -h
```

## The `atom` Tool

The `atom` global tool (`Invex.Atom.Tool`) provides the same interface but discovers your build project automatically:

```shell
atom Compile Test --verbose
```

It searches for the Atom project in the current directory tree (or the directory specified by `-p`).

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


