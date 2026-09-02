using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.ListCategories;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListMyCategories;

public record ListMyCategoriesQuery(Guid UserId) : IQuery<List<CategoryDto>>;

public class ListMyCategoriesHandler(IApplicationDbContext db) : IQueryHandler<ListMyCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(ListMyCategoriesQuery query, CancellationToken ct) =>
        await db.Categories
            .Where(c => c.OwnerUserId == query.UserId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Icon))
            .ToListAsync(ct);
}
