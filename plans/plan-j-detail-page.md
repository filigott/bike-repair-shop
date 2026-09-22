# Phase J: detail page rework (#10)

## At a glance

The order detail page stops being three always-open forms and becomes a read-only view of the
order with an action area underneath it. Seven commands live there: four lifecycle moves that are
a confirm, three edit commands that each open a dialog holding only their own form, and delete set
apart from the rest. Which actions appear comes from `allowedActions` and `isEditable` on the
response, so a closed order simply has fewer, and the browser decides nothing. The first step earns
that by rekeying the domain's state machine from target statuses to named actions.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| J1. Rekey the state machine by action | ✅ | One edge table; allowedActions on the response |
| J2. Action area with the lifecycle moves | ✅ | Four actions as confirms; inline forms stay |
| J3. Edit commands move into dialogs | ✅ | Page goes read-only; three more buttons on the bar |
| J4a. Delete, layout and the duplication | ✅ | Delete on the page, back link, type, facts once |
| J4c. Actions column and the empty rows | ✅ | Actions beside the facts, grouped; every fact keeps its row |
| J4b. Dialog state in the URL | ✅ | `?edit=` param, redirect on success, no effect left |
| J4d. Thin the route module | 🟡 | Command dispatch and the edit dialogs leave the route |

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

**A closed order shows no actions, not disabled ones.** `allowedActions` is empty for
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

**The state machine is keyed by the action, not by the target status.** `Received` to `InProgress`
is "Start" and `Completed` to `InProgress` is "Reopen": one target, two different edges. A matrix
keyed by `from -> [to]` cannot tell them apart, so something has to bridge statuses back to command
names, and every placement of that bridge is wrong. In the browser it rots when a status is added.
On the server it is still a hand-written lookup and two response fields that can disagree.

Keying the machine by the action removes the bridge instead of moving it. One table of
`(from, action) -> to` edges is the single source, `AllowedActions` is what the UI renders,
`AllowedTransitions` is derived from it for the status legend, and the two cannot drift because one
is computed from the other. `Start` and `Reopen` gain real origins, so posting `start` to a
completed order is refused without a second rule sitting beside the matrix. The better exception
message falls out rather than being written, which is the sign the shape is right.

**One route action, one intent enum.** The existing envelope already switches on an `intent` read
from `FormData`, and the transitions and delete join it rather than gaining route resources of their
own. Adding seven page routes for seven commands was the alternative and costs seven loaders and a
full navigation apiece.

## Steps

Ordered so the page works after every commit. The inline forms are not removed until the dialogs
that replace them exist.

**J1. Rekey the state machine by action.** An `OrderAction` enum in the domain and one edge table
replacing the `from -> [to]` matrix. `AllowedActions` and `AllowedTransitions` both derive from it,
`InvalidStateTransitionException` carries the action rather than a target status, and the entity's
four transition methods become thin wrappers over one `Apply`. The response gains `allowedActions`.

Tests first, since this is a transition matrix: the unit suite gains the per-status action sets and
the origin refusals, and the two existing assertions that pin the old behaviour change. Starting a
completed order stops being a reopen and becomes a 409, which is the behaviour this rekeying exists
to make expressible.

**J2. Action area with the lifecycle moves.** Endpoint wrappers for the four transitions, four
intents, the route action branches, a confirm dialog component, and an action bar rendered from
`allowedActions`. The three inline edit forms stay exactly where they are for this step, below
the new bar. Both locales gain the action labels and the confirm sentences.

Tests: the bar offers only the moves the loaded order allows, the confirm posts the right command,
and a cancelled order offers none.

**J3. Edit commands move into dialogs.** The three `CommandForm` blocks become three dialogs opened
from three more buttons on the bar, and the page body goes read-only: the summary card plus the
contact, bike and notes card, for open and closed orders alike. The `isEditable` branch stops
choosing between two page bodies and starts choosing which buttons exist.

Tests: opening a dialog and saving sends only that command, and the page shows no form until a
button is pressed.

**J4a. Delete, layout and the duplication.** Delete on the bar, set apart, reusing the list's
confirmation. Then the presentation complaints from the manual pass: the back link stops being a
ghost button in the corner, the body type scale comes up, and the page separates what the order is
from what can be done to it. The facts read first as two cards, the actions follow in their own
boxed area, and the narrow column is centred rather than pinned to the left edge.

`Service` and `Due date` no longer appear twice: J3 took the form legends off the page, so the
summary card is the only place either is stated. The complaint is closed by that step, not by this
one.

Settled on 2026-09-22: **delete stays available on a closed order.** It removes a record rather than
moving a state, nothing in the domain forbids it, and a cancellation made in error should be
clearable without a second route. It is never in `allowedActions`, so it is drawn on its own and
the projection stays the only thing deciding lifecycle moves.

**J4c. Actions column and the empty rows.** A second layout pass, from looking at what J4a
produced. Four things: the actions move off the bottom of the page and stand beside the facts, the
delete button stops being the only one without a background, the three kinds of action gain physical
separation matching their logical grouping, and every fact keeps its row whether or not it holds a
value. `notes` is the only nullable field on the response, so the last one is a single row that
currently disappears.

This revisits the first design decision on this page, which rejected a right-hand column. That
rejection was written before the button count was known. It is seven on an open order, which is
roughly the height of the two cards, so the argument that a column would be mostly empty no longer
holds. What it costs is one breakpoint.

Settled on 2026-09-22, all four:

- **The actions stand in a right-hand column**, `md:grid-cols-[1fr_16rem]`, stacking under the facts
  below that width. The alternative was a horizontal row under the order number, which keeps one
  column but makes the page open with what can be done before it says what the order is.
- **Delete is an outline error button.** Ghost with no fill made it the one action that disappeared.
  Outline gives it the weight of the others while the fill difference keeps it from reading as
  another lifecycle move, and it stays distinguishable from `Cancel order`, which is soft red.
- **The three kinds of action are separated by a rule, not a label.** The buttons name themselves,
  and the honest group names are jargon a counter does not use about a bike.
- **A fact keeps its row when it has no value**, and the value side is simply empty. `notes` is the
  only nullable field on the response. A worded placeholder was the alternative; an empty cell was
  preferred as quieter, at the cost of looking the same as a value that failed to arrive.

The two columns are matched in height by letting the fact cards absorb the difference: the grid row
stretches, the cards take `flex-1` and spread their rows into whatever space that gives, and the
action box is `self-start` so it keeps its own height. Padding tuned by hand would only be right for
one status, since a cancelled order is one button beside two full cards and the taller side swaps.

The component is renamed from `OrderActionBar` to `OrderActions` with it, since a grouped vertical
column is not a bar.

**J4d. Thin the route module.** By the end of J4b `order-detail.tsx` was 269 lines against 162 for
the next largest file on the front end, which is the point at which it stops reading as a route and
starts reading as a feature. Two things leave it and one stays.

The intent switch and the transition record become `runOrderCommand` in `app/services/`, since
turning a posted intent into an endpoint call is not route wiring and is worth testing on its own.
The three near-identical `CommandDialog` blocks become one `OrderEditDialog`, which owns the mapping
from a command to its title and its fields, so the route renders one element instead of three
branches. The label map behind both that dialog and the action column moves onto the intent enum as
`editCommandLabels`, declared `as const satisfies` so the typed locale keys survive.

`shouldRevalidate` stays. It is a route module export that React Router reads off the route, its
body is route policy in five lines, and moving it would leave the export behind as a forwarder.

No test changes for the move itself: the behaviour is identical and the existing suite is what
proves it. The dispatch then gained its own tests, which is what earned the extraction.

**The posted vocabulary is parsed, not asserted.** `as OrderIntent` and `as OrderAction` on values
read out of `FormData` were telling the compiler something no one had checked. The intent goes
through `orderIntentOf` and the action through an `isOrderAction` predicate, both throwing a named
error that reaches the error boundary, since a request the UI cannot produce is not a form failure.
The predicate uses `Object.hasOwn` rather than `in`, because `in` walks the prototype chain and
would accept `toString` as a lifecycle action. Dropping the switch's `default` is what makes the
compiler enforce that every intent is handled.

`serviceType` keeps its cast. The catalogue lives on the server, the only runtime list of valid
codes is the one the API publishes, and hardcoding a copy in the browser is the thing the
projection rules exist to prevent. A wrong code comes back as a 422 the form renders.

**`runOrderCommand` stays one function with that name.** The suggestion was that it does not
separate lifecycle moves from edits. It should not: every one of the seven is a command, the caller
has one posted intent and cannot know its kind until it is parsed, and splitting would make the
route branch on the kind before dispatching, which is more code for no gain. The distinction is
already visible in the body, where the lifecycle moves go through the `transitions` record and the
edits are cases of their own.

**J4b. Dialog state in the URL.** Open question 1, settled on 2026-09-22 in favour of taking it:
`?edit=details` and friends, opening as a link, the action redirecting to the bare path on success
so the dialog unmounts through a navigation, and a failure returning as usual so the param and the
typed input survive. The codebase's only `useEffect` goes with it.

## Manual pass checklist

Ran clean on 2026-09-22, every step, alongside the phase C steps that were unreachable until the
lifecycle and demo data existed. One thing came back: the page is a narrow column pinned to the left
edge while the dialogs centre themselves, which J4a fixes.

Run before anything here is pushed. Start `docker compose up -d db` (already up), then the API and
`npm --prefix frontend run dev` in their own terminals.

1. The list shows an `Order` column, demo orders numbered 1000 to 1025.
2. Place an order. It takes the next number and appears in the table with it.
3. Open it. The heading is the number with the customer name beneath, then the summary card, then a
   row of actions, then the contact card. No form is open.
4. `Start work`, `Complete` and `Cancel order` are offered on a new order; `Reopen` is not.
5. Press `Cancel order`, read the confirmation, dismiss it. Nothing changed.
6. Press `Start work` and confirm. The status becomes In progress and the buttons become
   `Complete`, `Cancel order` and the three edit commands.
7. `Complete` it. The page says it can no longer be changed, the edit buttons are gone, and the only
   action left is `Reopen`.
8. `Reopen` it, then `Cancel order` it. A cancelled order offers nothing at all.
9. On an open order, press `Edit details`. A dialog holds only those fields, prefilled. Save closes
   it and the page shows the new values.
10. `Endre frist` with a date in the past. The dialog stays open and shows the message.
11. Switch to Norwegian. Every action label, dialog title and confirmation reads naturally, and the
    order number renders with no thousands separator.
12. Delete an order from the list. The confirmation names the order number and the customer.
13. Reset the demo data. Numbering continues past the highest used rather than restarting.

## Out of scope

- The status legend above the orders table, which is the next roadmap item and belongs to the list.
- The status history and `isReopened`, which have no data behind them yet.
- The list polish still tracked under #2.
- Localized API validation messages (#11), which this page will surface unchanged.

## Open questions

1. **Should the open dialog live in the URL rather than in component state?** Today `CommandDialog`
   closes itself with the codebase's only `useEffect`, watching the fetcher for an accepted command.
   Native HTML cannot replace it: `dialog`, `popover` and the invoker attributes all close on a
   gesture, and this close depends on the server's answer, because a rejected command has to keep
   the dialog open with its message and the typed input.

   The alternative that does remove it is `/orders/{id}?edit=details`. Opening is a link, the
   action redirects to the bare path on success so the dialog unmounts through a navigation, and a
   failure returns as usual so the param and the dialog stay. Post, redirect, get, with the server
   deciding when the dialog closes, plus a linkable dialog and a back button that shuts it.

   Cost: a short param enum, a redirect inside an action that otherwise always answers with
   `ActionResult`, and a navigation per open and close. **Settled on 2026-09-22: take it**, as J4b.
   The manual pass raised one item and it was smaller than this, so the condition the assumption
   named was met.
