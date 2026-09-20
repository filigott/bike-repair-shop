# Phase A: foundation (#7)

## At a glance

Foundation for everything else: conventions and pins, the solution scaffold, the domain model with its state machine under test, and persistence with a real PostgreSQL container in the test loop. Complete.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| A1a. Conventions and plan | ✅ | CLAUDE.md, plans, versions checked against registries |
| A1b. Toolchain pins | ✅ | global.json, central packages, editorconfig, build props |
| A1c. Solution scaffold | ✅ | Five projects, health endpoint, compose service for the database |
| A2. Domain model and state machine | ✅ | Value objects, status matrix, entity; three commits |
| A3. Persistence and test harness | ✅ | DbContext, migration, clock, harness, health check; five commits |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. Everything here is done and kept as the record of how the solution was built. The findings that surfaced along the way live in [reference/verified-on-this-machine.md](reference/verified-on-this-machine.md).

## Design decisions

None beyond the main plan yet. A decision that only this phase needs goes here.

## Steps

**A1a. Conventions and plan.** `CLAUDE.md`, `plans/README.md`, `plans/ROADMAP.md` and this file.
Versions checked against the package registries rather than carried over. No code.

**A1b. Toolchain pins.** `global.json`, `.gitignore`, `.editorconfig`, `Directory.Build.props` and
`Directory.Packages.props` with every version checked against the registries, plus a local tool
manifest for `dotnet-ef`.

**A1c. Solution scaffold.** The backend solution with `Domain`, `Database`, `Api`, `Tests.Unit` and
`Tests.Integration` wired up, a health endpoint, and a compose service for PostgreSQL 18 so there is a
database to develop against. Verified by a build and a passing placeholder test run.

**A2. Domain model and state machine.** `ServiceType` and `OrderStatus` enums with behaviour in
extensions, `EmailAddress` and `PhoneNumber` value objects, the `MaintenanceOrder` entity with a
static `Create()`, private setters, and transition methods. Domain exception hierarchy. Failing tests
first for every transition, legal and illegal, and for value-object parsing. No persistence yet.

**A3. Persistence and test harness.** `BikeRepairShopDbContext`, one configuration per entity, the
initial migration, the `service_types` table seeded with its metadata, migration apply-on-start behind
the explicit setting. The Testcontainers harness with one migrated database and per-test row reset,
plus one integration test proving a saved order reloads with its status, value objects and placed-at
stamp intact, and the database health check.
