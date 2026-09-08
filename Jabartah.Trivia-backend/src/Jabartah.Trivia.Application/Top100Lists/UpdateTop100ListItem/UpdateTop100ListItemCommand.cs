using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Lists.UpdateTop100ListItem;

public record UpdateTop100ListItemCommand(Guid ItemId, string Label, int Position, List<string> AlternateSpellings) : ICommand<bool>;

public class UpdateTop100ListItemHandler(IApplicationDbContext db) : ICommandHandler<UpdateTop100ListItemCommand, bool>
{
    public async Task<bool> Handle(UpdateTop100ListItemCommand command, CancellationToken ct)
    {
        var item = await db.Top100ListItems.FirstOrDefaultAsync(i => i.Id == command.ItemId, ct)
            ?? throw new KeyNotFoundException("Item not found.");

        var conflict = await db.Top100ListItems.AnyAsync(
            i => i.Top100ListId == item.Top100ListId && i.Position == command.Position && i.Id != command.ItemId, ct);
        if (conflict)
            throw new InvalidOperationException("يوجد عنصر آخر بنفس الترتيب في هذه القائمة بالفعل.");

        item.UpdateDetails(command.Label, command.Position, command.AlternateSpellings);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
