namespace Jabartah.Trivia.Domain.Categories;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;   // Arabic display name, e.g. "رياضة"
    public string? Icon { get; private set; }               // emoji or icon key

    private Category() { } // EF Core

    public static Category Create(string name, string? icon = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = icon
        };

    public void Rename(string name) => Name = name;
}
