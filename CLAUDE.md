# Bike Repair Shop

A maintenance-order manager for a bicycle workshop: staff place, find, edit and delete orders for
the three services the shop offers (wheel adjustment, chain replacement, brake maintenance). The
solution is a .NET HTTP API holding the business logic, a React single-page front end that consumes
it, and PostgreSQL behind both, packaged so `docker compose up` runs the whole stack.

## Read at session start

1. This file.
2. `plans/ROADMAP.md` for what is in flight and what is blocked.
3. The active plan under `plans/`, its At a glance and Progress sections first.

Plans and this file are committed on purpose. They are the record of how the solution was designed
and built, not scratch notes.

## Stack

Current stable across the board, checked 2026-09-20. No pre-release packages: .NET 11 and
AutoFixture 5 are at release candidate and are deliberately not used.

| Layer | Choice | Version |
|---|---|---|
| Runtime | .NET, LTS | 10.0 |
| API | ASP.NET Core controllers, feature-sliced CQRS handlers | 10.0 |
| Data | PostgreSQL, EF Core with Npgsql, code-first migrations | 18, EF 10.0 |
| Front end | React Router framework mode with `ssr: false`, Vite, React | 8.4, Vite 8, React 19 |
| Language | TypeScript | see `frontend/package.json` |
| Styling | Tailwind CSS plus DaisyUI, semantic colour tokens only | 4.3, DaisyUI 5.7 |
| i18n | i18next plus react-i18next, typed resources | 26, 17 |
| API contract | OpenAPI, browsed with Scalar, typed client from `openapi-typescript` | Scalar 2.17 |
| Backend tests | xUnit v3, Testcontainers, AwesomeAssertions, AutoFixture, Respawn | 4.0, 4.15, 9.6 |
| Front-end tests | Vitest plus Testing Library | 5.0, RTL 16.3 |
| Tooling | Biome for the front end, `dotnet format` for the back end | Biome 2.5 |
| Packaging | Multi-stage Dockerfiles, one `docker-compose.yml` at the repo root | Node 24 LTS, nginx stable |

## Layout

```
backend/
  BikeRepairShop.Domain/          entities, enums, value objects, policies, domain exceptions
  BikeRepairShop.Database/        DbContext, Configurations/, Specifications/, Migrations/
  BikeRepairShop.Api/             Controllers/, Features/<Slice>/, Exceptions/, Configuration/
  BikeRepairShop.Tests.Unit/
  BikeRepairShop.Tests.Integration/
  BikeRepairShop.Tests.Shared/
frontend/
  app/routes/                     one route per page, resource routes are action-only
  app/services/endpoints/         one wrapper file per API controller
  app/types/intents/              one intent enum per domain
  app/locales/                    en and nb, kept in step
plans/                            ROADMAP plus one plan file per track
docs/                             topical reference docs, updated with the code that changes them
```

`Domain` references nothing framework-specific. `Database` references `Domain`. `Api` references both.

## Backend conventions

**Slices own everything but the controller.** `Features/<Slice>/{Commands,Queries,Handlers,Models,Validators}`
plus a `<Slice>ServiceCollectionExtensions.cs` with a single `Add<Slice>()`. Controllers live in
`Controllers/` and never inside a slice. `Program.cs` is a flat list of `Add<Slice>()` calls.

**Handlers orchestrate, they do not hold logic.** Primary constructors, parameters used directly.
Body reads: fetch with includes, check policy, call a domain method, save. Commands and queries are
bare records. Controllers stay thin and take the handler per action via `[FromServices]`.

**Entities are rich.** Private parameterless constructor for EF, a static `Create()` that validates,
`private set` on anything a domain method owns, `init` on `Id` and immutable foreign keys. Every
mutator guards the lifecycle. An order's status moves only through domain methods.

**Exception types match the layer.** `ArgumentException` for bad method input. `DomainException` and
its subtypes (`BusinessRuleViolationException`, `InvalidStateTransitionException`) for invariants.
`NotFoundException`, `ConflictException` and friends live in the API layer only and are mapped to
ProblemDetails by one middleware. "Not found" is never a domain exception.

**DTO names carry the shape**: `Response`, `Request`, `Summary`, `Item`, `View`. Never a bare `Dto`.
Mapping is a static factory: `MaintenanceOrderSummary.From(order)`.

**EF rules.** One configuration file per entity. Enums stored as strings via `HasConversion<string>()`
with a `HasMaxLength`. Value objects via `HasConversion` or `OwnsOne`. Queries reference mapped
properties only, because computed getters do not translate to SQL. Migrations auto-apply in
Development only. Reference data is seeded in a migration.

**Modern C#**: file-scoped namespaces, `var`, target-typed `new()`, collection expressions, `is null`,
injected `TimeProvider` rather than `DateTime.UtcNow`.

## Front-end conventions

**Routes split by purpose.** A page route per form, each with its own loader. Inline mutations share
one route action that switches on an `intent` value read from `FormData`. The intent set is an enum
in `app/types/intents/`, namespaced so two slices on one route cannot collide.

**One action envelope.** Every action returns `ActionResult<T>`, a union of success and failure, and
consumers narrow with `isActionFailure`. No `success` or `message` keys. ProblemDetails mapping is a
single shared helper: 5xx rethrows to the error boundary, 4xx becomes a typed failure the form renders.

**Transport is one core module** with a timeout and ProblemDetails normalisation, wrapped by one file
per API controller under `app/services/endpoints/`. Those wrappers are the only callers of the client.
Request and response types come from the generated OpenAPI client, never hand-written twice.

**UI rules.** Signals of intent over booleans: native `<dialog>` with `showModal()` for modals, with
the parent holding `selected: T | null`. DaisyUI semantic tokens only (`primary`, `base-100`, `error`
and so on), never raw Tailwind palette colours. `react-icons/lu` for icons. React Router control flow,
no `useEffect` for data loading. Never mirror a prop into state.

**i18n keys only.** `t("key")` with no inline English fallback. Both locales and the key union change
in the same commit. Generic words reuse `common.*`.

## Testing

Pragmatic TDD: a failing test first for branching and derived logic (the order state machine,
validators, search filtering, mappers), no ceremony for glue and configuration.

A test written alongside the code is shown red before it counts. Break the exact line under test,
watch it fail, restore.

Every test body carries `// Arrange`, `// Act`, `// Assert` markers.

Integration tests assert persisted state from a fresh read: clear the change tracker or open a new
scope, re-query, then assert. Never assert on the entity the handler returned.

The integration harness runs one `postgres:18` Testcontainer per process with a migrated template
database that each collection clones, and resets rows between tests. Shared fixtures, mutators and
factories live in `Tests.Shared`, never as private helpers inside a test class.

Front-end tests are co-located `*.test.tsx`, mock the endpoint layer rather than `fetch`, and query by
role, text or label.

## Commands

```bash
# Backend
dotnet build backend/BikeRepairShop.sln
dotnet test  backend/BikeRepairShop.sln
dotnet ef migrations add <Name> --project backend/BikeRepairShop.Database --startup-project backend/BikeRepairShop.Api

# Front end
npm --prefix frontend run dev          # started by the developer, never by the agent
npm --prefix frontend run codefix
npm --prefix frontend run typecheck
npm --prefix frontend run test

# Whole stack
docker compose up --build
```

## Working agreement

- One reviewable unit at a time. Build it, present it, stop. An open design question is answered by
  picking the recommended option and saying so, not by downing tools.
- The review stop names the unit, lists what changed one bullet per file, reports the gates on one
  line, flags risks, then gives the commit message in a fenced block.
- Commits are conventional and end the subject with the issue number: `feat(orders): ... (#4)`.
  Body is one summary sentence then three to six verb-first bullets.
- An issue exists before the work does.
- Repo docs change in the same commit as the code that invalidated them.
- Comments explain a non-obvious why in one line, or they do not exist. A comment that needs a
  paragraph is a sign the code needs a better name or an extracted method.
