# Phase E: search (#4)

## At a glance

Finding an order: filters on the list endpoint composed with specifications, and a filter bar driven by search params.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| E1. Search API | ⏳ | TDD on the filter composition |
| E2. Search UI | ⏳ | Driven by search params |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. Extends the list query from phase B. Filters compose with specifications; the empty filter must return the plain list.

## Design decisions

None beyond the main plan yet. A decision that only this phase needs goes here.

## Steps

**E1. Search API.** Extend the list query with filters on customer name, phone, email, service type,
status and a due-date range, plus sorting, composed with specifications. Failing tests first for
filter composition and for the empty-filter case.

**E2. Search UI.** A filter bar writing to search params, with the loader reading them and a
revalidation guard so typing is not one request per keystroke.
