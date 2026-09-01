namespace Jabartah.Trivia.Domain.Top100Game;

public class Top100Team
{
    public Guid Id { get; private set; }
    public Guid Top100GameSessionId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Score { get; private set; }

    // EF Core doesn't guarantee a collection navigation reloads in insertion order, so the
    // turn-alternation logic in Top100GameSession.StartNextRound cannot rely on _teams'
    // iteration order -- it needs this explicit, stable ordinal instead.
    public int TurnOrder { get; private set; }

    private Top100Team() { } // EF Core

    public static Top100Team Create(Guid top100GameSessionId, string name, int turnOrder) =>
        new()
        {
            Id = Guid.NewGuid(),
            Top100GameSessionId = top100GameSessionId,
            Name = name,
            Score = 0,
            TurnOrder = turnOrder
        };

    public void AddPoints(int points) => Score += points;
}
