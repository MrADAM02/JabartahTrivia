using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.GameSessions.ActivateTimerDebuff;

public record ActivateTimerDebuffCommand(Guid GameSessionId, Guid TeamId) : ICommand<ActivateTimerDebuffResult>;

public record ActivateTimerDebuffResult(Guid DebuffedTeamId);

public class ActivateTimerDebuffHandler(IApplicationDbContext db) : ICommandHandler<ActivateTimerDebuffCommand, ActivateTimerDebuffResult>
{
    public async Task<ActivateTimerDebuffResult> Handle(ActivateTimerDebuffCommand command, CancellationToken ct)
    {
        var session = await db.GameSessions
            .Include(s => s.Teams)
            .FirstOrDefaultAsync(s => s.Id == command.GameSessionId, ct)
            ?? throw new KeyNotFoundException("Game session not found.");

        session.ActivateTimerDebuff(command.TeamId); // throws if not this team's turn, already pending, etc.
        await db.SaveChangesAsync(ct);

        return new ActivateTimerDebuffResult(session.PendingTimerDebuffTeamId!.Value);
    }
}
