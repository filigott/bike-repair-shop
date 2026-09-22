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

Everything below is beyond the brief, which is met. Ordered for a repository about to be read by
someone else: enforcement and visible defects first, polish behind them.

1. **CI (#6).** A workflow running both suites on every pull request. The repository's claim is that
   nothing lands unreviewed and untested, and nothing currently enforces it. Highest signal for the
   effort. Optimistic concurrency is the issue's other half and can wait; its README half shipped.
2. **Localized validation messages from the API (#11).** The only user-visible defect: a Norwegian
   user submitting a bad due date gets an English message back. It is also the only string the front
   end cannot key, so nothing else can fix it.
3. **Search by service and due date (#4).** Searching by customer name, phone and email already
   works and satisfies what the brief asks for, so this widens a feature rather than adding one. The
   order number should become searchable here.
4. **Form and list polish (#18).** Field error styling with `aria-invalid` and a described-by
   message, clearing a stale message when its input changes, a clear-search control the empty state
   also offers, and `autocomplete` on the contact fields.
5. **Bike brand suggestions on the place form**, from a new endpoint returning the brands already on
   file. The values come from the database, not the loaded page and not a fixed list.
6. **Status change history (#5).** A second table recording who moved an order and when, plus
   somewhere to show it. The brief never asks for it, so it is scoped on its own and #5 stays open
   until it lands.

### Why this order

The earlier ordering optimised for depth, putting the state machine, the lifecycle and the detail
page first because they held the only real domain logic in an otherwise CRUD-shaped problem. That
work is done and the audience has changed. What matters now is that a reader finds nothing obviously
broken and sees the process actually working, which is why CI leads and the one visible defect
follows it.

Nothing from item 3 down is load-bearing for anything else.

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

As of 2026-09-22. `main` is the only branch and carries everything: the four staff tasks, the order
reference, the lifecycle over HTTP, the detail page built around one action at a time, the status
legend, the README and the development record.

Green on `main`: backend 163 tests, front-end `verify` at 50, and the packaged stack up from an empty
volume with its schema migrated, 26 demo orders seeded and the page answering on 8080.

The brief has been met since day three, so every open issue is work beyond it rather than an
unfinished deliverable. What the brief asked for is listed in
[reference/time-and-scope.md](reference/time-and-scope.md) under `Against the brief`; the original
document itself is not in this repository, which is worth fixing if a copy exists.

### Start here next session

1. Verify rather than trust this file: `git log --oneline -3`, then confirm both suites are green.
2. Take `Next up` item 1 below. Ask before writing a plan.
3. The `db` container is the only thing that should be running. Bring the API and the front end up
   yourself when you need them.

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

## Blockers

Nothing. Everything is merged, both suites are green on `main` and the packaged stack has been
verified from an empty volume.

What the smoke tests raised and how each item was settled is in
[reference/smoke-test-findings.md](reference/smoke-test-findings.md).
