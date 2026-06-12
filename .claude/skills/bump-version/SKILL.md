---
name: bump-version
description: Bump the SSW.VerticalSliceArchitecture.Template NuGet package version to cut a new release. Use when the user says "bump the version", "cut a release", "release a new version", "publish a new package version", or similar for this template repo. Edits the version in VerticalSliceArchitecture.nuspec — which is what triggers the publish — but never pushes or opens a PR.
author: Daniel Mackay
---

## What "bump the version" means in this repo

This repo is a `dotnet new` template published to NuGet as `SSW.VerticalSliceArchitecture.Template`. The release is push-triggered, not tag-triggered.

`.github/workflows/package.yml` runs only when a push to `main` changes `VerticalSliceArchitecture.nuspec`. When it does, it:

1. Packs the template with `nuget pack`.
2. Pushes the `.nupkg` to NuGet.org.
3. Reads `<version>` from the nuspec and creates a matching git tag.
4. Publishes a GitHub release on that tag with auto-generated notes.

So the `<version>` value in `VerticalSliceArchitecture.nuspec` *is* the release. Changing it and merging to `main` ships a new package. Nothing else cuts a release.

## Versioning scheme: CalVer

Versions are calendar dates, `YYYY.MM.DD` (e.g. `2025.12.10`, `2026.06.12`). There is no SemVer major/minor/patch decision — the new version is simply today's date. Confirm with `git tag --sort=-v:refname | head` if you need to see recent releases.

## Steps

1. Read the current `<version>` from `VerticalSliceArchitecture.nuspec`.
2. Compute today's date as `YYYY.MM.DD`. That is the new version.
3. Guard against a same-day re-release: run `git tag` and check the date isn't already a tag. If it is, a release already shipped today — append a numeric suffix (`YYYY.MM.DD.1`) or ask the user how they want to disambiguate. Don't silently overwrite.
4. Update `<version>` to the new value.
5. Refresh `<releaseNotes>`: summarise what changed since the last release. Get the commit list with `git log --oneline <last-tag>..HEAD` and distil it into a short, human summary (one or two sentences). This is the NuGet page blurb — GitHub generates the full PR-list notes separately, so keep it terse.
6. Run the `humanizer` skill over the release-notes prose before finishing (per the repo/global content-writing rule).

## Guardrails

- Don't push. Don't open a PR. Don't merge to `main`. Editing the nuspec on `main` is what triggers the publish, so that final step is the user's call — leave the change as a reviewable diff.
- Only touch `VerticalSliceArchitecture.nuspec`. The package versions in `Directory.Packages.props` are unrelated to the template's release version.
- Don't edit the workflow.

## Verify

The change is metadata only — no build or test impact. Re-read the nuspec diff to confirm the version and notes are right. CI runs `nuget pack` (which needs mono) on merge, so there's nothing useful to run locally.
