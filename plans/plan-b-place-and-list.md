# Phase B: place and find (#1)

## At a glance

The first vertical: a place command and a list query with a customer search term, the front-end scaffold, the orders list page with its search box, and the create form. Covers two of the four tasks in the brief, place and find. Ends in the first manual smoke test: place an order, find it by name.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| B1. Place and list API | ✅ | One commit per row below |
| B1a. TryParse on value objects | ✅ | Feeds the request validators |
| B1b. API plumbing | ✅ | JSON enums, validation filter, ProblemDetails, OpenAPI with Scalar |
| B1c. Place command | ✅ | Validator, handler, controller, integration tests |
| B1d. List query | ✅ | Search term, offset paging, tiebreaker |
| B1e. Service catalogue read | ✅ | Needed by the create form in B4 |
| B2. Front-end scaffold | ⏳ | Router, styling, generated client, i18n from day one |
| B3. Orders list page | ⏳ | Search box writing to a search param |
| B4. Create form | ⏳ | Smoke test: place an order, find it by name |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. The domain and persistence from phase A are in place. Actions are explicit tasks end to end, so the create endpoint is a place-order command, not a POST of a row. The front end starts here and carries the toolchain risk called out in the reference doc: TypeScript 7 native with React Router 8 and the generated client is unproven on a real project.

## Design decisions

- **Domain exceptions map to two status codes.** `InvalidStateTransitionException` is a 409, every
  other `DomainException` a 422, with the exception message as the ProblemDetails `detail`. Anything
  else stays a 500 through the default handler, so a validator gap surfaces as a bug instead of
  hiding as a client error.

- **Validation runs in one action filter.** FluentValidation has no ASP.NET Core hook on the 12.x line,
  so a filter resolves the validator for each bound argument and short-circuits with the same
  `ValidationProblemDetails` shape MVC uses for binding errors. Keys are camelCased to match the
  JSON, so the form can render them per field.

- **Value objects are complex types, not converted scalars.** `HasConversion` makes the column
  opaque to LINQ: an `ILIKE` on the phone or email column hands the pattern parameter the value
  object's converter and fails at runtime. `ComplexProperty` with an explicit column name keeps the
  same schema, so no migration, and lets `o.Email.Value` translate. Phase E filters build on this.

- **The list sorts by due date, then id.** Version-7 GUIDs are time-ordered, so the tiebreaker is
  placement order and a page boundary never duplicates or drops a row.

- **Validation messages leave the API in English.** Pragmatic for now; whether the UI maps error
  codes to locale keys or shows the server text is an open question for B4.

## Steps

**B1. Place and list API.** Place as a command and list as a query with handlers under
`Features/Orders`, a FluentValidation validator on the place request, a thin `OrdersController`,
global exception middleware mapping domain and API exceptions to ProblemDetails, and OpenAPI with
Scalar for browsing. The list takes an optional search term matched case-insensitively against
customer name, phone and email, because finding an order for a customer is one of the four tasks in
the brief. It is paged from the start with a deterministic tiebreaker. Integration tests per handler
asserting persisted state from a fresh read, and a failing test first for the search match.

**B2. Front-end scaffold.** React Router 8 project with `ssr: false`, Tailwind v4 and DaisyUI v5,
Biome, Vitest. The transport core with ProblemDetails normalisation, the `ActionResult` envelope, the
generated OpenAPI client and the orders endpoint wrapper. App shell with header, locale switch and
both locale files. Verified by a render test of the shell and the tooling gates.

**B3. Orders list page.** Loader reading the list endpoint, a search box writing its term to a
search param the loader reads, a table of orders with service type, status, customer and due date,
empty and error states. Component tests mocking the endpoint layer.

**B4. Create form.** The order form component and its page route, service types read from the API,
field-level error rendering from the ProblemDetails validation payload. Tests covering a successful
submit and a rejected one. Ends with the first manual smoke test: place an order, find it by name.
