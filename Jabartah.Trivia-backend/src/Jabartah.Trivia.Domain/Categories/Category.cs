namespace Jabartah.Trivia.Domain.Categories;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;   // Arabic display name, e.g. "رياضة"
    public string? Icon { get; private set; }               // emoji or icon key
    public Guid? OwnerUserId { get; private set; }           // null = seeded/global; non-null = a user's own تصنيفاتي category

    private Category() { } // EF Core

    public static Category Create(string name, string? icon = null, Guid? ownerUserId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("اسم الفئة مطلوب.", nameof(name));

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = icon,
            OwnerUserId = ownerUserId
        };
    }

    public void Rename(string name) => Name = name;

    public void UpdateDetails(string name, string? icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("اسم الفئة مطلوب.", nameof(name));

        Name = name;
        Icon = icon;
    }
}
