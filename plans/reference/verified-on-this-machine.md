> **Reference doc: load only when working in this area**
> - **Covers:** toolchain and library behaviour verified by running it on this machine
> - **Used by:** every phase plan; read before touching the toolchain, packages or the harness
> - **Updated:** 2026-09-22

# Verified on this machine, not assumed

Both stacks were built and run on throwaway scaffolds before any of this repository's code was
written, because the failures below are the kind that look like mysteries when they surface mid-unit.

- `dotnet test` fails outright on the .NET 10 SDK with xunit v3 unless `global.json` opts into
  `Microsoft.Testing.Platform`. The VSTest target is gone. The opt-in is repo-wide, not per project.
- `dotnet test` also needs `--no-build`, with the build run as its own step first. Letting it build
  discovers nothing: it reports `Zero tests ran` and exit code 5 for every project, including ones
  that have not been touched. The run that works prints the target as `net10.0|x64` and the one that
  finds nothing prints `net10.0`, so the built-in build is resolving a different target than the one
  the assemblies land in. Running the test executable under `bin/Debug/net10.0/` directly also works
  and is the quickest way to see a failure message in full.
- Npgsql 10.0.3 floors EF Core at 10.0.4. Pinning 10.0.0 fails restore with NU1605.
- PostgreSQL 18 with Npgsql 10, EF 10 and Testcontainers 4.15 was exercised end to end with a real
  container and a real save-and-reload. No compatibility issue, no readiness-probe issue.
- `Testcontainers.PostgreSql` 4.15 has obsoleted the parameterless `PostgreSqlBuilder()`. Pass the
  image: `new PostgreSqlBuilder("postgres:18")`.
- A `Database` project with no composition root needs an `IDesignTimeDbContextFactory`, or the `Api`
  project passed as `--startup-project`, for `dotnet ef migrations add` to work.
- React Router 8 with `ssr: false` produces a genuine SPA build, and Tailwind v4 with DaisyUI v5 in the
  CSS-first form survives it.
- `erasableSyntaxOnly` is not enabled by default in a React Router 8 project, so TypeScript `enum` is
  available and the no-magic-strings rule stands as written.
- DaisyUI 5 dropped `label-text` and `form-control`, and neither is in the package. Nothing warns:
  Tailwind emits no rule for a class it does not know, so the markup renders unstyled and looks
  merely plain rather than broken. Both had been sitting in `FormField` since it was written, which
  is why its labels ran small and its rows had no spacing. Grep `node_modules/daisyui` for a class
  before trusting a v4 example. The replacements that do exist are `fieldset-legend` for the field
  heading and `fieldset-label` for helper text, both of which expect DaisyUI's `fieldset` markup, so
  plain Tailwind is the lighter answer for a standalone label.
- `btn-soft` and `badge-soft` are real DaisyUI 5 modifiers and combine with a semantic colour, which
  is how an action carries a background before it is hovered.
- `mcr.microsoft.com/dotnet/sdk:10.0` resolved to SDK 10.0.201 here, older than the 10.0.400 that
  `global.json` pins, and `rollForward` only ever moves up. `dotnet restore` inside the image dies
  with exit 155 and an SDK-not-found message that lists what it does have. The `10.0.400` tag exists,
  so the fix is to pin the build stage to it rather than to loosen the repo's pin for the container's
  benefit. It has to be bumped with `global.json`.
- `openapi-typescript` 7.13 peer-depends on TypeScript `^5.x` because it uses the JavaScript
  compiler API, and `typescript@7.0.2` ships only the native `tsc` with no `typescript.js`. A plain
  `npm install` of the generator therefore either conflicts or leaves it without its API. Through
  `npx -y` it gets its own TypeScript 5 in the npx cache and the project stays on 7.
- `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` cannot be rewound: `SetUtcNow`
  throws `ArgumentOutOfRangeException("Cannot go back in time")`. A fixture shared by a whole xunit
  collection therefore cannot reset it between tests, which is why the harness carries its own
  `TestTimeProvider` instead and the package is not referenced.
- i18next 26 renamed `initImmediate` to `initAsync`; with bundled resources, `initAsync: false` keeps
  the first render out of Suspense.
- React Router 8's build writes `isbot` into `package.json` with a caret when no custom server entry
  exists. Pin it exactly afterwards.
- TypeScript 7.0.2, the native compiler, type-checks the React Router 8.4 scaffold including the
  `typegen` output and `rootDirs`, with no config change from the template. Proved in B2.
- Biome 2.5 run from `frontend/` needs `vcs.root: ".."` to find the repo's `.gitignore`, uses
  `linter.rules.preset` instead of the deprecated `recommended`, and rejects `@plugin` in CSS unless
  `css.parser.tailwindDirectives` is on.
- A React Router 8 build with `ssr: false` still writes `build/server/`; it is the prerender step for
  `build/client/index.html`, and only `build/client/` is served.
- Package pins were checked against nuget.org a second time on 2026-09-20. EF Core had been pinned at
  the Npgsql floor rather than at current stable, and `FluentValidation.AspNetCore` is retired on the
  11.x line: validation is wired through `FluentValidation.DependencyInjectionExtensions` 12 instead.
- The SDK's built-in `dotnet new xunit` template still produces an xunit v2 project. Test projects are
  written by hand against `xunit.v3`.
- Under the `Microsoft.Testing.Platform` runner, `xunit.v3` 4.0 needs neither `Microsoft.NET.Test.Sdk`
  nor `xunit.runner.visualstudio`. A test project is an `Exe` with `IsTestProject` set and the one
  package; both VSTest packages were dropped from the central pins after a green run without them.
- The SDK sets `IsPackable` to false for `IsTestProject` on its own, so a `Directory.Build.props`
  block doing the same is dead: the condition runs before the project file sets the property anyway.
- The `postgres:18` image moved `PGDATA` to `/var/lib/postgresql/18/docker`. The compose volume mounts
  `/var/lib/postgresql`, one level up, as the image's own notes recommend for 18.
- Vitest runs without `globals`, so Testing Library never registers its own auto-cleanup and the
  jsdom document keeps every previous render. `afterEach(cleanup)` belongs in `test/setup.ts`, or
  queries start finding duplicates as soon as a second test renders the same component.
- `openapi-fetch` 0.17 has no TypeScript peer dependency, so unlike `openapi-typescript` it installs
  without pulling the 5.x line in beside `typescript@7`.
- `openapi-fetch` builds an absolute `Request` before calling fetch, so a relative base URL throws
  `Failed to parse URL` under Node and jsdom. The client is given `window.location.origin`, which is
  the same-origin path the dev proxy and Caddy both serve.
- SPA mode permits `HydrateFallback` on the root route only, and an invalid route export fails in the
  Vite plugin at dev or build time. Neither `tsc` nor Vitest sees it, because both import the module
  directly, so `npm run build` is the gate that catches it.
- Adding an identity column to a table that already has rows works in one statement. EF emits
  `ALTER TABLE orders ADD order_number bigint GENERATED BY DEFAULT AS IDENTITY (START WITH 1000)`,
  not an `ADD COLUMN ... DEFAULT 0` followed by an alter, so PostgreSQL fills the existing rows with
  distinct ascending values and a unique index over the column creates cleanly in the same
  migration. Confirmed against the dev database holding 26 demo orders: they came out 1000 to 1025.
  The `defaultValue: 0L` in the generated migration file is a model artifact and never reaches SQL.
- The integration harness fails a test class in `InitializeAsync` with `PendingModelChangesWarning`
  when the EF model and the migration snapshot disagree. It fires before any test body runs, so a
  mapping change made without regenerating the migration reads as a fixture failure rather than an
  assertion failure.
- `openapi-typescript` 7.13 emits `number | string` for an OpenAPI `integer` with `format: int64`,
  which is what a C# `long` produces. It is defensive about large values rather than wrong, and the
  only way to get a plain `number` is to narrow the property to `int` on the API.
