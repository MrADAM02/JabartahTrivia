using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.DeletePasswordCategory;

public record DeletePasswordCategoryCommand(Guid CategoryId) : ICommand<bool>;

// Same integrity concern as trivia's DeleteCategoryCommand: PasswordGameSession.CategoryIds
// is a uuid[] primitive collection referencing a session's chosen categories directly, even
// ones that never actually had a word drawn from them.
public class DeletePasswordCategoryHandler(IApplicationDbContext db) : ICommandHandler<DeletePasswordCategoryCommand, bool>
{
    public async Task<bool> Handle(DeletePasswordCategoryCommand command, CancellationToken ct)
    {
        var category = await db.PasswordCategories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        var isUsedInAnySession = await db.PasswordGameSessions.AnyAsync(s => s.CategoryIds.Contains(command.CategoryId), ct);
        if (isUsedInAnySession)
            throw new InvalidOperationException("لا يمكن حذف فئة تم استخدامها في لعبة سابقة.");

        db.PasswordWords.RemoveRange(await db.PasswordWords.Where(w => w.PasswordCategoryId == command.CategoryId).ToListAsync(ct));
        db.PasswordCategories.Remove(category);

        await db.SaveChangesAsync(ct);
        return true;
    }
}
