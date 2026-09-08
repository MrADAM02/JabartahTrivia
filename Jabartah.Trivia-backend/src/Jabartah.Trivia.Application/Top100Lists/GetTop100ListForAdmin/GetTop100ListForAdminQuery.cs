using Jabartah.Trivia.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Application.Top100Lists.GetTop100ListForAdmin;

public record GetTop100ListForAdminQuery(Guid ListId) : IQuery<AdminTop100ListDetailDto>;

public record AdminTop100ListItemDto(Guid Id, string Label, int Position, List<string> AlternateSpellings);
public record AdminTop100ListDetailDto(Guid Id, string Title, Guid CategoryId, List<AdminTop100ListItemDto> Items);

public class GetTop100ListForAdminHandler(IApplicationDbContext db) : IQueryHandler<GetTop100ListForAdminQuery, AdminTop100ListDetailDto>
{
    public async Task<AdminTop100ListDetailDto> Handle(GetTop100ListForAdminQuery query, CancellationToken ct)
    {
        var list = await db.Top100Lists.FirstOrDefaultAsync(l => l.Id == query.ListId, ct)
            ?? throw new KeyNotFoundException("List not found.");

        // Mapped in-memory after materializing (rather than inside .Select()) since
        // AlternateSpellings is a primitive-collection (array) column -- ToList() on it
        // doesn't reliably translate inside a server-side projection.
        var itemEntities = await db.Top100ListItems
            .Where(i => i.Top100ListId == query.ListId)
            .OrderBy(i => i.Position)
            .ToListAsync(ct);
        var items = itemEntities
            .Select(i => new AdminTop100ListItemDto(i.Id, i.Label, i.Position, i.AlternateSpellings.ToList()))
            .ToList();

        return new AdminTop100ListDetailDto(list.Id, list.Title, list.Top100CategoryId, items);
    }
}
