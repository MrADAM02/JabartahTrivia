using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListCategories;

public record ListCategoriesQuery : IQuery<List<CategoryDto>>;

public record CategoryDto(Guid Id, string Name, string? Icon);

public class ListCategoriesHandler(IApplicationDbContext db) : IQueryHandler<ListCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(ListCategoriesQuery query, CancellationToken ct) =>
        await db.Categories
            .Where(c => c.OwnerUserId == null) // never leak other users' تصنيفاتي categories into the shared picker
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Icon))
            .ToListAsync(ct);
}
