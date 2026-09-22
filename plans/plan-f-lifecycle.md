# Phase F: lifecycle over HTTP (#5)

## At a glance

Four named commands that move an order through its states, calling the domain methods that already
exist and are already unit-tested. The transition matrix stays the only authority on what is legal,
an illegal move comes back as a 409, and nothing new is projected onto the response because
`allowedTransitions` is already there. This is the API half of #5 only: the status UI and the
history table are separate units and the issue stays open after it.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| F1. Transition commands over HTTP | ✅ | Four commands; coverage widened to the seam after review |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md). Second in the roadmap's `Next up`
queue, and the largest functional gap left: the state machine is written, unit-tested and projected
onto every response, and no caller can reach it. The demo seeder in
[plan-h-demo-data.md](plan-h-demo-data.md) had to call the domain methods in process to produce a
completed order, which is the clearest evidence the HTTP surface is missing.

Two of the three open questions on the issue were answered by the code before this phase started.
The response carries `allowedTransitions` as a list of statuses, not capability flags, and
`InvalidStateTransitionException` already maps to a 409 with ProblemDetails in
`ProblemDetailsExceptionHandler`. The third, whether the history is shown and whether a reopened
order is marked, belongs to the unit that builds the history.

## Design decisions

**Four named commands, not a status field.** `start`, `complete`, `cancel` and `reopen` are separate
endpoints, each calling the domain method of the same name. A single `PUT /{id}/status` carrying a
target would be smaller and would be the wrong shape: the whole design puts the state machine in the
code rather than in the head of whoever is at the counter, and a target-status field hands the
sequencing back to the caller.

**`start` and `reopen` stay interchangeable on the server.** Both are `TransitionTo(InProgress)`,
so posting to `start` on a completed order does what `reopen` does. Giving each an origin guard
would read better and would put a second transition rule beside the matrix, which is the one thing
this design exists to prevent. The names are for the caller; the matrix decides what is legal. The
UI labels the move by the pair it is travelling between, so it never offers the wrong word.

**`POST`, not `PUT`.** These are actions rather than replacements of a sub-resource, and none of
them is idempotent: a second `start` on an order already in progress is a 409, not a no-op. The
existing edit commands stay `PUT` because each does replace the thing it names.

**Each command answers with the updated order.** The same `MaintenanceOrderResponse` the edit
commands return, so the caller gets the new status and the new `allowedTransitions` without a second
round trip. The front end reloads through its loader anyway, but the API should not depend on that.

**No concurrency token yet.** A transition posted against a stale view is a real race and it is
`#6`'s work, which adds `version` across every mutating endpoint at once. Adding it to four
endpoints here and the rest later would mean two shapes in the contract at the same time.

## Steps

**F1. Transition commands over HTTP.** Four bare command records, four handlers and four controller
actions. The handlers are the existing shape: load, call the domain method, save, log, project. No
new domain code, no new mapping, no new DTO.

Tests: one integration test per command asserting the persisted status from a fresh read rather
than from the returned entity, one asserting an illegal move returns 409 with ProblemDetails, and
one asserting an unknown id returns 404. The transition matrix itself is already covered by the
unit suite and is not re-driven over HTTP.

Widened after review to cover the seam rather than the rules. A single test walks an order from
placed to completed through the API alone and watches `allowedTransitions` narrow at each hop, which
is the only place the four endpoints are proved to compose. One pins `isEditable` going false once
an order closes, since that flag is what the UI hangs the edit affordance on. One pins that posting
to `start` on a completed order reopens it, because that is a decision taken deliberately and an
origin guard added later would otherwise break nothing. A theory drives all three moves against a
cancelled order, which is the only absorbing state and the one worth stating at the HTTP boundary.

Reconfirm at implementation time: whether `cancel` is allowed from `Completed`. The matrix currently
says no, so a completed order can only be reopened, and that is the behaviour the tests will pin.

## Out of scope

- The status history. A second table, a migration and somewhere to show it, scoped on its own in the
  roadmap because the brief never asks for it and nothing else waits on it.
- `isReopened` on the response, which is derived from the history and cannot exist before it does.
- The status actions in the UI, which are part of the detail page rework and are built once with
  the three edit commands rather than twice.
- The concurrency token, which is `#6`.

## Open questions

None. The endpoint surface, the absence of origin guards and the test depth were settled on
2026-09-22.
