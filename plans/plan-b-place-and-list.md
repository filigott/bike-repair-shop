# Phase B: place and list (#1)

## At a glance

The first vertical: a create and a list endpoint, the front-end scaffold, the orders list page and the create form. Ends in the first manual smoke test: place an order and see it listed.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| B1. Create and list API | ⏳ | Handlers, validators, ProblemDetails, OpenAPI |
| B2. Front-end scaffold | ⏳ | Router, styling, generated client, i18n from day one |
| B3. Orders list page | ⏳ |  |
| B4. Create form | ⏳ | Smoke test: place an order, see it listed |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. The domain and persistence from phase A are in place. Actions are explicit tasks end to end, so the create endpoint is a place-order command, not a POST of a row. The front end starts here and carries the toolchain risk called out in the reference doc: TypeScript 7 native with React Router 8 and the generated client is unproven on a real project.

## Design decisions

None beyond the main plan yet. A decision that only this phase needs goes here.

## Steps

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
