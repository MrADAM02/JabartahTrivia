using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Top100Game.CreateTop100GameSession;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Game.SubmitGuess;

public record SubmitGuessCommand(Guid Top100GameSessionId, Guid RoundId, string GuessText) : ICommand<SubmitGuessResult>;

public record Top100RevealedItemDto(Guid Id, string Label, int Position, bool WasGuessed);

public record SubmitGuessResult(
    bool Matched, Guid? MatchedItemId, string? MatchedLabel, int? MatchedPosition, int PointsAwarded,
    Guid GuessingTeamId, string GuessingTeamName, Guid NextTurnTeamId, string NextTurnTeamName,
    bool RoundComplete, List<Top100RevealedItemDto>? FullList,
    List<Top100TeamDto> Teams, bool IsSessionComplete);

public class SubmitGuessHandler(IApplicationDbContext db) : ICommandHandler<SubmitGuessCommand, SubmitGuessResult>
{
    public async Task<SubmitGuessResult> Handle(SubmitGuessCommand command, CancellationToken ct)
    {
        var session = await db.Top100GameSessions
            .Include(s => s.Teams).Include(s => s.Rounds)
            .FirstOrDefaultAsync(s => s.Id == command.Top100GameSessionId, ct)
            ?? throw new KeyNotFoundException("Top100 game session not found.");

        var round = session.Rounds.FirstOrDefault(r => r.Id == command.RoundId)
            ?? throw new KeyNotFoundException("Round not found.");

        var items = await db.Top100ListItems.Where(i => i.Top100ListId == round.Top100ListId).ToListAsync(ct);

        var normalizedGuess = Top100AnswerNormalizer.Normalize(command.GuessText);
        var matchedItem = items
            .Where(i => !round.GuessedItemIds.Contains(i.Id))
            .FirstOrDefault(i =>
                Top100AnswerNormalizer.Normalize(i.Label) == normalizedGuess
                || i.AlternateSpellings.Any(alt => Top100AnswerNormalizer.Normalize(alt) == normalizedGuess));

        var (guessingTeamId, matchedItemId, points, complete) =
            session.SubmitGuess(round.Id, matchedItem?.Id, matchedItem?.Position ?? 0, items.Count);

        await db.SaveChangesAsync(ct);

        var guessingTeam = session.Teams.First(t => t.Id == guessingTeamId);
        var nextTeam = session.Teams.First(t => t.Id == round.CurrentTurnTeamId);

        List<Top100RevealedItemDto>? fullList = null;
        if (complete)
        {
            fullList = items.OrderBy(i => i.Position)
                .Select(i => new Top100RevealedItemDto(i.Id, i.Label, i.Position, round.GuessedItemIds.Contains(i.Id)))
                .ToList();
        }

        return new SubmitGuessResult(
            matchedItemId is not null, matchedItemId, matchedItem?.Label, matchedItem?.Position, points,
            guessingTeamId, guessingTeam.Name, nextTeam.Id, nextTeam.Name,
            complete, fullList,
            session.Teams.Select(t => new Top100TeamDto(t.Id, t.Name, t.Score)).ToList(),
            session.Status == Top100GameSessionStatus.Completed
        );
    }
}
