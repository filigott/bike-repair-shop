# Phase G: hardening (#6)

## At a glance

Hardening: optimistic concurrency on update, CI on push, and the README and docs pass.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| G1. Optimistic concurrency | ⏳ | `version` field backed by `xmin` |
| G2. CI workflow | ⏳ | Unit and integration on push |
| G3. README and docs pass | ⏳ |  |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. Concurrency uses PostgreSQL `xmin` as a `version` field, decided in the main plan. CI needs Docker on the runner for the integration tests.

## Design decisions

None beyond the main plan yet. A decision that only this phase needs goes here.

## Steps

**G1. Optimistic concurrency.** PostgreSQL `xmin` as the concurrency token, exposed as a `version`
field on the order response, required on the update request, and a mismatch mapped to a conflict the
edit form explains. Integration test driving two writers against one row.

**G2. CI workflow.** GitHub Actions running the build, unit tests and integration tests on push, with
the front-end gates alongside.

**G3. README and docs pass.** Root README with the one-command run, the architecture in a paragraph
and a diagram, and the API tour. A sweep over `docs/` and `CLAUDE.md` for anything the code
invalidated.
