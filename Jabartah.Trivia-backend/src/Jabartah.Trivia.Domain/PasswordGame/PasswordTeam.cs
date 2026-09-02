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

    public string? Color { get; private set; }   // hex string, e.g. "#EF4444"
    public string? Icon { get; private set; }    // lucide icon name, e.g. "i-lucide-trophy"

    private PasswordTeam() { } // EF Core

    public static PasswordTeam Create(Guid passwordGameSessionId, string name, int turnOrder, string? color = null, string? icon = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            PasswordGameSessionId = passwordGameSessionId,
            Name = name,
            Score = 0,
            TurnOrder = turnOrder,
            Color = color,
            Icon = icon
        };

    public void AddPoints(int points) => Score += points;
}
