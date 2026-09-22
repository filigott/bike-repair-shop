> **Reference doc: load only when working in this area**
> - **Covers:** what each manual smoke test raised, and where every item was settled
> - **Used by:** [ROADMAP.md](../ROADMAP.md), [plan-c-find-edit-delete.md](../plan-c-find-edit-delete.md) and [plan-h-demo-data.md](../plan-h-demo-data.md)
> - **Updated:** 2026-09-22

# Smoke test findings

The manual passes are the highest-yield step in this build: each one surfaced things no test was
going to. This holds what they raised and how every item was settled, so the roadmap can stay a
status view.

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
