using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.PasswordGame.CreatePasswordGameSession;
using Jabartah.Trivia.Application.PasswordGame.GetSession;
using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.PasswordGame.EndGame;

public record EndPasswordGameSessionCommand(Guid PasswordGameSessionId) : ICommand<PasswordSessionDto>;

public class EndPasswordGameSessionHandler(IApplicationDbContext db) : ICommandHandler<EndPasswordGameSessionCommand, PasswordSessionDto>
{
    public async Task<PasswordSessionDto> Handle(EndPasswordGameSessionCommand command, CancellationToken ct)
    {
        var session = await db.PasswordGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.PasswordGameSessionId, ct)
            ?? throw new KeyNotFoundException("Password game session not found.");

        session.Complete();
        await db.SaveChangesAsync(ct);

        var pending = session.Rounds.FirstOrDefault(r => r.Outcome == PasswordRoundOutcome.Pending);
        PasswordPendingRoundDto? pendingDto = pending is null
            ? null
            : new PasswordPendingRoundDto(pending.Id, pending.TeamId, session.Teams.First(t => t.Id == pending.TeamId).Name, pending.RoundNumber);

        return new PasswordSessionDto(
            session.Id,
            session.Status.ToString(),
            session.Teams.Select(t => new PasswordTeamDto(t.Id, t.Name, t.Score, t.Color, t.Icon, t.ExtraTimeAvailable)).ToList(),
            session.Rounds.Count(r => r.Outcome != PasswordRoundOutcome.Pending),
            session.MaxRounds,
            pendingDto
        );
    }
}
