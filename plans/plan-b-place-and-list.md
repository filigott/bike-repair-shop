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
| B2. Front-end scaffold | ✅ | One commit per row below |
| B2a. Toolchain | ✅ | React Router 8 SPA, TypeScript 7, Tailwind and DaisyUI, Biome, Vitest |
| B2b. Shell and i18n | ✅ | Header, locale switch, both locale files, render test |
| B2c. Transport and client | ✅ | openapi-fetch client, ActionResult, generated types, endpoint wrappers |
| B3. Orders list page | ✅ | Loader, search form, table, prev/next paging; components split per file |
| B4. Create form | ✅ | Place-order page, service types from the API, field errors; manual smoke test closes the phase |

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

- **Service type and status labels are locale keys, not the catalogue's `display_name`.** The
  `service_types` table stays the one home for which services exist and in what order, and the create
  form reads it from the API. What the staff member sees per code is a translation key, because the
  seeded display name is English only while the UI ships in two languages. Moving the label into the
  database later is an additive column plus a fallback, and nothing in the list page changes.

- **The list page reads its state from the URL.** The search term and the page number are search
  params the loader parses, so a search is shareable, the back button works and no component holds
  copied state. The search box is a plain GET form rather than a debounced auto-submit, which keeps
  the timer and the history churn out of the component.

- **Validation messages leave the API in English, and the form shows them as they arrive.** Mapping
  them to locale keys needs the API to emit a stable error code per rule, since FluentValidation's
  own codes name the validator rather than the rule, plus a key per code in both locales and a
  fallback for anything unmapped. Worth doing when the API stops being the only writer of these
  strings; not worth it for one form. Field labels, buttons and every other string are keys as usual.

- **A missing due date is a binder error, not a field error.** `PlaceOrderRequest.DueDate` is a
  non-nullable `DateOnly`, so an empty string fails model binding before the validator runs and MVC
  answers with a key of `$.dueDate` that no field lookup matches. The date input is therefore
  `required`, so the browser never sends an unbindable payload. Reporting it per field means a
  nullable `DateOnly?` with `NotNull()` on the validator, which is worth doing when the edit form in
  phase C gives that request shape a second writer.

- **The email input is plain text on purpose.** `type="email"` would let the browser reject `ada@`
  before the request leaves, hiding the API rule and its message. The server's `EmailAddress.TryParse`
  is the authority for the format, so the field posts and renders what the API says.

- **Field errors are keyed by the generated request type.** `ActionFailure<TField>` carries
  `Partial<Record<TField, string[]>>`, and the place-order form asks for
  `ActionFailure<keyof PlaceOrderRequest>`, so a field renamed in the API breaks the form at compile
  time instead of silently rendering nothing. The envelope keeps a plain string default for actions
  that have no single request type.

- **The create action redirects on success.** Staff land back on the list with the new order in it,
  and the loader reruns on its own, so the action returns a failure or nothing. `useActionData` is
  therefore only ever a failure on this route.

## Steps

**B1. Place and list API.** Place as a command and list as a query with handlers under
`Features/Orders`, a FluentValidation validator on the place request, a thin `OrdersController`,
global exception middleware mapping domain and API exceptions to ProblemDetails, and OpenAPI with
Scalar for browsing. The list takes an optional search term matched case-insensitively against
customer name, phone and email, because finding an order for a customer is one of the four tasks in
the brief. It is paged from the start with a deterministic tiebreaker. Integration tests per handler
asserting persisted state from a fresh read, and a failing test first for the search match.

**B2. Front-end scaffold.** React Router 8 project with `ssr: false`, Tailwind v4 and DaisyUI v5,
Biome, Vitest. The transport core over `openapi-fetch` with ProblemDetails normalisation, the
`ActionResult` envelope, the generated schema types and the orders endpoint wrapper. App shell with header, locale switch and
both locale files. Verified by a render test of the shell and the tooling gates.

**B3. Orders list page.** Client loader reading the list endpoint, a search box writing its term to a
search param the loader reads, prev and next links carrying the term with them, a table of orders
with service type, status, customer and due date, and an empty state. Component tests mocking the
endpoint layer and driving the page through a route stub.

**B4. Create form.** The place-order page at `/orders/place` with its form component, the service
catalogue read from the API for the select, and field-level error rendering from the ProblemDetails
validation payload. A link from the list page, and a redirect back to it on success. Tests covering
the command sent, the redirect and a rejected field. Ends with the first manual smoke test: place an
order, find it by name.
