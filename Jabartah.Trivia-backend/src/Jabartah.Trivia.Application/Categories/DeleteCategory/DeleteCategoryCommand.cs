using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.DeleteCategory;

public record DeleteCategoryCommand(Guid CategoryId) : ICommand<bool>;

// Sessions are never deleted, and GameSession.CategoryIds/.BoardQuestionIds (uuid[]
// primitive-collection columns, not FKs) hold direct references to a session's chosen
// categories/questions even for cells that were never actually revealed -- so a category
// that was ever *selected* for a board, not just answered, can't be removed without
// leaving a dangling reference in historical game data. Block it with a clear message
// instead of silently corrupting that history.
public class DeleteCategoryHandler(IApplicationDbContext db) : ICommandHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteCategoryCommand command, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        var isUsedInAnySession = await db.GameSessions.AnyAsync(s => s.CategoryIds.Contains(command.CategoryId), ct);

        if (isUsedInAnySession)
            throw new InvalidOperationException("لا يمكن حذف فئة تم استخدامها في لعبة سابقة.");

        db.Questions.RemoveRange(await db.Questions.Where(q => q.CategoryId == command.CategoryId).ToListAsync(ct));
        db.Categories.Remove(category);

        await db.SaveChangesAsync(ct);
        return true;
    }
}
