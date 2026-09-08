using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Questions.DeleteQuestion;

public record DeleteQuestionCommand(Guid QuestionId) : ICommand<bool>;

// Same integrity concern as DeleteCategoryCommand: GameSession.BoardQuestionIds is a
// uuid[] primitive collection holding direct Question.Id references for every session's
// board, even for cells that were never revealed -- deleting a question that was ever
// picked onto a board would leave a dangling reference in historical game data.
public class DeleteQuestionHandler(IApplicationDbContext db) : ICommandHandler<DeleteQuestionCommand, bool>
{
    public async Task<bool> Handle(DeleteQuestionCommand command, CancellationToken ct)
    {
        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == command.QuestionId, ct)
            ?? throw new KeyNotFoundException("Question not found.");

        var isUsedInAnySession = await db.GameSessions.AnyAsync(s => s.BoardQuestionIds.Contains(command.QuestionId), ct);

        if (isUsedInAnySession)
            throw new InvalidOperationException("لا يمكن حذف سؤال تم استخدامه في لعبة سابقة.");

        db.Questions.Remove(question);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
