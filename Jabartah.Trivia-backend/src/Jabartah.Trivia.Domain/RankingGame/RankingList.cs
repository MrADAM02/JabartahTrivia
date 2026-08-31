namespace Jabartah.Trivia.Domain.RankingGame;

public class RankingList
{
    public Guid Id { get; private set; }
    public Guid RankingCategoryId { get; private set; }
    public string Title { get; private set; } = default!; // e.g. "رتب هذه الأحداث حسب التسلسل الزمني"

    private RankingList() { } // EF Core

    public static RankingList Create(Guid categoryId, string title)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));

        return new RankingList
        {
            Id = Guid.NewGuid(),
            RankingCategoryId = categoryId,
            Title = title
        };
    }
}
