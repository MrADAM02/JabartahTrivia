namespace Jabartah.Trivia.Domain.Teams;

public class Team
{
    public Guid Id { get; private set; }
    public Guid GameSessionId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Score { get; private set; }

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
}
