# Roadmap

## At a glance

One track is active: build the maintenance-order application end to end, from the domain model to a
running `docker compose up`. Everything currently planned lives in
[plan-bike-repair-shop.md](plan-bike-repair-shop.md).

## In flight

| Track | Plan | Status |
|---|---|---|
| Maintenance orders end to end | `plan-bike-repair-shop.md` | 🟡 A, B, D, F, H, I and J done, C all but its polish; the brief is met and everything left is extra |

## Next up

Everything below is beyond the brief, which is met. Reordered on 2026-09-22 for a repository that is
about to be read by someone else: visible defects first, then signal, then polish.

1. **Localized validation messages from the API (#11).** The only user-visible defect left. Switch
   to Norwegian, submit a due date in the past, and the message comes back in English. It is also
   the only string the front end cannot key, so nothing else can fix it.
2. **CI (#6).** A workflow running both suites on every pull request. Disproportionate signal for
   the work, and the rule that everything lands through a pull request has nothing enforcing it yet.
   Optimistic concurrency is the other half of the issue and can wait; the README half shipped.
3. **C4b (#2)**, the four items left from the phase B pass: field error styling with `aria-invalid`
   and a described-by message, clearing a stale message when its input changes, a clear-search
   control the empty state also offers, and `autocomplete` on the contact fields.
4. **Search by service and due date (#4).** Searching by customer already works, so this widens a
   feature rather than adding one. The order number should become searchable here.
5. **Bike brand suggestions on the place form**, from a new endpoint returning the brands already on
   file. The values come from the database, not the loaded page and not a fixed list.
6. **Status change history (#5).** A second table recording who moved an order and when, plus
   somewhere to show it. The brief never asks for it, so it is scoped on its own. #5 stays open
   until this lands.

### Why this order

The earlier ordering optimised for depth: it put the state machine, the lifecycle and the detail
page first because those were the only real domain logic in an otherwise CRUD-shaped problem. That
work is done, and the audience has changed. What matters now is that someone opening the repository
finds nothing obviously broken and sees the process working, which puts the one visible defect first
and CI second.

Everything from item 3 down is polish, and none of it is load-bearing for anything else.

## Landed

- **Status legend (#13)**, 2026-09-22, [plan-l-status-legend.md](plan-l-status-legend.md). A status
  catalogue endpoint publishing the matrix whole, a native disclosure above the orders table showing
  the badges at rest and the edges on request, and the retirement of two projections nothing read.
- **Root README and the AI development record**, 2026-09-22. The README was the last piece of #6
  that did not depend on the rest of it, so it went early; concurrency and CI stay in the issue.
- **Detail page rework (#10)**, 2026-09-22, [plan-j-detail-page.md](plan-j-detail-page.md). The
  lifecycle keyed by action rather than target status, all seven commands on the page, the page
  read-only with a dialog per edit, the actions standing beside the facts, and the open dialog held
  in the URL. The route module was thinned back to a route at the end of it.
- **Order reference (#9)**, 2026-09-22, [plan-i-order-reference.md](plan-i-order-reference.md). A
  `bigint` identity column starting at 1000, carried through to the table, the detail heading and
  the delete confirmation. Both manual passes before it had to search by customer name.
- **Order lifecycle, the API half (#5)**, 2026-09-22, [plan-f-lifecycle.md](plan-f-lifecycle.md).
  `start`, `complete`, `cancel` and `reopen` as POST commands answering with the updated order. The
  issue stays open for the history.

## State

As of 2026-09-22, end of day five. **Everything is merged and `main` is the only branch.** Pull
requests 14, 15 and 16 landed the order reference, the lifecycle API, the detail page rework, the
status legend, the root README and the development record. Verified on merged `main`: backend 163
tests, front-end `verify` green at 50, and the packaged stack brought up from `docker compose down
-v` with the schema migrated, 26 demo orders seeded and the page answering on 8080.

The brief has been met since day three. Every open issue is work beyond it, so none of them is an
unfinished deliverable.

### Start here next session

1. Verify rather than trust this file: confirm `README.md`, `docs/ai-development.md` and the status
   legend are on `main`, and that the tracker shows #9, #10 and #13 closed.
2. Take `Next up` item 1 below. Ask before writing a plan.
3. The `db` container is the only thing that should be running. Bring the API and the front end up
   yourself when you need them.

### The trap that cost an hour on day five

A stacked pull request merged before GitHub has retargeted it lands on its **parent branch**, not on
`main`. Pull request 15 merged nine seconds after 14 and went into `feat/10-detail-page-rework`, so
the legend, the README and the development record were absent from `main` while every local check
said they were fine. Recovering it took a third pull request. **Merge one, wait for the retarget to
show `main` as the base, then merge the next.**

Two smaller ones from the same hour. `Closes #9, #10` in a pull request body closes only the first
issue: GitHub needs a keyword per issue, `Closes #9, closes #10`. And a phase letter is claimed when
the phase is identified, not when it is written, so check the master plan's table before naming a new
plan file.

## Delivery line

Phases A through D are the product: a working application, packaged and runnable with one command.
Phases E through G are depth on top of it, taken in order and only while the earlier phases stay
green. From phase B on, each phase is a thin vertical through API and front end that ends in a
manual smoke test, so there is always a shippable state and something to try by hand.

Issues #8 to #11 came out of the phase C pass and sit between the product phases and the depth
phases. #8 went before packaging, because packaging cannot be demonstrated on an empty table; the
rest follow it.

## Pace

Measured against the brief in [reference/time-and-scope.md](reference/time-and-scope.md). The brief
is met: all four staff tasks work end to end in a browser and `docker compose up --build` serves
them from an empty volume. Everything in `Next up` is beyond it, which is why the manual pass can be
batched rather than gating each unit.

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

**Every unit of work lands through a pull request from now on, off an issue, on a branch named for
it.** Nothing commits straight to `main`. Settled on 2026-09-22: holding issues to a standard while
pushing the work itself to `main` unreviewed was the inconsistency that prompted it. Phase D already
ran this way on `feat/3-docker-packaging` through pull request 12.

One exemption, so the rule does not tax its own bookkeeping: a commit touching **only `plans/`** may
go on a `chore/` branch without an issue behind it. It still goes through a pull request, because
`plans/` is committed and a branch costs nothing.

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

Nothing blocks the work. One thing blocks a push: the manual passes are being batched rather than
run per step, so phase I is code-complete and gated but unverified by hand, and steps 5 and 8 of the
phase C checklist are reachable at last but still unrun. Both belong to the next batch pass, and
nothing is pushed or merged before it comes back clean. The checklist for phase I is at the end of
its plan.
