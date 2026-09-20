# Roadmap

## At a glance

One track is active: build the maintenance-order application end to end, from the domain model to a
running `docker compose up`. Everything currently planned lives in
[plan-bike-repair-shop.md](plan-bike-repair-shop.md).

## In flight

| Track | Plan | Status |
|---|---|---|
| Maintenance orders end to end | `plan-bike-repair-shop.md` | 🟡 phase B, B1 and B2 done, B3 next |

## Delivery line

Phases A through D are the product: a working application, packaged and runnable with one command.
Phases E through G are depth on top of it, taken in order and only while the earlier phases stay
green. From phase B on, each phase is a thin vertical through API and front end that ends in a
manual smoke test, so there is always a shippable state and something to try by hand.

## Pace

Measured against the brief in [reference/time-and-scope.md](reference/time-and-scope.md). The
foundation is done and none of the four staff tasks is usable end to end yet, so phases E through G
stay untouched until `docker compose up` serves all four.

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
| Cards instead of table rows | Recommend against | after D |
| A multi-step guided place-order flow | Recommend against | after D |

The last two are worth arguing before anyone builds them. A table is what a counter needs: many
orders scanned at once, columns lined up, sortable later. Cards spend vertical space per order and
lose the alignment that makes scanning work; if the driver is small screens, a table that stacks at
narrow widths gets that without giving up the desktop view. A wizard over seven fields that fit on
one screen adds step state, partial validation and a back path, and the brief's task is one command,
placing an order, not a process with stages. Grouping the fields into two labelled sections gets the
guidance without the machinery.

## Blockers

None.
