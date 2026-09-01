namespace Jabartah.Trivia.Domain.Top100Game;

public class Top100List
{
    public Guid Id { get; private set; }
    public Guid Top100CategoryId { get; private set; }
    public string Title { get; private set; } = default!;

    private Top100List() { } // EF Core

    public static Top100List Create(Guid categoryId, string title)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));

        return new Top100List
        {
            Id = Guid.NewGuid(),
            Top100CategoryId = categoryId,
            Title = title
        };
    }
}
