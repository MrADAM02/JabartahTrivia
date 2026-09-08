using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.DeleteTop100Category;

public record DeleteTop100CategoryCommand(Guid CategoryId) : ICommand<bool>;

public class DeleteTop100CategoryHandler(IApplicationDbContext db) : ICommandHandler<DeleteTop100CategoryCommand, bool>
{
    public async Task<bool> Handle(DeleteTop100CategoryCommand command, CancellationToken ct)
    {
        var category = await db.Top100Categories.FirstOrDefaultAsync(c => c.Id == command.CategoryId, ct)
            ?? throw new KeyNotFoundException("Category not found.");

        var isUsedInAnySession = await db.Top100GameSessions.AnyAsync(s => s.CategoryIds.Contains(command.CategoryId), ct);
        if (isUsedInAnySession)
            throw new InvalidOperationException("لا يمكن حذف فئة تم استخدامها في لعبة سابقة.");

        var listIds = await db.Top100Lists.Where(l => l.Top100CategoryId == command.CategoryId).Select(l => l.Id).ToListAsync(ct);
        db.Top100ListItems.RemoveRange(await db.Top100ListItems.Where(i => listIds.Contains(i.Top100ListId)).ToListAsync(ct));
        db.Top100Lists.RemoveRange(await db.Top100Lists.Where(l => l.Top100CategoryId == command.CategoryId).ToListAsync(ct));
        db.Top100Categories.Remove(category);

        await db.SaveChangesAsync(ct);
        return true;
    }
}
