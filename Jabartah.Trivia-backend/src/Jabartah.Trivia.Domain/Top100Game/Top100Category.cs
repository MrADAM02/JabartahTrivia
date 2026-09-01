namespace Jabartah.Trivia.Domain.Top100Game;

public class Top100Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Icon { get; private set; }

    private Top100Category() { } // EF Core

    public static Top100Category Create(string name, string? icon = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = icon
        };

    public void Rename(string name) => Name = name;
}
