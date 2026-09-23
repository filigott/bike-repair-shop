# Phase G: hardening (#6)

## At a glance

CI on every pull request, then optimistic concurrency on the edit commands. The README half of #6
already shipped. Both steps run on the last day before the interview, inside a four-hour window
whose final 45 minutes are fixed for the packaged run from an empty volume and the walkthrough, so a
step that is not green by then is dropped, not rushed.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| G1. CI workflow | 🟡 | Own PR, lands first so G2's PR runs through it |
| G2a. Concurrency, API | ⏳ | `xmin` token, `version` on response and edit requests, 409 |
| G2b. Concurrency, UI | ⏳ | Conflict message in the edit dialogs |
| G3. README and docs pass | ✅ | Shipped with the development record on 2026-09-22 |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which settles the token: a `version`
field backed by PostgreSQL `xmin`, not an ETag. `CLAUDE.md` already describes that mechanism as if
it existed, and nothing in the code implements it, which is the strongest reason G2 is not just
reworded away. There is no `.github/` folder at all.

Time budget: G1 about 45 minutes, G2 about 75. #11 follows only if both land clean.

## Design decisions

**CI is one workflow with two jobs.** `backend` restores, builds and runs the unfiltered test suite
on `ubuntu-latest`, whose Docker daemon serves the Testcontainers PostgreSQL. `frontend` runs
`npm ci` then `npm run verify`, the same gate a local commit passes. One file, parallel jobs,
triggered on `pull_request` and on push to `main`. `dotnet format --verify-no-changes`
runs too, since the tree is already clean and the build alone does not fail on style.

**The SDK comes from `global.json`**, so CI and the Dockerfile cannot drift from the local toolchain.
Node 24 matches the front-end Dockerfile.

## Steps

**G1. CI workflow.** `.github/workflows/ci.yml` with the two jobs above, an npm cache on
`package-lock.json`. No NuGet cache: `setup-dotnet` caching needs lock files the repo does not have. The build runs before `dotnet test --no-build`,
the same order the local commands need. Verified by the PR's own run; the README gains the badge.

**G2a. Concurrency, API.** `xmin` mapped as the order's row version, `version` projected onto the
order response, required on the edit requests, set as the original value before save so EF's own
check raises the conflict. `DbUpdateConcurrencyException` maps to 409 ProblemDetails in the existing
middleware. Integration test: two writers holding the same version, the second gets 409 and the
first write survives a fresh read.

**G2b. Concurrency, UI.** The edit dialogs post the version they loaded. A 409 becomes a typed
failure the dialog renders, telling the user the order changed and showing the reloaded values,
both locales.

## Out of scope

- ETag and `If-Match`, rejected in the main plan.
- Deployment and load testing, as #6 says.

## Open questions

1. Which commands carry `version`? (Assumed: the three edit commands. The lifecycle commands and
   delete are already guarded by the state machine, and the token on the entity still catches a
   race between load and save for every command.)
2. `version` as a property on the entity or an EF shadow property? (Assumed: a `uint` with a private
   setter on the entity, so `Summary.From(order)` can project it without the context.)
