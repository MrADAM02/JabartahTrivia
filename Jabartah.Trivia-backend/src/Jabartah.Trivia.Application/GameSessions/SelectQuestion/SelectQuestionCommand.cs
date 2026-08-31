using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.GameSessions.SelectQuestion;

public record SelectQuestionCommand(Guid GameSessionId, Guid QuestionId) : ICommand<SelectQuestionResult>;

public record SelectQuestionResult(Guid QuestionId, int PointValue, string Prompt, string? MediaUrl);

public class SelectQuestionHandler(IApplicationDbContext db)
    : ICommandHandler<SelectQuestionCommand, SelectQuestionResult>
{
    public async Task<SelectQuestionResult> Handle(SelectQuestionCommand command, CancellationToken ct)
    {
        var session = await db.GameSessions
            .Include(s => s.QuestionStates)
            .FirstOrDefaultAsync(s => s.Id == command.GameSessionId, ct)
            ?? throw new KeyNotFoundException("Game session not found.");

        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == command.QuestionId, ct)
            ?? throw new KeyNotFoundException("Question not found.");

        var newState = session.RevealQuestion(question.Id); // throws if already used
        db.MarkAdded(newState);
        await db.SaveChangesAsync(ct);

        // Answer is intentionally NOT returned here -- the host reveals it separately
        // (AwardPoints handler / a future RevealAnswer endpoint) so it doesn't leak to the display screen early.
        return new SelectQuestionResult(question.Id, question.PointValue, question.Prompt, question.MediaUrl);
    }
}
