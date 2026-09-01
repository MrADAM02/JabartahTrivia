namespace Jabartah.Trivia.Domain.Teams;

public class Team
{
    public Guid Id { get; private set; }
    public Guid GameSessionId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Score { get; private set; }
    public bool DoublePointsAvailable { get; private set; } = true;
    public bool TwoAnswersAvailable { get; private set; } = true;

    private Team() { } // EF Core

    public static Team Create(Guid gameSessionId, string name) =>
        new()
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            Name = name,
            Score = 0
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
