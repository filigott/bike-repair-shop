# Phase K: localized API messages (#11)

## At a glance

Every 4xx the UI renders carries a machine code, and the front end turns the code into copy from its
locale files. The API stays language-neutral and keeps its English `detail` for other clients. The
user-visible outcome: a Norwegian user never sees an English sentence from the API again, whether it
comes from a validator or a domain rule.

## Progress

Complete. Merged on 2026-09-23 through #21.

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| K1. Codes from the API | ✅ | `errorCodes` beside `errors`, `code` on every problem |
| K2. Codes into copy | ✅ | Parse codes at the boundary, render keys only |

## Context

Last known defect, taken on the last day before the interview after CI and concurrency (#6) landed.
Branched off `feat/6-optimistic-concurrency` because both touch the exception handler and the edit
dialog; the PR targets `main` once #20 merges. Settled with the user on 2026-09-23.

Render sites today: `order-form.tsx`, `command-dialog.tsx`, `orders.tsx` and `order-detail.tsx` show
`failure.detail ?? failure.title`; `FormField` shows the field messages verbatim.

## Design decisions

**Codes, not Accept-Language.** The locale files stay the single home for copy. Server-side
localization would also mean flipping `InvariantGlobalization`, since the `nb` culture cannot be
created under it.

**`errorCodes` rides beside `errors`.** `errors` keeps its English messages, so the standard
ValidationProblemDetails shape stays readable in Scalar and curl. Keys match `errors` one to one.

**Every problem the UI can render carries `code`.** Domain exceptions carry a `DomainErrorCode`
(`DueDateInPast`, `OrderNotEditable`, `InvalidTransition`); the API's own exceptions carry theirs
(`OrderNotFound`, `OrderChanged`), and a validation failure is `InvalidRequest`. Two C# enums, one
wire namespace, because "not found" is never a domain concept.

**Validation codes are ours, not FluentValidation's.** `Required`, `TooLong`, `InvalidPhoneNumber`,
`InvalidEmail`, `UnknownValue`, set explicitly on the rules. A failure without one of ours becomes
`Invalid`, so library names never reach the contract.

**The front end parses codes into enums at the boundary**, with `Unknown` and `Invalid` as the keyed
fallbacks. No English from the API is rendered anywhere; the conflict message moves under the same
scheme.

## Steps

**K1. Codes from the API.** Enums, `WithErrorCode` on the rules, the filter adding `errorCodes` and
`code`, the exception handler adding `code`. Integration tests assert the codes for a bad field, a
past due date and a stale version.

**K2. Codes into copy.** `ActionFailure` carries a parsed `code` and per-field code lists; the four
render sites and `FormField` render keys; `problems.*` and `validation.*` in both locales. Tests
assert Norwegian copy for a field error and a domain rule.

## Out of scope

- Localizing log output, and date and number formatting, as #11 says.
- The list's search validator: its fields are URL state, never a rendered form.
