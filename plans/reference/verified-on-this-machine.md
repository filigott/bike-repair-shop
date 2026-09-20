> **Reference doc: load only when working in this area**
> - **Covers:** toolchain and library behaviour verified by running it on this machine
> - **Used by:** every phase plan; read before touching the toolchain, packages or the harness
> - **Updated:** 2026-09-20

# Verified on this machine, not assumed

Both stacks were built and run on throwaway scaffolds before any of this repository's code was
written, because the failures below are the kind that look like mysteries when they surface mid-unit.

- `dotnet test` fails outright on the .NET 10 SDK with xunit v3 unless `global.json` opts into
  `Microsoft.Testing.Platform`. The VSTest target is gone. The opt-in is repo-wide, not per project.
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
- `openapi-typescript` 7.13 peer-depends on TypeScript `^5.x` because it uses the JavaScript
  compiler API, and `typescript@7.0.2` ships only the native `tsc` with no `typescript.js`. A plain
  `npm install` of the generator therefore either conflicts or leaves it without its API. Through
  `npx -y` it gets its own TypeScript 5 in the npx cache and the project stays on 7.
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
