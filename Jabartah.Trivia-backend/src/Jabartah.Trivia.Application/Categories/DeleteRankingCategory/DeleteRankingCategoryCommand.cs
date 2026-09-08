using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.DeleteRankingCategory;

public record DeleteRankingCategoryCommand(Guid CategoryId) : ICommand<bool>;

public class DeleteRankingCategoryHandler(IApplicationDbContext db) : ICommandHandler<DeleteRankingCategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteRankingCategoryCommand command, CancellationToken ct)
    {
        var category = await db.RankingCategories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        var isUsedInAnySession = await db.RankingGameSessions.AnyAsync(s => s.CategoryIds.Contains(command.CategoryId), ct);
        if (isUsedInAnySession)
            throw new InvalidOperationException("لا يمكن حذف فئة تم استخدامها في لعبة سابقة.");

        var listIds = await db.RankingLists.Where(l => l.RankingCategoryId == command.CategoryId).Select(l => l.Id).ToListAsync(ct);
        db.RankingListItems.RemoveRange(await db.RankingListItems.Where(i => listIds.Contains(i.RankingListId)).ToListAsync(ct));
        db.RankingLists.RemoveRange(await db.RankingLists.Where(l => l.RankingCategoryId == command.CategoryId).ToListAsync(ct));
        db.RankingCategories.Remove(category);

        await db.SaveChangesAsync(ct);
        return true;
    }
}
