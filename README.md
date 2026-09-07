# Jabartah Trivia (جولة)

An Arabic-language, single-shared-screen party game app (phone/tablet/TV), modeled on تدري؟ / ساحة التحدي / جمعة. Four self-contained game modes chosen from a landing-page mode picker: a Jeopardy-style trivia board (لعبة الأسئلة), a QR-code word-guessing game (كلمة السر), a tap-to-reorder ranking game (رتبها), and a turn-based ranked-list guessing game (تحدي الـ100).

Two independent projects, no shared package/workspace tooling between them:

- [`Jabartah.Trivia-backend/`](Jabartah.Trivia-backend/README.md) — ASP.NET Core API (.NET 10)
- [`Jabartah.Trivia-frontend/`](Jabartah.Trivia-frontend/README.md) — Nuxt 4 SPA (Nuxt UI, Arabic RTL)

Each has its own README with setup/run instructions. For full architecture details (domain model, CQRS pattern, LAN/multi-device play, per-mode API surface, etc.), see [`CLAUDE.md`](CLAUDE.md).

## Quick start

Run both projects, each in its own terminal:

```bash
# Backend — needs a local Postgres, see Jabartah.Trivia-backend/README.md
cd Jabartah.Trivia-backend/src/Jabartah.Trivia.Api
dotnet run --urls http://localhost:5081

# Frontend
cd Jabartah.Trivia-frontend
npm install
npm run dev
```

Then open `http://localhost:3030`.
