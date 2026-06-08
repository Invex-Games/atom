# File System

Atom provides `IRootedFileSystem` — a build-aware file system abstraction that layers path resolution on top of
`System.IO.Abstractions.IFileSystem`.

## `IRootedFileSystem`

Available via `IBuildAccessor.RootedFileSystem`, this interface gives you:

- All standard `IFileSystem` operations (read, write, delete, etc.)
- **Path resolution** via `GetPath(key)` — looks up well-known paths by key
- A `CurrentDirectory` property returning a `RootedPath`

### Resolving Well-Known Paths

```csharp
var root = RootedFileSystem.GetPath("Root");
var artifacts = RootedFileSystem.GetPath("Artifacts");
var publish = RootedFileSystem.GetPath("Publish");
```

Paths are resolved by querying registered `IPathProvider` implementations in priority order and are cached after first
resolution.

## `RootedPath`

`RootedPath` is a value type that wraps an absolute directory/file path and is bound to an `IRootedFileSystem` instance.
It supports the `/` operator for path combination:

```csharp
var projectDir = RootedFileSystem.GetPath("Root") / "src" / "MyProject";
var csproj = projectDir / "MyProject.csproj";
```

Because a `RootedPath` carries a reference to the file system, you can perform I/O directly:

```csharp
var content = RootedFileSystem.File.ReadAllText(csproj);
```

## `IPathProvider`

Implement `IPathProvider` to register custom well-known paths:

```csharp
public interface IPathProvider
{
    RootedPath? GetPath(string key);
}
```

Return `null` if your provider doesn't handle the requested key — the next provider in the chain will be tried.

### Built-in Path Keys

| Key         | Description                |
|-------------|----------------------------|
| `Root`      | Repository / solution root |
| `Artifacts` | Build artifacts directory  |
| `Publish`   | Publish output directory   |

## `IPathMarker`

For statically determined paths (e.g. source-generated project paths), implement `IPathMarker`:

```csharp
public interface IPathMarker
{
    static abstract RootedPath Path(IRootedFileSystem fileSystem);
}
```

Resolve with `RootedFileSystem.GetPath<MyPathMarker>()`.

## Next Steps

→ [Process Runner](process-runner.md)

