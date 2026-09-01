using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListTop100Categories;

public record ListTop100CategoriesQuery : IQuery<List<Top100CategoryDto>>;

public record Top100CategoryDto(Guid Id, string Name, string? Icon);

public class ListTop100CategoriesHandler(IApplicationDbContext db) : IQueryHandler<ListTop100CategoriesQuery, List<Top100CategoryDto>>
{
    public async Task<List<Top100CategoryDto>> Handle(ListTop100CategoriesQuery query, CancellationToken ct) =>
        await db.Top100Categories
            .OrderBy(c => c.Name)
            .Select(c => new Top100CategoryDto(c.Id, c.Name, c.Icon))
            .ToListAsync(ct);
}
