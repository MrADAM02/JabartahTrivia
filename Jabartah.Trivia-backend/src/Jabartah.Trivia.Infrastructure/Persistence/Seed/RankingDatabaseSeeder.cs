using System.Text.Json;
using Jabartah.Trivia.Domain.RankingGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Infrastructure.Persistence.Seed;

public static class RankingDatabaseSeeder
{
    private record SeedRankingList(string Title, List<string> Items);
    private record SeedRankingCategory(string CategoryName, string? Icon, List<SeedRankingList> Lists);

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.RankingCategories.AnyAsync())
            return; // already seeded

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Persistence", "Seed", "ranking.seed.json");
        var json = await File.ReadAllTextAsync(jsonPath);
        var seedCategories = JsonSerializer.Deserialize<List<SeedRankingCategory>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        foreach (var sc in seedCategories)
        {
            var category = RankingCategory.Create(sc.CategoryName, sc.Icon);
            db.RankingCategories.Add(category);

            foreach (var sl in sc.Lists)
            {
                var list = RankingList.Create(category.Id, sl.Title);
                db.RankingLists.Add(list);

                // Items array order IS the correct order -- CorrectPosition = index + 1.
                for (var i = 0; i < sl.Items.Count; i++)
                    db.RankingListItems.Add(RankingListItem.Create(list.Id, sl.Items[i], i + 1));
            }
        }

        await db.SaveChangesAsync(default);
    }
}
