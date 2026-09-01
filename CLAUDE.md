# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Jabartah Trivia (in-app name: جولة) is an Arabic-language, single-shared-screen party game app (phone/tablet/TV), modeled on تدري؟ / ساحة التحدي / جمعة. Four self-contained game modes, chosen from a landing-page mode picker:

- **Trivia board** (`/trivia/...`) — Jeopardy-style category × point-value grid, **exactly 6 categories** selected at setup (not pre-selected). Two teams take turns picking a cell; the host reveals the question and marks which team answered correctly. Each team also gets two one-time-per-game power-ups: **مضاعفة النقاط** (double points) and **محاولتين** (two answers/one retry) — see the power-ups section below.
- **Password / كلمة السر** (`/password/...`) — two teams alternate turns; the shared screen shows a QR code (never the word itself), the clue-giver scans it with their **own phone** to see the word privately, gives one hint, and the host judges the outcome. Round count is selectable at setup ({5, 7, 10} rounds per team). See the QR-reveal-token section below.
- **Ranking / رتبها** (`/ranking/...`) — teams tap shuffled cards into what they believe is the correct order (e.g. rivers longest-to-shortest); scored by how many land in the correct position, with a bonus for a fully correct order. Round count selectable ({2, 4, 6} rounds per team).
- **تحدي الـ100** (`/top100/...`) — teams alternate individual guesses (one at a time, typed) at a themed ranked list (e.g. ancient civilizations, most-to-least famous); a correct guess scores points equal to that item's list position, rewarding recall of the more obscure/lower-ranked entries over the obvious ones. Round count selectable ({1, 2, 3} rounds per team — each round is a whole list, so far fewer rounds than the other modes). Answer matching is simple normalization (Arabic letter-variant folding + whitespace), not fuzzy/typo-tolerant — see `Top100AnswerNormalizer`.

Every mode's winner screen can end in a **draw** (`app/composables/useWinner.ts`'s `getWinner`) — ties are shown as "🤝 تعادل" listing every team at the top score, never resolved by silently picking one.

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

The frontend derives its API base at runtime from whatever host it was itself loaded from (`useApi.ts`: `` `http://${window.location.hostname}:${apiPort}` ``, `apiPort` from `runtimeConfig.public.apiPort`, default `5081`) — **not** a hardcoded URL. `nuxt.config.ts`'s `devServer.host: '0.0.0.0'` and the backend's CORS policy (`Program.cs`, `IsAllowedOrigin`) work together with this so the app works identically whether loaded via `localhost` or a LAN IP, on the host device or a second device (e.g. Password mode's QR flow) — see "LAN/multi-device play" below. The backend's `Cors:AllowedFrontendPort` setting (`appsettings.Development.json`) only needs to match the frontend's **port**, not a full origin.

## Backend architecture

Clean architecture, four projects, dependencies flow one way: `Api → Infrastructure → Application → Domain`.

- **Domain** — four separate, self-contained aggregate roots, one per game mode: `GameSession` (trivia board, `Domain/GameSessions/`), `PasswordGameSession` (`Domain/PasswordGame/`), `RankingGameSession` (`Domain/RankingGame/`), `Top100GameSession` (`Domain/Top100Game/`). **Deliberately not a shared/polymorphic session type** — each owns its own `Team`-shaped child (`Team`, `PasswordTeam`, `RankingTeam`, `Top100Team`, all near-identical Id/Name/Score(/TurnOrder) shapes) rather than forcing one `Team` entity to be polymorphic across four different parent FKs. Small duplication accepted on purpose. No framework dependencies. Entities use private setters + static `Create(...)` factories; business rules live here, not in handlers (e.g. `RevealQuestion`/`StartNextRound`-style methods throw if the round/question was already played or the session isn't in the right state). Each session aggregate owns its child entities (`Team`/`GameQuestionState`, `PasswordTeam`/`PasswordRound`, `RankingTeam`/`RankingRound`, `Top100Team`/`Top100Round`) via private `List<T>` backing fields exposed as `IReadOnlyCollection<T>`. Password/Ranking/Top100 sessions all require exactly 2 teams and have an **instance** `RoundsPerTeam` (picked at setup from a fixed `AllowedRoundsPerTeam` array per mode) — not a `const`, unlike earlier in this project's history.
- **Application** — hand-rolled CQRS, no MediatR. `ICommand<TResponse>`/`IQuery<TResponse>` + matching handler interfaces (`Abstractions/Messaging.cs`), dispatched via `Dispatcher` (`Abstractions/Dispatcher.cs`), which resolves handlers from DI by type using `dynamic`. Each command/query lives in its own folder under `GameSessions/`, `PasswordGame/`, `RankingGame/`, `Top100Game/`, or `Categories/` (e.g. `GameSessions/SelectQuestion/SelectQuestionCommand.cs` contains the command, its result DTO, and its handler together — not split into separate files). New handlers must be registered manually in `Application/DependencyInjection.cs` — at 22 handlers now, past the "~15-20, switch to Scrutor scanning" threshold noted in that file's own comment; the next feature that adds a handful more should actually make that switch instead of continuing to hand-add.
- **Infrastructure** — EF Core + Npgsql. `AppDbContext` implements `IApplicationDbContext` (the interface Application code depends on, so Application never references Infrastructure). Entity configs live in `Persistence/Configurations/`, one file per aggregate root usually also configuring its owned child entity (e.g. `GameSessionConfiguration.cs` also configures `GameQuestionStateConfiguration`).
- **Api** — minimal API endpoints under `Endpoints/`, one static class per resource, each with a `Map...Endpoints(this IEndpointRouteBuilder app)` extension called from `Program.cs`. A small inline exception-mapping middleware in `Program.cs` converts `ArgumentException`/`InvalidOperationException` → 400 and `KeyNotFoundException` → 404 as problem-details JSON; anything else falls through to the dev exception page in Development.

### The client-generated-GUID / EF change-tracking gotcha

Entity IDs are set client-side (`Guid.NewGuid()` in domain factories), not DB-generated. When a **new** child entity is attached to an **already-tracked** parent purely by mutating a private collection field (e.g. adding a `GameQuestionState`/`PasswordRound`/`RankingRound` to a session loaded via `.Include(...)`), EF Core cannot tell it's new — it assumes the entity already exists and emits a no-op `UPDATE` instead of an `INSERT`, which throws `DbUpdateConcurrencyException` ("expected to affect 1 row(s), but actually affected 0"). The fix pattern (see `SelectQuestionCommand.Handle`, `StartNextPasswordRoundCommand.Handle`, `StartNextRankingRoundCommand.Handle`): have the domain method return the newly created child, then call `db.MarkAdded(newChild)` (declared on `IApplicationDbContext`, implemented in `AppDbContext` as `Entry(entity).State = EntityState.Added`) before `SaveChangesAsync`. This does **not** apply to an entity added directly via its own top-level `DbSet.Add()` (e.g. `PasswordRevealToken` in `IssueRevealTokenCommand` — it's never attached through a tracked parent's collection, so EF's default "new = Added" already holds). Apply this pattern for any future feature that adds new entities to an existing aggregate (lifelines/power-ups, etc.) — it will otherwise fail the same way.

### The team-turn-order gotcha (EF collection-navigation ordering is not guaranteed)

`PasswordGameSession`/`RankingGameSession` alternate turns between exactly two teams. The first implementation picked the active team via `_teams.ElementAt((roundNumber - 1) % _teams.Count)` after reloading the session from the DB — this broke in practice: EF Core does **not** guarantee a collection navigation reloads in the order rows were inserted (no `ORDER BY` on the join), so team A/team B could come back swapped on a fresh load, scrambling turn order. Fixed by adding an explicit `TurnOrder` (int) column on `PasswordTeam`/`RankingTeam`, set at creation time, and alternating via `_teams.First(t => t.TurnOrder == turnOrder)` instead of positional access. Any future ordering-dependent logic over a reloaded collection needs an explicit sort key the same way — never rely on collection iteration order surviving a DB round-trip.

### Trivia power-ups and the `IsResolved` guard

`GameQuestionState` gained `PowerUpTeamId`/`ActivePowerUp` (which team activated which power-up, if any, when the question was opened via `RevealQuestion`) and `AttemptFailed`/`IsResolved`. A power-up is **consumed at activation time** (`Team.UseDoublePoints()`/`UseTwoAnswers()`, flips a one-time-use flag), regardless of whether it ends up mattering — simplest rule, no "refund" edge cases to reason about. `GameSession.AwardPoints` now returns `Guid? retryTeamId` instead of `void`: if محاولتين is active and the first `AwardPoints` call comes back with no winner, the question is **not** marked resolved (`GameQuestionState.RecordAttempt` returns `true`, allowing exactly one more call) and the API response's `CorrectAnswer` stays `null` so the frontend doesn't leak it while a retry is pending. This also introduced a real guard that didn't exist before: `IsResolved` now rejects a second `AwardPoints` call on the same question outside the one legitimate retry case — previously nothing stopped double-awarding, it just never happened because the UI only ever called it once. مضاعفة النقاط doubles `question.PointValue` only when the winning team matches `PowerUpTeamId` and `ActivePowerUp == DoublePoints`. Only one power-up can be active per question (not stackable) — that's the domain's own validation in `RevealQuestion`, not a UI-only rule.

### QR-reveal-token flow (Password mode)

`PasswordRevealToken` (`Domain/PasswordGame/PasswordRevealToken.cs`) is a short-lived, single-use, url-safe random token minted by `IssueRevealTokenCommand` when the host clicks "عرض رمز QR" for the current round. It denormalizes `PasswordWordId` directly onto itself (rather than being looked up via `PasswordRoundId`) specifically so `ConsumeRevealTokenCommand` never needs a bare-`Id` lookup of `PasswordRound`, which — like `GameQuestionState` — is deliberately not exposed as its own `DbSet`. The frontend encodes `POST /api/reveal/{token}`'s target URL (`{origin}/reveal/{token}`) into a QR code client-side (the `qrcode` npm package) and renders it on the shared screen; the clue-giver scans it with their own phone, which hits the standalone `app/pages/reveal/[token].vue` page — a genuinely unauthenticated route (the token itself is the auth) that is not part of the normal setup/board navigation flow. Consuming an unknown token is a 404 (`KeyNotFoundException`); consuming an expired or already-used token is a normal 200 with `{success:false, expired|alreadyConsumed:true}` — not an error — since that's an expected part of the token's lifecycle, not a client mistake.

### API surface

```
GET  /api/categories
POST /api/game-sessions                                                  { teamNames: string[] (exactly 6 categoryIds), categoryIds: guid[] }
GET  /api/game-sessions/{id}/board
POST /api/game-sessions/{id}/questions/{questionId}/select               { activatingTeamId: guid | null, powerUp: "DoublePoints" | "TwoAnswers" | null }
POST /api/game-sessions/{id}/questions/{questionId}/award                { winningTeamId: guid | null }

GET  /api/password-categories
POST /api/password-sessions                                              { teamNames: string[] (exactly 2), categoryIds: guid[], roundsPerTeam: int (5|7|10) }
GET  /api/password-sessions/{id}
POST /api/password-sessions/{id}/rounds/next
POST /api/password-sessions/{id}/rounds/{roundId}/reveal-token
POST /api/password-sessions/{id}/rounds/{roundId}/resolve                { correct: bool }
POST /api/reveal/{token}

GET  /api/ranking-categories
POST /api/ranking-sessions                                               { teamNames: string[] (exactly 2), categoryIds: guid[], roundsPerTeam: int (2|4|6) }
GET  /api/ranking-sessions/{id}
POST /api/ranking-sessions/{id}/rounds/next
POST /api/ranking-sessions/{id}/rounds/{roundId}/submit                  { orderedItemIds: guid[] }

GET  /api/top100-categories
POST /api/top100-sessions                                                { teamNames: string[] (exactly 2), categoryIds: guid[], roundsPerTeam: int (1|2|3) }
GET  /api/top100-sessions/{id}
POST /api/top100-sessions/{id}/rounds/next
POST /api/top100-sessions/{id}/rounds/{roundId}/guess                    { guessText: string }
```

Password/Ranking/Top100 session creation validates up front (400) that the selected categories have enough content for the *requested* round count (`session.RoundsPerTeam * 2` — Password needs that many words, Ranking/Top100 need that many lists) — content sufficiency is checked once at creation against the actual chosen `roundsPerTeam`, not discovered mid-game on some later round, and not against a fixed constant anymore now that round counts are selectable. When bumping a mode's max `AllowedRoundsPerTeam`, check the corresponding `*.seed.json` has enough entries for the new worst case (all categories selected, max rounds) — Ranking's content was expanded from 3 to 7 categories for exactly this reason when {2,4,6} replaced a fixed 3.

### Not implemented yet (intentional gaps)

- Auth/JWT — not needed for local shared-screen play.
- Admin CRUD for categories/questions/words/ranking-lists/top100-lists — expand the relevant `*.seed.json` directly for now.
- Mixing modes within one session (a single "game night" spanning multiple modes) — each mode is currently a fully separate session type; a future composing feature could sit on top without reworking these.
- تحدي الـ100's answer matching is intentionally simple (normalization, not fuzzy/typo-tolerant) — see `Top100AnswerNormalizer`. Upgrade if it turns out to matter in practice.
- تحدي الـ100's visual design currently matches the other 3 modes' Nuxt UI look; a تدري؟-matching redesign is expected once reference screenshots are provided — not done yet.

## Frontend architecture

Nuxt 4, `ssr: false` (pure SPA — this is a shared-screen kiosk app, not a content site), Nuxt UI v4, Tailwind v4. Arabic-only: `app.vue` sets `html lang="ar" dir="rtl"`, loads Tajawal from Google Fonts, and passes `:locale="ar"` (from `@nuxt/ui/locale`) to `<UApp>`. There is no i18n module — don't add one; hardcode Arabic strings in components as the rest of the app does.

- `app/composables/useApi.ts` — the only place that calls the backend. `apiBase` is a `computed` derived from `window.location.hostname` (see "LAN/multi-device play" below), not a static config value — every `$fetch` call passes `baseURL: apiBase.value`. Add new backend calls here, not ad hoc `$fetch` in components.
- `app/composables/useWinner.ts` — plain exported `getWinner(teams)` util (not stateful, doesn't need the `use*`-composable convention to be auto-imported) used by every mode's `game/[id].vue` to decide the end-of-game screen: single winner, or a draw if multiple teams share the top score. Never determine a winner by sorting and taking `[0]` directly in a page — ties need `isDraw` handling every time.
- `app/types/api.ts` — TypeScript types mirroring the backend's C# DTOs — keep these in sync by hand when backend response shapes change; there's no shared/generated contract.
- `app/pages/index.vue` — the mode picker: four cards linking to `/trivia/setup`, `/password/setup`, `/ranking/setup`, `/top100/setup`.
- Each mode is `pages/<mode>/setup.vue` (team names + category picker, **not pre-selected** — the player actively picks; trivia additionally caps at exactly 6 → create session → navigate to `.../game/{id}`) + `pages/<mode>/game/[id].vue` (the play screen, ending in the shared `useWinner`-based winner/draw screen, "لعبة جديدة" linking back to `/`). Password/Ranking/Top100 setup screens also have a rounds-per-team picker (a row of `UButton`s over that mode's `AllowedRoundsPerTeam` values) whose selection is passed straight through to `createXGameSession`. Trivia's setup/game screens are the template for a category-grid-shaped mode; `password/` is the template for a fixed-2-team round-based mode (Ranking and Top100 both copied it).
- `pages/password/game/[id].vue` also drives the QR flow: mint a token (`issueRevealToken`), render it client-side with the `qrcode` package (`QRCode.toDataURL`), and run a frontend-only visual countdown — the backend never enforces round timing, the host manually judges the outcome the same way trivia's `awardPoints` does.
- `pages/reveal/[token].vue` is a standalone page outside the normal mode flow — no back-nav or shared chrome, meant to be opened on a different physical device (the clue-giver's phone) via the QR code. Does not auto-fetch on mount (a click gates the reveal, so a link-preview bot can't burn the token).
- `pages/ranking/game/[id].vue` uses a tap-in-order UI (pool of shuffled `UButton`s → tap moves an item into a numbered list) rather than drag-to-reorder — deliberate, since this may run on a TV with no pointer/touch input, and Nuxt UI v4 ships no sortable primitive anyway.
- `pages/top100/game/[id].vue` is turn-based like Password/Ranking but *within* a round teams alternate individual typed guesses (a `UInput` + submit, not one bulk submission) — after each guess the turn flips regardless of hit/miss, and already-guessed items are shown so players don't waste a guess repeating one. On round completion the full list is revealed (✅/⬜ per item) before "التالي".
- `pages/trivia/game/[id].vue`'s power-up buttons (💰/🔁 under each team's score card) arm a `{teamId, type}` pair that's passed into the next `selectQuestion(...)` call, then cleared — arming is purely local UI state, the backend is the one that actually consumes the power-up (at question-select time) and enforces one-time-use. If `awardPoints` comes back with `canRetry: true`, the modal stays open with an inline note instead of showing the answer (which is `null` from the API in that case) — only a second, non-retry `award()` call closes it.
- Resuming a mid-round session after a page reload: `Get*SessionQuery` for Password/Ranking/Top100 all return the pending round's data (Ranking's items **freshly re-shuffled**; Top100's only the already-**guessed** items, never the hidden ones) so the frontend can redraw the in-progress round — `StartNextRound` can't be called again while a round is still pending, so this is the only way back in after a refresh.

`UModal` (Nuxt UI v4 / Reka UI, used in the trivia board's question dialog) uses the `#content` slot with a `UCard` inside. Its enter transition takes a moment — don't judge a screenshot/render taken immediately after the modal opens as broken; the content fades/scales in.

### LAN/multi-device play

Password mode's QR flow needs the clue-giver to open the reveal link on a **different physical device** than the shared screen — this used to break because the QR encoded `window.location.origin` (fine) but `apiBase` was a hardcoded `http://localhost:5081` (broken: `localhost` on the phone means the phone itself) and the backend's CORS only allowed one exact origin. Fixed with three coordinated pieces, all of which matter together:
1. `nuxt.config.ts`: `devServer.host: '0.0.0.0'` — the dev server actually listens on the LAN interface, not just loopback.
2. `useApi.ts`: `apiBase` computed from `window.location.hostname` (same host the page itself loaded from) + `runtimeConfig.public.apiPort` — works out correctly whether the page was loaded via `localhost` or a LAN IP, on the host device or a second device, no per-network config.
3. Backend `Program.cs`: CORS uses `SetIsOriginAllowed` with a predicate (`IsAllowedOrigin`) instead of a fixed origin string — allows `localhost`/`127.0.0.1` or any RFC1918 private-network IP, on the port from `Cors:AllowedFrontendPort` (`appsettings.Development.json`), so it works on any home network without editing config per-network.

To actually test cross-device: run the API bound to all interfaces (e.g. `dotnet run --urls http://0.0.0.0:5081`), load the shared screen via the host machine's LAN IP (not `localhost`) — the QR generated from there will resolve correctly from any other device on the same network.
