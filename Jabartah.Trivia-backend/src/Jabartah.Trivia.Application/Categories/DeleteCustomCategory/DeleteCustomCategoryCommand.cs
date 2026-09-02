using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.DeleteCustomCategory;

public record DeleteCustomCategoryCommand(Guid UserId, Guid CategoryId) : ICommand<bool>;

public class DeleteCustomCategoryHandler(IApplicationDbContext db) : ICommandHandler<DeleteCustomCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteCustomCategoryCommand command, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        // Never let a user delete another user's custom category by guessing its GUID.
        if (category.OwnerUserId != command.UserId)
            throw new KeyNotFoundException("Category not found.");

        // No FK/cascade is configured from Questions.CategoryId -> Categories.Id
        // (QuestionConfiguration.cs has no HasOne/WithMany), so questions must be
        // removed explicitly here or they'd be orphaned.
        var questions = await db.Questions.Where(q => q.CategoryId == command.CategoryId).ToListAsync(ct);
        db.Questions.RemoveRange(questions);
        db.Categories.Remove(category);

        await db.SaveChangesAsync(ct);
        return true;
    }
}
