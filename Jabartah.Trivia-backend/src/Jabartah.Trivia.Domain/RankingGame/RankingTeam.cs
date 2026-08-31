namespace Jabartah.Trivia.Domain.RankingGame;

public class RankingTeam
{
    public Guid Id { get; private set; }
    public Guid RankingGameSessionId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Score { get; private set; }

    // EF Core doesn't guarantee a collection navigation reloads in insertion order, so the
    // turn-alternation logic in RankingGameSession.StartNextRound cannot rely on _teams'
    // iteration order -- it needs this explicit, stable ordinal instead.
    public int TurnOrder { get; private set; }

    private RankingTeam() { } // EF Core

    public static RankingTeam Create(Guid rankingGameSessionId, string name, int turnOrder) =>
        new()
        {
            Id = Guid.NewGuid(),
            RankingGameSessionId = rankingGameSessionId,
            Name = name,
            Score = 0,
            TurnOrder = turnOrder
        };

    public void AddPoints(int points) => Score += points;
}
