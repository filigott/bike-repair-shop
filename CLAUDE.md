# Bike Repair Shop

A maintenance-order manager for a bicycle workshop: staff place, find, edit and delete orders for
the three services the shop offers (wheel adjustment, chain replacement, brake maintenance). The
solution is a .NET HTTP API holding the business logic, a React single-page front end that consumes
it, and PostgreSQL behind both, packaged so `docker compose up` runs the whole stack.

## Read at session start

1. This file.
2. `plans/ROADMAP.md`, whose `Next up` section is where a session starts, plus what is in flight,
   what has landed and what is blocked.
3. `plans/plan-bike-repair-shop.md` for the design decisions and the phase table, then the active
   phase plan it points to, its At a glance and Progress sections first.

Plans and this file are committed on purpose. They are the record of how the solution was designed
and built, not scratch notes, which is also why the README and `docs/` may point into `plans/`.
Code, commit messages and pull requests still carry no plan vocabulary.

## Stack

Current stable across the board, checked 2026-09-20. No pre-release packages: .NET 11 is at
release candidate and is deliberately not used.

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
| Backend tests | xUnit v3, Testcontainers, AwesomeAssertions, Respawn | 4.0, 4.15, 9.6 |
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
  app/routes/                     one route per page
  app/services/endpoints/         one wrapper file per API controller
  app/types/intents/              one intent enum per domain
  app/types/params/               one search-param enum per route that carries UI state
  app/locales/                    en and nb, kept in step
plans/                            ROADMAP plus one plan file per track
docs/                             topical reference docs, updated with the code that changes them
```

`Domain` references nothing framework-specific. `Database` references `Domain`. `Api` references both.

There are two test projects, not three. A shared-fixture project earns its keep only once both of the
others consume it.

Package versions are central: `backend/Directory.Packages.props` lists every version, and a
`PackageReference` in a project carries no `Version`. Build properties shared by all five projects
live in `backend/Directory.Build.props`.

Two settings are load-bearing and were established by running them, not by reading docs:

- `global.json` sets `"test": { "runner": "Microsoft.Testing.Platform" }`. Without it `dotnet test`
  fails outright on the .NET 10 SDK with xunit v3, because the VSTest target is gone.
- EF Core cannot go below 10.0.4. Npgsql 10.0.3 floors it there, and 10.0.0 fails restore with NU1605.

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
only through domain methods.

**A value with a rule is parsed once, into a type.** A request carries primitives. Its validator
delegates to the value object's `TryParse` (`OrderFieldRules`), so the 400 and the domain share one
rule. `<Command>.From(request)` parses into `EmailAddress`, `PhoneNumber` and enums, and the domain
factory and mutators accept only those types. A string whose only rule is a length stays a string,
guarded by the entity.

**The lifecycle is one table of edges, keyed by the action.** `(from, action) to` in
`OrderStatus.cs` is the single source: `AllowedActions`, `AllowedTransitions` and `CanTransitionTo`
are all derived from it, and the entity's `Start`, `Complete`, `Cancel` and `Reopen` are wrappers
over one `Apply`. Keying by target status instead cannot tell `Start` from `Reopen`, which share
`InProgress` from different origins, and leaves something to bridge statuses back to command names.

**Exception types match the layer.** `ArgumentException` for bad method input. `DomainException` and
its subtypes (`BusinessRuleViolationException`, `InvalidStateTransitionException`) for invariants.
`NotFoundException`, `ConflictException` and friends live in the API layer only and are mapped to
ProblemDetails by one middleware. "Not found" is never a domain exception.

**Every problem carries a machine `code`; `detail` is English for other clients.** A domain exception
carries a `DomainErrorCode` and the API's own exceptions derive from `ApiException` with an
`ApiErrorCode`. A validation failure is `InvalidRequest` and adds `errorCodes` beside `errors`, one
`ValidationErrorCode` per message, set on each rule with `WithErrorCode`; a rule without one reports
`Invalid`, so FluentValidation's own names never reach the contract.

**DTO names carry the shape**: `Response`, `Request`, `Summary`, `Item`, `View`. Never a bare `Dto`.
Mapping is a static factory: `MaintenanceOrderSummary.From(order, today)`.

**EF rules.** Identifiers are snake_case through `UseSnakeCaseNamingConvention()`, decided before the
first migration because changing it later is a full-table rename. One configuration file per entity. Enums stored as strings via `HasConversion<string>()`
with a `HasMaxLength`. Value objects are complex types (`ComplexProperty`) with an explicit column
name, so their `Value` is a queryable column; a `HasConversion` value object cannot be searched with
`LIKE`. Queries reference mapped properties only, because computed getters do not translate to SQL. Reference data is seeded in a
migration. Migration files are generated code, excluded from style enforcement in `.editorconfig`.

**Migrations apply on start behind an explicit `RunMigrationsOnStartup` setting**, not behind the
environment name, so the composed stack creates its schema without also switching on the developer
exception page. `DemoData:Enabled` follows the same pattern: off unless set, it seeds a demo set of
orders into an empty table at startup and exposes a reset endpoint. The seeder builds every order
through the public domain API, placing back-dated ones with an `IShopClock` positioned in the past,
so there is no second way to construct an order. A test host sets the flag itself rather than
inheriting it from the Development settings file.

**Optimistic concurrency is a `version` field on the DTO**, backed by the PostgreSQL `xmin` system
column. The three edit requests (details, service, due date) carry it back, the handler compares it
with the loaded order, and a mismatch is a 409; a write racing between load and save trips EF's own
`xmin` check and maps to the same 409. The lifecycle commands and delete carry no version, since the
state machine already refuses a stale transition. Deliberately not an ETag with
`If-Match`: that requires strong comparison, and a strong validator asserts byte-identical
representations, which stops being true as soon as a response varies.

**Modern C#**: file-scoped namespaces, `var`, target-typed `new()`, collection expressions, `is null`,
injected `TimeProvider` rather than `DateTime.UtcNow`.

## Front-end conventions

**Route module exports are validated by the build, not by the type checker.** `npm run build` runs
the React Router Vite plugin, which is the only gate that rejects an export a route may not have, and
in SPA mode `HydrateFallback` belongs on the root route alone. `npm run verify` chains all four, and is
the gate a front-end commit passes.

**Routes split by purpose.** A page route per form, each with its own loader. Inline mutations share
one route action that switches on an `intent` value read from `FormData`. The intent set is an enum
in `app/types/intents/`, namespaced so two slices on one route cannot collide.

**One action envelope.** An action answers with `ActionResult<T>`, a union of success and failure,
and consumers narrow with `isActionFailure`. No `success` or `message` keys. A command whose success
takes the user somewhere else redirects instead, which is how a dialog held in the URL closes.
ProblemDetails mapping is a single shared helper: 5xx rethrows to the error boundary, 4xx becomes a
typed failure the form renders. The failure carries the problem's `code` and the per-field
`errorCodes`, parsed into enums at `ApiError` with `Unknown` and `Invalid` as the fallbacks.

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
commands (place, reschedule, start, complete, cancel, reopen), and the UI presents them as actions
that appear only when the current state allows them. A status dropdown or a generic edit-anything
form puts the state machine in the user's head; a "Complete order" button that the API's capability
flags switch on keeps it in the code.

**The UI never recomputes a server rule.** `allowedActions`, `isEditable`, `isOverdue` and
`isReopened` are projected onto the order response by the API, and the status matrix is published
whole by `GET /api/order-statuses`. The order's action column renders one control per
`allowedActions` entry, so it never works out which command moves an order from one status to
another. Computing overdue in the browser lets the viewer's device timezone decide
whether an order is late, and deriving editability from the status puts the state machine back in the
client.

**UI rules.** Signals of intent over booleans: native `<dialog>` with `showModal()` for modals. A
dialog that only confirms is held by the parent as `selected: T | null`; a dialog holding a form is
held in the URL as a search param, so it is linkable, the back button shuts it, and the server
decides when it closes by redirecting off the param. Reading a param means parsing it into the
command it names, never trusting the string. DaisyUI semantic tokens only (`primary`, `base-100`,
`error` and so on), never raw Tailwind palette colours. React Router control flow, and no `useEffect`
at all: there is none in the app. Never mirror a prop into state.

**i18n keys only.** `t("key")` with no inline English fallback. Both locales and the key union change
in the same commit. Generic words reuse `common.*`. Text from the API is never rendered: a failure
shows `problems.<code>` through `ProblemAlert`, and a field shows `validation.<code>`.

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
each test. The test host also replaces `TimeProvider` with a resettable double, so a test can move the
shop's day forward and reach rules that only fire with time, such as an overdue order. The seeded catalogue and the migration history survive the reset. Test classes derive from
`IntegrationTest` and reach the host through `DatabaseFixture`. Fixtures, mutators and factories live
in a `Fixtures/` folder of the test project that owns them, never as private helpers inside a test
class.

`.github/workflows/ci.yml` runs the same gates on every pull request and on `main`: build,
`dotnet format --verify-no-changes`, the unfiltered backend suite, and the front-end `verify`.

Front-end tests are co-located `*.test.tsx`, mock the endpoint layer rather than `fetch`, and query by
role, text or label.

## Commands

```bash
# Backend (the API needs the compose database: docker compose up -d db)
dotnet build backend/BikeRepairShop.sln
dotnet run   --project backend/BikeRepairShop.Api    # http://localhost:5260/health, /scalar for the API reference
# Build first: dotnet test discovers nothing when it builds for itself. See plans/reference/verified-on-this-machine.md
dotnet test  backend/BikeRepairShop.sln --no-build
dotnet test  backend/BikeRepairShop.Tests.Unit/BikeRepairShop.Tests.Unit.csproj --no-build -- --filter-method "*Reopen*"
dotnet ef migrations add <Name> --project backend/BikeRepairShop.Database --startup-project backend/BikeRepairShop.Api -- --environment Development

# Front end
npm --prefix frontend run dev          # started by the developer, never by the agent
npm --prefix frontend run verify         # lint, typecheck, test and build: the gate before a commit
npm --prefix frontend run codefix
npm --prefix frontend run typecheck
npm --prefix frontend run test
npm --prefix frontend run generate:api   # regenerates the OpenAPI types; needs the API on 5260, which the agent may start for this and stop

# Whole stack
docker compose up --build              # everything containerised on http://localhost:8080
docker compose stop api web            # back to the dev loop, keeping the database up
```

## Running it locally

Two modes, and they are not interchangeable.

**The dev loop is the default.** `docker compose up -d db`, then `dotnet run` and
`npm --prefix frontend run dev` in their own terminals. The front end is on 5173, the API on 5260,
the database on 5434, and Vite proxies `/api` to the API so the browser still sees one origin. Both
halves hot-reload, and `npm run generate:api` only works in this mode, because it reads the OpenAPI
document off the running API.

**The packaged stack is a verification step, not a place to work.** `docker compose up --build`
serves the whole thing on 8080 through Caddy. Nothing hot-reloads: a code change needs another
`--build`, so iterating here wastes minutes per edit. Run it to prove the images still build and the
four staff tasks still work before handing the repository to anyone, and prove it from
`docker compose down -v` at least once, since that is the only way to know the schema and the demo
data come up from nothing.

**Both modes share one database**, the `db` service and its `db-data` volume. That is deliberate:
one set of orders whichever way the app is running. It also means the containerised API and a
`dotnet run` API write to the same rows, so run one or the other, not both.

The database is meant to stay up between sessions, because `dotnet run` needs it. `api` and `web`
are not: stop those when a packaged run is finished, with `docker compose stop api web`. Use
`docker compose down` without `-v` to remove the containers and keep the data.

## Working agreement

- One reviewable unit at a time. Build it, present it, stop. An open design question is answered by
  picking the recommended option and saying so, not by downing tools.
- The review stop names the unit, lists what changed one bullet per file, reports the gates on one
  line, flags risks, then gives the commit message in a fenced block.
- Commits are conventional and end the subject with the issue number: `feat(orders): ... (#4)`.
  Body is one summary sentence then three to six verb-first bullets.
- An issue exists before code work does. A commit touching only `plans/` needs none, and goes
  straight to `main` without a branch or pull request; anything with source, tests or configuration
  in it takes the pull request route.
- Repo docs change in the same commit as the code that invalidated them.
- Comments explain a non-obvious why in one line, or they do not exist. A comment that needs a
  paragraph is a sign the code needs a better name or an extracted method.
- A session that started the packaged stack ends by handing over `docker compose stop api web`,
  checked against what is actually running rather than an earlier tool result. The database stays up.
