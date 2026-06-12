# Lifecycle Hooks

`IAtomLifecycleHook` lets you run code at specific points in the Atom build lifecycle without modifying target
definitions.

## Interface

```csharp
public interface IAtomLifecycleHook
{
    Task BeforeExecute(CancellationToken cancellationToken) => Task.CompletedTask;
    Task AfterExecute(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

Both methods have default no-op implementations, so you only override what you need.

## Hook Points

| Method          | When it runs                                              | Use cases                                        |
|-----------------|-----------------------------------------------------------|--------------------------------------------------|
| `BeforeExecute` | After the build model is resolved, before targets execute | Validation, file generation, precondition checks |
| `AfterExecute`  | After all targets complete (success or failure)           | Cleanup, reporting, notifications                |

## Registering a Hook

```csharp
public class MyHook : IAtomLifecycleHook
{
    public Task BeforeExecute(CancellationToken cancellationToken)
    {
        // e.g. verify workflow files are up to date
        return Task.CompletedTask;
    }
}

// In your build definition or module:
builder.Services.AddSingleton<IAtomLifecycleHook, MyHook>();
```

Multiple hooks can be registered. They run in registration order.

## Built-in Hooks

The `WorkflowLifecycleHook` (registered by `IWorkflowBuildDefinition`) uses `BeforeExecute` to regenerate workflow
files in interactive mode, or — in headless mode — to fail the build when the generated files are outdated.

## Next Steps

→ [Logging & Reports](logging-and-reports.md)

