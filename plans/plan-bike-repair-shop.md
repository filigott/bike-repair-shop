# Maintenance orders end to end (#1 to #7)

## At a glance

Build the bicycle workshop's maintenance-order application: a .NET domain and HTTP API, a React
single-page front end that consumes it, PostgreSQL behind both, and a root `docker-compose.yml` that
runs all three. The work goes in vertical slices, back end before front end inside each slice, with
the order lifecycle modelled as a state machine on the entity rather than a status column anyone can
write to. After the foundation, work goes in thin verticals that each end in a manual smoke test:
place and find, then edit and delete, then packaging. Those four phases are the product; search,
lifecycle and hardening add depth on top of a green base. Verified by unit tests on the domain, integration tests against a real PostgreSQL
container, component tests on the front end, and a manual pass through the running stack.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Phase | Plan | Issue | Status |
|---|---|---|---|
| A. Foundation | [plan-a-foundation.md](plan-a-foundation.md) | #7 | ✅ |
| B. Place and find | [plan-b-place-and-list.md](plan-b-place-and-list.md) | #1 | ✅ |
| C. Find, edit and delete | [plan-c-find-edit-delete.md](plan-c-find-edit-delete.md) | #2 | 🟡 |
| D. Packaging | [plan-d-packaging.md](plan-d-packaging.md) | #3 | ✅ |
| E. Search | [plan-e-search.md](plan-e-search.md) | #4 | ⏳ |
| F. Lifecycle | [plan-f-lifecycle.md](plan-f-lifecycle.md) | #5 | ✅ |
| G. Hardening | [plan-g-hardening.md](plan-g-hardening.md) | #6 | ⏳ |
| H. Demo data | [plan-h-demo-data.md](plan-h-demo-data.md) | #8 | ✅ |
| I. Order reference | [plan-i-order-reference.md](plan-i-order-reference.md) | #9 | ✅ |
| J. Detail page rework | [plan-j-detail-page.md](plan-j-detail-page.md) | #10 | ✅ |
| K. Localized API messages | not yet written | #11 | ⏳ |

Each phase plan carries its own step table. From B on, the last step of a phase ends in a manual
smoke test through the running API and front end before the phase is called done. Phases I and F
are code-complete and gated, with their manual passes batched into one run rather than taken per
phase; the roadmap's `Blockers` names what that pass owes. Phase F covers the API half of #5 only,
so the issue stays open for the status history.

The letters record when a phase was identified, not the order it runs in. H through K came out of
the phase C smoke test, and the running order so far has been A, B, C, H, D, I, F, with J next.
The roadmap's `Next up` is the one place that order lives.

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

Toolchain and library behaviour established by running it lives in
[reference/verified-on-this-machine.md](reference/verified-on-this-machine.md). Read it before
touching packages, the test harness or the front-end toolchain.

### What the build has cost so far

Hours spent, commits landed and the distance still left to the brief are recorded in
[reference/time-and-scope.md](reference/time-and-scope.md). It also holds the scope correction the
phase table is now read against: nothing from E, F or G until the product phases run.

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
  (place, reschedule, start, complete, cancel, reopen, correct the contact details) and a matching
  action in the UI, shown only when the order's state allows it. A single update endpoint plus a form with a
  status dropdown would leave the lifecycle rules to be remembered by whoever is at the counter. The
  edit form therefore covers the details an order can always carry, and lifecycle moves are buttons
  driven by the transitions the API projects onto the response.

- **An enum with behaviour in an extension block when the cases carry no data, a record union when
  they do.** `OrderStatus` stays a real enum, so EF stores a string, JSON emits a string, OpenAPI
  emits an enum and the generated TypeScript gets a union, all for free. Its behaviour sits in a C# 14
  `extension` block in the same file, so it reads as members without paying a converter at every
  boundary. A smart-enum class or a record hierarchy would cost exactly those converters and buy
  nothing for four data-less statuses; the record union is reserved for shapes that carry data.

- **Email and phone number are value objects, bike brand and notes are not.** The first two carry a
  format invariant worth parsing once at the boundary and trusting afterwards. The other two are free
  text with a length cap, and wrapping them would be pattern application for its own sake. For an
  application this size, plain strings behind a validator would be defensible; the value objects are
  kept because the invariant then travels with the type instead of with a convention, the rule has
  one home, and the reversal cost is asymmetric: collapsing to a string later is mechanical, adding
  the type after un-normalised data exists needs a migration. The bar is per field, not per project:
  wrap when there is a parse rule, not when there is only a length cap.

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

- **The domain owns its clock through `IShopClock`.** The interface declares `Now` and `Today`, where
  `Today` is the shop's calendar day rather than the server's. `Place` stamps `PlacedAt` from it and
  judges the due date against it, `Reschedule` does the same, and the API implements it once over
  `TimeProvider` and the configured timezone. Dependency inversion in its plain form: the domain says
  what it needs, the composition root supplies it, and tests pass a fixed clock. An EF interceptor
  stamping `CreatedAt` was the first design; it kept the domain clock-free but hid the mechanism and
  cost the domain word. `UpdatedAt` is dropped until something reads it; the status history in phase F
  carries its own timestamps.

- **The service catalogue is keyed by its text code, and orders carry a foreign key to it.**
  `service_types(code, display_name, sort_order)` is seeded in the initial migration, `orders.service_type`
  stores the enum name and references `code`. The database refuses an unknown service, the enum stays
  the typed key, and `psql` reads the column without a join. An integer surrogate would make every
  session and log line a lookup.

- **One test database, one collection, rows reset per test.** One `postgres:18` container per run,
  migrated once, every integration class in a single xunit collection so they run in sequence, and
  Respawn wiping rows before each test. A template database cloned per collection was the first
  design; it pays off past fifty-plus tests and is over-built for a few dozen, and can be reinstated
  without touching a test.

- **`/health` includes the database.** The EF Core health check makes the endpoint go unhealthy when
  PostgreSQL is unreachable, so compose can gate the API on it and the integration harness exercises
  it against the real container.

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

- **The state machine is one table of edges, keyed by the action.** `(from, action) to`, with
  `AllowedActions`, `AllowedTransitions` and `CanTransitionTo` all derived from it and every
  transition method funnelling through one `Apply`. Two copies of the same matrix is the bug this
  whole design exists to prevent, and keying by target status was a third copy waiting to happen:
  `Start` and `Reopen` share `InProgress` from different origins, so a `from -> [to]` map cannot
  tell them apart and something has to bridge statuses back to command names. Whichever side that
  bridge sits, it is hand-written and can disagree with the matrix. Keying by the action removes it.

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

- **React Router loaders and actions are the only data layer.** No client-side query cache such as
  TanStack Query. Framework mode already owns loading, mutation and revalidation, so a cache on top
  would be a second source of freshness that every route has to pick between. Data arrives through a
  loader, changes go through an action, and the loader reruns.

- **The transport stays small and typed from the schema.** `openapi-fetch` over the committed
  schema, a per-call timeout, one `unwrap` that turns its `{ data, error }` result into a value or a
  throwing `ApiError`, one action envelope, and one wrapper file per API controller as the mock seam
  for tests. The hand-written fetch core it replaced already took its parameter types from the
  schema, so the gain is the path literal and the query shape: with parameterised routes arriving in
  phase C, a drifted route is a compile error instead of a runtime 404. The wrappers stay, because
  they are what tests mock and where the response is unwrapped.

- **Delete is a real delete.** The brief asks for delete, and `Cancelled` already covers the case
  where the order should be kept for the record. Two mechanisms for going away would need a reason.

## Steps

One plan per phase, linked from the Progress table above. Phases A through D are the product;
E through G add depth on a green base, taken in order.

## Out of scope

- Authentication and user accounts. There is no actor in the brief and no authority rule to enforce,
  so adding a login would be scenery.
- Customers as an entity. An order carries contact details as stated; deduplicating customers is a
  different product.
- A bike brand catalogue. The brief lists the brand as a field the customer states, staff type it, and
  a lookup table would need an admin flow and an "other" escape hatch to be honest. It stays a capped
  string; if misspellings become a problem, an autocomplete over the distinct values already stored
  needs no schema change, and a real catalogue later is an additive table plus a backfill.
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
12. **Shop timezone.** RESOLVED: `Europe/Oslo`, already the default in `appsettings.json` and set
    explicitly in compose so the value is visible where the stack is assembled. The shape was settled
    earlier. `IShopClock` is implemented
    in the API over `TimeProvider` and a configured `TimeZoneInfo`, and the runtime image has to carry
    tz data. Development runs with a default of `Europe/Oslo`.
13. **Compose credentials.** RESOLVED, against what this question proposed: inline defaults of the
    form `${POSTGRES_PASSWORD:-bikeshop}`, overridable from the environment, with no file to copy
    first. An ignored `.env` would make the brief's one-command start fail on a fresh clone. The compose project name is pinned and the host port is 5434,
    because 5432, 5433 and 5480 are all taken by other compose stacks on this machine.
