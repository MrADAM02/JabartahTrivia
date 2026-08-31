using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.PasswordGame.IssueRevealToken;

public record IssueRevealTokenCommand(Guid PasswordGameSessionId, Guid RoundId) : ICommand<IssueRevealTokenResult>;

public record IssueRevealTokenResult(string Token, DateTime ExpiresAt);

public class IssueRevealTokenHandler(IApplicationDbContext db) : ICommandHandler<IssueRevealTokenCommand, IssueRevealTokenResult>
{
    public async Task<IssueRevealTokenResult> Handle(IssueRevealTokenCommand command, CancellationToken ct)
    {
        var session = await db.PasswordGameSessions.Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.PasswordGameSessionId, ct)
            ?? throw new KeyNotFoundException("Password game session not found.");

        var round = session.Rounds.FirstOrDefault(r => r.Id == command.RoundId)
            ?? throw new KeyNotFoundException("Round not found.");
        if (round.Outcome != PasswordRoundOutcome.Pending)
            throw new InvalidOperationException("This round was already resolved.");

        var token = PasswordRevealToken.Create(round.Id, round.PasswordWordId);
        db.PasswordRevealTokens.Add(token); // its own independent top-level DbSet.Add() -- never attached to the
                                             // tracked session/round graph -- default "new = Added" holds, no MarkAdded.
        await db.SaveChangesAsync(ct);

        return new IssueRevealTokenResult(token.Token, token.ExpiresAt);
    }
}
