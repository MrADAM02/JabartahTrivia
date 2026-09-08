using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingLists.DeleteRankingListItem;

public record DeleteRankingListItemCommand(Guid ItemId) : ICommand<bool>;

// Unlike Password/Trivia, a played RankingRound never stores individual item references
// (only the final PointsAwarded), so removing one item from a list carries no historical
// integrity risk the way deleting a whole list does -- no "used in a session" guard needed.
public class DeleteRankingListItemHandler(IApplicationDbContext db) : ICommandHandler<DeleteRankingListItemCommand, bool>
{
    public async Task<bool> Handle(DeleteRankingListItemCommand command, CancellationToken ct)
    {
        var item = await db.RankingListItems.FirstOrDefaultAsync(i => i.Id == command.ItemId, ct)
            ?? throw new KeyNotFoundException("Item not found.");

        db.RankingListItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
