namespace Jabartah.Trivia.Domain.PasswordGame;

public class PasswordTeam
{
    public Guid Id { get; private set; }
    public Guid PasswordGameSessionId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Score { get; private set; }

    // EF Core doesn't guarantee a collection navigation reloads in insertion order, so the
    // turn-alternation logic in PasswordGameSession.StartNextRound cannot rely on _teams'
    // iteration order -- it needs this explicit, stable ordinal instead.
    public int TurnOrder { get; private set; }

    private PasswordTeam() { } // EF Core

    public static PasswordTeam Create(Guid passwordGameSessionId, string name, int turnOrder) =>
        new()
        {
            Id = Guid.NewGuid(),
            PasswordGameSessionId = passwordGameSessionId,
            Name = name,
            Score = 0,
            TurnOrder = turnOrder
        };

    public void AddPoints(int points) => Score += points;
}
