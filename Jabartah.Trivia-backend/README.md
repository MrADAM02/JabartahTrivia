# Jabartah Trivia — Backend

Clean architecture, CQRS without MediatR (see `Application/Abstractions/Dispatcher.cs`).
Trivia board MVP: `CreateGameSession → GetBoard → SelectQuestion → AwardPoints`.

See the root [`README.md`](../README.md) for the project overview and [`CLAUDE.md`](../CLAUDE.md) for full architecture details, including the other three game modes (Password, Ranking, Top100).

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
   sample Arabic categories from `Infrastructure/Persistence/Seed/categories.seed.json`
   (and each other mode's own `*.seed.json`).
4. Test it:
   ```
   POST /api/game-sessions           { "teamNames": ["فريق ١","فريق ٢"], "categoryIds": [...] }
   GET  /api/game-sessions/{id}/board
   POST /api/game-sessions/{id}/questions/{questionId}/select
   POST /api/game-sessions/{id}/questions/{questionId}/award   { "winningTeamId": "..." }
   ```

## What's deliberately NOT here yet

- Admin CRUD for categories/questions/words/lists — expand the relevant `*.seed.json` directly for now.
- Mixing modes within one session — see `CLAUDE.md` for why each mode is a fully separate session type today.

## Known simplification

`Dispatcher` uses `dynamic` for handler resolution — trivial reflection cost, fine at this scale.
Swap to a source-generated dispatcher (or the `Mediator` NuGet package by martinothamar, MIT-licensed)
only if profiling ever shows this matters.
