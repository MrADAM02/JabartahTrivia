using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.GameSessions;
using Jabartah.Trivia.Domain.PasswordGame;
using Jabartah.Trivia.Domain.RankingGame;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Admin.GetDashboardStats;

public record GetDashboardStatsQuery : IQuery<DashboardStatsDto>;

public record ModeStatsDto(int Total, int Completed, int InProgress);
public record TopCategoryDto(Guid CategoryId, string Name, string? Icon, int TimesPlayed);

public record DashboardStatsDto(
    int TotalUsers,
    int TotalGames,
    int CompletedGames,
    ModeStatsDto Trivia,
    ModeStatsDto Password,
    ModeStatsDto Ranking,
    ModeStatsDto Top100,
    List<TopCategoryDto> TopCategories);

// Cross-mode, admin-wide -- unlike GetMySessionsQuery (user-scoped, Take(20)-capped),
// this counts every session ever created, across all users and guests.
public class GetDashboardStatsHandler(IApplicationDbContext db) : IQueryHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery query, CancellationToken ct)
    {
        var totalUsers = await db.Users.CountAsync(ct);

        // Status is stored via HasConversion<string>() per mode, so counts are done per
        // mode's own enum rather than casting to a shared int (which EF can't translate
        // against a string-converted column).
        var trivia = new ModeStatsDto(
            await db.GameSessions.CountAsync(ct),
            await db.GameSessions.CountAsync(s => s.Status == GameSessionStatus.Completed, ct),
            await db.GameSessions.CountAsync(s => s.Status == GameSessionStatus.InProgress, ct));

        var password = new ModeStatsDto(
            await db.PasswordGameSessions.CountAsync(ct),
            await db.PasswordGameSessions.CountAsync(s => s.Status == PasswordGameSessionStatus.Completed, ct),
            await db.PasswordGameSessions.CountAsync(s => s.Status == PasswordGameSessionStatus.InProgress, ct));

        var ranking = new ModeStatsDto(
            await db.RankingGameSessions.CountAsync(ct),
            await db.RankingGameSessions.CountAsync(s => s.Status == RankingGameSessionStatus.Completed, ct),
            await db.RankingGameSessions.CountAsync(s => s.Status == RankingGameSessionStatus.InProgress, ct));

        var top100 = new ModeStatsDto(
            await db.Top100GameSessions.CountAsync(ct),
            await db.Top100GameSessions.CountAsync(s => s.Status == Top100GameSessionStatus.Completed, ct),
            await db.Top100GameSessions.CountAsync(s => s.Status == Top100GameSessionStatus.InProgress, ct));

        // A category counts once per session it appeared on the board in, regardless of
        // how many of its questions were actually revealed within that session.
        var topCategories = await db.GameSessions
            .SelectMany(s => s.CategoryIds)
            .GroupBy(categoryId => categoryId)
            .Select(g => new { CategoryId = g.Key, TimesPlayed = g.Count() })
            .OrderByDescending(g => g.TimesPlayed)
            .Take(5)
            .Join(db.Categories, g => g.CategoryId, c => c.Id, (g, c) => new TopCategoryDto(c.Id, c.Name, c.Icon, g.TimesPlayed))
            .ToListAsync(ct);

        return new DashboardStatsDto(
            totalUsers,
            trivia.Total + password.Total + ranking.Total + top100.Total,
            trivia.Completed + password.Completed + ranking.Completed + top100.Completed,
            trivia, password, ranking, top100,
            topCategories);
    }
}
