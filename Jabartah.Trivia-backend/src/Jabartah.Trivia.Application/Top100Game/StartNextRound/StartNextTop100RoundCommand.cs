using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Game.StartNextRound;

public record StartNextTop100RoundCommand(Guid Top100GameSessionId) : ICommand<StartNextTop100RoundResult>;

public record StartNextTop100RoundResult(
    Guid RoundId, Guid CurrentTurnTeamId, string CurrentTurnTeamName, string ListTitle, int ItemCount, int MaxGuesses);

public class StartNextTop100RoundHandler(IApplicationDbContext db)
    : ICommandHandler<StartNextTop100RoundCommand, StartNextTop100RoundResult>
{
    public async Task<StartNextTop100RoundResult> Handle(StartNextTop100RoundCommand command, CancellationToken ct)
    {
        var session = await db.Top100GameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.Top100GameSessionId, ct)
            ?? throw new KeyNotFoundException("Top100 game session not found.");

        // Only one round is ever played per session now, so there's no "already used" list
        // to exclude -- any list from the chosen categories is a valid pick.
        var candidateIds = await db.Top100Lists
            .Where(l => session.CategoryIds.Contains(l.Top100CategoryId))
            .Select(l => l.Id).ToListAsync(ct);
        if (candidateIds.Count == 0)
            throw new InvalidOperationException("لا توجد قوائم متاحة لهذه الفئات.");

        var listId = candidateIds[Random.Shared.Next(candidateIds.Count)];
        var itemCount = await db.Top100ListItems.CountAsync(i => i.Top100ListId == listId, ct);

        var round = session.StartRound(listId); // throws if not InProgress / already started
        db.MarkAdded(round); // new child (client GUID) attached to an already-tracked aggregate -> required

        await db.SaveChangesAsync(ct);

        var list = await db.Top100Lists.FirstAsync(l => l.Id == listId, ct);
        var team = session.Teams.First(t => t.Id == round.CurrentTurnTeamId);

        return new StartNextTop100RoundResult(round.Id, team.Id, team.Name, list.Title, itemCount, round.MaxGuesses);
    }
}
