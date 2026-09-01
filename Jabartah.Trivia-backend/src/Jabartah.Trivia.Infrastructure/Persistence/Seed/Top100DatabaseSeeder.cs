using System.Text.Json;
using Jabartah.Trivia.Domain.Top100Game;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Infrastructure.Persistence.Seed;

public static class Top100DatabaseSeeder
{
    private record SeedTop100Item(string Label, List<string>? AlternateSpellings);
    private record SeedTop100List(string Title, List<SeedTop100Item> Items);
    private record SeedTop100Category(string CategoryName, string? Icon, List<SeedTop100List> Lists);

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Top100Categories.AnyAsync())
            return; // already seeded

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Persistence", "Seed", "top100.seed.json");
        var json = await File.ReadAllTextAsync(jsonPath);
        var seedCategories = JsonSerializer.Deserialize<List<SeedTop100Category>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        foreach (var sc in seedCategories)
        {
            var category = Top100Category.Create(sc.CategoryName, sc.Icon);
            db.Top100Categories.Add(category);

            foreach (var sl in sc.Lists)
            {
                var list = Top100List.Create(category.Id, sl.Title);
                db.Top100Lists.Add(list);

                // Items array order is most-obvious-first -- Position = index + 1 also doubles as the point value.
                for (var i = 0; i < sl.Items.Count; i++)
                    db.Top100ListItems.Add(Top100ListItem.Create(list.Id, sl.Items[i].Label, i + 1, sl.Items[i].AlternateSpellings));
            }
        }

        await db.SaveChangesAsync(default);
    }
}
