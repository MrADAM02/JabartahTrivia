# Jabartah Trivia — Backend Scaffold

Clean architecture, CQRS without MediatR (see `Application/Abstractions/Dispatcher.cs`).
Trivia board MVP: `CreateGameSession → GetBoard → SelectQuestion → AwardPoints`.

## Setup

1. Have PostgreSQL running locally (or update the connection string).
   `appsettings.Development.json` assumes `jabartah_trivia` DB, user `postgres` / `postgres`.
2. Install the EF Core CLI tool if you don't have it: `dotnet tool install --global dotnet-ef`
3. Restore + create the first migration:
   ```
   cd src/Jabartah.Trivia.Api
   dotnet restore
   dotnet ef migrations add InitialCreate -p ../Jabartah.Trivia.Infrastructure -s .
   dotnet run
   ```
   On first run in Development, the app auto-applies migrations and seeds the
   3 sample Arabic categories from `Infrastructure/Persistence/Seed/categories.seed.json`.
4. Test it:
   ```
   POST /api/game-sessions           { "teamNames": ["فريق ١","فريق ٢"], "categoryIds": [...] }
   GET  /api/game-sessions/{id}/board
   POST /api/game-sessions/{id}/questions/{questionId}/select
   POST /api/game-sessions/{id}/questions/{questionId}/award   { "winningTeamId": "..." }
   ```

## What's deliberately NOT here yet

- Auth/JWT — add when you're ready to persist accounts/history; not needed for local play.
- Admin CRUD for categories/questions — expand `categories.seed.json` for now.
- The other two game modes (Password, Ranking) — same pattern, new folders under
  `Application/GameSessions/` (or a new `Application/<ModeName>/` root once modes diverge enough).
- Lifelines/power-ups — bolt onto `GameSession` as a new entity + `UsePowerUpCommand` when ready.

## Known simplification

`Dispatcher` uses `dynamic` for handler resolution — trivial reflection cost, fine at this scale.
Swap to a source-generated dispatcher (or the `Mediator` NuGet package by martinothamar, MIT-licensed)
only if profiling ever shows this matters.
