using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingLists.DeleteRankingList;

public record DeleteRankingListCommand(Guid ListId) : ICommand<bool>;

// RankingRound.RankingListId is a real FK-shaped reference to a played round's list --
// a list that was ever played can't be removed without corrupting that history.
public class DeleteRankingListHandler(IApplicationDbContext db) : ICommandHandler<DeleteRankingListCommand, bool>
{
    public async Task<bool> Handle(DeleteRankingListCommand command, CancellationToken ct)
    {
        var list = await db.RankingLists.FirstOrDefaultAsync(l => l.Id == command.ListId, ct)
            ?? throw new KeyNotFoundException("List not found.");

        var isUsedInAnyRound = await db.RankingGameSessions
            .SelectMany(s => s.Rounds)
            .AnyAsync(r => r.RankingListId == command.ListId, ct);

        if (isUsedInAnyRound)
            throw new InvalidOperationException("لا يمكن حذف قائمة تم استخدامها في لعبة سابقة.");

        db.RankingListItems.RemoveRange(await db.RankingListItems.Where(i => i.RankingListId == command.ListId).ToListAsync(ct));
        db.RankingLists.Remove(list);

        await db.SaveChangesAsync(ct);
        return true;
    }
}
