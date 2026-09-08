using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingLists.CreateRankingListItem;

public record CreateRankingListItemCommand(Guid ListId, string Label, int CorrectPosition) : ICommand<CreateRankingListItemResult>;

public record CreateRankingListItemResult(Guid ItemId);

public class CreateRankingListItemHandler(IApplicationDbContext db) : ICommandHandler<CreateRankingListItemCommand, CreateRankingListItemResult>
{
    public async Task<CreateRankingListItemResult> Handle(CreateRankingListItemCommand command, CancellationToken ct)
    {
        if (!await db.RankingLists.AnyAsync(l => l.Id == command.ListId, ct))
            throw new KeyNotFoundException("List not found.");
        if (await db.RankingListItems.AnyAsync(i => i.RankingListId == command.ListId && i.CorrectPosition == command.CorrectPosition, ct))
            throw new InvalidOperationException("يوجد عنصر آخر بنفس الترتيب في هذه القائمة بالفعل.");

        var item = RankingListItem.Create(command.ListId, command.Label, command.CorrectPosition);
        db.RankingListItems.Add(item);
        await db.SaveChangesAsync(ct);
        return new CreateRankingListItemResult(item.Id);
    }
}
