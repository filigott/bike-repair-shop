# Roadmap

## At a glance

The application is built and the brief is met. The interview moved to 2026-10-14 at 14:30, so the
active track is now making this repository a reference for future projects:
[plan-reference-repository.md](plan-reference-repository.md). The application's own history lives in
[plan-bike-repair-shop.md](plan-bike-repair-shop.md). Work freezes around 2026-10-07 so the practice
material and `main` agree on the day.

## In flight

| Track | Plan | Status |
|---|---|---|
| Maintenance orders end to end | `plan-bike-repair-shop.md` | ✅ A, B, D, F, G, H, I, J, K, L and M done; the brief is met and everything left is extra |
| Reference repository | `plan-reference-repository.md` | 🟡 Step 5, the freeze, runs as phase N |
| Interview freeze sweep | `plan-n-interview-freeze.md` | 🟡 Started 2026-10-13; cut-off 21:00 |

## Next up

The order is set by [plan-reference-repository.md](plan-reference-repository.md): the drift audit,
then search with sorted paging (#4), end-to-end tests, cheap hardening, and the freeze. Auth and the
status history (#5) come after the interview. Outside that plan, still queued as product depth:

1. **Bike brand suggestions on the place form**, from a new endpoint returning the brands already on
   file. The values come from the database, not the loaded page and not a fixed list.

Nothing here is load-bearing for anything else.

## Landed

- **Form and list polish (#18)**, 2026-09-23, [plan-m-form-and-list-polish.md](plan-m-form-and-list-polish.md).
  The place page at the detail page's type scale, rejected fields marked on the input and cleared
  once edited, autofill on the contact fields, and a way back from any search.
- **Localized API messages (#11)**, 2026-09-23, [plan-k-localized-api-messages.md](plan-k-localized-api-messages.md).
  Every problem carries a machine code, validation adds `errorCodes` beside `errors`, and the front
  end renders only locale copy. No English sentence from the API reaches the screen.
- **Optimistic concurrency and CI (#6)**, 2026-09-23, [plan-g-hardening.md](plan-g-hardening.md).
  `xmin` as the order version on the three edits, a stale one refused with 409 and explained in the
  dialog; a GitHub Actions workflow running both suites and `dotnet format` on every pull request.
  #19, #20 and #21 were merged by the agent on the developer's explicit one-off waiver of the
  never-merge rule; the rule itself stands.
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

As of 2026-09-23. `main` is the only branch and carries everything: the four staff tasks, the order
reference, the lifecycle, the detail page, the status legend, optimistic concurrency, localized API
messages, the form and list polish, CI, the README and the development record.

Green on `main`: backend 169 tests, `dotnet format` clean, front-end `verify` at 62, and the CI
workflow on the merge. The packaged stack was last verified from an empty volume on 2026-09-22 and
is due again at the freeze, before the repository is handed over. The repository has not been
shared with the interviewers yet.

The brief has been met since day three, so every open issue is work beyond it rather than an
unfinished deliverable. What the brief asked for is listed in
[reference/time-and-scope.md](reference/time-and-scope.md) under `Against the brief`; the original
document itself is not in this repository, which is worth fixing if a copy exists.

### Start here next session

1. Verify rather than trust this file: `git log --oneline -3`, then confirm both suites are green.
2. Open [plan-reference-repository.md](plan-reference-repository.md) and take the first pending step.
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

Nothing. Everything is merged and both suites are green on `main`, locally and in CI.

What the smoke tests raised and how each item was settled is in
[reference/smoke-test-findings.md](reference/smoke-test-findings.md).
