# Bike Repair Shop

A maintenance-order manager for a bicycle workshop: staff place, find, edit and delete orders for
the three services the shop offers (wheel adjustment, chain replacement, brake maintenance). The
solution is a .NET HTTP API holding the business logic, a React single-page front end that consumes
it, and PostgreSQL behind both, packaged so `docker compose up` runs the whole stack.

## Read at session start

1. This file.
2. `plans/ROADMAP.md` for what is in flight and what is blocked.
3. `plans/plan-bike-repair-shop.md` for the design decisions and the phase table, then the active
   phase plan it points to, its At a glance and Progress sections first.

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
| Language | TypeScript, the native compiler | 7.0 |
| Styling | Tailwind CSS plus DaisyUI, semantic colour tokens only | 4.3, DaisyUI 5.7 |
| i18n | i18next plus react-i18next, typed resources | 26, 17 |
| API contract | OpenAPI, browsed with Scalar, types from `openapi-typescript`, calls through `openapi-fetch` | Scalar 2.17, openapi-fetch 0.17 |
| Backend tests | xUnit v3, Testcontainers, AwesomeAssertions, AutoFixture, Respawn | 4.0, 4.15, 9.6 |
| Front-end tests | Vitest plus Testing Library | 5.0, RTL 16.3 |
| Tooling | Biome for the front end, `dotnet format` for the back end | Biome 2.5 |
| Packaging | Multi-stage Dockerfiles, one `docker-compose.yml` at the repo root | Node 24 LTS, Caddy 2 |

## Layout

```
backend/
  BikeRepairShop.Domain/          entities, enums, value objects, policies, domain exceptions
  BikeRepairShop.Database/        DbContext, Configurations/, Specifications/, Migrations/
  BikeRepairShop.Api/             Controllers/, Features/<Slice>/, Models/ (shared shapes), Exceptions/, Configuration/
  BikeRepairShop.Tests.Unit/
  BikeRepairShop.Tests.Integration/
frontend/
  app/routes/                     one route per page, resource routes are action-only
  app/services/endpoints/         one wrapper file per API controller
  app/types/intents/              one intent enum per domain
  app/locales/                    en and nb, kept in step
plans/                            ROADMAP plus one plan file per track
docs/                             topical reference docs, updated with the code that changes them
```

`Domain` references nothing framework-specific. `Database` references `Domain`. `Api` references both.

There are two test projects, not three. A shared-fixture project earns its keep only once both of the
others consume it.

Package versions are central: `backend/Directory.Packages.props` lists every version, and a
`PackageReference` in a project carries no `Version`. Build properties shared by all six projects
live in `backend/Directory.Build.props`.

Two settings are load-bearing and were established by running them, not by reading docs:

- `global.json` sets `"test": { "runner": "Microsoft.Testing.Platform" }`. Without it `dotnet test`
  fails outright on the .NET 10 SDK with xunit v3, because the VSTest target is gone.
- EF Core is pinned to 10.0.4. Npgsql 10.0.3 floors it there, and 10.0.0 fails restore with NU1605.

## Backend conventions

**Slices own everything but the controller.** `Features/<Slice>/{Commands,Queries,Handlers,Models,Validators}`
plus a `<Slice>ServiceCollectionExtensions.cs` with a single `Add<Slice>()`. Controllers live in
`Controllers/` and never inside a slice. `Program.cs` is a flat list of `Add<Slice>()` calls.

**Handlers orchestrate, they do not hold logic.** Primary constructors, parameters used directly.
Body reads: fetch with includes, check policy, call a domain method, save. Commands and queries are
bare records. Controllers stay thin and take the handler per action via `[FromServices]`.

**Entities are rich.** Private parameterless constructor for EF, a static factory named for the
business action (`MaintenanceOrder.Place()`) that validates, `private set` on anything a domain method
owns, `init` on `Id` and immutable foreign keys. Mutators are named for the task (`Reschedule`,
`CorrectDetails`), never a bare `Update`, and every one guards the lifecycle. An order's status moves
only through domain methods, and the transition matrix lives in exactly one place.

**Exception types match the layer.** `ArgumentException` for bad method input. `DomainException` and
its subtypes (`BusinessRuleViolationException`, `InvalidStateTransitionException`) for invariants.
`NotFoundException`, `ConflictException` and friends live in the API layer only and are mapped to
ProblemDetails by one middleware. "Not found" is never a domain exception.

**DTO names carry the shape**: `Response`, `Request`, `Summary`, `Item`, `View`. Never a bare `Dto`.
Mapping is a static factory: `MaintenanceOrderSummary.From(order)`.

**EF rules.** Identifiers are snake_case through `UseSnakeCaseNamingConvention()`, decided before the
first migration because changing it later is a full-table rename. One configuration file per entity. Enums stored as strings via `HasConversion<string>()`
with a `HasMaxLength`. Value objects are complex types (`ComplexProperty`) with an explicit column
name, so their `Value` is a queryable column; a `HasConversion` value object cannot be searched with
`LIKE`. Queries reference mapped properties only, because computed getters do not translate to SQL. Reference data is seeded in a
migration. Migration files are generated code, excluded from style enforcement in `.editorconfig`.

**Migrations apply on start behind an explicit `RunMigrationsOnStartup` setting**, not behind the
environment name, so the composed stack creates its schema without also switching on the developer
exception page.

**Optimistic concurrency is a `version` field on the DTO**, backed by the PostgreSQL `xmin` system
column. The update request carries it back and a mismatch is a 409. Deliberately not an ETag with
`If-Match`: that requires strong comparison, and a strong validator asserts byte-identical
representations, which stops being true as soon as a response varies.

**Modern C#**: file-scoped namespaces, `var`, target-typed `new()`, collection expressions, `is null`,
injected `TimeProvider` rather than `DateTime.UtcNow`.

## Front-end conventions

**Routes split by purpose.** A page route per form, each with its own loader. Inline mutations share
one route action that switches on an `intent` value read from `FormData`. The intent set is an enum
in `app/types/intents/`, namespaced so two slices on one route cannot collide.

**One action envelope.** Every action returns `ActionResult<T>`, a union of success and failure, and
consumers narrow with `isActionFailure`. No `success` or `message` keys. ProblemDetails mapping is a
single shared helper: 5xx rethrows to the error boundary, 4xx becomes a typed failure the form renders.

**Transport is `openapi-fetch` typed by the generated `paths`.** One core module owns the client, a
per-call timeout and `unwrap`, which turns the `{ data, error }` result into a value or an `ApiError`
carrying ProblemDetails, so the rest of the app keeps a throwing seam. One wrapper file per API
controller under `app/services/endpoints/` is the only caller of the client: it names the operation,
aliases the schema types and is what tests mock. The wrapper is not a pass-through, because the path
literal, the query shape and the body are checked against the committed schema at the call, and a
renamed route or parameter is a compile error rather than a 404.

**`openapi-typescript` is invoked through `npx`, pinned, and is not a project dependency.** It needs
the TypeScript 5 JavaScript compiler API at runtime, and the `typescript@7` package ships only the
native `tsc`, so the two cannot share one `node_modules`. It is a one-shot generator whose output,
`app/services/openapi.generated.ts`, is committed and excluded from Biome.

**Actions are explicit tasks, on the API and in the UI alike.** The API exposes intention-revealing
commands (place, reschedule, start, complete, cancel), and the UI presents them as actions that appear
only when the current state allows them. A status dropdown or a generic edit-anything form puts the
state machine in the user's head; a "Complete order" button that the API's capability flags switch on
keeps it in the code.

**The UI never recomputes a server rule.** `allowedTransitions`, `isOverdue` and `isReopened` are
projected onto the response by the API. Computing overdue in the browser lets the viewer's device
timezone decide whether an order is late.

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

Test bodies carry `// Arrange`, `// Act`, `// Assert` markers for the sections they have. A section
with nothing in it gets no marker, and `// Act & Assert` labels a single expression that does both.

Integration tests assert persisted state from a fresh read: clear the change tracker or open a new
scope, re-query, then assert. Never assert on the entity the handler returned.

The integration harness runs one `postgres:18` Testcontainer per run, migrated once through the API's
own startup path, with every integration class in one xunit collection and Respawn wiping rows before
each test. The seeded catalogue and the migration history survive the reset. Test classes derive from
`IntegrationTest` and reach the host through `DatabaseFixture`. Fixtures, mutators and factories live
in a `Fixtures/` folder of the test project that owns them, never as private helpers inside a test
class.

Front-end tests are co-located `*.test.tsx`, mock the endpoint layer rather than `fetch`, and query by
role, text or label.

## Commands

```bash
# Backend (the API needs the compose database: docker compose up -d db)
dotnet build backend/BikeRepairShop.sln
dotnet run   --project backend/BikeRepairShop.Api    # http://localhost:5260/health, /scalar for the API reference
dotnet test  backend/BikeRepairShop.sln
dotnet test  backend/BikeRepairShop.Tests.Unit/BikeRepairShop.Tests.Unit.csproj -- --filter-method "*Reopen*"
dotnet ef migrations add <Name> --project backend/BikeRepairShop.Database --startup-project backend/BikeRepairShop.Api -- --environment Development

# Front end
npm --prefix frontend run dev          # started by the developer, never by the agent
npm --prefix frontend run codefix
npm --prefix frontend run typecheck
npm --prefix frontend run test
npm --prefix frontend run generate:api   # regenerates the OpenAPI types; needs the API running on 5260

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
