using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Top100Game.CreateTop100GameSession;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Game.GetSession;

public record GetTop100SessionQuery(Guid Top100GameSessionId) : IQuery<Top100SessionDto>;

// One entry per guess attempt, correct or not -- the frontend filters this single log into
// both the discovered-items list (Matched == true) and the shared mistakes pile (Matched == false).
public record Top100GuessLogEntryDto(int SequenceNumber, Guid TeamId, string TeamName, string GuessText, bool Matched, string? MatchedLabel, int? MatchedPosition);

public record Top100PendingRoundDto(
    Guid RoundId, string ListTitle, int ItemCount, int MaxGuesses, int GuessesMade,
    Guid CurrentTurnTeamId, string CurrentTurnTeamName, List<Top100GuessLogEntryDto> Guesses);

public record Top100CompletedRoundSummaryDto(string ListTitle, List<Top100GuessLogEntryDto> Guesses);

public record Top100SessionDto(
    Guid Id, string Status, int GuessesPerTeam, List<Top100TeamDto> Teams,
    Top100PendingRoundDto? PendingRound, Top100CompletedRoundSummaryDto? CompletedRound);

public class GetTop100SessionHandler(IApplicationDbContext db) : IQueryHandler<GetTop100SessionQuery, Top100SessionDto>
{
    public async Task<Top100SessionDto> Handle(GetTop100SessionQuery query, CancellationToken ct)
    {
        var session = await db.Top100GameSessions
            .Include(s => s.Teams).Include(s => s.Rounds).ThenInclude(r => r.Guesses)
            .FirstOrDefaultAsync(s => s.Id == query.Top100GameSessionId, ct)
            ?? throw new KeyNotFoundException("Top100 game session not found.");

        var round = session.Rounds.FirstOrDefault(); // 0 or 1 always, exactly one round per session now
        Top100PendingRoundDto? pendingDto = null;
        Top100CompletedRoundSummaryDto? completedDto = null;

        if (round is not null)
        {
            var list = await db.Top100Lists.FirstAsync(l => l.Id == round.Top100ListId, ct);
            var itemCount = await db.Top100ListItems.CountAsync(i => i.Top100ListId == round.Top100ListId, ct);

            var matchedItemIds = round.Guesses.Where(g => g.MatchedItemId is not null).Select(g => g.MatchedItemId!.Value).ToList();
            var matchedItemsById = (await db.Top100ListItems.Where(i => matchedItemIds.Contains(i.Id)).ToListAsync(ct)).ToDictionary(i => i.Id);

            var log = round.Guesses
                .OrderBy(g => g.SequenceNumber)
                .Select(g =>
                {
                    var team = session.Teams.First(t => t.Id == g.TeamId);
                    var matchedItem = g.MatchedItemId is { } id && matchedItemsById.TryGetValue(id, out var item) ? item : null;
                    return new Top100GuessLogEntryDto(g.SequenceNumber, g.TeamId, team.Name, g.GuessText, matchedItem is not null, matchedItem?.Label, matchedItem?.Position);
                })
                .ToList();

            if (round.Status == Top100RoundStatus.Completed)
            {
                completedDto = new Top100CompletedRoundSummaryDto(list.Title, log);
            }
            else
            {
                var currentTeam = session.Teams.First(t => t.Id == round.CurrentTurnTeamId);
                pendingDto = new Top100PendingRoundDto(
                    round.Id, list.Title, itemCount, round.MaxGuesses, round.GuessesMade, round.CurrentTurnTeamId, currentTeam.Name, log);
            }
        }

        return new Top100SessionDto(
            session.Id,
            session.Status.ToString(),
            session.GuessesPerTeam,
            session.Teams.Select(t => new Top100TeamDto(t.Id, t.Name, t.Score, t.Color, t.Icon)).ToList(),
            pendingDto,
            completedDto
        );
    }
}
