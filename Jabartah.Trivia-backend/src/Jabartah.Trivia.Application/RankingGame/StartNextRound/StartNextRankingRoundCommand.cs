using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingGame.StartNextRound;

public record StartNextRankingRoundCommand(Guid RankingGameSessionId) : ICommand<StartNextRankingRoundResult>;

public record StartNextRankingRoundResult(Guid RoundId, Guid TeamId, string TeamName, int RoundNumber, int TotalRounds, string ListTitle, List<RankingItemOptionDto> Items);
public record RankingItemOptionDto(Guid Id, string Label);

public class StartNextRankingRoundHandler(IApplicationDbContext db)
    : ICommandHandler<StartNextRankingRoundCommand, StartNextRankingRoundResult>
{
    public async Task<StartNextRankingRoundResult> Handle(StartNextRankingRoundCommand command, CancellationToken ct)
    {
        var session = await db.RankingGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.RankingGameSessionId, ct)
            ?? throw new KeyNotFoundException("Ranking game session not found.");

        var usedListIds = session.Rounds.Select(r => r.RankingListId).ToList();
        var candidateIds = await db.RankingLists
            .Where(l => session.CategoryIds.Contains(l.RankingCategoryId) && !usedListIds.Contains(l.Id))
            .Select(l => l.Id).ToListAsync(ct);
        if (candidateIds.Count == 0)
            throw new InvalidOperationException("لا توجد قوائم متبقية لهذه الجلسة.");

        var listId = candidateIds[Random.Shared.Next(candidateIds.Count)];

        var round = session.StartNextRound(listId); // throws if not InProgress / at MaxRounds / a round still Pending
        db.MarkAdded(round); // new child (client GUID) attached to an already-tracked aggregate -> required

        await db.SaveChangesAsync(ct);

        var list = await db.RankingLists.FirstAsync(l => l.Id == listId, ct);
        var items = await db.RankingListItems.Where(i => i.RankingListId == listId).ToListAsync(ct);
        var shuffled = items.OrderBy(_ => Random.Shared.Next())
            .Select(i => new RankingItemOptionDto(i.Id, i.Label)).ToList(); // CorrectPosition stripped -- must not leak the answer

        var team = session.Teams.First(t => t.Id == round.TeamId);
        return new StartNextRankingRoundResult(round.Id, team.Id, team.Name, round.RoundNumber, session.MaxRounds, list.Title, shuffled);
    }
}
