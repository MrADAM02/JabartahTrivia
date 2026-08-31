namespace Jabartah.Trivia.Domain.RankingGame;

public class RankingListItem
{
    public Guid Id { get; private set; }
    public Guid RankingListId { get; private set; }
    public string Label { get; private set; } = default!;
    public int CorrectPosition { get; private set; } // 1-based

    private RankingListItem() { } // EF Core

    public static RankingListItem Create(Guid listId, string label, int correctPosition)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Label is required.", nameof(label));
        if (correctPosition <= 0) throw new ArgumentException("Correct position must be positive.", nameof(correctPosition));

        return new RankingListItem
        {
            Id = Guid.NewGuid(),
            RankingListId = listId,
            Label = label,
            CorrectPosition = correctPosition
        };
    }
}
