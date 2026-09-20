# Phase C: find, edit and delete (#2)

## At a glance

Completes the four tasks from the brief: get by id, the explicit edit commands and delete on the API, the edit form and the delete flow on the front end. Ends in a smoke test of all four tasks.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| C1. Get, update and delete API | ⏳ | Explicit commands, not one update endpoint |
| C2. Edit form | ⏳ | Shares the form component with B4 |
| C3. Delete flow | ⏳ | Smoke test: all four tasks from the brief |
| C4. List and form polish | ⏳ | Only once C3's smoke test passes; see the roadmap's parked list |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. Edit is not one update endpoint. The domain already has `CorrectDetails`, `ChangeService` and `Reschedule`, the API exposes them as commands, and the edit form composes them. Delete is a real delete.

## Design decisions

None beyond the main plan yet. A decision that only this phase needs goes here.

## Steps

**C1. Get, update and delete API.** Get by id, update and delete as a query and two commands, a
validator on the update request, integration tests per handler.

**C2. Edit form.** The form component from B4 serving edit through its own page route with a loader.
Tests covering a loaded edit and a rejected submit.

**C3. Delete flow.** Native `<dialog>` confirmation, fetcher submit against an intent, optimistic
removal from the list, failure surfaced without losing the row. Ends with a smoke test of all four
tasks from the brief.

**C4. List and form polish.** The five items the phase B smoke test raised and the roadmap parks:
error styling on the field itself with `aria-invalid` and a described-by message, clearing a stale
message when its input changes without re-validating in the browser, a clear-search control that the
empty state also offers, status badges coloured per status, and `autocomplete` attributes on the
contact fields. Runs only after C3 is smoke-tested, and does not hold up phase D.
