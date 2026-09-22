# Phase K: status legend (#13)

## At a glance

The orders table gains a legend that names the four statuses at rest and explains the lifecycle when
the user asks for it. The edges come from a new status catalogue endpoint read off the domain's edge
table, the way the service catalogue already is, so no edge is ever spelled out in a locale file.
The per-order `allowedTransitions` comes off the response in the same track, having been projected
for this feature and read by nothing.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| K1. Status catalogue endpoint | ✅ | `Features/OrderStatuses` mirroring the service catalogue |
| K2. The legend above the table | 🟡 | Badges at rest, edges on disclosure, both locales |
| K3. Drop the per-order transitions | ⏳ | Off the response, off the fixtures, out of CLAUDE.md |

## Context

First item in the roadmap's `Next up` and the first unit under the pull request rule, so it runs on
`feat/13-status-legend` off an issue rather than on `main`. The branch is stacked on
`feat/10-detail-page-rework` until that batch merges, because the legend sits on a page that batch
rewrote.

The lifecycle is the one real domain rule in an otherwise CRUD-shaped product, and until now it has
been invisible to the user. [plan-j-detail-page.md](plan-j-detail-page.md) put the commands on the
page, which tells a user what they can do to one order. It does not tell them what the statuses are
or how they connect, and the terminal edge cannot be discovered by clicking at all: that a cancelled
order is final is only learned by cancelling one.

## Design decisions

**A catalogue endpoint, not the per-order field.** `AllowedTransitions` on an order answers "where
can this one go", which is the wrong shape for a legend that describes all four statuses and appears
above a list. It is also projected onto the detail response only, never onto the list summary, so a
legend over the table could not read it even if the shape fit. `Features/ServiceTypes` is the
precedent: a small slice whose handler reads the domain and answers with a list.

**The legend shows badges at rest and edges on request.** Four badges and their names is what a
reader needs to decode the table, and the transitions underneath them would crowd the thing they
sit above. Settled with the user on 2026-09-22.

**The disclosure is a native `<details>`.** Its summary carries the four badges, and opening it
reveals one row per status with the statuses it can become. No state, no effect, keyboard and screen
reader behaviour for free. A popover per badge was the alternative and answers a narrower question
("what happens to a Received order") at the cost of four anchored overlays and their positioning.

**The edges stay data.** A row reads as the status badge, a connective from the locale file, then a
badge per target. The locale holds the connective and the status names it already holds; it never
holds an edge. Writing "Received can be started or cancelled" as a sentence per status would put the
matrix in two locale files, which is the duplication this endpoint exists to prevent.

## Steps

**K1. Status catalogue endpoint.** `Features/OrderStatuses` with a query, a handler, an
`OrderStatusItem` model and `AddOrderStatuses()`, plus `OrderStatusesController`. Each item carries
the status, its allowed actions and its target statuses, all read from the existing edge table.

Tests: the catalogue covers every status the enum declares, the edges match the domain matrix rather
than a second copy, and `Cancelled` comes back with none.

**K2. The legend above the table.** An endpoint wrapper, the orders loader fetching it alongside the
list, and a component rendering the summary badges and the disclosed rows. Both locales gain the
legend title and the connective.

Tests: the legend names every status the catalogue returns, and the edges it shows are the ones the
catalogue gave rather than anything computed in the browser.

**K3. Drop the per-order transitions.** Remove `AllowedTransitions` from `MaintenanceOrderResponse`,
regenerate `openapi.generated.ts`, drop the field from the front-end test fixtures, and take it out
of the list of projected fields in `CLAUDE.md`. `AllowedActions` stays: the action column renders
from it.

Reconfirm at implementation time: whether the catalogue returns action names as well as target
statuses. K2 needs only the targets, but the shape is cheap to include and the alternative is a
second endpoint change later.

## Out of scope

- Status change history (#5), which has no data behind it.
- The badge colours themselves, which are already carrying meaning.
- A legend on the detail page. That page states one order's status and offers its commands, so it
  answers the question locally already.

## Open questions

None outstanding. The disclosure mechanism and the rest-state were settled on 2026-09-22 and are
recorded above as decisions.
