# Roadmap

## At a glance

One track is active: build the maintenance-order application end to end, from the domain model to a
running `docker compose up`. Everything currently planned lives in
[plan-bike-repair-shop.md](plan-bike-repair-shop.md).

## In flight

| Track | Plan | Status |
|---|---|---|
| Maintenance orders end to end | `plan-bike-repair-shop.md` | 🟡 phases A and B done and smoke-tested, phase C in progress |

## Next up

1. Phase C in order: C1 the get, update and delete API including the nullable due date, C2 the edit
   form, C3 the delete flow. Smoke test all four brief tasks before the phase is called done.
2. Phase D packaging, on a branch with a pull request, as the one place the repository shows that
   flow. C4 polish is last and optional.

## Delivery line

Phases A through D are the product: a working application, packaged and runnable with one command.
Phases E through G are depth on top of it, taken in order and only while the earlier phases stay
green. From phase B on, each phase is a thin vertical through API and front end that ends in a
manual smoke test, so there is always a shippable state and something to try by hand.

## Pace

Measured against the brief in [reference/time-and-scope.md](reference/time-and-scope.md). Two of the
four staff tasks now work end to end in a browser. Phases E through G stay untouched until
`docker compose up` serves all four.

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
| Bike brand suggestions from the brands already stored, as a `datalist` | Decide first | after D |
| A cards view beside the table, as a second presentation of the same data | Showcase, if time | after D |
| A multi-step guided place-order flow | Dropped: two labelled groups in C2 instead | C2 |

Settled on 2026-09-20: the table stays the primary list, because a counter scans many orders at once
and columns keep that readable. A cards view may be added later as a second presentation, chosen by
the viewer, which is a better showcase than replacing the table. The place-order form stays one form
and gains two labelled groups in C2, since placing an order is one command rather than a process with
stages, and a wizard would buy step state and partial validation for seven fields that fit on one
screen.

Work lands on `main` for now and is pushed as it goes: `main` is current as of the phase C start.
One later phase runs on a branch with a proper pull request, so the repository shows that flow too;
phase D packaging is the natural candidate.

## Blockers

None.
