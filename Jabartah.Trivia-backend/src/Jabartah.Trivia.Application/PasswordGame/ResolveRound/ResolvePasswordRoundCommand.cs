using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.PasswordGame.CreatePasswordGameSession;
using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.PasswordGame.ResolveRound;

public record ResolvePasswordRoundCommand(Guid PasswordGameSessionId, Guid RoundId, bool Correct) : ICommand<ResolvePasswordRoundResult>;

public record ResolvePasswordRoundResult(List<PasswordTeamDto> Teams, bool IsSessionComplete);

public class ResolvePasswordRoundHandler(IApplicationDbContext db) : ICommandHandler<ResolvePasswordRoundCommand, ResolvePasswordRoundResult>
{
    public async Task<ResolvePasswordRoundResult> Handle(ResolvePasswordRoundCommand command, CancellationToken ct)
    {
        var session = await db.PasswordGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.PasswordGameSessionId, ct)
            ?? throw new KeyNotFoundException("Password game session not found.");

        session.ResolveRound(command.RoundId, command.Correct ? PasswordRoundOutcome.Correct : PasswordRoundOutcome.Skipped);
        await db.SaveChangesAsync(ct);

        return new ResolvePasswordRoundResult(
            session.Teams.Select(t => new PasswordTeamDto(t.Id, t.Name, t.Score, t.Color, t.Icon, t.ExtraTimeAvailable)).ToList(),
            session.Status == PasswordGameSessionStatus.Completed
        );
    }
}
