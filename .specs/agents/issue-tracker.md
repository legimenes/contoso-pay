# Issue tracker: Local Markdown

Issues and specs for this repo live as markdown files in `.specs/scratch/`.

There is no external tracker. Do not call `gh`, `glab`, or any other tracker CLI for this repo.

## Conventions

- One feature per directory: `.specs/scratch/<feature-slug>/`
- The spec is `.specs/scratch/<feature-slug>/spec.md`
- Implementation issues are one file per ticket at `.specs/scratch/<feature-slug>/issues/<NN>-<slug>.md`, numbered from `01`, never a single combined tickets file
- Blocking edges are recorded as a `Blocked by:` line near the top of each ticket file, listing the numbers/titles of the tickets that gate it
- State is recorded as a `Status:` line near the top of each issue file
- Comments and conversation history append to the bottom of the file under a `## Comments` heading
- Ticket files are committed to git: they are the tracker of record, not throwaway scratch

## When a skill says "publish to the issue tracker"

Create a new file under `.specs/scratch/<feature-slug>/` (creating the directory if needed).

## When a skill says "fetch the relevant ticket"

Read the file at the referenced path. The user will normally pass the path or the issue number directly.

## Wayfinding operations

Used by `/wayfinder`. The **map** is a file with one **child** file per ticket.

- **Map**: `.specs/scratch/<effort>/map.md` (the Notes / Decisions-so-far / Fog body).
- **Child ticket**: `.specs/scratch/<effort>/issues/NN-<slug>.md`, numbered from `01`, with the question in the body. A `Type:` line records the ticket type (`research`/`prototype`/`grilling`/`task`); a `Status:` line records `claimed`/`resolved`.
- **Blocking**: a `Blocked by: NN, NN` line near the top. A ticket is unblocked when every file it lists is `resolved`.
- **Frontier**: scan `.specs/scratch/<effort>/issues/` for files that are open, unblocked, and unclaimed; first by number wins.
- **Claim**: set `Status: claimed` and save before any work.
- **Resolve**: append the answer under an `## Answer` heading, set `Status: resolved`, then append a context pointer (gist + link) to the map's Decisions-so-far in `map.md`.
