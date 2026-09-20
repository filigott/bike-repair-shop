# Phase F: lifecycle (#5)

## At a glance

The lifecycle over HTTP: transition endpoints calling the domain methods, a history row per transition, and status actions in the UI offering only what the API allows.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| F1. Status transitions and history | ⏳ |  |
| F2. Status UI | ⏳ |  |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md), which holds the design decisions,
the scope boundary and the open questions; none of that is restated here. The transition matrix lives on `OrderStatus`; this phase only exposes it and records history. The UI derives `isReopened` from the history the API projects.

## Design decisions

None beyond the main plan yet. A decision that only this phase needs goes here.

## Steps

**F1. Status transitions and history.** Transition endpoints calling the domain methods, and a status
history row written per transition with the timestamp and the previous status. Illegal transitions
return a mapped conflict. Tests cover history rows written in order.

**F2. Status UI.** Status actions on the order view, offering only the transitions the current status
allows, driven by a capability flag the API projects onto the response rather than a client-side rule.
