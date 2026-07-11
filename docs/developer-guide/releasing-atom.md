# Releasing Atom

Atom releases use the generated `CreateRelease` GitHub Actions workflow. Continuous `Build` and
`Validate` workflows never receive production publishing credentials.

## Required repository configuration

Create a GitHub Environment named `production` and configure:

1. Required reviewers who are authorised to publish Atom.
2. A deployment branch rule allowing only `main`.
3. The `NUGET_PUSH_API_KEY` environment secret.

The generated workflow independently requires `workflow_dispatch` on `refs/heads/main`. Environment
protection is still required because reviewer and deployment-branch rules are repository settings,
not properties stored in the workflow file.

## Release procedure

1. Confirm the intended release commit is merged to `main` and the validation workflow is green.
2. Open **Actions > CreateRelease > Run workflow**.
3. Select `main`; do not dispatch another branch or tag.
4. Review and approve the `production` deployment.
5. Confirm the workflow builds, tests, packages, creates an exact-SHA draft, pushes NuGet packages,
   verifies every expected GitHub release asset, publishes the release, and then publishes docs.

The deployment job is the only job that receives the NuGet key. It creates a draft release tied to
`GITHUB_SHA` and makes it public only after package pushes and the remote asset manifest succeed.

## Failure and rollback

If deployment fails after draft creation, Atom deletes the draft and its uploaded assets
automatically. If draft deletion also fails, delete the draft and tag manually before retrying.

NuGet publication is immutable and cannot be rolled back transactionally. If one package was
published before a later push failed:

1. Deprecate every package published with the affected version and explain that the release is
   incomplete.
2. Do not overwrite or unlist-and-reuse that version.
3. Fix the failure and publish a new patch version.
4. Delete any remaining draft release and assets for the failed version.

For a failed permission or ref check, do not bypass the condition. Dispatch the workflow from
`main` and use the protected `production` environment.
