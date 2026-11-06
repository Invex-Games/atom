# DetectAffectedProducts

The `DetectAffectedProducts` target works out which monorepo products a pull request affects, so that a validation
workflow can run only those products' jobs.

**Interface:** `IProductsDefinition` · **Package:** `Invex.Atom.Build`

## What It Does

1. Reads the pull request's base branch from `GITHUB_BASE_REF` (GitHub Actions), or from
   `SYSTEM_PULLREQUEST_TARGETBRANCHNAME` or `SYSTEM_PULLREQUEST_TARGETBRANCH` (Azure Pipelines).
2. Lists the files changed since the base branch with
   `git diff --name-only --no-renames --relative -z origin/<base>...HEAD`, run in the Atom root directory. Renamed
   files count on both sides, so a file moved out of a product still affects it.
3. Marks a product affected if a changed file matches its paths or the paths of a product it depends on. A change to a
   global path affects every product.
4. Writes the affected products' identifiers to the `AffectedProducts` variable (argument name `affected-products`),
   wrapped in commas, for example `,appa,shared,`, or `,` when no product is affected.
5. Adds the affected products to the build report.

When there is no base branch (a push, a manual run or a local run), or git fails (for example because the base branch
was not fetched), every product is affected, so nothing is skipped by mistake. A git failure is logged as a warning.

## Usage

Implement [`IProductsDefinition`](../workflows/monorepos.md#declare-the-products) to get the target. Run it locally to
see which products a branch affects:

```shell
GITHUB_BASE_REF=main atom DetectAffectedProducts
```

In a workflow, [`WorkflowPresets.ProductValidate`](../workflows/monorepos.md#the-workflow) runs it first and conditions
each product job on its output with `Tex.ProductAffected("AppA")`. A job that uses the output must depend on
`DetectAffectedProducts` directly.

To read the base branch from another CI system, override `IProductsDefinition.AffectedProductsBaseRef`.

## Produced Variables

| Variable           | Argument name       | Example         |
|--------------------|---------------------|-----------------|
| `AffectedProducts` | `affected-products` | `,appa,shared,` |

The wrapping commas let a condition match `,appa,` without also matching a product called `appa2`.

## See also

- [Monorepos](../workflows/monorepos.md): the product model and the validation workflow.
- [Variables in Workflows](../workflows/variables-in-workflows.md): how variables pass between jobs.
- <xref:Invex.Atom.Build.IProductsDefinition>
