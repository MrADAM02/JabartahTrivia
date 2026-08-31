using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.RankingGame.CreateRankingGameSession;
using Jabartah.Trivia.Application.RankingGame.StartNextRound;
using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingGame.GetSession;

public record GetRankingSessionQuery(Guid RankingGameSessionId) : IQuery<RankingSessionDto>;

public record RankingSessionDto(Guid Id, string Status, List<RankingTeamDto> Teams, int RoundsPlayed, int TotalRounds, RankingPendingRoundDto? PendingRound);

// Carries the shuffled items too (not just round metadata) so a page reload mid-round can
// redisplay the tap-in-order UI -- StartNextRound can't be called again while a round is
// still Pending, so this is the only way to recover a round in progress after a refresh.
public record RankingPendingRoundDto(Guid RoundId, Guid TeamId, string TeamName, int RoundNumber, string ListTitle, List<RankingItemOptionDto> Items);

public class GetRankingSessionHandler(IApplicationDbContext db) : IQueryHandler<GetRankingSessionQuery, RankingSessionDto>
{
    public async Task<RankingSessionDto> Handle(GetRankingSessionQuery query, CancellationToken ct)
    {
        var session = await db.RankingGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == query.RankingGameSessionId, ct)
            ?? throw new KeyNotFoundException("Ranking game session not found.");

        var pending = session.Rounds.FirstOrDefault(r => r.Status == RankingRoundStatus.Pending);
        RankingPendingRoundDto? pendingDto = null;
        if (pending is not null)
        {
            var list = await db.RankingLists.FirstAsync(l => l.Id == pending.RankingListId, ct);
            var items = await db.RankingListItems.Where(i => i.RankingListId == pending.RankingListId).ToListAsync(ct);
            var shuffled = items.OrderBy(_ => Random.Shared.Next()).Select(i => new RankingItemOptionDto(i.Id, i.Label)).ToList();
            pendingDto = new RankingPendingRoundDto(
                pending.Id, pending.TeamId, session.Teams.First(t => t.Id == pending.TeamId).Name, pending.RoundNumber, list.Title, shuffled);
        }

        return new RankingSessionDto(
            session.Id,
            session.Status.ToString(),
            session.Teams.Select(t => new RankingTeamDto(t.Id, t.Name, t.Score)).ToList(),
            session.Rounds.Count(r => r.Status != RankingRoundStatus.Pending),
            session.MaxRounds,
            pendingDto
        );
    }
}
