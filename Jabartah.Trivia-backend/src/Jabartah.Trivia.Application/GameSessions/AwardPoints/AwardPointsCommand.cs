using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.GameSessions.CreateGameSession;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.GameSessions.AwardPoints;

// WinningTeamId is nullable: null means "nobody got it right", which is a valid outcome.
public record AwardPointsCommand(Guid GameSessionId, Guid QuestionId, Guid? WinningTeamId) : ICommand<AwardPointsResult>;

public record AwardPointsResult(List<TeamDto> Teams, string? CorrectAnswer, bool CanRetry, Guid? RetryTeamId, string? RetryTeamName);

public class AwardPointsHandler(IApplicationDbContext db)
    : ICommandHandler<AwardPointsCommand, AwardPointsResult>
{
    public async Task<AwardPointsResult> Handle(AwardPointsCommand command, CancellationToken ct)
    {
        var session = await db.GameSessions
            .Include(s => s.Teams)
            .Include(s => s.QuestionStates)
            .FirstOrDefaultAsync(s => s.Id == command.GameSessionId, ct)
            ?? throw new KeyNotFoundException("Game session not found.");

        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == command.QuestionId, ct)
            ?? throw new KeyNotFoundException("Question not found.");

        var retryTeamId = session.AwardPoints(question.Id, command.WinningTeamId, question.PointValue);
        await db.SaveChangesAsync(ct);

        var retryTeamName = retryTeamId is { } id ? session.Teams.First(t => t.Id == id).Name : null;

        return new AwardPointsResult(
            session.Teams.Select(t => new TeamDto(t.Id, t.Name, t.Score, t.DoublePointsAvailable, t.TwoAnswersAvailable, t.Color, t.Icon)).ToList(),
            retryTeamId is null ? question.Answer : null, // must NOT leak the answer while a retry is pending
            retryTeamId is not null,
            retryTeamId,
            retryTeamName
        );
    }
}
