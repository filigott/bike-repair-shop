# Maintenance orders end to end (#1 to #7)

## At a glance

Build the bicycle workshop's maintenance-order application: a .NET domain and HTTP API, a React
single-page front end that consumes it, PostgreSQL behind both, and a root `docker-compose.yml` that
runs all three. The work goes in vertical slices, back end before front end inside each slice, with
the order lifecycle modelled as a state machine on the entity rather than a status column anyone can
write to. After the foundation, work goes in thin verticals that each end in a manual smoke test:
place and list, then find, edit and delete, then packaging. Those four phases are the product; search,
lifecycle and hardening add depth on top of a green base. Verified by unit tests on the domain, integration tests against a real PostgreSQL
container, component tests on the front end, and a manual pass through the running stack.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| A1a. Conventions and plan | ✅ | CLAUDE.md, plans, versions checked against registries |
| A1b. Toolchain pins | ✅ | global.json, central packages, editorconfig, build props |
| A1c. Solution scaffold | ✅ | Five projects, health endpoint, compose service for the database |
| A2. Domain model and state machine | 🟡 | TDD, #7 |
| A3. Persistence and test harness | ⏳ | EF, migration, seeded services, Testcontainers, #7 |
| B1. Create and list API | ⏳ | Handlers, validators, ProblemDetails, OpenAPI |
| B2. Front-end scaffold | ⏳ | Router, styling, generated client, i18n from day one |
| B3. Orders list page | ⏳ | |
| B4. Create form | ⏳ | Smoke test: place an order, see it listed |
| C1. Get, update and delete API | ⏳ | |
| C2. Edit form | ⏳ | Shares the form component with B4 |
| C3. Delete flow | ⏳ | Smoke test: all four tasks from the brief |
| D1. Docker and compose | ⏳ | Hard requirement, done before the extras |
| E1. Search API | ⏳ | TDD on the filter composition |
| E2. Search UI | ⏳ | Driven by search params |
| F1. Status transitions and history | ⏳ | |
| F2. Status UI | ⏳ | |
| G1. Optimistic concurrency | ⏳ | `version` field backed by `xmin` |
| G2. CI workflow | ⏳ | Unit and integration on push |
| G3. README and docs pass | ⏳ | |

Each phase is one GitHub issue: A2 and A3 are #7, then B is #1, C is #2, D is #3, E is #4, F is #5
and G is #6. From B on, the last step of a phase ends in a manual smoke test through the running API
and front end before the phase is called done.

## Context

The shop offers three services: wheel adjustment, chain replacement and brake maintenance. An order
carries the customer's name, phone number and email, the bike brand, the service wanted, an expected
due date and optional notes. Staff need to place, find, edit and delete orders.

The application has to be two parts, a web page and an HTTP API carrying the business logic, with a
database behind them and Docker packaging that runs the whole thing from one compose file. React
components are required on the front end. Those constraints are fixed; everything else is a design
choice made here.

Conventions for both halves are in [CLAUDE.md](../CLAUDE.md) and are not restated below.

### Verified on this machine, not assumed

Both stacks were built and run on throwaway scaffolds before any of this repository's code was
written, because the failures below are the kind that look like mysteries when they surface mid-unit.

- `dotnet test` fails outright on the .NET 10 SDK with xunit v3 unless `global.json` opts into
  `Microsoft.Testing.Platform`. The VSTest target is gone. The opt-in is repo-wide, not per project.
- Npgsql 10.0.3 floors EF Core at 10.0.4. Pinning 10.0.0 fails restore with NU1605.
- PostgreSQL 18 with Npgsql 10, EF 10 and Testcontainers 4.15 was exercised end to end with a real
  container and a real save-and-reload. No compatibility issue, no readiness-probe issue.
- `Testcontainers.PostgreSql` 4.15 has obsoleted the parameterless `PostgreSqlBuilder()`. Pass the
  image: `new PostgreSqlBuilder("postgres:18")`.
- A `Database` project with no composition root needs an `IDesignTimeDbContextFactory`, or the `Api`
  project passed as `--startup-project`, for `dotnet ef migrations add` to work.
- React Router 8 with `ssr: false` produces a genuine SPA build, and Tailwind v4 with DaisyUI v5 in the
  CSS-first form survives it.
- `erasableSyntaxOnly` is not enabled by default in a React Router 8 project, so TypeScript `enum` is
  available and the no-magic-strings rule stands as written.
- `openapi-typescript` 7.13 peer-depends on TypeScript `^5.x`, and a plain `npm install` therefore
  downgrades the project to 5.9.3 silently. Hence the `npx` route. The front-end spike's own lockfile
  shows the downgrade, which also means TypeScript 7 itself has not run yet; B2 proves it.
- Package pins were checked against nuget.org a second time on 2026-09-20. EF Core had been pinned at
  the Npgsql floor rather than at current stable, and `FluentValidation.AspNetCore` is retired on the
  11.x line: validation is wired through `FluentValidation.DependencyInjectionExtensions` 12 instead.
- The SDK's built-in `dotnet new xunit` template still produces an xunit v2 project. Test projects are
  written by hand against `xunit.v3`.
- Under the `Microsoft.Testing.Platform` runner, `xunit.v3` 4.0 needs neither `Microsoft.NET.Test.Sdk`
  nor `xunit.runner.visualstudio`. A test project is an `Exe` with `IsTestProject` set and the one
  package; both VSTest packages were dropped from the central pins after a green run without them.
- The SDK sets `IsPackable` to false for `IsTestProject` on its own, so a `Directory.Build.props`
  block doing the same is dead: the condition runs before the project file sets the property anyway.
- The `postgres:18` image moved `PGDATA` to `/var/lib/postgresql/18/docker`. The compose volume mounts
  `/var/lib/postgresql`, one level up, as the image's own notes recommend for 18.

## Design decisions

- **Feature-sliced CQRS over a service layer.** Each operation is a command or query with its own
  handler, so a slice can be read top to bottom without opening a class that serves five callers.
  The application is small enough that a mediator library would be ceremony, so handlers are resolved
  straight from DI and injected per controller action.

- **Service type is a seeded reference table keyed by a domain enum.** The brief asks for a database
  hosting the shop's services and their metadata, so `service_types` is the requirement, not
  decoration. The table owns the metadata (code, display name, sort order), seeded in a migration,
  and the create form reads it from the API rather than from a hard-coded list. The enum is the typed
  key the entity and the foreign key use, and nothing else: metadata has exactly one home. A rule that
  branches on service type, if one ever appears, lives on the domain and is tested there.

- **Order status is a state machine on the entity.** `Received`, `InProgress`, `Completed`,
  `Cancelled`, with transitions exposed as domain methods that throw a typed domain exception on an
  illegal move. There is no public setter, so an invalid sequence cannot be expressed in code. This
  is the main piece of real domain logic in an otherwise CRUD-shaped problem, and it is where TDD
  earns its place.

- **Task-based actions end to end, not CRUD.** Every change to an order is a named command on the API
  (place, reschedule, start, complete, cancel, correct the contact details) and a matching action in
  the UI, shown only when the order's state allows it. A single update endpoint plus a form with a
  status dropdown would leave the lifecycle rules to be remembered by whoever is at the counter. The
  edit form therefore covers the details an order can always carry, and lifecycle moves are buttons
  driven by the transitions the API projects onto the response.

- **Email and phone number are value objects, bike brand and notes are not.** The first two carry a
  format invariant worth parsing once at the boundary and trusting afterwards. The other two are free
  text with a length cap, and wrapping them would be pattern application for its own sake.

- **Single-page front end, not server rendering.** React Router 8 in framework mode with `ssr: false`
  keeps route modules, loaders, actions and the typed action envelope, while the deliverable stays a
  bundle of static files that Caddy serves. The API is then genuinely consumed over HTTP from the
  browser, which is what the brief asks for, and the compose topology stays three plain services.

- **The front-end client is generated from the OpenAPI document.** Request and response types are not
  written twice. A backend contract change surfaces as a TypeScript error rather than a runtime
  surprise.

- **i18n is set up in the first front-end unit, not retrofitted.** Adding keys after four routes exist
  means rewriting every string. English is the default, Norwegian the second locale, and both change
  in the same commit as the key that needs them.

- **Current stable everywhere, no pre-release.** Versions were checked against the registries on
  2026-09-20 rather than carried over from an older project. That moved PostgreSQL to 18, React
  Router to 8 with Vite 8, and TypeScript to the 7.x line. Release candidates are excluded on
  purpose: .NET stays on 10 LTS with 11 at RC, and AutoFixture stays on 4 with 5 at RC. The one
  choice with real risk is TypeScript 7, the native compiler, so it is proved out on a throwaway
  scaffold before the front-end unit starts rather than discovered mid-route.

- **Optimistic concurrency is a `version` field, not an ETag.** `If-Match` requires strong comparison
  under RFC 9110, so a weak validator never matches and a strong one asserts byte-identical
  representations that a varying response cannot honour. A `version` on the response and a required
  `version` on the update request gives the same guarantee, appears in the OpenAPI document so the
  generated types carry it, and is testable in four lines. PostgreSQL `xmin` backs it.

- **Caddy serves the bundle and proxies `/api`.** Same-origin deletes the CORS policy, the build-time
  base URL and the one-image-per-environment problem together. The cost is a Caddyfile and a matching
  Vite dev proxy so local development and the composed stack behave identically.

- **The order carries a human-readable reference alongside its id.** Staff on the phone say "order
  1042", not a GUID prefix. One identity column now; a migration and a backfill later.

- **Four statuses, with reopen as a transition rather than a fifth state.** `Completed` back to
  `InProgress` is allowed so a mis-click is recoverable, and `Received` straight to `Completed` is
  allowed because a counter-side adjustment should not need a trip through a state nobody was in. The
  UI distinguishes rework through an `isReopened` flag derived from the history, which keeps the
  transition matrix, the locale keys and the test cases from multiplying for a distinction the history
  already records.

- **The transition matrix is expressed exactly once**, as a static map that every transition method,
  `CanTransitionTo` and `AllowedTransitions` read from. Two copies of the same matrix is the bug this
  whole design exists to prevent.

- **snake_case identifiers, decided before the first migration.** EF's PascalCase defaults need
  quoting in every hand-written query and every `psql` session, and switching afterwards is a
  full-table rename.

- **Migrations apply behind an explicit setting, not the environment name.** Tying schema creation to
  the same switch that turns on the developer exception page means the composed stack has to run as
  Development to get a schema.

- **`openapi-typescript` runs through `npx` and stays out of `package.json`.** Its `^5.x` peer range
  silently downgrades the project to TypeScript 5.9.3, with no warning, on a plain `npm install`.

- **Docker packaging lands at the end of phase D, before any extra.** It is a hard requirement, so it
  is finished while there is room, not squeezed in at the end.

- **Delete is a real delete.** The brief asks for delete, and `Cancelled` already covers the case
  where the order should be kept for the record. Two mechanisms for going away would need a reason.

## Steps

### Phase A: foundation

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
the explicit setting. The Testcontainers harness with the template-database clone and per-test reset, plus
one integration test proving a saved order reloads with its status and value objects intact.

### Phase B: place and list

**B1. Create and list API.** Create and list as a command and a query with handlers under
`Features/Orders`, a FluentValidation validator on the create request, a thin `OrdersController`,
global exception middleware mapping domain and API exceptions to ProblemDetails, and OpenAPI with
Scalar for browsing. The list is paged from the start with a deterministic tiebreaker. Integration
tests per handler asserting persisted state from a fresh read.

**B2. Front-end scaffold.** React Router 8 project with `ssr: false`, Tailwind v4 and DaisyUI v5,
Biome, Vitest. The transport core with ProblemDetails normalisation, the `ActionResult` envelope, the
generated OpenAPI client and the orders endpoint wrapper. App shell with header, locale switch and
both locale files. Verified by a render test of the shell and the tooling gates.

**B3. Orders list page.** Loader reading the list endpoint, a table of orders with service type,
status, customer and due date, empty and error states. Component tests mocking the endpoint layer.

**B4. Create form.** The order form component and its page route, service types read from the API,
field-level error rendering from the ProblemDetails validation payload. Tests covering a successful
submit and a rejected one. Ends with the first manual smoke test: place an order and see it listed.

### Phase C: find, edit and delete

**C1. Get, update and delete API.** Get by id, update and delete as a query and two commands, a
validator on the update request, integration tests per handler.

**C2. Edit form.** The form component from B4 serving edit through its own page route with a loader.
Tests covering a loaded edit and a rejected submit.

**C3. Delete flow.** Native `<dialog>` confirmation, fetcher submit against an intent, optimistic
removal from the list, failure surfaced without losing the row. Ends with a smoke test of all four
tasks from the brief.

### Phase D: packaging

**D1. Docker and compose.** Multi-stage Dockerfile for the API (SDK to ASP.NET runtime, non-root) and
for the front end (node build to Caddy, which serves the bundle and proxies `/api`). Root
`docker-compose.yml` with database, API and web, health checks and dependency ordering, credentials
moved to `.env`. The shop's timezone comes from configuration, and the runtime image has to carry tz
data, which the Alpine variants do not by default: "today" computed in UTC wrongly rejects an order
placed at 01:30 local in summer. Verified by a clean `docker compose up --build` and a manual pass
through the four tasks.

### Phase E: search

**E1. Search API.** Extend the list query with filters on customer name, phone, email, service type,
status and a due-date range, plus sorting, composed with specifications. Failing tests first for
filter composition and for the empty-filter case.

**E2. Search UI.** A filter bar writing to search params, with the loader reading them and a
revalidation guard so typing is not one request per keystroke.

### Phase F: lifecycle

**F1. Status transitions and history.** Transition endpoints calling the domain methods, and a status
history row written per transition with the timestamp and the previous status. Illegal transitions
return a mapped conflict. Tests cover history rows written in order.

**F2. Status UI.** Status actions on the order view, offering only the transitions the current status
allows, driven by a capability flag the API projects onto the response rather than a client-side rule.

### Phase G: hardening

**G1. Optimistic concurrency.** PostgreSQL `xmin` as the concurrency token, exposed as a `version`
field on the order response, required on the update request, and a mismatch mapped to a conflict the
edit form explains. Integration test driving two writers against one row.

**G2. CI workflow.** GitHub Actions running the build, unit tests and integration tests on push, with
the front-end gates alongside.

**G3. README and docs pass.** Root README with the one-command run, the architecture in a paragraph
and a diagram, and the API tour. A sweep over `docs/` and `CLAUDE.md` for anything the code
invalidated.

## Out of scope

- Authentication and user accounts. There is no actor in the brief and no authority rule to enforce,
  so adding a login would be scenery.
- Customers as an entity. An order carries contact details as stated; deduplicating customers is a
  different product.
- Scheduling, mechanic assignment, capacity and pricing.
- Notification of any kind.
- Deployment beyond local compose.
- End-to-end browser tests, unless the earlier phases finish with room to spare.

## Open questions

All four opened with this plan are resolved.

1. **Does an order hold one service or several?** RESOLVED: one. The brief lists the service type as a
   single field, and a line-item collection would change the entity, the table and the form.
2. **Is the expected due date a date or a timestamp?** RESOLVED: `DateOnly`. A workshop promises a day.
3. **Should the list be paged from the start?** RESOLVED: yes, offset paging with a total count and a
   deterministic tiebreaker on the sort, because without the tiebreaker a page boundary silently
   duplicates and drops rows whenever two orders share a due date.
4. **Is there a rule about due dates in the past?** RESOLVED: rejected on create, allowed to remain on
   an existing order, so an overdue order stays editable.

Resolved later, during the design pass:

5. **Concurrency token.** RESOLVED: a `version` field on the DTO over an ETag with `If-Match`.
6. **Browser to API.** RESOLVED: Caddy serves the bundle and reverse-proxies `/api`. No CORS policy.
7. **Reopening a completed order.** RESOLVED: allowed, as a transition back to `InProgress`, with a
   derived `isReopened` flag on the response rather than a fifth status.
8. **Quick complete.** RESOLVED: `Received` straight to `Completed` is allowed.
9. **Migration application.** RESOLVED: an explicit `RunMigrationsOnStartup` setting.
10. **TypeScript version.** RESOLVED: 7.0.2, with `openapi-typescript` kept out of the dependency tree.
11. **Where service metadata lives.** RESOLVED: in the `service_types` table only. The enum is the key.
12. **Shop timezone.** OPEN until D1: the due-date rule needs a configured timezone and an image with
    tz data. Does not block phases A to C.
13. **Compose credentials.** OPEN until D1: the database credentials are literal dev values in the
    compose file. They move to a committed `.env.example` plus an ignored `.env` when the API service
    joins and needs the same values. The compose project name is pinned and the host port is 5433 so
    the other compose stacks on this machine never collide with it.
