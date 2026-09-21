> **Reference doc: load only when working in this area**
> - **Covers:** what the build has cost so far and how far the result is from the brief
> - **Used by:** [ROADMAP.md](../ROADMAP.md) and [plan-bike-repair-shop.md](../plan-bike-repair-shop.md); read before adding a phase or a design decision
> - **Updated:** 2026-09-21

# Time and scope, measured

The brief asks for four staff tasks (place, find, edit, delete), a React page, an HTTP API holding
the business logic, a database, and `docker compose up` for the whole stack. This doc keeps the plan
structure honest by measuring it against that list rather than against its own tidiness.

## The clock

| Window | Length | What landed |
|---|---|---|
| Day one, session start to 17:36 | 2h00 | No commits: brief, stack selection, registry version checks, throwaway scaffolds, `CLAUDE.md`, the plan skeleton |
| Day one, 17:36 to 19:52 | 2h16 | Phase A: domain, EF mapping, initial migration, integration harness, health check |
| Day one, 20:10 to 20:54 | 0h44 | Phase B1 and B2: place and list API, front-end scaffold, shell, transport |
| Day one, 21:30 to 22:00 | 0h30 | Phase B3: the orders list page |
| Day two, 12:10 to 14:27 | 1h00 | Phase C: order by id, the three edit commands, delete, the detail page with one form per command, delete from the list |

Day one total: 5h30 and 32 commits. Two of those hours preceded the first commit, and ten of the
commits touch only plans or conventions.

Day two is counted as focused time, not wall clock. The window spans 2h17 and carried 10 commits,
but it was shared with other work rather than worked straight through, so 1h00 is the figure that
goes into the running total: **6h30 across two days**.

## Against the brief

| Requirement | State after day two |
|---|---|
| Place an order | Done end to end |
| Find an order | Done end to end: list, customer search, paging |
| Edit an order | API and UI done, awaiting the manual pass |
| Delete an order | API and UI done, awaiting the manual pass |
| React page consuming the API | Three routes: the list, the place form, the order detail page |
| HTTP API with the business logic | Eight endpoints: list, get, place, correct details, change service, reschedule, delete, service catalogue |
| Database holding the services | Done: seeded catalogue, migrations, integration tests |
| `docker compose up` runs the stack | Database service only; no API or front-end image. This is phase D and the last gap in the brief |

The first 5h30 bought the whole foundation and none of the four staff tasks end to end. The next
hour turned that into all four, which is the payback the foundation was bet on. What the brief still
misses is packaging, not features.

## Where scope ran ahead of the brief

- **Seven phases for a brief that needs three.** Search (E), lifecycle history (F) and hardening (G)
  are additions made here, not asks.
- **The order state machine was built first, in phase A.** Phase C finally reaches it: `isEditable`
  gates the detail page's forms, and a closed order shows a notice instead.
- **Concurrency, paging and the queryable-value-object refactor precede their consumers**, the last
  of them serving a phase E filter the brief never asks for.

The foundation underneath is worth having: the migrated database, the container-backed harness and
the typed client are what made phase C quick.

## The correction

- Nothing from phases E, F or G until `docker compose up` serves all four staff tasks.
- Plan status edits ride in the step's own commit instead of taking a commit of their own.
- A new design decision needs a consumer in the current or next phase, or it waits.
- Estimated remaining work to brief-complete: about 1h30, the smoke test plus C4 polish plus phase D
  packaging. The 3h30 estimated after day one is holding.

## What the hour cost

One rebuild, worth naming so the pattern is visible. The edit UI was first built as a single form
that diffed the order and fanned out to the three commands, which was rejected on sight: it puts
concurrent writes and a race behind one save button, and it hides the commands the API exposes. The
replacement is one form per command on a detail page, an intent read from `FormData`. Roughly
35 minutes, and the rule it confirms is already in `CLAUDE.md` under actions being explicit on the
API and in the UI alike. Check a screen against that rule before building it, not after.
