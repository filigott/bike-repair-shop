> **Reference doc: load only when working in this area**
> - **Covers:** what the build has cost so far and how far the result is from the brief
> - **Used by:** [ROADMAP.md](../ROADMAP.md) and [plan-bike-repair-shop.md](../plan-bike-repair-shop.md); read before adding a phase or a design decision
> - **Updated:** 2026-09-20

# Time and scope, measured

The brief asks for four staff tasks (place, find, edit, delete), a React page, an HTTP API holding
the business logic, a database, and `docker compose up` for the whole stack. This doc keeps the plan
structure honest by measuring it against that list rather than against its own tidiness.

## The clock

| Window | Length | What landed |
|---|---|---|
| Session start to 17:36 | 2h00 | No commits: brief, stack selection, registry version checks, throwaway scaffolds, `CLAUDE.md`, the plan skeleton |
| 17:36 to 19:52 | 2h16 | Phase A: domain, EF mapping, initial migration, integration harness, health check |
| 20:10 to 20:54 | 0h44 | Phase B1 and B2: place and list API, front-end scaffold, shell, transport |
| 21:30 to 22:00 | 0h30 | Evening session: B3, the orders list page |

Day total before the evening session: 5h30 and 32 commits. Two of those hours preceded the first
commit, and ten of the commits touch only plans or conventions.

## Against the brief

| Requirement | State after the first working day |
|---|---|
| Place an order | API yes, no UI |
| Find an order | List and customer search API yes, no UI |
| Edit an order | Not started |
| Delete an order | Not started |
| React page consuming the API | Shell, i18n and transport only; no page calls an endpoint |
| HTTP API with the business logic | Three endpoints: list orders, place order, list service types |
| Database holding the services | Done: seeded catalogue, migrations, integration tests |
| `docker compose up` runs the stack | Database service only; no API or front-end image |

The first 5h30 bought the whole foundation and none of the four staff tasks end to end.

## Where scope ran ahead of the brief

- **Seven phases for a brief that needs three.** Search (E), lifecycle history (F) and hardening (G)
  are additions made here, not asks.
- **The order state machine was built first, in phase A.** No endpoint and no screen reaches it, and
  none of the four required tasks needs it.
- **Concurrency, paging and the queryable-value-object refactor precede their consumers**, the last
  of them serving a phase E filter the brief never asks for.

The foundation underneath is worth having: the migrated database, the container-backed harness and
the typed client are what make the remaining verticals quick. They just have to start paying back.

## The correction

- Nothing from phases E, F or G until `docker compose up` serves all four staff tasks.
- Plan status edits ride in the step's own commit instead of taking a commit of their own.
- A new design decision needs a consumer in the current or next phase, or it waits.
- Estimated remaining work to brief-complete: about 3h30, an estimate rather than a measurement.
