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

    public string? Color { get; private set; }   // hex string, e.g. "#EF4444"
    public string? Icon { get; private set; }    // lucide icon name, e.g. "i-lucide-trophy"
    public bool RevealPositionAvailable { get; private set; } = true;

    private RankingTeam() { } // EF Core

    public static RankingTeam Create(Guid rankingGameSessionId, string name, int turnOrder, string? color = null, string? icon = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            RankingGameSessionId = rankingGameSessionId,
            Name = name,
            Score = 0,
            TurnOrder = turnOrder,
            Color = color,
            Icon = icon
        };

    public void AddPoints(int points) => Score += points;

    public void UseRevealPosition()
    {
        if (!RevealPositionAvailable) throw new InvalidOperationException("Reveal position was already used by this team.");
        RevealPositionAvailable = false;
    }
}
