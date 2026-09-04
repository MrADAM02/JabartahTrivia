using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingGame.RevealPosition;

public record RevealRankingPositionCommand(Guid RankingGameSessionId, Guid RoundId, Guid TeamId) : ICommand<RevealRankingPositionResult>;

// Reveals exactly one (position, item) pair from the round's correct order --
// never the rest, so the team still has to place everything else themselves.
public record RevealRankingPositionResult(int Position, string ItemLabel);

public class RevealRankingPositionHandler(IApplicationDbContext db) : ICommandHandler<RevealRankingPositionCommand, RevealRankingPositionResult>
{
    public async Task<RevealRankingPositionResult> Handle(RevealRankingPositionCommand command, CancellationToken ct)
    {
        var session = await db.RankingGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.RankingGameSessionId, ct)
            ?? throw new KeyNotFoundException("Ranking game session not found.");

        var round = session.Rounds.FirstOrDefault(r => r.Id == command.RoundId)
            ?? throw new KeyNotFoundException("Round not found.");

        session.UseRevealPosition(command.RoundId, command.TeamId); // throws if not this team's round, already used, etc.

        var items = await db.RankingListItems.Where(i => i.RankingListId == round.RankingListId).ToListAsync(ct);
        var revealed = items[Random.Shared.Next(items.Count)];

        await db.SaveChangesAsync(ct);

        return new RevealRankingPositionResult(revealed.CorrectPosition, revealed.Label);
    }
}
