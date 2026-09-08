using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Lists.UpdateTop100List;

public record UpdateTop100ListCommand(Guid ListId, string Title) : ICommand<bool>;

public class UpdateTop100ListHandler(IApplicationDbContext db) : ICommandHandler<UpdateTop100ListCommand, bool>
{
    public async Task<bool> Handle(UpdateTop100ListCommand command, CancellationToken ct)
    {
        var list = await db.Top100Lists.FirstOrDefaultAsync(l => l.Id == command.ListId, ct)
            ?? throw new KeyNotFoundException("List not found.");

        list.UpdateDetails(command.Title);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
