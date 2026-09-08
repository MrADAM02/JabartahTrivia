using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingLists.UpdateRankingList;

public record UpdateRankingListCommand(Guid ListId, string Title) : ICommand<bool>;

public class UpdateRankingListHandler(IApplicationDbContext db) : ICommandHandler<UpdateRankingListCommand, bool>
{
    public async Task<bool> Handle(UpdateRankingListCommand command, CancellationToken ct)
    {
        var list = await db.RankingLists.FirstOrDefaultAsync(l => l.Id == command.ListId, ct)
            ?? throw new KeyNotFoundException("List not found.");

        list.UpdateDetails(command.Title);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
