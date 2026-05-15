# Vertical Slices - Use a folder per slice

- Status: accepted
- Deciders: Daniel Mackay, Anton Polkanov
- Date: 2026-05-15
- Tags: dotnet, architecture, vsa, project-structure, fastendpoints

## Context and Problem Statement

Each vertical slice in the template started life as a single file. `CreateHeroEndpoint.cs` held the request, the response, the endpoint, the validator, and the OpenAPI summary all together. That works for a small slice: the whole use case fits on one screen.

It stops working as a slice grows. Add a second response shape, a mapping helper, a few more validators, and the file turns into a long scroll where unrelated types compete for attention. The obvious fix for that one slice is to give it a folder and split the types across files. But once some slices are folders and others are files, the structure is inconsistent, and you have to open a slice to find out which kind it is.

Should slices stay as single files and grow a folder only when they need one, or should every slice be a folder from the start?

## Decision Drivers

- **Consistency**: one predictable shape for every slice, regardless of size
- **Scalability**: a slice that grows should not need to be restructured
- **Navigability for people**: find a use case and its parts without guessing
- **Navigability for AI tooling**: a predictable layout makes automated edits more reliable
- **Discoverability**: a slice's full surface area should be visible in one place
- **Low ceremony for simple slices**: a folder of small files should not feel heavy

## Considered Options

1. **Keep single-file-per-slice** — every slice is one file holding all of its types
2. **Hybrid** — a single file by default, promoted to a folder once the slice grows complex
3. **Folder per slice, one file per class** — every slice is a folder; each class gets its own file

## Decision Outcome

Chosen option: **"Folder per slice, one file per class"**, because it removes the inconsistency. Every slice has the same shape whether it holds three types or thirteen, so a slice that grows is never restructured; it just gains files in a folder that already exists. The folder name is the use case, the file name is the class, and that holds everywhere. People and AI tooling can both rely on it.

The namespace follows the folder (for example `Features.Heroes.CreateHero`), so the logical and physical structure stay in step.

### Consequences

- ✅ **Consistent**: every slice looks the same, so there is nothing to check before opening one
- ✅ **Scales without rework**: a complex slice already has the folder it needs
- ✅ **Easier to navigate**: folder name maps to the use case, file name maps to the class
- ✅ **Better for AI tooling**: a predictable layout makes it easier to locate and edit the right file
- ✅ **Sharper diffs and reviews**: a change to one type touches one file
- ❌ **More files**: a slice that used to be a single file is now four or five
- ❌ **More folders** in the feature tree
- ❌ **Slightly more ceremony** when creating a new slice

## Pros and Cons of the Options

### Keep single-file-per-slice

Every slice stays a single file containing all of its types.

- ✅ Fewest files; a simple slice is one short file
- ✅ The whole use case is visible in one scroll
- ❌ Long, unfocused files once a slice grows
- ❌ No clear home for a slice's supporting types
- ❌ Diffs and reviews mix unrelated types in the same file

### Hybrid

Slices stay single files and are promoted to folders only when they grow complex.

- ✅ Low ceremony for the slices that stay simple
- ❌ The structure is inconsistent — some slices are files, some are folders
- ❌ You have to open a slice to learn which kind it is
- ❌ Promotion is a manual judgement call, so it gets applied unevenly

### Folder per slice, one file per class

Every slice is a folder; every class gets its own file.

- ✅ One consistent shape for every slice
- ✅ A growing slice never needs restructuring
- ✅ Predictable for both people and AI tooling
- ❌ More files and folders overall
- ❌ A little more ceremony to create a slice

## Links

- Refines [API - Use FastEndpoints instead of Minimal APIs](20251018-api-use-fastendpoints-instead-of-minimal-apis.md)
