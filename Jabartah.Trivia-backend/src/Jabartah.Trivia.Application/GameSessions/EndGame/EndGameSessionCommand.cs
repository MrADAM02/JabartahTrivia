using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.GameSessions.CreateGameSession;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.GameSessions.EndGame;

// Finalizes the session right now with whatever scores currently exist -- used both for an
// explicit "end game early" action and, from the frontend, once when the board is naturally
// fully revealed (Complete() is otherwise never invoked for trivia).
public record EndGameSessionCommand(Guid GameSessionId) : ICommand<EndGameSessionResult>;

public record EndGameSessionResult(List<TeamDto> Teams);

public class EndGameSessionHandler(IApplicationDbContext db) : ICommandHandler<EndGameSessionCommand, EndGameSessionResult>
{
    public async Task<EndGameSessionResult> Handle(EndGameSessionCommand command, CancellationToken ct)
    {
        var session = await db.GameSessions
            .Include(s => s.Teams)
            .FirstOrDefaultAsync(s => s.Id == command.GameSessionId, ct)
            ?? throw new KeyNotFoundException("Game session not found.");

        session.Complete();
        await db.SaveChangesAsync(ct);

        return new EndGameSessionResult(
            session.Teams.Select(t => new TeamDto(t.Id, t.Name, t.Score, t.DoublePointsAvailable, t.TwoAnswersAvailable, t.Color, t.Icon, t.HalfOpponentTimerAvailable)).ToList()
        );
    }
}
