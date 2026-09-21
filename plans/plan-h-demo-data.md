# Phase H: demo data (#8)

## At a glance

A demo data set behind an explicit setting, so the two smoke-test steps that could not run finally
can, search and paging have something to work against, and `docker compose up` opens on a populated
table instead of an empty one.

## Progress

Legend: ⏳ pending · 🟡 in progress · ✅ done · ⏸️ blocked

| Step | Status | Notes |
|---|---|---|
| H1. Seed fixture and the API slice | 🟡 | Setting, fixture, two endpoints, seed on startup when empty |
| H2. Reset control in the UI | ⏳ | Button plus confirm dialog, shown only when the API says enabled |

## Context

Part of [plan-bike-repair-shop.md](plan-bike-repair-shop.md). Comes out of the phase C smoke test,
whose steps 5 and 8 were unreachable: nothing moves an order off `Received` over HTTP until #5, and
neither `Place` nor `Reschedule` will accept a past due date.

## Design decisions

**Nothing bypasses the domain.** The first reading of this problem was that demo data needs states
the domain refuses to build, and that a fixture would have to write them straight through the
DbContext. That is wrong on both counts. `Start`, `Complete` and `Cancel` are public domain methods,
so a closed order is one call away; only the HTTP surface is missing. And `Place` takes an
`IShopClock`, so an overdue order is placed with a clock positioned two weeks back and a due date
that was in the future at that moment. That is how a real overdue order comes about, it gives
`PlacedAt` a believable value, and it means no reflection, no change-tracker poking and no second
way to construct an order.

**The automatic path never destroys, the destructive path is never automatic.** On startup the seed
runs only when the orders table is empty, so a restart cannot take away work in progress. Wiping is
reachable only through an explicit reset, confirmed in a dialog that names how many orders it will
remove.

**Gated by a setting, not by the environment name.** `DemoData:Enabled` mirrors
`RunMigrationsOnStartup`: the composed stack turns it on without also switching on a developer
exception page, and the setting is off unless set.

**The front end asks whether the feature exists.** A read endpoint reports whether demo data is
enabled, and the button appears on that answer rather than on a build-time flag. One bundle serves
every environment, and the rule that the UI never recomputes a server decision holds.

## Steps

**H1. Seed fixture and the API slice.** A `DemoData` slice with the options, the fixture set, a
handler and a controller. Roughly 26 orders so paging at 20 has a second page: a spread of open
orders across the three services with future due dates, two overdue, two completed and two
cancelled. `GET /api/demo-data` reports whether the feature is enabled and how many orders exist.
`POST /api/demo-data/reset` wipes the orders and reseeds, and answers 404 when the feature is off.
Startup seeds only into an empty table. Integration tests cover the seed shape, the empty-table
guard, the reset, and that the set really does contain an overdue order and a closed one, since
those two are the reason the slice exists.

**H2. Reset control in the UI.** A control that appears only when the read endpoint says the feature
is enabled, with a native `<dialog>` confirmation naming the number of orders to be removed, posting
through a route action and an intent. Both locales in the same commit.

## Out of scope

- Lifecycle endpoints (#5). The seeder calls the domain methods directly; the HTTP surface for
  status changes is a separate slice.
- Any notion of demo data in the production sense: there is no import, no fixtures file and no
  configuration of the set itself.

## Open questions

None outstanding. Wipe semantics, the trigger and the compose behaviour were settled on 2026-09-21;
the reasoning is in the roadmap under `Parked from the phase C smoke test`.
