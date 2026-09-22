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
| C3.1 Smoke test of the four brief tasks | ✅ | Six of eight steps clean; 5 and 8 unreachable, see #8 |
| C4. List and form polish | 🟡 | Split once the visual half was pulled ahead of packaging |
| C4a. Status colours, soft actions, readable labels | ✅ | Also fixed two DaisyUI v4 classes that render nothing in v5 |
| C4b. The four items left from the phase B pass | ⏳ | Status badges landed in C4a |

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
contact fields.

The phase C pass added three more, and they come first because they are markup in components that
already exist: a colour per status wherever a status is shown, a soft fill on the row and header
actions so a button carries a background before it is hovered, and a readable size for the form
labels, which `FormField` renders through DaisyUI's `label-text` and is therefore one change for
every form. The colour map is typed `Record<OrderStatus, string>`, so a new status fails the build
rather than falling through to a default.

### C3.1 smoke-test checklist

Run `docker compose up -d db`, `dotnet run --project backend/BikeRepairShop.Api`, and
`npm --prefix frontend run dev` in separate terminals. The developer runs all three.

1. **Find.** The list shows the demo orders from #8, the search box matches on name, phone and
   email, and paging holds at a page size of 20. Only the service catalogue is seeded by a
   migration, so on a fresh volume without #8 this step has nothing to run against.
2. **Place.** Place an order, leaving the due date empty: the API answers with a `dueDate` field
   error under the input rather than a page-level failure. Fill it in and the order appears in the
   list.
3. **Open.** The row's Open link reaches `/orders/:id`, which shows the status, service, due date and
   placed date, plus one form per command.
4. **Edit, one command at a time.** Change the customer name and save details: only that form
   submits, and the button shows its own pending state. Change the service. Reschedule to a later
   date. Reschedule to a past date and confirm the refusal lands under the due-date form, not on the
   others.
5. **Closed order.** Nothing moves an order off `Received` until #5, and no endpoint accepts a
   status, so the only closed order is one #8 seeded. Its page shows a notice and no forms.
6. **Delete.** Delete from the list: the dialog names the customer, Escape and the backdrop both
   cancel, and confirming removes the row.
7. **Locale.** Switch to Norwegian and confirm every new string on the detail page, the dialog and
   the list has a translation.
8. **Overdue.** An order whose due date has passed shows the overdue badge in the list and on the
   detail page, and its details are still editable. Only #8 can produce one: `Place` and
   `Reschedule` both refuse a past due date, and the API runs on the system clock.

### C3.1 outcome, 2026-09-21

Steps 1, 2, 3, 4, 6 and 7 passed. Find, place with the empty-due-date field error, open, all three
edit commands with the past-date refusal landing on the right form, the delete dialog, and the
Norwegian locale all behaved as built.

Step 5 and step 8 could not be run. Nothing moves an order off `Received`, so a closed order is
unreachable, and the checklist's suggestion to reach one through the API directly is wrong: there is
no such endpoint until #5. An overdue order cannot be produced either, because `Place` and
`Reschedule` both refuse a past due date on purpose and the API runs on `TimeProvider.System`. Both
are now covered by #8, and step 5 becomes real once #5 lands.

What the pass raised beyond the checklist is in [reference/smoke-test-findings.md](reference/smoke-test-findings.md), under `Parked from the phase C smoke
test` and is split across #8, #9, #10 and #11. The detail page reworking into one dialog per command
supersedes the C2 decision below, which argued for one small form per command all visible at once:
the shape was right, the presentation was not.
