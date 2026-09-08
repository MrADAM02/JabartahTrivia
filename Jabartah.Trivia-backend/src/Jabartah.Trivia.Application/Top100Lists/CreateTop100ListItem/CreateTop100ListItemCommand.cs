using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Lists.CreateTop100ListItem;

public record CreateTop100ListItemCommand(Guid ListId, string Label, int Position, List<string> AlternateSpellings) : ICommand<CreateTop100ListItemResult>;

public record CreateTop100ListItemResult(Guid ItemId);

public class CreateTop100ListItemHandler(IApplicationDbContext db) : ICommandHandler<CreateTop100ListItemCommand, CreateTop100ListItemResult>
{
    public async Task<CreateTop100ListItemResult> Handle(CreateTop100ListItemCommand command, CancellationToken ct)
    {
        if (!await db.Top100Lists.AnyAsync(l => l.Id == command.ListId, ct))
            throw new KeyNotFoundException("List not found.");
        if (await db.Top100ListItems.AnyAsync(i => i.Top100ListId == command.ListId && i.Position == command.Position, ct))
            throw new InvalidOperationException("يوجد عنصر آخر بنفس الترتيب في هذه القائمة بالفعل.");

        var item = Top100ListItem.Create(command.ListId, command.Label, command.Position, command.AlternateSpellings);
        db.Top100ListItems.Add(item);
        await db.SaveChangesAsync(ct);
        return new CreateTop100ListItemResult(item.Id);
    }
}
