using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Sessions.GetMySessions;

public record GetMySessionsQuery(Guid UserId) : IQuery<List<MySessionDto>>;

public record MyTeamDto(string Name, int Score);

public record MySessionDto(
    Guid Id,
    string Mode,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    List<MyTeamDto> Teams,
    List<string> WinnerTeamNames,
    bool IsDraw
);

public class GetMySessionsHandler(IApplicationDbContext db) : IQueryHandler<GetMySessionsQuery, List<MySessionDto>>
{
    public async Task<List<MySessionDto>> Handle(GetMySessionsQuery query, CancellationToken ct)
    {
        // Only sessions that actually finished (early-ended or naturally completed) belong in
        // history -- a session someone just walked away from mid-game never gets a CompletedAt.
        var trivia = await db.GameSessions
            .Include(s => s.Teams)
            .Where(s => s.UserId == query.UserId && s.CompletedAt != null)
            .Select(s => new { s.Id, s.CreatedAt, s.CompletedAt, Teams = s.Teams.Select(t => new MyTeamDto(t.Name, t.Score)).ToList() })
            .ToListAsync(ct);

        var password = await db.PasswordGameSessions
            .Include(s => s.Teams)
            .Where(s => s.UserId == query.UserId && s.CompletedAt != null)
            .Select(s => new { s.Id, s.CreatedAt, s.CompletedAt, Teams = s.Teams.Select(t => new MyTeamDto(t.Name, t.Score)).ToList() })
            .ToListAsync(ct);

        var ranking = await db.RankingGameSessions
            .Include(s => s.Teams)
            .Where(s => s.UserId == query.UserId && s.CompletedAt != null)
            .Select(s => new { s.Id, s.CreatedAt, s.CompletedAt, Teams = s.Teams.Select(t => new MyTeamDto(t.Name, t.Score)).ToList() })
            .ToListAsync(ct);

        var top100 = await db.Top100GameSessions
            .Include(s => s.Teams)
            .Where(s => s.UserId == query.UserId && s.CompletedAt != null)
            .Select(s => new { s.Id, s.CreatedAt, s.CompletedAt, Teams = s.Teams.Select(t => new MyTeamDto(t.Name, t.Score)).ToList() })
            .ToListAsync(ct);

        var all = new List<MySessionDto>();
        all.AddRange(trivia.Select(s => Map("Trivia", s.Id, s.CreatedAt, s.CompletedAt, s.Teams)));
        all.AddRange(password.Select(s => Map("Password", s.Id, s.CreatedAt, s.CompletedAt, s.Teams)));
        all.AddRange(ranking.Select(s => Map("Ranking", s.Id, s.CreatedAt, s.CompletedAt, s.Teams)));
        all.AddRange(top100.Select(s => Map("Top100", s.Id, s.CreatedAt, s.CompletedAt, s.Teams)));

        // History is capped to the most recent 20 across all 4 modes -- a personal
        // play log, not something that needs real pagination at this app's scale.
        return all.OrderByDescending(s => s.CreatedAt).Take(20).ToList();
    }

    private static MySessionDto Map(string mode, Guid id, DateTime createdAt, DateTime? completedAt, List<MyTeamDto> teams)
    {
        var (winners, isDraw) = WinnerCalculator.Calculate(teams);
        return new MySessionDto(id, mode, createdAt, completedAt, teams, winners, isDraw);
    }
}
