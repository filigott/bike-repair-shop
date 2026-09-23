# Phase M: form and list polish (#18)

## At a glance

The place page catches up with the detail page's type scale and layout, rejected fields carry their
error on the input itself for sighted and screen-reader users alike, the contact fields autofill,
and a search that matched nothing has a way back. Last feature work before the interview freeze;
anything not green by then stays on #18.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| M1. Place page type scale and layout | 🟡 | DaisyUI `fieldset` renders its legends at 12px |
| M2. Field error state and autocomplete | ⏳ | `input-error`, `aria-invalid`, described-by, `autocomplete` |
| M3. Clear-search control | ⏳ | A link, beside the box and in the empty state |

## Context

Raised by the phase B and C smoke tests and moved out of plan C to #18. The user noticed the small
place-page text again on 2026-09-23: #10 fixed the type scale on the detail page only. DaisyUI 5's
`.fieldset` sets `font-size: .75rem`, which its legends inherit; `FormField` labels escape it only
because they set `text-base` themselves.

## Design decisions

**The field wires its own accessibility.** `FormField` owns the label and the messages, so it also
owns the message id, and hands the input `aria-invalid`, `aria-describedby` and the error class
through a render prop. No input repeats the wiring.

**Clearing the search is a link to the bare list**, which drops the page along with the term. The
search lives in the URL, so there is no state to reset.

## Steps

**M1.** Centred page container matching the detail page, group headings at a readable size, the
two groups as bordered panels like the detail page's cards. Visual only; checked by hand.

**M2.** `FormField` renders its children through a function receiving the control props; the error
messages get an id. `autocomplete` on name, phone and email. A test asserts `aria-invalid` and the
accessible description on a rejected field.

**M3.** A clear link beside the search box while a term is active, and in the empty state. A test
asserts both lead to the unfiltered list.

## Out of scope

- Clearing a stale message as its input changes. Without re-validating in the browser it needs
  per-field state reset on each new failure, which mirrors a prop into state; it needs a design of
  its own and stays on #18.
