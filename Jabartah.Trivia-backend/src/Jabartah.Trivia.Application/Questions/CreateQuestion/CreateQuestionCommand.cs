using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Questions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Questions.CreateQuestion;

public record CreateQuestionCommand(Guid CategoryId, int PointValue, string Prompt, string Answer, string? MediaUrl) : ICommand<CreateQuestionResult>;

public record CreateQuestionResult(Guid QuestionId);

public class CreateQuestionHandler(IApplicationDbContext db) : ICommandHandler<CreateQuestionCommand, CreateQuestionResult>
{
    public async Task<CreateQuestionResult> Handle(CreateQuestionCommand command, CancellationToken ct)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == command.CategoryId, ct))
            throw new KeyNotFoundException("Category not found.");

        var question = Question.Create(command.CategoryId, command.PointValue, command.Prompt, command.Answer, command.MediaUrl);
        db.Questions.Add(question);
        await db.SaveChangesAsync(ct);
        return new CreateQuestionResult(question.Id);
    }
}
