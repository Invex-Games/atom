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
dotnet tool install --global DecSm.Atom.Tool
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

The `atom` global tool (`DecSm.Atom.Tool`) provides the same interface but discovers your build project automatically:

```shell
atom Compile Test --verbose
```

It searches for the Atom project in the current directory tree (or the directory specified by `-p`).

