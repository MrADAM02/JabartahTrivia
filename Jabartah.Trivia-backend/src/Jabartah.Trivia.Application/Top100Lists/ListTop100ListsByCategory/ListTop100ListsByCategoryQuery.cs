using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Lists.ListTop100ListsByCategory;

public record ListTop100ListsByCategoryQuery(Guid CategoryId) : IQuery<List<AdminTop100ListDto>>;

public record AdminTop100ListDto(Guid Id, string Title, int ItemCount);

public class ListTop100ListsByCategoryHandler(IApplicationDbContext db) : IQueryHandler<ListTop100ListsByCategoryQuery, List<AdminTop100ListDto>>
{
    public async Task<List<AdminTop100ListDto>> Handle(ListTop100ListsByCategoryQuery query, CancellationToken ct) =>
        await db.Top100Lists
            .Where(l => l.Top100CategoryId == query.CategoryId)
            .OrderBy(l => l.Title)
            .Select(l => new AdminTop100ListDto(l.Id, l.Title, db.Top100ListItems.Count(i => i.Top100ListId == l.Id)))
            .ToListAsync(ct);
}
