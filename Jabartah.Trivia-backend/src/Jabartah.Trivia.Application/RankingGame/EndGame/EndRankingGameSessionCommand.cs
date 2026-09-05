using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.RankingGame.CreateRankingGameSession;
using Jabartah.Trivia.Application.RankingGame.GetSession;
using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingGame.EndGame;

public record EndRankingGameSessionCommand(Guid RankingGameSessionId) : ICommand<RankingSessionDto>;

public class EndRankingGameSessionHandler(IApplicationDbContext db) : ICommandHandler<EndRankingGameSessionCommand, RankingSessionDto>
{
    public async Task<RankingSessionDto> Handle(EndRankingGameSessionCommand command, CancellationToken ct)
    {
        var session = await db.RankingGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.RankingGameSessionId, ct)
            ?? throw new KeyNotFoundException("Ranking game session not found.");

        session.Complete();
        await db.SaveChangesAsync(ct);

        // Ending early discards any round still pending -- no points were awarded for it anyway.
        return new RankingSessionDto(
            session.Id,
            session.Status.ToString(),
            session.Teams.Select(t => new RankingTeamDto(t.Id, t.Name, t.Score, t.Color, t.Icon, t.RevealPositionAvailable)).ToList(),
            session.Rounds.Count(r => r.Status != RankingRoundStatus.Pending),
            session.MaxRounds,
            null
        );
    }
}
