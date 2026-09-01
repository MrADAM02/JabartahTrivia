using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Top100Game.CreateTop100GameSession;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Game.GetSession;

public record GetTop100SessionQuery(Guid Top100GameSessionId) : IQuery<Top100SessionDto>;

public record Top100GuessedItemDto(Guid Id, string Label, int Position);

// Only ever exposes GUESSED items -- the still-hidden ones must never leak, including on a page reload.
public record Top100PendingRoundDto(
    Guid RoundId, string ListTitle, int ItemCount, int MaxGuesses, int GuessesMade,
    Guid CurrentTurnTeamId, string CurrentTurnTeamName, List<Top100GuessedItemDto> GuessedItems);

public record Top100SessionDto(
    Guid Id, string Status, int RoundsPerTeam, List<Top100TeamDto> Teams, int RoundsPlayed, int TotalRounds, Top100PendingRoundDto? PendingRound);

public class GetTop100SessionHandler(IApplicationDbContext db) : IQueryHandler<GetTop100SessionQuery, Top100SessionDto>
{
    public async Task<Top100SessionDto> Handle(GetTop100SessionQuery query, CancellationToken ct)
    {
        var session = await db.Top100GameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == query.Top100GameSessionId, ct)
            ?? throw new KeyNotFoundException("Top100 game session not found.");

        var pending = session.Rounds.FirstOrDefault(r => r.Status == Top100RoundStatus.Pending);
        Top100PendingRoundDto? pendingDto = null;
        if (pending is not null)
        {
            var list = await db.Top100Lists.FirstAsync(l => l.Id == pending.Top100ListId, ct);
            var guessedItems = await db.Top100ListItems
                .Where(i => i.Top100ListId == pending.Top100ListId && pending.GuessedItemIds.Contains(i.Id))
                .OrderBy(i => i.Position)
                .Select(i => new Top100GuessedItemDto(i.Id, i.Label, i.Position))
                .ToListAsync(ct);
            var itemCount = await db.Top100ListItems.CountAsync(i => i.Top100ListId == pending.Top100ListId, ct);
            var currentTeam = session.Teams.First(t => t.Id == pending.CurrentTurnTeamId);

            pendingDto = new Top100PendingRoundDto(
                pending.Id, list.Title, itemCount, pending.MaxGuesses, pending.GuessesMade,
                pending.CurrentTurnTeamId, currentTeam.Name, guessedItems);
        }

        return new Top100SessionDto(
            session.Id,
            session.Status.ToString(),
            session.RoundsPerTeam,
            session.Teams.Select(t => new Top100TeamDto(t.Id, t.Name, t.Score)).ToList(),
            session.Rounds.Count(r => r.Status != Top100RoundStatus.Pending),
            session.MaxRounds,
            pendingDto
        );
    }
}
