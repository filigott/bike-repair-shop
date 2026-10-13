# Reference repository (#4, #5, issues to come)

## At a glance

The brief is met and the interview moved to 2026-10-14 at 14:30, so this repository now has a second
purpose: a real-world example of how the developer builds software with an agent, to point future
projects at. The global `~/.claude/CLAUDE.md` stays the standalone source of every rule; this repo
shows them applied and adds the patterns a real product has and this one still lacks. No code before
the next session, and a freeze before the interview.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| 1a. Drift audit, clear fixes | ✅ | 2026-09-23 |
| 1b. Drift audit, decisions | ⏳ | Seven questions below |
| 2. Search and sorted paging (#4) | ⏳ | After the interview |
| 3. End-to-end tests in CI | ⏳ | After the interview |
| 4. Cheap hardening | ⏳ | After the interview |
| 5. Interview freeze | 🟡 | 2026-10-13, [plan-n-interview-freeze.md](plan-n-interview-freeze.md) |
| 6. Authentication | ⏳ | After the interview; issue to create |
| 7. Status history (#5) | ⏳ | On top of step 6 |
| 8. Observability and image builds | ⏳ | After the interview |

## Context

Planned on 2026-09-23, the day the interview was postponed. Nothing was built that day beyond this
plan. The repository has not been shared with the interviewers yet, so work lands on `main` through
the usual issue, branch, PR and smoke-test route until the freeze.

Settled with the developer the same day:

- The global `CLAUDE.md` stands alone. The repo is another real-world example, not the canonical
  copy, and a change to a global rule must not force a change here.
- Drift is still unwanted. The mechanism is in the design decisions below.
- Plans stay committed in this repo exactly as they are. Globally, committing plans is a per-project
  opt-in, with a lean against it once a repo is shared with other people.
- Extracting the workflow into skills, hooks or templates waits for a second project, which is the
  only honest test of what gets reused.

## Design decisions

**Rules live globally, the repo shows them.** The global file wins every disagreement. The repo's
`CLAUDE.md` keeps project facts, worked examples and its deliberate deviations; anything else it
says that contradicts the global file is drift.

**Drift is found by reading, not by syncing.** A deviation the repo makes on purpose is stated in its
own `CLAUDE.md`, as committing plans already is. Any other difference is drift, and a session that
meets it flags it rather than copies it. Step 1 does one full pass so the baseline is clean; after
that the flag-on-sight rule, recorded in the global file, is the whole mechanism. No script, no
generated section, nothing to maintain when a global rule changes.

**Each code step is a pattern worth lifting**, ranked by how often a new project needs it: a paged
list, a browser test, a lock file, then auth and an audit trail. Depth for its own sake stays out.

**Anything that lands before the interview changes the walkthrough.** The freeze exists so the
practice page, the packaged stack and `main` agree on the day.

## Steps

**1. Drift audit.** Read the repo `CLAUDE.md`, `plans/README.md` and the project memory against the
global file; the findings and the open decisions are below. Known candidates: plans committed here (deliberate, keep), plans over issues in memory
against the global "issue before any work", and the rules restated in both, such as error handling,
CQRS and the React rules. Each difference ends as recorded-as-deliberate, fixed here, or a question
for the developer about the global rule. Prose only.

### Drift audit findings, 2026-09-23

Read: the repo `CLAUDE.md`, `plans/README.md`, `README.md`, `docs/`, the project memory, and the
global file and plan template. Consistent and left alone: AAA markers, commit format, comment rules,
round-trip assertions, exception layering, CQRS slices, i18n keys, `useLocale`, semantic colours,
plan skeleton and status rules. No icons are used, so the Lucide rule has nothing to apply to.

Fixed on the spot:

- Repo `CLAUDE.md` now states its three deliberate departures: `README` and `docs/` point into
  `plans/`; plans-only commits go straight to `main` without an issue or PR; the agent may start the
  API to regenerate the OpenAPI types.
- Memory `plans-over-github-issues` rewritten: it still claimed issues never gate work, which the PR
  route overturned on 2026-09-22.
- Memory `autocommit-plan-and-doc-updates`: the merge waiver covered #19 to #22, not #19 to #21.
- Global file contradicted its own template on where plans live (project root against `plans/`).

Left for step 1b, each one the developer's call because it changes a global rule:

1. **Mutations answer with the order, not 204.** Global says POST, PATCH and DELETE return 204
   because the router reloads after an action. Here place answers 201 with the order, and the edits
   and lifecycle commands answer with the updated order; only delete is 204. Check whether the front
   end reads those bodies, then either state it as a departure here or relax the global rule.
2. **Form dialogs held in the URL.** Global prescribes `selectedItem: T | null` in the parent for
   every modal. The repo keeps that for confirm-only dialogs and holds form dialogs in a search
   param. Promotion candidate.
3. **Value objects as `ComplexProperty`.** Global says `HasConversion`; the repo uses complex types
   because a converted value cannot be searched with `LIKE`. Promotion candidate, as "complex type
   when queried".
4. **Rules learned here that the global file lacks:** no `useEffect` and never mirror a prop; API
   text never rendered, only `problems.<code>`; the UI never recomputes a server rule; a lifecycle
   table keyed by action; concurrency as a version field rather than an ETag; a new test shown red
   before it counts; one `verify` script as the front-end gate. Promote each, or leave it as a
   worked example.
5. **Design questions.** The repo says an open design question is answered by picking the
   recommended option and saying so; the global file leans on asking. Probably compatible (design
   calls against business rules), but worth one line in whichever file owns it.
6. **Dated stack line.** "Checked 2026-09-20" in the repo `CLAUDE.md` against the timeless-docs
   rule. Likely fine as a verification stamp; confirm.

7. **Proving a test red.** The repo says "Break the exact line under test, watch it fail, restore";
   the global rule says a test counts only once it fails against the code it replaces, and never
   against an invented mutant. The repo line permits exactly that. Align the repo with the global.

Not fixable: early commit subjects carry plan vocabulary (`close phase M's steps`). History stays.

**2. Search by service and due date, with sorted paging (#4).** Check the original brief's wording
for "find" first; if it names these criteria, this was a gap in the brief rather than depth. The
order number becomes searchable here, as the ROADMAP already notes. TDD on the specification.

**3. End-to-end tests.** Playwright against the packaged stack, the four staff tasks as the
scenarios, run as a third CI job. Create the issue first.

**4. Cheap hardening.** Architecture tests for the layer references `CLAUDE.md` states, NuGet lock
files so CI can cache restores, and Dependabot for both ecosystems. One issue, small commits.

**5. Interview freeze.** Cold start from `docker compose down -v`, the five-step smoke test, refresh
the practice page (a private artifact, outside the repo) against whatever landed, and bring the
ROADMAP state and `reference/time-and-scope.md` up to date.

**6. Authentication.** Staff sign-in; the mechanism is an open question for its own plan.

**7. Status history (#5).** Who moved an order and when, which needs step 6 for the who.

**8. Observability and image builds.** OpenTelemetry traces and metrics, and CI building the images.

## Out of scope

- Deployment to a hosted environment. Building the images in CI is as far as this plan goes.
- Skills, hooks or templates extracted from this repo, until a second project asks for them.
- Bike brand suggestions stay in the ROADMAP queue as product depth, not a reference pattern.

## Open questions

- RESOLVED: freeze date. The freeze ran on 2026-10-13, the evening before.
- RESOLVED: steps 2 to 4 wait for after the interview; the freeze sweep took the last evening.
- Where the original brief lives. The ROADMAP notes it is not in the repository; step 2 needs it.
