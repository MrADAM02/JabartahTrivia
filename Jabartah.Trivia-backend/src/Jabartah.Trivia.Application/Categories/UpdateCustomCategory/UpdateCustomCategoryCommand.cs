using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.UpdateCustomCategory;

public record CustomQuestionUpdateInput(Guid QuestionId, string Prompt, string Answer);

public record UpdateCustomCategoryCommand(
    Guid UserId,
    Guid CategoryId,
    string Name,
    string? Icon,
    List<CustomQuestionUpdateInput> Questions
) : ICommand<bool>;

public class UpdateCustomCategoryHandler(IApplicationDbContext db) : ICommandHandler<UpdateCustomCategoryCommand, bool>
{
    public async Task<bool> Handle(UpdateCustomCategoryCommand command, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        // Never let a user edit another user's custom category by guessing its GUID.
        if (category.OwnerUserId != command.UserId)
            throw new KeyNotFoundException("Category not found.");

        category.UpdateDetails(command.Name, command.Icon);

        foreach (var input in command.Questions)
        {
            var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == input.QuestionId, ct)
                ?? throw new KeyNotFoundException("Question not found.");

            // Same guard as the category above -- also rules out a question that belongs
            // to a different (even same-owner) category being edited via this endpoint.
            if (question.CategoryId != command.CategoryId)
                throw new KeyNotFoundException("Question not found.");

            // Point value is fixed at creation (it's the tier identity CreateCustomCategoryCommand
            // enforces exactly-one-per-tier against) -- only prompt/answer are editable here.
            question.UpdateDetails(question.PointValue, input.Prompt, input.Answer, null);
        }

        await db.SaveChangesAsync(ct);
        return true;
    }
}
