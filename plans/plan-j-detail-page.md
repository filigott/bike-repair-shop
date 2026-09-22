# Phase J: detail page rework (#10)

## At a glance

The order detail page stops being three always-open forms and becomes a read-only view of the
order with an action area underneath it. Seven commands live there: four lifecycle moves that are
a confirm, three edit commands that each open a dialog holding only their own form, and delete set
apart from the rest. Which actions appear comes from `allowedTransitions` and `isEditable` on the
response, so a closed order simply has fewer, and the browser decides nothing.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| J1. Action area with the lifecycle moves | ⏳ | Four transitions as confirms; inline forms stay |
| J2. Edit commands move into dialogs | ⏳ | Page goes read-only; three more buttons on the bar |
| J3. Delete, layout and the duplication | ⏳ | Delete on the page, back link, type, facts once |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), first in the roadmap's `Next up`
queue. Four of the eight things the phase C manual pass raised are on this page, and the issue was
deliberately held back until [plan-f-lifecycle.md](plan-f-lifecycle.md) shipped the transition
endpoints, so the action area is built once with all seven commands rather than twice.

The issue lists status transitions as out of scope. That was written before they existed over HTTP
and no longer holds: they are the larger half of this unit.

Two of the issue's three open questions were settled on 2026-09-22 and the third answers itself
from the contract. They are recorded below as decisions.

## Design decisions

**An action bar under the summary, not a right-hand column.** The order's facts read top to bottom
and the actions follow them, which matches the task: see what it is, then decide what to do. A
sticky right column reads well on a counter monitor and costs a breakpoint plus a stacking rule for
phone width, on a page of about six facts that never scrolls.

**A closed order shows no actions, not disabled ones.** `allowedTransitions` is empty for
`Cancelled` and `isEditable` is false for both closed states, so absence falls straight out of the
projection. Greying buttons out would mean the browser deciding what is unavailable, which is the
one thing the UI is not allowed to work out for itself. The existing "this order can no longer be
changed" line stays and carries the explanation.

**Delete joins the page, set apart from the rest.** Someone who opened an order to check it is the
right one should not have to navigate back to the list to remove it. It sits away from the
constructive actions and reuses the confirmation the list already has, which now names the order
number as well as the customer, so acting on the wrong order takes a deliberate misread.

**Two dialog shapes, not one.** A transition is a confirm: a sentence and two buttons. An edit is a
form: fields, Save and Cancel. Forcing both through one component would mean a dialog that
sometimes has children and sometimes does not, so there are two small components instead.

**The button label comes from the pair, not the target.** `Received` to `InProgress` is "Start" and
`Completed` to `InProgress` is "Reopen", the same target status from two different origins. The UI
maps the pair to a command, which is also why the API keeps `start` and `reopen` as separate
endpoints despite being the same domain call.

**One route action, one intent enum.** The existing envelope already switches on an `intent` read
from `FormData`, and the transitions and delete join it rather than gaining route resources of their
own. Adding seven page routes for seven commands was the alternative and costs seven loaders and a
full navigation apiece.

## Steps

Ordered so the page works after every commit. The inline forms are not removed until the dialogs
that replace them exist.

**J1. Action area with the lifecycle moves.** Endpoint wrappers for the four transitions, four
intents, the route action branches, a confirm dialog component, and an action bar rendered from
`allowedTransitions`. The three inline edit forms stay exactly where they are for this step, below
the new bar. Both locales gain the action labels and the confirm sentences.

Tests: the bar offers only the moves the loaded order allows, the confirm posts the right command,
and a cancelled order offers none.

**J2. Edit commands move into dialogs.** The three `CommandForm` blocks become three dialogs opened
from three more buttons on the bar, and the page body goes read-only: the summary card plus the
contact, bike and notes card, for open and closed orders alike. The `isEditable` branch stops
choosing between two page bodies and starts choosing which buttons exist.

Tests: opening a dialog and saving sends only that command, and the page shows no form until a
button is pressed.

**J3. Delete, layout and the duplication.** Delete on the bar, set apart, reusing the list's
confirmation. Then the four presentation complaints from the manual pass: the back link stops being
a ghost button in the corner, the body type scale comes up, `Service` and `Due date` appear once
rather than in both the summary and a form legend, and the page separates what the order is from
what can be done to it.

Reconfirm at implementation time: whether delete stays available on a closed order. It is a real
delete rather than a state, so nothing in the domain forbids it, but the page may still want to
withhold it once an order is cancelled.

## Out of scope

- The status legend above the orders table, which is the next roadmap item and belongs to the list.
- The status history and `isReopened`, which have no data behind them yet.
- The list polish still tracked under #2.
- Localized API validation messages (#11), which this page will surface unchanged.

## Open questions

None outstanding. The layout, delete's place and the closed-order behaviour were settled on
2026-09-22.
