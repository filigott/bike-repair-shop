> **Reference doc: load only when working in this area**
> - **Covers:** what the build has cost so far and how far the result is from the brief
> - **Used by:** [ROADMAP.md](../ROADMAP.md) and [plan-bike-repair-shop.md](../plan-bike-repair-shop.md); read before adding a phase or a design decision
> - **Updated:** 2026-09-22

# Time and scope, measured

The brief asks for four staff tasks (place, find, edit, delete), a React page, an HTTP API holding
the business logic, a database, and `docker compose up` for the whole stack. This doc keeps the plan
structure honest by measuring it against that list rather than against its own tidiness.

## The clock

| Window | Length | What landed |
|---|---|---|
| Day one, session start to 17:36 | 2h00 | No commits: brief, stack selection, registry version checks, throwaway scaffolds, `CLAUDE.md`, the plan skeleton |
| Day one, 17:36 to 19:52 | 2h16 | Phase A: domain, EF mapping, initial migration, integration harness, health check |
| Day one, 20:10 to 20:54 | 0h44 | Phase B1 and B2: place and list API, front-end scaffold, shell, transport |
| Day one, 21:30 to 22:00 | 0h30 | Phase B3: the orders list page |
| Day two, 12:10 to 14:27 | 1h00 | Phase C: order by id, the three edit commands, delete, the detail page with one form per command, delete from the list |
| Day three, 19:30 to 21:45 | 1h30 | The phase C smoke test, demo data end to end, the visual pass, and phase D packaging |
| Day four, 09:15 to 13:16 | 4h00 | The order reference (#9), the lifecycle API (#5), and three of the four detail-page steps (#10) including the state-machine rekey |
| Day five, 15:13 onward | 2h00 | The detail page finished and its route thinned, the status legend end to end (#13), the pull request rule, the root README and the AI development record |

Day one total: 5h30 and 32 commits. Two of those hours preceded the first commit, and ten of the
commits touch only plans or conventions.

Day two is counted as focused time, not wall clock. The window spans 2h17 and carried 10 commits,
but it was shared with other work rather than worked straight through, so 1h00 is the figure that
goes into the running total.

Day three is counted the way day two was, as focused time rather than wall clock. The window spans
2h15; 1h30 is the figure that goes into the running total: 8h00 across three days. It is the
evening the brief was finally met, and two manual passes drove most of it, which is the trade those
passes are supposed to make: both turned up things no test was ever going to.

Day four is wall clock read off the commit stamps, not self-reported focused time, so it is the one
row measured differently from the two before it. The window is 4h01 and the longest gap in it is
71 minutes, which carried the state-machine rekey and its plan, so nothing suggests a stretch where
the session was idle. Running total: **12h00 across four days**, 16 commits on day four of which
seven touch code.

## Against the brief

| Requirement | State after day four |
|---|---|
| Place an order | Done end to end |
| Find an order | Done end to end: list, customer search, paging |
| Edit an order | Done end to end, confirmed by the manual pass |
| Delete an order | Done end to end, confirmed by the manual pass |
| React page consuming the API | Three routes: the list, the place form, the order detail page |
| HTTP API with the business logic | Ten endpoints: the eight order and catalogue ones, plus demo data status and reset |
| Database holding the services | Done: seeded catalogue, migrations, integration tests |
| `docker compose up` runs the stack | Done: three services from one command, verified from a deleted volume |

The first 5h30 bought the whole foundation and none of the four staff tasks end to end. The next
hour turned that into all four, which is the payback the foundation was bet on. The brief has been
fully met since day three, so everything on day four was beyond it: an order reference staff can
read out, the lifecycle reachable over HTTP at last, and the detail page rebuilt around one action
at a time.

## Where scope ran ahead of the brief

- **Seven phases for a brief that needs three.** Search (E), lifecycle history (F) and hardening (G)
  are additions made here, not asks.
- **The order state machine was built first, in phase A.** Phase C finally reaches it: `isEditable`
  gates the detail page's forms, and a closed order shows a notice instead.
- **Concurrency, paging and the queryable-value-object refactor precede their consumers**, the last
  of them serving a phase E filter the brief never asks for.

The foundation underneath is worth having: the migrated database, the container-backed harness and
the typed client are what made phase C quick.

## The correction

- Nothing from phases E, F or G until `docker compose up` serves all four staff tasks.
- Plan status edits ride in the step's own commit instead of taking a commit of their own.
- A new design decision needs a consumer in the current or next phase, or it waits.
- Brief-complete as of day three. Everything still on the list is beyond what was asked.
- Revised after day four. The order reference is done and the detail page rework is three steps of
  four. What remains is about 2h: J4, C4b polish, the localized API messages (#11), the status
  legend, the brand suggestions and the status history.
- Day four's manual pass was batched rather than run per phase, which is a departure worth watching.
  It bought speed and it means three phases are unverified by hand at once.

## What day four cost

One design was rebuilt, and the pattern is worth naming because it nearly shipped wrong. The
lifecycle needed the UI to know that `Completed` to `InProgress` is `reopen` while `Received` to
`InProgress` is `start`. Three options were put up for where that mapping should live: in the
browser, beside the existing field on the server, or replacing it. All three were rejected in one
sentence, and rightly: every one of them moved the mapping rather than removed it. The real fault
was that the matrix was keyed by target status, which cannot tell two edges apart when they share a
target.

Rekeying it by the action deleted the mapping outright and, unprompted, fixed two defects the old
shape could not express. Roughly 70 minutes including the plan. The rule it leaves behind: when
every option on the table relocates a problem, none of them is the answer, and the thing to question
is what the data is keyed by.

The other call worth recording is a deferral. J4 is delete on the detail page plus the type scale,
the back link and the remaining duplication, and it was deliberately left unbuilt so the manual pass
steers it rather than reviewing it afterwards. The plan had it before the pass; that was wrong.

## What the hour cost

One rebuild, worth naming so the pattern is visible. The edit UI was first built as a single form
that diffed the order and fanned out to the three commands, which was rejected on sight: it puts
concurrent writes and a race behind one save button, and it hides the commands the API exposes. The
replacement is one form per command on a detail page, an intent read from `FormData`. Roughly
35 minutes, and the rule it confirms is already in `CLAUDE.md` under actions being explicit on the
API and in the UI alike. Check a screen against that rule before building it, not after.

## What day five cost

Two corrections came from the developer rather than from a test, and both were about the shape of
the code rather than its behaviour. Form values were being cast to their enum types, which asserts
what nobody has checked; they are parsed now, and a prototype key like `toString` is refused by name.
The order detail route had grown to 269 lines against 162 for the next largest file, which is where a
route stops reading as a route; the command dispatch and the three dialog branches moved out.

The third was a process observation with more reach than either: issues were being held to a
standard while the work itself went straight to `main` unreviewed. Everything runs through a pull
request off an issue from now on, and the 29 commits that predate the rule moved to a branch rather
than being pushed to `main`.

A third correction came out of the delivery route itself. Two stacked pull requests were merged
within nine seconds, and the second landed on its parent branch rather than on `main`, so the
legend, the README and the development record were missing from `main` while every local check
passed. It took a third pull request to recover, and the rule it leaves behind is in the roadmap:
merge one, wait for the retarget, then merge the next. The wider lesson is the same one day four
produced about relocating a problem. Stacking pull requests moves the sequencing risk from the
branches to the merge button, and the cheaper shape is to merge the first and let the second rebase.

Two smaller ones from the same hour, both cheap to avoid next time. `Closes #9, #10` in a pull
request body closes only the first issue, because GitHub wants a keyword per issue. And a phase
letter belongs to the phase that claimed it when it was identified, not to the one written first,
which is why the legend plan had to be renamed from K to L after the fact.

Running total: **14h30 across five days**.
