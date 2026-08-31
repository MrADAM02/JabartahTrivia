# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Jabartah Trivia (in-app name: جولة) is an Arabic-language, single-shared-screen party trivia game (phone/tablet/TV), modeled on تدري؟ / ساحة التحدي / جمعة. MVP scope is one game mode: a Jeopardy-style trivia board (category × point-value grid). Two teams play by taking turns picking a cell, the host reveals the question, then marks which team answered correctly.

Two independent projects, no shared package/workspace tooling between them:
- `Jabartah.Trivia-backend/` — ASP.NET Core API (.NET 10)
- `Jabartah.Trivia-frontend/` — Nuxt 4 SPA (Nuxt UI, Arabic RTL)

## Commands

### Backend (`Jabartah.Trivia-backend/`)

```bash
dotnet build                                          # from repo root of the backend solution
cd src/Jabartah.Trivia.Api && dotnet run --urls http://localhost:5081
```

EF Core migrations (run from `src/Jabartah.Trivia.Api`, since the Api project is the one with `Microsoft.EntityFrameworkCore.Design` referenced):

```bash
dotnet ef migrations add <Name> -p ../Jabartah.Trivia.Infrastructure -s .
dotnet ef database update -p ../Jabartah.Trivia.Infrastructure -s .
```

In `Development`, `Program.cs` auto-applies pending migrations and seeds the sample Arabic categories/questions (`Infrastructure/Persistence/Seed/categories.seed.json`) on every startup — no separate seed step needed.

Requires a local Postgres reachable at the connection string in `appsettings.Development.json` (default `localhost:5432`, db `jabartah_trivia`, `postgres`/`postgres`). There is no `dotnet test` project yet.

### Frontend (`Jabartah.Trivia-frontend/`)

```bash
npm install
npm run dev          # http://localhost:3030
npm run build
npm run lint
npm run typecheck
```

Uses **npm**, not pnpm — the `@nuxt/ui` template defaults to pnpm; if you ever re-scaffold from the template, delete `pnpm-lock.yaml`/`pnpm-workspace.yaml` and the `packageManager` field in `package.json` before installing.

The frontend calls the backend via `useRuntimeConfig().public.apiBase`, set in `nuxt.config.ts` (`http://localhost:5081`). If you change the frontend's dev port, update the backend's `Cors:AllowedOrigin` in `appsettings.Development.json` to match, or requests will be blocked by CORS.

## Backend architecture

Clean architecture, four projects, dependencies flow one way: `Api → Infrastructure → Application → Domain`.

- **Domain** — `GameSession`, `Team`, `Category`, `Question`. No framework dependencies. Entities use private setters + static `Create(...)` factories; business rules live here, not in handlers (e.g. `GameSession.RevealQuestion()` throws if the question was already played or the session isn't in progress; `AwardPoints()` throws if the team isn't in the session). `GameSession` is the aggregate root — it owns `Team` and `GameQuestionState` via private `List<T>` backing fields exposed as `IReadOnlyCollection<T>`.
- **Application** — hand-rolled CQRS, no MediatR. `ICommand<TResponse>`/`IQuery<TResponse>` + matching handler interfaces (`Abstractions/Messaging.cs`), dispatched via `Dispatcher` (`Abstractions/Dispatcher.cs`), which resolves handlers from DI by type using `dynamic`. Each command/query lives in its own folder under `GameSessions/` or `Categories/` (e.g. `GameSessions/SelectQuestion/SelectQuestionCommand.cs` contains the command, its result DTO, and its handler together — not split into separate files). New handlers must be registered manually in `Application/DependencyInjection.cs`; there's no assembly scanning (the README notes this is fine at the current handler count and to switch to Scrutor scanning past ~15-20).
- **Infrastructure** — EF Core + Npgsql. `AppDbContext` implements `IApplicationDbContext` (the interface Application code depends on, so Application never references Infrastructure). Entity configs live in `Persistence/Configurations/`.
- **Api** — minimal API endpoints under `Endpoints/`, one static class per resource (`GameSessionEndpoints`, `CategoryEndpoints`), each with a `Map...Endpoints(this IEndpointRouteBuilder app)` extension called from `Program.cs`. A small inline exception-mapping middleware in `Program.cs` converts `ArgumentException`/`InvalidOperationException` → 400 and `KeyNotFoundException` → 404 as problem-details JSON; anything else falls through to the dev exception page in Development.

### The client-generated-GUID / EF change-tracking gotcha

Entity IDs are set client-side (`Guid.NewGuid()` in domain factories), not DB-generated. When a **new** child entity is attached to an **already-tracked** parent purely by mutating a private collection field (e.g. `GameSession.RevealQuestion()` adding a `GameQuestionState`), EF Core cannot tell it's new — it assumes the entity already exists and emits a no-op `UPDATE` instead of an `INSERT`, which throws `DbUpdateConcurrencyException` ("expected to affect 1 row(s), but actually affected 0"). The fix pattern (see `SelectQuestionCommand.Handle`): have the domain method return the newly created child, then call `db.MarkAdded(newState)` (declared on `IApplicationDbContext`, implemented in `AppDbContext` as `Entry(entity).State = EntityState.Added`) before `SaveChangesAsync`. Apply this same pattern for any future feature that adds new entities to an existing aggregate (lifelines/power-ups, etc.) — it will otherwise fail the same way.

### API surface (MVP)

```
GET  /api/categories
POST /api/game-sessions                                          { teamNames: string[], categoryIds: guid[] }
GET  /api/game-sessions/{id}/board
POST /api/game-sessions/{id}/questions/{questionId}/select
POST /api/game-sessions/{id}/questions/{questionId}/award         { winningTeamId: guid | null }
```

### Not implemented yet (intentional MVP gaps)

- Auth/JWT — not needed for local shared-screen play.
- Admin CRUD for categories/questions — expand `categories.seed.json` directly for now.
- Other game modes (Password/كلمة السر, Ranking/رتبها, charades, media rounds) — same CQRS pattern, new folders under `Application/` once a mode diverges enough to need one.
- Lifelines/power-ups — planned as a new entity + `UsePowerUpCommand` bolted onto `GameSession`.

## Frontend architecture

Nuxt 4, `ssr: false` (pure SPA — this is a shared-screen kiosk app, not a content site), Nuxt UI v4, Tailwind v4. Arabic-only: `app.vue` sets `html lang="ar" dir="rtl"`, loads Tajawal from Google Fonts, and passes `:locale="ar"` (from `@nuxt/ui/locale`) to `<UApp>`. There is no i18n module — don't add one; hardcode Arabic strings in components as the rest of the app does.

- `app/composables/useApi.ts` — the only place that calls the backend (`$fetch` with `baseURL` from `runtimeConfig.public.apiBase`). Add new backend calls here, not ad hoc `$fetch` in components.
- `app/types/api.ts` — TypeScript types mirroring the backend's C# DTOs (`BoardDto`, `TeamDto`, etc.) — keep these in sync by hand when backend response shapes change; there's no shared/generated contract.
- `app/pages/index.vue` — setup screen: team name inputs + category picker → `createGameSession` → navigates to `/game/{id}`.
- `app/pages/game/[id].vue` — the board screen: fetches the board, renders the category × point-value grid, opens a modal on cell click (`selectQuestion` → shows the prompt → `awardPoints` on a team button → reveals the answer), and shows a winner screen once every cell is revealed. Score/board state updates by refetching the board after each award (no local optimistic patching) — acceptable for MVP, revisit if it needs to feel snappier.

`UModal` here (Nuxt UI v4 / Reka UI) uses the `#content` slot with a `UCard` inside for the question dialog. Its enter transition takes a moment — don't judge a screenshot/render taken immediately after the modal opens as broken; the content fades/scales in.
