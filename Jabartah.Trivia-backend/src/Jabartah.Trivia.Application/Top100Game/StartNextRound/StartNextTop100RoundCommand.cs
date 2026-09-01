using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Game.StartNextRound;

public record StartNextTop100RoundCommand(Guid Top100GameSessionId) : ICommand<StartNextTop100RoundResult>;

public record StartNextTop100RoundResult(
    Guid RoundId, Guid CurrentTurnTeamId, string CurrentTurnTeamName, int RoundNumber, int TotalRounds, string ListTitle, int ItemCount, int MaxGuesses);

public class StartNextTop100RoundHandler(IApplicationDbContext db)
    : ICommandHandler<StartNextTop100RoundCommand, StartNextTop100RoundResult>
{
    public async Task<StartNextTop100RoundResult> Handle(StartNextTop100RoundCommand command, CancellationToken ct)
    {
        var session = await db.Top100GameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.Top100GameSessionId, ct)
            ?? throw new KeyNotFoundException("Top100 game session not found.");

        var usedListIds = session.Rounds.Select(r => r.Top100ListId).ToList();
        var candidateIds = await db.Top100Lists
            .Where(l => session.CategoryIds.Contains(l.Top100CategoryId) && !usedListIds.Contains(l.Id))
            .Select(l => l.Id).ToListAsync(ct);
        if (candidateIds.Count == 0)
            throw new InvalidOperationException("لا توجد قوائم متبقية لهذه الجلسة.");

        var listId = candidateIds[Random.Shared.Next(candidateIds.Count)];
        var itemCount = await db.Top100ListItems.CountAsync(i => i.Top100ListId == listId, ct);

        var round = session.StartNextRound(listId, itemCount); // throws if not InProgress / at MaxRounds / a round still Pending
        db.MarkAdded(round); // new child (client GUID) attached to an already-tracked aggregate -> required

        await db.SaveChangesAsync(ct);

        var list = await db.Top100Lists.FirstAsync(l => l.Id == listId, ct);
        var team = session.Teams.First(t => t.Id == round.CurrentTurnTeamId);

        return new StartNextTop100RoundResult(
            round.Id, team.Id, team.Name, round.RoundNumber, session.MaxRounds, list.Title, itemCount, round.MaxGuesses);
    }
}
