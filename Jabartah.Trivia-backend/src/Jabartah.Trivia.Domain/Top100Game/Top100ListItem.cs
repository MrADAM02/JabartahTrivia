namespace Jabartah.Trivia.Domain.Top100Game;

public class Top100ListItem
{
    public Guid Id { get; private set; }
    public Guid Top100ListId { get; private set; }
    public string Label { get; private set; } = default!;
    public int Position { get; private set; } // 1-based; also the point value

    private readonly List<string> _alternateSpellings = new();
    public IReadOnlyCollection<string> AlternateSpellings => _alternateSpellings.AsReadOnly();

    private Top100ListItem() { } // EF Core

    public static Top100ListItem Create(Guid listId, string label, int position, IEnumerable<string>? alternateSpellings = null)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Label is required.", nameof(label));
        if (position <= 0) throw new ArgumentException("Position must be positive.", nameof(position));

        var item = new Top100ListItem
        {
            Id = Guid.NewGuid(),
            Top100ListId = listId,
            Label = label,
            Position = position
        };

        if (alternateSpellings is not null)
            item._alternateSpellings.AddRange(alternateSpellings);

        return item;
    }
}
