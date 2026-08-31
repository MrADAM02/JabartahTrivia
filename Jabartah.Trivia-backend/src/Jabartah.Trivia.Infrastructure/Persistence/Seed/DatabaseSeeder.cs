using System.Text.Json;
using Jabartah.Trivia.Domain.Categories;
using Jabartah.Trivia.Domain.Questions;
using Microsoft.EntityFrameworkCore;

namespace Jabartah.Trivia.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    private record SeedQuestion(int Points, string Prompt, string Answer);
    private record SeedCategory(string Name, string? Icon, List<SeedQuestion> Questions);

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync())
            return; // already seeded

        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Persistence", "Seed", "categories.seed.json");
        var json = await File.ReadAllTextAsync(jsonPath);
        var seedCategories = JsonSerializer.Deserialize<List<SeedCategory>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];

        foreach (var sc in seedCategories)
        {
            var category = Category.Create(sc.Name, sc.Icon);
            db.Categories.Add(category);

            foreach (var sq in sc.Questions)
                db.Questions.Add(Question.Create(category.Id, sq.Points, sq.Prompt, sq.Answer));
        }

        await db.SaveChangesAsync(default);
    }
}
