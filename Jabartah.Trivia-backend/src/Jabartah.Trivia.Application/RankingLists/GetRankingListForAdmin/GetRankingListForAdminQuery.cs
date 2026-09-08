using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingLists.GetRankingListForAdmin;

public record GetRankingListForAdminQuery(Guid ListId) : IQuery<AdminRankingListDetailDto>;

public record AdminRankingListItemDto(Guid Id, string Label, int CorrectPosition);
public record AdminRankingListDetailDto(Guid Id, string Title, Guid CategoryId, List<AdminRankingListItemDto> Items);

public class GetRankingListForAdminHandler(IApplicationDbContext db) : IQueryHandler<GetRankingListForAdminQuery, AdminRankingListDetailDto>
{
    public async Task<AdminRankingListDetailDto> Handle(GetRankingListForAdminQuery query, CancellationToken ct)
    {
        var list = await db.RankingLists.FirstOrDefaultAsync(l => l.Id == query.ListId, ct)
            ?? throw new KeyNotFoundException("List not found.");

        var items = await db.RankingListItems
            .Where(i => i.RankingListId == query.ListId)
            .OrderBy(i => i.CorrectPosition)
            .Select(i => new AdminRankingListItemDto(i.Id, i.Label, i.CorrectPosition))
            .ToListAsync(ct);

        return new AdminRankingListDetailDto(list.Id, list.Title, list.RankingCategoryId, items);
    }
}
