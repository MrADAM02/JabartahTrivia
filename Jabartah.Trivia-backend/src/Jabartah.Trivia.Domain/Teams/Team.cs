namespace Jabartah.Trivia.Domain.Teams;

public class Team
{
    public Guid Id { get; private set; }
    public Guid GameSessionId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Score { get; private set; }
    public bool DoublePointsAvailable { get; private set; } = true;
    public bool TwoAnswersAvailable { get; private set; } = true;
    public string? Color { get; private set; }   // hex string, e.g. "#EF4444"
    public string? Icon { get; private set; }    // lucide icon name, e.g. "i-lucide-trophy"

    private Team() { } // EF Core

    public static Team Create(Guid gameSessionId, string name, string? color = null, string? icon = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            Name = name,
            Score = 0,
            Color = color,
            Icon = icon
        };

    public void AddPoints(int points) => Score += points;

    public void UseDoublePoints()
    {
        if (!DoublePointsAvailable) throw new InvalidOperationException("Double points was already used by this team.");
        DoublePointsAvailable = false;
    }

    public void UseTwoAnswers()
    {
        if (!TwoAnswersAvailable) throw new InvalidOperationException("Two answers was already used by this team.");
        TwoAnswersAvailable = false;
    }
}
