using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingLists.UpdateRankingListItem;

public record UpdateRankingListItemCommand(Guid ItemId, string Label, int CorrectPosition) : ICommand<bool>;

public class UpdateRankingListItemHandler(IApplicationDbContext db) : ICommandHandler<UpdateRankingListItemCommand, bool>
{
    public async Task<bool> Handle(UpdateRankingListItemCommand command, CancellationToken ct)
    {
        var item = await db.RankingListItems.FirstOrDefaultAsync(i => i.Id == command.ItemId, ct)
            ?? throw new KeyNotFoundException("Item not found.");

        var conflict = await db.RankingListItems.AnyAsync(
            i => i.RankingListId == item.RankingListId && i.CorrectPosition == command.CorrectPosition && i.Id != command.ItemId, ct);
        if (conflict)
            throw new InvalidOperationException("يوجد عنصر آخر بنفس الترتيب في هذه القائمة بالفعل.");

        item.UpdateDetails(command.Label, command.CorrectPosition);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
