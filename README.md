# Bike Repair Shop

[![CI](https://github.com/filigott/bike-repair-shop/actions/workflows/ci.yml/badge.svg)](https://github.com/filigott/bike-repair-shop/actions/workflows/ci.yml)

A maintenance-order manager for a bicycle workshop. Staff place, find, edit and delete orders for
the three services the shop offers, and move each order through its lifecycle. It is a .NET HTTP API
holding the business logic, a React single-page front end that consumes it, and PostgreSQL behind
both, packaged so one command runs the whole stack.

## Prerequisites

Docker with Compose is all the packaged stack needs. The dev loop and the tests also need the .NET 10
SDK (`global.json` pins 10.0.400 and rolls forward) and Node 24. The backend suite starts its own
PostgreSQL container, so Docker has to be running for `dotnet test` too.

## Run it

Everything containerised, served on <http://localhost:8080>:

```bash
docker compose up --build
```

That is the whole thing: the database creates its schema on first start and seeds a demo set of
orders, so the list has something in it immediately. `docker compose down` removes the containers
and keeps the data; add `-v` to start from nothing.

## Develop it

The dev loop runs the two halves outside Docker against the composed database, and both hot-reload.

```bash
docker compose up -d db                  # the database alone, on 5434
dotnet run --project backend/BikeRepairShop.Api   # the API, on 5260
npm --prefix frontend run dev            # the front end, on 5173
```

Vite proxies `/api` to the API, so the browser sees one origin. The API reference is at
<http://localhost:5260/scalar>, and `npm --prefix frontend run generate:api` regenerates the
TypeScript client types from the running API's OpenAPI document.

Both modes share the same database, so run one API or the other, not both.

## Test it

```bash
dotnet build backend/BikeRepairShop.sln
dotnet test  backend/BikeRepairShop.sln --no-build   # build first: the test host discovers nothing otherwise
npm --prefix frontend run verify                     # lint, typecheck, test and build
```

The backend suite is xUnit: unit tests on the domain, and integration tests against a real
PostgreSQL container through Testcontainers, migrated through the API's own startup path, which is
why the suite needs Docker. The front
end is Vitest with Testing Library, mocking the endpoint layer rather than `fetch`.

CI runs both suites, `dotnet format` and the front-end `verify` on every pull request and on `main`.

## How it is built

```
backend/
  BikeRepairShop.Domain/        entities, enums, value objects, the order lifecycle
  BikeRepairShop.Database/      DbContext, EF configurations, migrations
  BikeRepairShop.Api/           controllers plus feature-sliced CQRS handlers
  BikeRepairShop.Tests.Unit/
  BikeRepairShop.Tests.Integration/
frontend/
  app/routes/                   one route per page
  app/services/endpoints/       one wrapper file per API controller
  app/locales/                  English and Norwegian, kept in step
plans/                          how the solution was designed, phase by phase
docs/                           topical reference
```

The order lifecycle is the centre of the design. An order moves between statuses only through named
domain methods, over one table of `(from, action) to` edges that is the single source for what an
order can do next. The API exposes those as intention-revealing commands rather than a status field
anyone can write to, and projects onto every response which of them the order currently allows. The
front end renders one button per allowed action, so it never works out for itself which command
moves an order from one status to another.

`CLAUDE.md` holds the conventions both halves follow. `plans/` holds the design record: what was
built, in what order, and which alternatives were weighed and dropped.

## Not built yet

- Search by order number, service or due date, and sorting the list. Search today matches the
  customer name, phone number and email.
- The history of who moved an order between statuses, and when.
- Staff sign-in. Every visitor can do everything.

## Built with an AI assistant

The repository was written by a developer working with Claude Code: conventions in `CLAUDE.md`, one
plan per phase under `plans/`, one reviewed unit at a time, and a manual pass in the running
application that no test replaces. [docs/ai-development.md](docs/ai-development.md) describes the
method, what it was good at, where it needed correcting, and what it cost.
