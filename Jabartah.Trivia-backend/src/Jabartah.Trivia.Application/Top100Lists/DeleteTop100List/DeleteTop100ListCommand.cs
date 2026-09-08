using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Lists.DeleteTop100List;

public record DeleteTop100ListCommand(Guid ListId) : ICommand<bool>;

// Top100Round.Top100ListId is a real FK-shaped reference to a played round's list -- a list
// that was ever played can't be removed without corrupting that history.
public class DeleteTop100ListHandler(IApplicationDbContext db) : ICommandHandler<DeleteTop100ListCommand, bool>
{
    public async Task<bool> Handle(DeleteTop100ListCommand command, CancellationToken ct)
    {
        var list = await db.Top100Lists.FirstOrDefaultAsync(l => l.Id == command.ListId, ct)
            ?? throw new KeyNotFoundException("List not found.");

        var isUsedInAnyRound = await db.Top100GameSessions
            .SelectMany(s => s.Rounds)
            .AnyAsync(r => r.Top100ListId == command.ListId, ct);

        if (isUsedInAnyRound)
            throw new InvalidOperationException("لا يمكن حذف قائمة تم استخدامها في لعبة سابقة.");

        db.Top100ListItems.RemoveRange(await db.Top100ListItems.Where(i => i.Top100ListId == command.ListId).ToListAsync(ct));
        db.Top100Lists.Remove(list);

        await db.SaveChangesAsync(ct);
        return true;
    }
}
