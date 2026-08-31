using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.PasswordGame.StartNextRound;

public record StartNextPasswordRoundCommand(Guid PasswordGameSessionId) : ICommand<StartNextPasswordRoundResult>;

public record StartNextPasswordRoundResult(Guid RoundId, Guid TeamId, string TeamName, int RoundNumber, int TotalRounds);

public class StartNextPasswordRoundHandler(IApplicationDbContext db)
    : ICommandHandler<StartNextPasswordRoundCommand, StartNextPasswordRoundResult>
{
    public async Task<StartNextPasswordRoundResult> Handle(StartNextPasswordRoundCommand command, CancellationToken ct)
    {
        var session = await db.PasswordGameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.PasswordGameSessionId, ct)
            ?? throw new KeyNotFoundException("Password game session not found.");

        var usedWordIds = session.Rounds.Select(r => r.PasswordWordId).ToList();
        var candidateIds = await db.PasswordWords
            .Where(w => session.CategoryIds.Contains(w.PasswordCategoryId) && !usedWordIds.Contains(w.Id))
            .Select(w => w.Id).ToListAsync(ct);
        if (candidateIds.Count == 0)
            throw new InvalidOperationException("لا توجد كلمات متبقية لهذه الجلسة.");

        var wordId = candidateIds[Random.Shared.Next(candidateIds.Count)];

        var round = session.StartNextRound(wordId); // throws if not InProgress / at MaxRounds / a round still Pending
        db.MarkAdded(round); // new child (client GUID) attached to an already-tracked aggregate -> required

        await db.SaveChangesAsync(ct);

        var team = session.Teams.First(t => t.Id == round.TeamId);
        return new StartNextPasswordRoundResult(round.Id, team.Id, team.Name, round.RoundNumber, session.MaxRounds);
    }
}
