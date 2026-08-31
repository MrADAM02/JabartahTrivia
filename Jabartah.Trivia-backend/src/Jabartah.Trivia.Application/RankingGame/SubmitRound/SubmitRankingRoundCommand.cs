using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.RankingGame.CreateRankingGameSession;
using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingGame.SubmitRound;

public record SubmitRankingRoundCommand(Guid RankingGameSessionId, Guid RoundId, List<Guid> OrderedItemIds) : ICommand<SubmitRankingRoundResult>;

public record SubmitRankingRoundResult(int PointsAwarded, List<RankingItemResultDto> CorrectOrder, List<RankingTeamDto> Teams, bool IsSessionComplete);
public record RankingItemResultDto(Guid Id, string Label);

public class SubmitRankingRoundHandler(IApplicationDbContext db) : ICommandHandler<SubmitRankingRoundCommand, SubmitRankingRoundResult>
{
    public async Task<SubmitRankingRoundResult> Handle(SubmitRankingRoundCommand command, CancellationToken ct)
    {
        var session = await db.RankingGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.RankingGameSessionId, ct)
            ?? throw new KeyNotFoundException("Ranking game session not found.");

        var round = session.Rounds.FirstOrDefault(r => r.Id == command.RoundId)
            ?? throw new KeyNotFoundException("Round not found.");

        var items = await db.RankingListItems.Where(i => i.RankingListId == round.RankingListId).ToListAsync(ct);

        if (command.OrderedItemIds.Count != items.Count || command.OrderedItemIds.Distinct().Count() != items.Count
            || command.OrderedItemIds.Any(id => items.All(i => i.Id != id)))
            throw new ArgumentException("Submitted order must be a permutation of the round's items.", nameof(command.OrderedItemIds));

        var correctCount = command.OrderedItemIds
            .Select((id, index) => items.First(i => i.Id == id).CorrectPosition == index + 1 ? 1 : 0)
            .Sum();
        var points = correctCount + (correctCount == items.Count ? items.Count : 0);

        session.SubmitRound(round.Id, points);
        await db.SaveChangesAsync(ct);

        var correctOrder = items.OrderBy(i => i.CorrectPosition).Select(i => new RankingItemResultDto(i.Id, i.Label)).ToList();

        return new SubmitRankingRoundResult(
            points,
            correctOrder,
            session.Teams.Select(t => new RankingTeamDto(t.Id, t.Name, t.Score)).ToList(),
            session.Status == RankingGameSessionStatus.Completed
        );
    }
}
