# Triage Labels

The skills speak in terms of five canonical triage roles. This file maps those roles to the actual label strings used in this repo's issue tracker.

| Label in mattpocock/skills | Label in our tracker | Meaning                                  |
| -------------------------- | -------------------- | ---------------------------------------- |
| `needs-triage`             | `needs-triage`       | Maintainer needs to evaluate this issue  |
| `needs-info`               | `needs-info`         | Waiting on reporter for more information |
| `ready-for-agent`          | `ready-for-agent`    | Fully specified, ready for an AFK agent  |
| `ready-for-human`          | `ready-for-human`    | Requires human implementation            |
| `wontfix`                  | `wontfix`            | Will not be actioned                     |

When a skill mentions a role (e.g. "apply the AFK-ready triage label"), use the corresponding label string from this table.

Edit the right-hand column to match whatever vocabulary you actually use.

## These labels are not all created yet

Of the five, only `wontfix` currently exists on the repo — `gh issue edit --add-label needs-triage` fails with `'needs-triage' not found` until the label is created. Check with `gh label list`, and create anything missing before the first triage run:

```bash
gh label create needs-triage   --description "Maintainer needs to evaluate this issue" --color FBCA04
gh label create needs-info     --description "Waiting on reporter for more information" --color D876E3
gh label create ready-for-agent --description "Fully specified, ready for an AFK agent" --color 0E8A16
gh label create ready-for-human --description "Requires human implementation"           --color 1D76DB
```

`gh label create` errors if the label already exists; add `--force` to make it idempotent.
