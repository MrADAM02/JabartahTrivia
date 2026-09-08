using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Questions.UpdateQuestion;

public record UpdateQuestionCommand(Guid QuestionId, int PointValue, string Prompt, string Answer, string? MediaUrl) : ICommand<bool>;

public class UpdateQuestionHandler(IApplicationDbContext db) : ICommandHandler<UpdateQuestionCommand, bool>
{
    public async Task<bool> Handle(UpdateQuestionCommand command, CancellationToken ct)
    {
        var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == command.QuestionId, ct)
            ?? throw new KeyNotFoundException("Question not found.");

        question.UpdateDetails(command.PointValue, command.Prompt, command.Answer, command.MediaUrl);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
