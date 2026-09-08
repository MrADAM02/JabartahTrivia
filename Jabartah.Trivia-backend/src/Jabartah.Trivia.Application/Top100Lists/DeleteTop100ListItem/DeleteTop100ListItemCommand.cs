using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Lists.DeleteTop100ListItem;

public record DeleteTop100ListItemCommand(Guid ItemId) : ICommand<bool>;

// Unlike Ranking, a Top100Guess DOES store a real MatchedItemId reference when a guess
// is correct -- so an item that was ever matched in a played round can't be removed
// without corrupting that history, same integrity concern as deleting a word/question.
public class DeleteTop100ListItemHandler(IApplicationDbContext db) : ICommandHandler<DeleteTop100ListItemCommand, bool>
{
    public async Task<bool> Handle(DeleteTop100ListItemCommand command, CancellationToken ct)
    {
        var item = await db.Top100ListItems.FirstOrDefaultAsync(i => i.Id == command.ItemId, ct)
            ?? throw new KeyNotFoundException("Item not found.");

        var isMatchedInAnyGuess = await db.Top100GameSessions
            .SelectMany(s => s.Rounds)
            .SelectMany(r => r.Guesses)
            .AnyAsync(g => g.MatchedItemId == command.ItemId, ct);

        if (isMatchedInAnyGuess)
            throw new InvalidOperationException("لا يمكن حذف عنصر تم العثور عليه في لعبة سابقة.");

        db.Top100ListItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
