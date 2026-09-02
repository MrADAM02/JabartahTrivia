using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.GameSessions.RevealAnswer;

public record RevealAnswerCommand(Guid GameSessionId, Guid QuestionId) : ICommand<RevealAnswerResult>;

public record RevealAnswerResult(string Answer);

public class RevealAnswerHandler(IApplicationDbContext db) : ICommandHandler<RevealAnswerCommand, RevealAnswerResult>
{
    public async Task<RevealAnswerResult> Handle(RevealAnswerCommand command, CancellationToken ct)
    {
        var session = await db.GameSessions
            .Include(s => s.QuestionStates)
            .FirstOrDefaultAsync(s => s.Id == command.GameSessionId, ct)
            ?? throw new KeyNotFoundException("Game session not found.");

        // The host is trusted to judge when to reveal -- no server-side gate here, same
        // trust model as everywhere else in this app (host manually judges who answered).
        if (!session.QuestionStates.Any(q => q.QuestionId == command.QuestionId))
            throw new InvalidOperationException("Question was not revealed in this session yet.");

        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == command.QuestionId, ct)
            ?? throw new KeyNotFoundException("Question not found.");

        return new RevealAnswerResult(question.Answer);
    }
}
