# Phase D: packaging (#3)

## At a glance

The hard packaging requirement: Dockerfiles for the API and the front end, Caddy serving the bundle and proxying the API, one compose file running all three. Verified by a clean `docker compose up --build`.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| D1. Docker and compose | ⏳ | Hard requirement, done before the extras |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. Open questions 12 and 13 in the main plan resolve here: the shop timezone value and the move of compose credentials to `.env`. The runtime image has to carry tz data.

## Design decisions

None beyond the main plan yet. A decision that only this phase needs goes here.

## Steps

**D1. Docker and compose.** Multi-stage Dockerfile for the API (SDK to ASP.NET runtime, non-root) and
for the front end (node build to Caddy, which serves the bundle and proxies `/api`). Root
`docker-compose.yml` with database, API and web, health checks and dependency ordering, credentials
moved to `.env`. The shop's timezone comes from configuration, and the runtime image has to carry tz
data, which the Alpine variants do not by default: "today" computed in UTC wrongly rejects an order
placed at 01:30 local in summer. Verified by a clean `docker compose up --build` and a manual pass
through the four tasks.
