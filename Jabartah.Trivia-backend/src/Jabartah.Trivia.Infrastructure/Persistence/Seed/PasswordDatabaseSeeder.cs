using System.Text.Json;
using Jabartah.Trivia.Domain.PasswordGame;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Infrastructure.Persistence.Seed;

public static class PasswordDatabaseSeeder
{
    private record SeedPasswordCategory(string Name, string? Icon, List<string> Words);

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.PasswordCategories.AnyAsync())
            return; // already seeded

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Persistence", "Seed", "password.seed.json");
        var json = await File.ReadAllTextAsync(jsonPath);
        var seedCategories = JsonSerializer.Deserialize<List<SeedPasswordCategory>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        foreach (var sc in seedCategories)
        {
            var category = PasswordCategory.Create(sc.Name, sc.Icon);
            db.PasswordCategories.Add(category);

            foreach (var word in sc.Words)
                db.PasswordWords.Add(PasswordWord.Create(category.Id, word));
        }

        await db.SaveChangesAsync(default);
    }
}
