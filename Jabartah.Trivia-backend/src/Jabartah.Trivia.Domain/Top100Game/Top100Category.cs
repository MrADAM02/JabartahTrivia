namespace Jabartah.Trivia.Domain.Top100Game;

public class Top100Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Icon { get; private set; }
    public string? Description { get; private set; }

    private Top100Category() { } // EF Core

    public static Top100Category Create(string name, string? icon = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("اسم الفئة مطلوب.", nameof(name));

        return new Top100Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = icon,
            Description = description
        };
    }

    public void Rename(string name) => Name = name;

    public void UpdateDetails(string name, string? icon, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("اسم الفئة مطلوب.", nameof(name));

        Name = name;
        Icon = icon;
        Description = description;
    }
}
