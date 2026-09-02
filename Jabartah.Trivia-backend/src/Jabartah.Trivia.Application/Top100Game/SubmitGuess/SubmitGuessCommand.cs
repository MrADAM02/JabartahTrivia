using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Top100Game.CreateTop100GameSession;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Game.SubmitGuess;

public record SubmitGuessCommand(Guid Top100GameSessionId, Guid RoundId, string GuessText) : ICommand<SubmitGuessResult>;

public record SubmitGuessResult(
    bool Matched, Guid? MatchedItemId, string? MatchedLabel, int? MatchedPosition, int PointsAwarded,
    Guid GuessingTeamId, string GuessingTeamName, Guid NextTurnTeamId, string NextTurnTeamName,
    bool SessionComplete, List<Top100TeamDto> Teams);

public class SubmitGuessHandler(IApplicationDbContext db) : ICommandHandler<SubmitGuessCommand, SubmitGuessResult>
{
    public async Task<SubmitGuessResult> Handle(SubmitGuessCommand command, CancellationToken ct)
    {
        var session = await db.Top100GameSessions
            .Include(s => s.Teams).Include(s => s.Rounds).ThenInclude(r => r.Guesses)
            .FirstOrDefaultAsync(s => s.Id == command.Top100GameSessionId, ct)
            ?? throw new KeyNotFoundException("Top100 game session not found.");

        var round = session.Rounds.FirstOrDefault(r => r.Id == command.RoundId)
            ?? throw new KeyNotFoundException("Round not found.");

        var items = await db.Top100ListItems.Where(i => i.Top100ListId == round.Top100ListId).ToListAsync(ct);
        var alreadyGuessedIds = round.Guesses.Where(g => g.MatchedItemId is not null).Select(g => g.MatchedItemId!.Value).ToHashSet();

        var normalizedGuess = Top100AnswerNormalizer.Normalize(command.GuessText);
        var matchedItem = items
            .Where(i => !alreadyGuessedIds.Contains(i.Id))
            .FirstOrDefault(i =>
                Top100AnswerNormalizer.Normalize(i.Label) == normalizedGuess
                || i.AlternateSpellings.Any(alt => Top100AnswerNormalizer.Normalize(alt) == normalizedGuess));

        var (guessingTeamId, matchedItemId, points, sessionComplete) =
            session.SubmitGuess(round.Id, command.GuessText, matchedItem?.Id, matchedItem?.Position ?? 0, items.Count);

        // The new Top100Guess is a client-GUID child attached to an already-tracked round
        // via a private collection -- EF can't tell it's new without this.
        db.MarkAdded(round.Guesses.Last());

        await db.SaveChangesAsync(ct);

        var guessingTeam = session.Teams.First(t => t.Id == guessingTeamId);
        var nextTeam = session.Teams.First(t => t.Id == round.CurrentTurnTeamId);

        return new SubmitGuessResult(
            matchedItemId is not null, matchedItemId, matchedItem?.Label, matchedItem?.Position, points,
            guessingTeamId, guessingTeam.Name, nextTeam.Id, nextTeam.Name,
            sessionComplete,
            session.Teams.Select(t => new Top100TeamDto(t.Id, t.Name, t.Score, t.Color, t.Icon)).ToList()
        );
    }
}
