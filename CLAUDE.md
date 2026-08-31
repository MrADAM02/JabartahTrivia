# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Jabartah Trivia (in-app name: جولة) is an Arabic-language, single-shared-screen party game app (phone/tablet/TV), modeled on تدري؟ / ساحة التحدي / جمعة. Three self-contained game modes, chosen from a landing-page mode picker:

- **Trivia board** (`/trivia/...`) — Jeopardy-style category × point-value grid. Two teams take turns picking a cell; the host reveals the question and marks which team answered correctly.
- **Password / كلمة السر** (`/password/...`) — two teams alternate turns; the shared screen shows a QR code (never the word itself), the clue-giver scans it with their **own phone** to see the word privately, gives one hint, and the host judges the outcome. See the QR-reveal-token section below.
- **Ranking / رتبها** (`/ranking/...`) — teams tap shuffled cards into what they believe is the correct order (e.g. rivers longest-to-shortest); scored by how many land in the correct position, with a bonus for a fully correct order.

Two independent projects, no shared package/workspace tooling between them:
- `Jabartah.Trivia-backend/` — ASP.NET Core API (.NET 10)
- `Jabartah.Trivia-frontend/` — Nuxt 4 SPA (Nuxt UI, Arabic RTL)

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

- **Domain** — three separate, self-contained aggregate roots, one per game mode: `GameSession` (trivia board, `Domain/GameSessions/`), `PasswordGameSession` (`Domain/PasswordGame/`), `RankingGameSession` (`Domain/RankingGame/`). **Deliberately not a shared/polymorphic session type** — each owns its own `Team`-shaped child (`Team`, `PasswordTeam`, `RankingTeam`, all near-identical Id/Name/Score shapes) rather than forcing one `Team` entity to be polymorphic across three different parent FKs. Small duplication accepted on purpose; see the git history for the reasoning if extending this further. No framework dependencies. Entities use private setters + static `Create(...)` factories; business rules live here, not in handlers (e.g. `RevealQuestion`/`StartNextRound`-style methods throw if the round/question was already played or the session isn't in the right state). Each session aggregate owns its child entities (`Team`/`GameQuestionState`, `PasswordTeam`/`PasswordRound`, `RankingTeam`/`RankingRound`) via private `List<T>` backing fields exposed as `IReadOnlyCollection<T>`.
- **Application** — hand-rolled CQRS, no MediatR. `ICommand<TResponse>`/`IQuery<TResponse>` + matching handler interfaces (`Abstractions/Messaging.cs`), dispatched via `Dispatcher` (`Abstractions/Dispatcher.cs`), which resolves handlers from DI by type using `dynamic`. Each command/query lives in its own folder under `GameSessions/`, `PasswordGame/`, `RankingGame/`, or `Categories/` (e.g. `GameSessions/SelectQuestion/SelectQuestionCommand.cs` contains the command, its result DTO, and its handler together — not split into separate files). New handlers must be registered manually in `Application/DependencyInjection.cs` — at 17 handlers now, right at the "~15-20, switch to Scrutor scanning" threshold noted in that file's own comment; the next feature that adds a handful more should make that switch.
- **Infrastructure** — EF Core + Npgsql. `AppDbContext` implements `IApplicationDbContext` (the interface Application code depends on, so Application never references Infrastructure). Entity configs live in `Persistence/Configurations/`, one file per aggregate root usually also configuring its owned child entity (e.g. `GameSessionConfiguration.cs` also configures `GameQuestionStateConfiguration`).
- **Api** — minimal API endpoints under `Endpoints/`, one static class per resource, each with a `Map...Endpoints(this IEndpointRouteBuilder app)` extension called from `Program.cs`. A small inline exception-mapping middleware in `Program.cs` converts `ArgumentException`/`InvalidOperationException` → 400 and `KeyNotFoundException` → 404 as problem-details JSON; anything else falls through to the dev exception page in Development.

### The client-generated-GUID / EF change-tracking gotcha

Entity IDs are set client-side (`Guid.NewGuid()` in domain factories), not DB-generated. When a **new** child entity is attached to an **already-tracked** parent purely by mutating a private collection field (e.g. adding a `GameQuestionState`/`PasswordRound`/`RankingRound` to a session loaded via `.Include(...)`), EF Core cannot tell it's new — it assumes the entity already exists and emits a no-op `UPDATE` instead of an `INSERT`, which throws `DbUpdateConcurrencyException` ("expected to affect 1 row(s), but actually affected 0"). The fix pattern (see `SelectQuestionCommand.Handle`, `StartNextPasswordRoundCommand.Handle`, `StartNextRankingRoundCommand.Handle`): have the domain method return the newly created child, then call `db.MarkAdded(newChild)` (declared on `IApplicationDbContext`, implemented in `AppDbContext` as `Entry(entity).State = EntityState.Added`) before `SaveChangesAsync`. This does **not** apply to an entity added directly via its own top-level `DbSet.Add()` (e.g. `PasswordRevealToken` in `IssueRevealTokenCommand` — it's never attached through a tracked parent's collection, so EF's default "new = Added" already holds). Apply this pattern for any future feature that adds new entities to an existing aggregate (lifelines/power-ups, etc.) — it will otherwise fail the same way.

### The team-turn-order gotcha (EF collection-navigation ordering is not guaranteed)

`PasswordGameSession`/`RankingGameSession` alternate turns between exactly two teams. The first implementation picked the active team via `_teams.ElementAt((roundNumber - 1) % _teams.Count)` after reloading the session from the DB — this broke in practice: EF Core does **not** guarantee a collection navigation reloads in the order rows were inserted (no `ORDER BY` on the join), so team A/team B could come back swapped on a fresh load, scrambling turn order. Fixed by adding an explicit `TurnOrder` (int) column on `PasswordTeam`/`RankingTeam`, set at creation time, and alternating via `_teams.First(t => t.TurnOrder == turnOrder)` instead of positional access. Any future ordering-dependent logic over a reloaded collection needs an explicit sort key the same way — never rely on collection iteration order surviving a DB round-trip.

### QR-reveal-token flow (Password mode)

`PasswordRevealToken` (`Domain/PasswordGame/PasswordRevealToken.cs`) is a short-lived, single-use, url-safe random token minted by `IssueRevealTokenCommand` when the host clicks "عرض رمز QR" for the current round. It denormalizes `PasswordWordId` directly onto itself (rather than being looked up via `PasswordRoundId`) specifically so `ConsumeRevealTokenCommand` never needs a bare-`Id` lookup of `PasswordRound`, which — like `GameQuestionState` — is deliberately not exposed as its own `DbSet`. The frontend encodes `POST /api/reveal/{token}`'s target URL (`{origin}/reveal/{token}`) into a QR code client-side (the `qrcode` npm package) and renders it on the shared screen; the clue-giver scans it with their own phone, which hits the standalone `app/pages/reveal/[token].vue` page — a genuinely unauthenticated route (the token itself is the auth) that is not part of the normal setup/board navigation flow. Consuming an unknown token is a 404 (`KeyNotFoundException`); consuming an expired or already-used token is a normal 200 with `{success:false, expired|alreadyConsumed:true}` — not an error — since that's an expected part of the token's lifecycle, not a client mistake.

### API surface

```
GET  /api/categories
POST /api/game-sessions                                                  { teamNames: string[], categoryIds: guid[] }
GET  /api/game-sessions/{id}/board
POST /api/game-sessions/{id}/questions/{questionId}/select
POST /api/game-sessions/{id}/questions/{questionId}/award                { winningTeamId: guid | null }

GET  /api/password-categories
POST /api/password-sessions                                              { teamNames: string[] (exactly 2), categoryIds: guid[] }
GET  /api/password-sessions/{id}
POST /api/password-sessions/{id}/rounds/next
POST /api/password-sessions/{id}/rounds/{roundId}/reveal-token
POST /api/password-sessions/{id}/rounds/{roundId}/resolve                { correct: bool }
POST /api/reveal/{token}

GET  /api/ranking-categories
POST /api/ranking-sessions                                               { teamNames: string[] (exactly 2), categoryIds: guid[] }
GET  /api/ranking-sessions/{id}
POST /api/ranking-sessions/{id}/rounds/next
POST /api/ranking-sessions/{id}/rounds/{roundId}/submit                  { orderedItemIds: guid[] }
```

Password/Ranking session creation validates up front (400) that the selected categories have enough content for the fixed round count (`PasswordGameSession.RoundsPerTeam = 5` → 10 total; `RankingGameSession.RoundsPerTeam = 3` → 6 total) — content sufficiency is checked once at creation, not discovered mid-game on some later round.

### Not implemented yet (intentional gaps)

- Auth/JWT — not needed for local shared-screen play.
- Admin CRUD for categories/questions/words/ranking-lists — expand the relevant `*.seed.json` directly for now.
- Mixing modes within one session (a single "game night" spanning multiple modes) — each mode is currently a fully separate session type; a future composing feature could sit on top without reworking these.
- Lifelines/power-ups — planned as a new entity + `UsePowerUpCommand` bolted onto `GameSession`.

## Frontend architecture

Nuxt 4, `ssr: false` (pure SPA — this is a shared-screen kiosk app, not a content site), Nuxt UI v4, Tailwind v4. Arabic-only: `app.vue` sets `html lang="ar" dir="rtl"`, loads Tajawal from Google Fonts, and passes `:locale="ar"` (from `@nuxt/ui/locale`) to `<UApp>`. There is no i18n module — don't add one; hardcode Arabic strings in components as the rest of the app does.

- `app/composables/useApi.ts` — the only place that calls the backend (`$fetch` with `baseURL` from `runtimeConfig.public.apiBase`). Add new backend calls here, not ad hoc `$fetch` in components.
- `app/types/api.ts` — TypeScript types mirroring the backend's C# DTOs — keep these in sync by hand when backend response shapes change; there's no shared/generated contract.
- `app/pages/index.vue` — the mode picker: three cards linking to `/trivia/setup`, `/password/setup`, `/ranking/setup`.
- Each mode is `pages/<mode>/setup.vue` (team names + category picker → create session → navigate to `.../game/{id}`) + `pages/<mode>/game/[id].vue` (the actual play screen, ending in a shared winner-screen pattern: sort teams by score, show the top team, "لعبة جديدة" links back to `/`). Trivia's setup/game screens are the template the other two mirror; when adding a fourth mode, copy `password/` (closer in shape — fixed 2-team session, round-based) rather than `trivia/` (open-ended category-grid shape).
- `pages/password/game/[id].vue` also drives the QR flow: mint a token (`issueRevealToken`), render it client-side with the `qrcode` package (`QRCode.toDataURL`), and run a frontend-only visual countdown — the backend never enforces round timing, the host manually judges the outcome the same way trivia's `awardPoints` does.
- `pages/reveal/[token].vue` is a standalone page outside the normal mode flow — no back-nav or shared chrome, meant to be opened on a different physical device (the clue-giver's phone) via the QR code. Does not auto-fetch on mount (a click gates the reveal, so a link-preview bot can't burn the token).
- `pages/ranking/game/[id].vue` uses a tap-in-order UI (pool of shuffled `UButton`s → tap moves an item into a numbered list) rather than drag-to-reorder — deliberate, since this may run on a TV with no pointer/touch input, and Nuxt UI v4 ships no sortable primitive anyway.
- Resuming a mid-round session after a page reload: `GetPasswordSessionQuery`/`GetRankingSessionQuery` return the pending round's data (including a **freshly re-shuffled** item set for ranking) so the frontend can redraw the in-progress round — `StartNextRound` can't be called again while a round is still pending, so this is the only way back in after a refresh.

`UModal` (Nuxt UI v4 / Reka UI, used in the trivia board's question dialog) uses the `#content` slot with a `UCard` inside. Its enter transition takes a moment — don't judge a screenshot/render taken immediately after the modal opens as broken; the content fades/scales in.

### Known limitation: LAN/multi-device play

The Password mode's QR flow is designed for the clue-giver to scan it with a **different physical device** than the shared screen. That only works today if the shared screen was itself loaded via a LAN-reachable address (e.g. `http://192.168.x.x:3030`, with `nuxt dev --host`) rather than `localhost` — the QR encodes `window.location.origin`, so it inherits whatever origin the host device used. The backend's CORS (`Cors:AllowedOrigin` in `appsettings.Development.json`) is currently hardcoded to a single origin (`http://localhost:3030`), so a LAN IP won't be allowed through without updating that value too. Fine for same-machine dev/testing (open a second tab); revisit both settings before testing across real devices on a network.
