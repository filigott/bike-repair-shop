# Phase C: find, edit and delete (#2)

## At a glance

Completes the four tasks from the brief: get by id, the explicit edit commands and delete on the API, the edit form and the delete flow on the front end. Ends in a smoke test of all four tasks.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| C1. Get, update and delete API | ✅ | Explicit commands, not one update endpoint |
| C1a. Nullable due date on the request shapes | ✅ | A missing date is a field error, not `$.dueDate` |
| C1b. Get by id | ✅ | Brings `NotFoundException` and its 404 mapping |
| C1c. The three edit commands | ✅ | Details, service and due date, one endpoint each |
| C1c.1 Movable clock in the harness | ✅ | Fake `TimeProvider`, so an overdue order can be tested |
| C1d. Delete | ✅ | A real delete, 204 on success |
| C2. Edit form | ✅ | Order detail page, one intent-driven form per command |
| C2a. Shared form component | ✅ | Two legends, and the date input stops blocking the submit |
| C2b. Order detail page | ✅ | One loader, one action, one form per command |
| C3. Delete flow | ✅ | Confirmed on the list, optimistic, restores the row on failure |
| C3.1 Smoke test of the four brief tasks | ⏳ | Blocks calling the phase done |
| C4. List and form polish | ⏳ | Only once C3's smoke test passes; see the roadmap's parked list |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. Edit is not one update endpoint. The domain already has `CorrectDetails`, `ChangeService` and `Reschedule`, the API exposes them as commands, and the edit form composes them. Delete is a real delete.

## Design decisions

**Edit is three endpoints, one per domain method.** `PUT /api/orders/{id}/details`, `/service` and
`/due-date` map one to one onto `CorrectDetails`, `ChangeService` and `Reschedule`, instead of one
`PUT /api/orders/{id}` carrying the whole editable shape.

**The UI matches the commands, one form each.** The first attempt was a single edit form that diffed
what it loaded against what was submitted and fanned out to all three endpoints. That is the generic
edit form the main plan argues against, dressed as commands: it needed a diff, a call order and a
partial-save caveat, none of which are the user's problem. The order detail page now hosts one small
form per command, each with its own submit and its own fetcher, all posting to one route action that
switches on an `OrderIntent`. One gesture is one command is one atomic write, and the rule about a
past due date never comes up, because the reschedule form is only submitted when a reschedule is
meant. A page route per command is the next step up if a form outgrows the page; not needed yet.

**A past due date stays refused, without exception.** `SetDueDate` keeps its guard and nothing in the
UI works around it. An overdue order's contact details are corrected through the details form, which
never touches the date.

**404 is an API concern.** `NotFoundException` lives in `Api/Exceptions` and is mapped alongside the
domain exceptions, because "no row with that id" is not a broken invariant.

**The integration harness owns the day.** `ShopClock` reads `TimeProvider`, so the test host swaps in
a fake one and the suite can move the shop's calendar. Without it nothing can assert `isOverdue`, and
the rule that an overdue order stays editable is untestable above the domain. The alternative, a
`MaintenanceOrder` seeded with a past due date, is impossible by design: `Place` rejects one.

## Steps

**C1. Get, update and delete API.** One query and four commands: get by id, correct the details,
change the service, reschedule, delete. A validator per request shape and integration tests per
handler. Starts by making `DueDate` nullable on the request shapes with `NotNull()` on the validator,
so a missing date is a field error rather than the binder's `$.dueDate`, which the edit form would
otherwise hit the same way the place form did.

**C2. Edit form.** An order detail page at `/orders/:id` with its own loader, hosting one form per
edit command. The field groups are factored out of the place form so both compose the same inputs.
Tests covering the prefill, one test per command proving it sends only its own call, and a rejected
submit landing on the form that made it.

**C3. Delete flow.** Native `<dialog>` confirmation on the list, fetcher submit against an intent,
optimistic removal, failure surfaced without losing the row. Delete is offered on the list only,
because removing a mistaken order is a counter workflow rather than something reached through the
detail page. Ends with a manual smoke test of all four tasks from the brief.

**C4. List and form polish.** The five items the phase B smoke test raised and the roadmap parks:
error styling on the field itself with `aria-invalid` and a described-by message, clearing a stale
message when its input changes without re-validating in the browser, a clear-search control that the
empty state also offers, status badges coloured per status, and `autocomplete` attributes on the
contact fields. Runs only after C3 is smoke-tested, and does not hold up phase D.
