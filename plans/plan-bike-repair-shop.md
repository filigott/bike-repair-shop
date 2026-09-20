# Maintenance orders end to end (#TBD)

## At a glance

Build the bicycle workshop's maintenance-order application: a .NET domain and HTTP API, a React
single-page front end that consumes it, PostgreSQL behind both, and a root `docker-compose.yml` that
runs all three. The work goes in vertical slices, back end before front end inside each slice, with
the order lifecycle modelled as a state machine on the entity rather than a status column anyone can
write to. Phases A through C deliver a complete, runnable product; D through F add depth on top of a
green base. Verified by unit tests on the domain, integration tests against a real PostgreSQL
container, component tests on the front end, and a manual pass through the running stack.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| A1a. Conventions and plan | ✅ | CLAUDE.md, plans, versions checked against registries |
| A1b. Solution scaffold | ⏳ | Projects, health endpoint, compose service for the database |
| A2. Domain model and state machine | ⏳ | TDD |
| A3. Persistence and test harness | ⏳ | EF, migration, seeded services, Testcontainers |
| B1. Orders API slice | ⏳ | CRUD handlers, validators, ProblemDetails, OpenAPI |
| B2. Front-end scaffold | ⏳ | Router, styling, generated client, i18n from day one |
| B3. Orders list route | ⏳ | |
| B4. Create and edit route | ⏳ | |
| B5. Delete flow | ⏳ | |
| C1. Docker and compose | ⏳ | Hard requirement, done before the extras |
| D1. Search API | ⏳ | TDD on the filter composition |
| D2. Search UI | ⏳ | Driven by search params |
| E1. Status transitions and history | ⏳ | |
| E2. Status UI | ⏳ | |
| F1. Optimistic concurrency | ⏳ | ETag and If-Match |
| F2. CI workflow | ⏳ | Unit and integration on push |
| F3. README and docs pass | ⏳ | |

## Context

The shop offers three services: wheel adjustment, chain replacement and brake maintenance. An order
carries the customer's name, phone number and email, the bike brand, the service wanted, an expected
due date and optional notes. Staff need to place, find, edit and delete orders.

The application has to be two parts, a web page and an HTTP API carrying the business logic, with a
database behind them and Docker packaging that runs the whole thing from one compose file. React
components are required on the front end. Those constraints are fixed; everything else is a design
choice made here.

Conventions for both halves are in [CLAUDE.md](../CLAUDE.md) and are not restated below.

## Design decisions

- **Feature-sliced CQRS over a service layer.** Each operation is a command or query with its own
  handler, so a slice can be read top to bottom without opening a class that serves five callers.
  The application is small enough that a mediator library would be ceremony, so handlers are resolved
  straight from DI and injected per controller action.

- **Service type is an enum in the domain plus a seeded reference table.** The enum is the single
  source of truth and carries its behaviour (display name, estimated duration) through extension
  methods. The table exists so the database can enforce the foreign key and so service metadata has a
  home when pricing or duration becomes editable. The alternative, a table alone, would put a
  business rule where no test can reach it.

- **Order status is a state machine on the entity.** `Received`, `InProgress`, `Completed`,
  `Cancelled`, with transitions exposed as domain methods that throw a typed domain exception on an
  illegal move. There is no public setter, so an invalid sequence cannot be expressed in code. This
  is the main piece of real domain logic in an otherwise CRUD-shaped problem, and it is where TDD
  earns its place.

- **Email and phone number are value objects, bike brand and notes are not.** The first two carry a
  format invariant worth parsing once at the boundary and trusting afterwards. The other two are free
  text with a length cap, and wrapping them would be pattern application for its own sake.

- **Single-page front end, not server rendering.** React Router 8 in framework mode with `ssr: false`
  keeps route modules, loaders, actions and the typed action envelope, while the deliverable stays a
  bundle of static files that nginx serves. The API is then genuinely consumed over HTTP from the
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

- **Docker packaging lands at the end of phase C, before any extra.** It is a hard requirement, so it
  is finished while there is room, not squeezed in at the end.

- **Delete is a real delete.** The brief asks for delete, and `Cancelled` already covers the case
  where the order should be kept for the record. Two mechanisms for going away would need a reason.

## Steps

### Phase A: foundation

**A1a. Conventions and plan.** `CLAUDE.md`, `plans/README.md`, `plans/ROADMAP.md` and this file.
Versions checked against the package registries rather than carried over. No code.

**A1b. Solution scaffold.** `.gitignore`, `.editorconfig` and a
`Directory.Build.props` pinning language version and nullable settings. The backend solution with
`Domain`, `Database`, `Api` and the three test projects wired up, plus a health endpoint and a compose
service for PostgreSQL so there is a database to develop against. Verified by a build and a passing
placeholder test run.

**A2. Domain model and state machine.** `ServiceType` and `OrderStatus` enums with behaviour in
extensions, `EmailAddress` and `PhoneNumber` value objects, the `MaintenanceOrder` entity with a
static `Create()`, private setters, and transition methods. Domain exception hierarchy. Failing tests
first for every transition, legal and illegal, and for value-object parsing. No persistence yet.

**A3. Persistence and test harness.** `BikeRepairShopDbContext`, one configuration per entity, the
initial migration, the `service_types` table seeded from the enum, development-only migration
apply-on-start. The Testcontainers harness with the template-database clone and per-test reset, plus
one integration test proving a saved order reloads with its status and value objects intact.

### Phase B: orders slice

**B1. Orders API slice.** Create, get by id, list, update and delete as commands, queries and handlers
under `Features/Orders`. FluentValidation validators on the request models. A thin `OrdersController`.
Global exception middleware mapping domain and API exceptions to ProblemDetails. OpenAPI with Scalar
for browsing. Integration tests per handler asserting persisted state from a fresh read.

**B2. Front-end scaffold.** React Router 8 project with `ssr: false`, Tailwind v4 and DaisyUI v5,
Biome, Vitest. The transport core with ProblemDetails normalisation, the `ActionResult` envelope, the
generated OpenAPI client and the orders endpoint wrapper. App shell with header, locale switch and
both locale files. Verified by a render test of the shell and the tooling gates.

**B3. Orders list route.** Loader reading the list endpoint, a table of orders with service type,
status, customer and due date, empty and error states. Component tests mocking the endpoint layer.

**B4. Create and edit route.** One form component serving both, a page route each, field-level error
rendering from the ProblemDetails validation payload. Tests covering a successful submit and a
rejected one.

**B5. Delete flow.** Native `<dialog>` confirmation, fetcher submit against an intent, optimistic
removal from the list, failure surfaced without losing the row.

### Phase C: packaging

**C1. Docker and compose.** Multi-stage Dockerfile for the API (SDK to ASP.NET runtime, non-root) and
for the front end (node build to nginx). Root `docker-compose.yml` with database, API and web, health
checks and dependency ordering, CORS configured for the compose origin, environment configuration for
the API base URL at build time. Verified by a clean `docker compose up --build` and a manual pass
through the four tasks.

### Phase D: search

**D1. Search API.** Extend the list query with filters on customer name, phone, email, service type,
status and a due-date range, plus sorting and paging, composed with specifications. Failing tests
first for filter composition and for the empty-filter case.

**D2. Search UI.** A filter bar writing to search params, with the loader reading them and a
revalidation guard so typing is not one request per keystroke.

### Phase E: lifecycle

**E1. Status transitions and history.** Transition endpoints calling the domain methods, and a status
history row written per transition with the timestamp and the previous status. Illegal transitions
return a mapped conflict. Tests cover history rows written in order.

**E2. Status UI.** Status actions on the order view, offering only the transitions the current status
allows, driven by a capability flag the API projects onto the response rather than a client-side rule.

### Phase F: hardening

**F1. Optimistic concurrency.** PostgreSQL `xmin` as the concurrency token, exposed as an ETag on the
order response, required as `If-Match` on update, and a concurrency failure mapped to a conflict the
edit form explains. Integration test driving two writers against one row.

**F2. CI workflow.** GitHub Actions running the build, unit tests and integration tests on push, with
the front-end gates alongside.

**F3. README and docs pass.** Root README with the one-command run, the architecture in a paragraph
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

Each carries the assumption the work proceeds on, so nothing is blocked.

1. **Does an order hold one service or several?** (Assumption: one, since the brief lists the service
   type as a single field. A line-item collection would change the entity, the table and the form.)
2. **Is the expected due date a date or a timestamp?** (Assumption: date only, mapped to `DateOnly`,
   since a workshop promises a day.)
3. **Should the list be paged from the start?** (Assumption: yes, a page and size parameter from A1,
   because retrofitting paging into a loader and a table costs more than including it.)
4. **Is there a rule about due dates in the past?** (Assumption: rejected on create, allowed to remain
   on an existing order, so an overdue order can still be edited.)
