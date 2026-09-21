# Phase D: packaging (#3)

## At a glance

The hard packaging requirement: Dockerfiles for the API and the front end, Caddy serving the bundle and proxying the API, one compose file running all three. Verified by a clean `docker compose up --build`.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| D1. Docker and compose | ✅ | Smoke-tested from an empty volume on 2026-09-21 |
| D1a. API image | ✅ | Multi-stage, runs as uid 1654, tz data present |
| D1b. Front-end image and Caddy | ✅ | 63MB, SPA fallback, `/api` reverse-proxied |
| D1c. Compose file for all three | ✅ | One command, health-gated, only the web port published |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. Open questions 12 and 13 in the main plan resolve here: the shop timezone value, and how the compose credentials are supplied. The runtime image has to carry tz data.

## Design decisions

**Credentials are inline defaults, not a `.env` you must create first.** Compose reads
`${POSTGRES_PASSWORD:-bikeshop}` and friends, so a fresh clone runs on one command with nothing to
copy, and any value can still be overridden from the environment. This reverses what open question 13
proposed. A committed `.env.example` plus an ignored `.env` is the conventional answer, and it means
a reviewer's first `docker compose up` dies on a missing password, which is precisely the thing the
brief is checking. The values in the file are obvious throwaway development credentials and are not
protecting anything.

**Debian runtime images, not Alpine.** The shop's day comes from a configured `TimeZoneInfo`, and
`TimeZoneInfo.FindSystemTimeZoneById` needs tz data present in the image. The default
`mcr.microsoft.com/dotnet/aspnet` tags carry it; the Alpine variants do not, and the failure is a
startup crash rather than a wrong answer. Not worth the saved megabytes here.

**The healthcheck calls `/health` and the image carries `curl` for it.** The runtime image ships
`bash` but no `curl`, `wget` or `nc`, and a `bash` `/dev/tcp` probe would have given the same startup
ordering for free: the listener opens only after migrations and seeding, because both run before
`RunAsync`. That was the argument for dropping the package, and it was weighed and rejected on
2026-09-21. A port probe reports healthy with the database gone, where `/health` carries
`AddDbContextCheck`, and `curl` earns its few megabytes again the first time something has to be
debugged inside the container.

**The SDK build stage is pinned to the version `global.json` names.** The floating `sdk:10.0` tag
resolved to 10.0.201 here, below the 10.0.400 the repo asks for, and `rollForward` only moves
upward, so restore fails inside the image with exit 155. Pinning the stage to `sdk:10.0.400` means
the shipped artifact and a local build come from the same compiler. The tag has to move whenever
`global.json` does, which nothing enforces; the failure is loud rather than silent, which is why
that coupling was accepted over loosening the repo's floor.

**The web container is the only published port.** Caddy serves the bundle and reverse-proxies
`/api` to the API container, so the browser sees one origin and there is no CORS policy anywhere.
The database keeps its host port for local tooling; the API does not need one.

## Steps

**D1. Docker and compose.** Multi-stage Dockerfile for the API (SDK to ASP.NET runtime, non-root) and
for the front end (node build to Caddy, which serves the bundle and proxies `/api`). Root
`docker-compose.yml` with database, API and web, health checks and dependency ordering, and
credentials as overridable inline defaults. The shop's timezone comes from configuration, and the runtime image has to carry tz
data, which the Alpine variants do not by default: "today" computed in UTC wrongly rejects an order
placed at 01:30 local in summer. Verified by a clean `docker compose up --build` and a manual pass
through the four tasks.
