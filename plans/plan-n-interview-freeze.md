# Phase N: interview freeze sweep (#23)

## At a glance

The evening before the interview (2026-10-14, 14:30), a re-audit against the current global rules
found stale docs, one URL-triggerable bug, two demo-data defects and visible duplication on the
order pages. This phase fixes them in order of risk, ends with a cold start and the developer's smoke
test, and freezes `main`. Cut-off 21:00: whatever has not landed by then is dropped, not rushed.

## Progress

Complete. Merged on 2026-10-13 through #24.

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| N1. Docs match the code | ✅ | README, CLAUDE.md, development record, time and scope |
| N2. List params and the parse bugs (#23) | ✅ | `OrdersParam`, `Object.hasOwn`, `serviceTypeOf` |
| N3. Demo data reset | ✅ | Code on the 404, gate in the slice, one SaveChanges |
| N4a. One confirm dialog | ✅ | Shared, with a `ConfirmTone` enum |
| N4b. One key per field | ✅ | Three identical pairs merged; short headers kept |
| N4c. One date formatter | ✅ | The copies agreed; only the length sniff went |
| N4d. One form reader | ✅ | Own module, so the place route loads no lifecycle endpoints |
| N5. Cold start and smoke test | ✅ | Clean on 2026-10-13 |
| N6. Freeze record | ✅ | Merged through #24, CI green |

## Context

The repository was last touched on 2026-09-24. The global `CLAUDE.md` gained the shorthand,
cohesion and coupling, mock-only-out-of-process and prove-it-red rules since. Three read-only audits
on 2026-10-13 (backend, front end, docs and process) produced the findings; each one in scope was
re-checked in the code before it landed here.

Gates on 2026-10-13 before any change: backend 169 green, `dotnet format` clean, front end 62 green
after `npm ci` (the move to `~/dev` left `node_modules` behind). `npm audit` reports one high
finding in `source-map-js`, a transitive build dependency.

The repository will not be shared with the interviewers; the walkthrough runs from this laptop, so
the local cold start and smoke test are what count. A prep session follows on 2026-10-14 to choose
what to showcase and what to say.

## Design decisions

- **No new GitHub issues for this phase**, the developer's call on 2026-10-13. #23 already exists
  and N2 closes it; every other step is tracked here only.
- **One branch, `chore/interview-freeze`, a commit or more per step.** Nothing is pushed until the
  developer says so; whether it goes up as one PR or merges locally is asked at N6.
- **Ordered by risk.** Docs first because they are the most visible and cannot break anything;
  the front-end consolidation last because it is the biggest diff and the easiest to drop.
- **The `editCommandOf` fix rides with #23**: same file, adjacent defect.
- **A step goes ✅ only after the developer confirms its commit**, in a plans-only commit that also
  starts the next step. N1 to N4b carried their ✅ inside the step's own commit, which the global
  rule does not allow; corrected from N4c on.
- **N4c's tests are characterization tests**, not proven red: the replaced formatters behaved
  correctly, and breaking a line to watch them fail is the invented mutant the global rule rejects.
- **Left as talking points, not fixed tonight**: backend test helpers copied across classes, domain
  tests not asserting the error `code`, no LIKE-escape test, the drift questions in step 1b of the
  reference plan, and the memory contradictions the docs audit found.

## Steps

**N1. Docs match the code.** `docs/ai-development.md`: time spent, the PR-only claim, the share of
plans-only commits, the per-phase click-through claim. `CLAUDE.md`: EF version, project count,
AutoFixture (package removed too), `MaintenanceOrderSummary.From(order, today)`, resource routes,
the parked section. `README.md`: prerequisites, the backend suite needing Docker, what is not built
yet, a short section linking the development record. `plans/reference/time-and-scope.md`: endpoint
and phase counts, the stale remaining-work list.

**N2. List params and the edit param bug (#23).** One `OrdersParam` enum in `app/types/params/`
read by the loader, the search form and the pager. `editCommandOf` stops falling through to
`Object.prototype`, and the service type is parsed rather than cast in both places that read it;
each fix has a test shown red first. The development record already claims both, so this step makes
it true.

**N3. Demo data reset.** The enabled check moves into the slice and a disabled reset answers with
an `ApiException` carrying its own code; delete and insert run in one `SaveChanges`. The
developer chose `RemoveRange` over `ExecuteDelete` plus an explicit transaction: EF's docs advise
against mixing tracked and `Execute*` writes, and the demo table is small. Integration test
for the disabled case on the code, shown red.

**N4a to N4d. Front-end consolidation.** Delete and reset render the one confirm dialog, moved out
of `orders/`; table headers and form labels share one key per field in both locales; one module
formats a calendar day and an instant; one function reads the order fields from `FormData`. Each a commit, each droppable at the cut-off.

**N5. Cold start and smoke test.** `docker compose down -v`, `docker compose up --build`, then the
developer runs the smoke checklist on 8080. Stop `api` and `web` afterwards; `db` stays up.

**N6. Freeze record.** ROADMAP state and next session, the reference plan's step 5, and a check
that the practice page still matches what landed.

## Open questions

- RESOLVED: one PR, #24, merged by the developer on 2026-10-13.
