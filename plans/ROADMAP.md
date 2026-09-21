# Roadmap

## At a glance

One track is active: build the maintenance-order application end to end, from the domain model to a
running `docker compose up`. Everything currently planned lives in
[plan-bike-repair-shop.md](plan-bike-repair-shop.md).

## In flight

| Track | Plan | Status |
|---|---|---|
| Maintenance orders end to end | `plan-bike-repair-shop.md` | 🟡 A, B, D and H done, C all but its polish; the brief is met and everything left is extra |

## Next up

Everything below is beyond the brief, which is met. Reordered on 2026-09-21 after a second look at
the priorities; the reasoning is under `Why this order` below.

1. Order reference (#9). A database-generated sequential number, so an order can be read out over
   the phone. First because both manual passes on 2026-09-21 had to search by customer name, and
   because the detail page is meant to lead with it.
2. Order lifecycle, the API half only (#5). `start`, `complete` and `cancel` as commands over HTTP,
   calling the domain methods that already exist and are already unit-tested. No UI in this unit and
   no history table.
3. Detail page rework (#10). The page goes read-only and every command becomes an action: three edit
   commands opening a dialog with their own form, and three transitions that are a confirm rather
   than a form. Built once, with all six in hand.
4. A status legend above the orders table. Its transitions come from the API, read off the domain's
   matrix the way the service catalogue is, never written out in both locale files.
5. C4b (#2), the four items left from the phase B pass: field error styling with `aria-invalid` and
   a described-by message, clearing a stale message when its input changes, a clear-search control
   the empty state also offers, and `autocomplete` on the contact fields.
6. Localized validation messages from the API (#11). The only strings the front end cannot key.
7. Bike brand suggestions on the place form, from a new endpoint returning the brands already on
   file. The values come from the database, not the loaded page and not a fixed list.
8. Status change history, split out of #5 on 2026-09-21. A second table recording who moved an order
   and when, plus somewhere to show it. The brief never asks for it, so it is scoped on its own
   rather than folded into the transitions.
9. Search, concurrency, CI and the README (#4, #6), the remaining original phases. The order
   number from item 1 should become searchable here rather than in #9.

### Why this order

The state machine is the one piece of real domain logic in an otherwise CRUD-shaped problem, and
nothing can currently reach it: the transition matrix is written and tested, `allowedTransitions` is
projected onto every response, the status colours are on screen, and no caller exists. The demo
seeder had to call the domain methods directly to produce a completed order. That is the largest
functional gap left, which is why the lifecycle API moved from fifth place to second.

The detail rework follows it rather than preceding it because that rework builds an action area, and
half the actions belonging in it did not exist. They are also a different shape: a transition is a
confirm, not a form in a dialog. Building the area for three commands and returning for the other
three is avoidable by doing them in one pass.

Splitting #5 keeps that unit reviewable. Transitions make the state machine reachable and unblock
both the detail page and the legend; the history is a table, a migration and a view that nothing
else waits on.

Starting state at the end of 2026-09-21: `main` is clean, pushed and carries everything including
the merged packaging pull request. The suites are green at 134 backend tests and the front-end
`verify` gate. No branch is in flight. `docker compose up --build` serves all four staff tasks from
an empty volume.

## Delivery line

Phases A through D are the product: a working application, packaged and runnable with one command.
Phases E through G are depth on top of it, taken in order and only while the earlier phases stay
green. From phase B on, each phase is a thin vertical through API and front end that ends in a
manual smoke test, so there is always a shippable state and something to try by hand.

Issues #8 to #11 came out of the phase C pass and sit between the product phases and the depth
phases. Only #8 comes before packaging, because packaging cannot be demonstrated on an empty table.

## Pace

Measured against the brief in [reference/time-and-scope.md](reference/time-and-scope.md). 7h30
across three days. All four staff tasks work end to end in a browser and the manual pass confirmed
four of them, so packaging is the last gap in the brief. Phases E through G stay untouched until
`docker compose up` serves all four.

## Parked from the phase C smoke test

Six of the eight checklist steps passed. Place, open, edit one command at a time, delete, locale and
find all behaved. Two steps could not be run at all, and the rest of what the pass raised is UI
quality rather than brief coverage. Everything below is now an issue, because a parked row in this
file does not gate a pull request.

| Item | Call | Where |
|---|---|---|
| Closed-order step unreachable: nothing moves an order off `Received` | Do | #8 now, properly in #5 |
| Overdue step unreachable: `Place` and `Reschedule` both refuse a past due date by design | Do | #8 |
| List starts empty on a fresh volume, so search and paging have nothing to exercise | Do | #8 |
| Detail page shows three always-open forms instead of an action you choose | Do | #10 |
| `Service` and `Due date` each appear twice, in the summary card and again as legend and label | Do | #10 |
| Back link is a ghost button in the top-right corner and is easy to miss | Do | #10 |
| Body text runs small, and the layout is left-aligned with no split between facts and actions | Do | #10 |
| No human-readable order reference anywhere in the UI | Do | #9 |
| API validation messages arrive in English whatever the locale | Do | #11 |

Settled on 2026-09-21, from the pass:

- **Editing is one action at a time, in a dialog.** The detail page becomes read-only and each
  command opens a native `<dialog>` holding only that form, with Save and Cancel. Page routes per
  command were the alternative and cost three loaders and a full navigation for a one-field form.
  Inline disclosure was rejected for keeping every form's markup on one busy page.
- **The order reference is a database-generated sequential number.** A short slice of the GUID needs
  no migration but cannot be dictated or remembered, which is the entire point of having one.
- **Demo seeding goes through the domain, not around it.** The first reading was that a fixture
  would have to write states the domain refuses to build. Reading the entity corrected it: `Start`,
  `Complete` and `Cancel` are public, so only the HTTP surface for them is missing, and `Place`
  takes an `IShopClock`, so an overdue order is one placed with the clock set two weeks back. The
  slice lives in the API behind a setting that is off unless set. Details in
  [plan-h-demo-data.md](plan-h-demo-data.md).
- **Packaging goes before the UI work.** Reversed from the first instinct, which was to fix what the
  pass had just raised. The UI list is around eight items and open-ended; packaging is mandatory and
  bounded, so it carries the smaller risk of being the thing left undone.

## Parked from the phase B smoke test

The first manual pass through the running stack was clean on all ten steps. What it raised is UI
quality, not brief coverage, so it waits behind the product phases. Cheap and worth doing sits in
phase C as its own step; the rest needs a decision before it needs code.

| Item | Call | Where |
|---|---|---|
| Field error state beyond red text: `input-error`, `aria-invalid`, message tied to the input | Do | C4 |
| Clear the stale message when the field changes, without re-validating in the browser | Do | C4 |
| A clear-search control, and the same escape from the empty state | Do | C4 |
| Status badges that carry meaning: info, warning, success, neutral per status | Do | C4 |
| `autocomplete` attributes on the contact fields, which is free browser autofill | Do | C4 |
| Theme: the default DaisyUI primary is purple. Pick a built-in theme or define brand tokens | Decide first | after D |
| Bike brand suggestions from the brands already stored, as a `datalist` | Do, from a new endpoint | after D |
| A cards view beside the table, as a second presentation of the same data | Showcase, if time | after D |
| A multi-step guided place-order flow | Dropped: two labelled groups in C2 instead | C2 |

Settled on 2026-09-20: the table stays the primary list, because a counter scans many orders at once
and columns keep that readable. A cards view may be added later as a second presentation, chosen by
the viewer, which is a better showcase than replacing the table. The place-order form stays one form
and gains two labelled groups in C2, since placing an order is one command rather than a process with
stages, and a wizard would buy step state and partial validation for seven fields that fit on one
screen.

Work lands on `main` and is pushed as it goes. Phase D ran on `feat/3-docker-packaging` and merged
through pull request 12, so the repository shows that flow once; there is no need to repeat it for
every phase.

## Parked, toolchain

| Item | Call | Where |
|---|---|---|
| Generate the OpenAPI document at build time instead of from a running API | Decide first | after D |

`npm run generate:api` needs the API listening on 5260, so `openapi.generated.ts` drifts behind the
contract for as long as a backend step runs and nothing catches it. `Microsoft.Extensions.ApiDescription.Server`
writes the document during `dotnet build`, which would turn drift into a diff. Unverified on this
machine, and it touches the backend build, the front-end script and phase D packaging, so it waits
until the product phases are done.

## Blockers

None. Steps 5 and 8 of the phase C checklist stay unverified until #8 lands.
