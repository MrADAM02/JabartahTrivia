namespace Jabartah.Trivia.Domain.RankingGame;

public class RankingCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Icon { get; private set; }

    private RankingCategory() { } // EF Core

    public static RankingCategory Create(string name, string? icon = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = icon
        };

    public void Rename(string name) => Name = name;
}
