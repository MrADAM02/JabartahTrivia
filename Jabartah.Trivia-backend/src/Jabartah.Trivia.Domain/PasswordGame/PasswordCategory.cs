namespace Jabartah.Trivia.Domain.PasswordGame;

public class PasswordCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Icon { get; private set; }

    private PasswordCategory() { } // EF Core

    public static PasswordCategory Create(string name, string? icon = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = icon
        };

    public void Rename(string name) => Name = name;

    public void UpdateDetails(string name, string? icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("اسم الفئة مطلوب.", nameof(name));

        Name = name;
        Icon = icon;
    }
}
