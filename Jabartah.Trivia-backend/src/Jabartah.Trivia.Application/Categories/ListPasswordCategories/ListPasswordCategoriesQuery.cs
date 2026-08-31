using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Categories.ListPasswordCategories;

public record ListPasswordCategoriesQuery : IQuery<List<PasswordCategoryDto>>;

public record PasswordCategoryDto(Guid Id, string Name, string? Icon);

public class ListPasswordCategoriesHandler(IApplicationDbContext db) : IQueryHandler<ListPasswordCategoriesQuery, List<PasswordCategoryDto>>
{
    public async Task<List<PasswordCategoryDto>> Handle(ListPasswordCategoriesQuery query, CancellationToken ct) =>
        await db.PasswordCategories
            .OrderBy(c => c.Name)
            .Select(c => new PasswordCategoryDto(c.Id, c.Name, c.Icon))
            .ToListAsync(ct);
}
