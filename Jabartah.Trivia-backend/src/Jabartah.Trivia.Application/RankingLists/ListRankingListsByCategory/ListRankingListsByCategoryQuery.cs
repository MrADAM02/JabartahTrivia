using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.RankingLists.ListRankingListsByCategory;

public record ListRankingListsByCategoryQuery(Guid CategoryId) : IQuery<List<AdminRankingListDto>>;

public record AdminRankingListDto(Guid Id, string Title, int ItemCount);

public class ListRankingListsByCategoryHandler(IApplicationDbContext db) : IQueryHandler<ListRankingListsByCategoryQuery, List<AdminRankingListDto>>
{
    public async Task<List<AdminRankingListDto>> Handle(ListRankingListsByCategoryQuery query, CancellationToken ct) =>
        await db.RankingLists
            .Where(l => l.RankingCategoryId == query.CategoryId)
            .OrderBy(l => l.Title)
            .Select(l => new AdminRankingListDto(l.Id, l.Title, db.RankingListItems.Count(i => i.RankingListId == l.Id)))
            .ToListAsync(ct);
}
