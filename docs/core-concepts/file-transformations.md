# File Transformations

`TransformFileScope` provides disposable scopes for performing temporary, reversible changes to file content. This is
useful when a build step needs a file modified temporarily (e.g. patching a version in a `.csproj`) without permanently
altering it.

## How It Works

1. The file's current content is captured.
2. A transform function is applied and the result is written to disk.
3. When the scope is disposed, the original content is restored.

If the file didn't exist before the scope was opened, it is deleted on disposal.

## Async Usage

```csharp
await using var scope = await TransformFileScope.CreateAsync(
    projectFile,
    content => content.Replace("1.0.0", buildVersion));

// File now contains the patched version.
// Build or pack the project here.

// On disposal, the original content is restored.
```

## Sync Usage

```csharp
using var scope = TransformFileScope.Create(
    projectFile,
    content => content.Replace("1.0.0", buildVersion));
```

## Chaining Transforms

Apply additional transforms within the same scope:

```csharp
var scope = await TransformFileScope.CreateAsync(file, c => c.Replace("a", "b"));
await scope.AddAsync(c => c.Replace("x", "y"));
```

Disposal still restores to the **original** content (before the first transform), not the intermediate state.

## Committing Changes

Call `CancelRestore()` to make the transform permanent — the file will **not** be restored on disposal:

```csharp
await using var scope = await TransformFileScope.CreateAsync(file, transform);
// ... verify the transform is correct ...
scope.CancelRestore(); // file keeps the transformed content
```

## `TransformMultiFileScope`

For transforming multiple files atomically, use `TransformMultiFileScope` which manages a collection of
`TransformFileScope` instances.

## Next Steps

→ [Workflows Overview](../workflows/overview.md)

