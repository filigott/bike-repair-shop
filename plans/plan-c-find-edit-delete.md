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
| C2. Edit form | 🟡 | Shares the form component with B4; groups the fields under two headings |
| C3. Delete flow | ⏳ | Smoke test: all four tasks from the brief |
| C4. List and form polish | ⏳ | Only once C3's smoke test passes; see the roadmap's parked list |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. Edit is not one update endpoint. The domain already has `CorrectDetails`, `ChangeService` and `Reschedule`, the API exposes them as commands, and the edit form composes them. Delete is a real delete.

## Design decisions

**Edit is three endpoints, one per domain method.** `PUT /api/orders/{id}/details`, `/service` and
`/due-date` map one to one onto `CorrectDetails`, `ChangeService` and `Reschedule`. The alternative,
one `PUT /api/orders/{id}` carrying the whole editable shape, is a single atomic round trip, but it
is the generic update endpoint the main plan argues against and it reschedules on every save. The
cost taken instead: the edit form diffs what it loaded against what was submitted and issues up to
three calls, which are not atomic. Acceptable because each call is independently valid and the form
reloads afterwards.

**A past due date is only rejected when it changes.** `SetDueDate` keeps its guard, so the domain
rule is untouched. The edit form skips the `/due-date` call when the submitted date equals the loaded
one, which is what lets an overdue order have its phone number corrected without inventing a new
date. Putting the exception in the domain, by allowing a past date that equals the stored one, was
rejected: it weakens an invariant to serve a UI flow.

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

**C2. Edit form.** The form component from B4 serving edit through its own page route with a loader,
with the fields grouped under a customer heading and a bike and service heading while it is being
factored for reuse. Tests covering a loaded edit and a rejected submit.

**C3. Delete flow.** Native `<dialog>` confirmation, fetcher submit against an intent, optimistic
removal from the list, failure surfaced without losing the row. Ends with a smoke test of all four
tasks from the brief.

**C4. List and form polish.** The five items the phase B smoke test raised and the roadmap parks:
error styling on the field itself with `aria-invalid` and a described-by message, clearing a stale
message when its input changes without re-validating in the browser, a clear-search control that the
empty state also offers, status badges coloured per status, and `autocomplete` attributes on the
contact fields. Runs only after C3 is smoke-tested, and does not hold up phase D.
