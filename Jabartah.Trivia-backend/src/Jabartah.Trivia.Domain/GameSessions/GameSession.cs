using Jabartah.Trivia.Domain.Teams;

namespace Jabartah.Trivia.Domain.GameSessions;

public enum GameSessionStatus
{
    Setup = 0,
    InProgress = 1,
    Completed = 2
}

// Tracks which questions have been played in this session, and who won them.
public class GameQuestionState
{
    public Guid Id { get; private set; }
    public Guid GameSessionId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid? WonByTeamId { get; private set; }
    public DateTime RevealedAt { get; private set; }

    private GameQuestionState() { } // EF Core

    public static GameQuestionState Create(Guid gameSessionId, Guid questionId) =>
        new()
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            QuestionId = questionId,
            RevealedAt = DateTime.UtcNow
        };

    public void AwardTo(Guid? teamId) => WonByTeamId = teamId;
}

// Aggregate root: owns Teams and QuestionStates for consistency (e.g. can't award
// points to a team that isn't in this session, can't reveal the same question twice).
public class GameSession
{
    public Guid Id { get; private set; }
    public GameSessionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<Team> _teams = new();
    public IReadOnlyCollection<Team> Teams => _teams.AsReadOnly();

    private readonly List<Guid> _categoryIds = new();
    public IReadOnlyCollection<Guid> CategoryIds => _categoryIds.AsReadOnly();

    private readonly List<GameQuestionState> _questionStates = new();
    public IReadOnlyCollection<GameQuestionState> QuestionStates => _questionStates.AsReadOnly();

    private GameSession() { } // EF Core

    public static GameSession Create(IEnumerable<string> teamNames, IEnumerable<Guid> categoryIds)
    {
        var names = teamNames.ToList();
        var categories = categoryIds.ToList();

        if (names.Count < 2)
            throw new InvalidOperationException("A game session needs at least 2 teams.");
        if (categories.Count == 0)
            throw new InvalidOperationException("A game session needs at least 1 category.");

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = GameSessionStatus.Setup,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var name in names)
            session._teams.Add(Team.Create(session.Id, name));

        session._categoryIds.AddRange(categories);
        return session;
    }

    public void Start()
    {
        if (Status != GameSessionStatus.Setup)
            throw new InvalidOperationException("Only a session in Setup can be started.");
        Status = GameSessionStatus.InProgress;
    }

    public GameQuestionState RevealQuestion(Guid questionId)
    {
        if (Status != GameSessionStatus.InProgress)
            throw new InvalidOperationException("Session is not in progress.");
        if (_questionStates.Any(q => q.QuestionId == questionId))
            throw new InvalidOperationException("This question was already used in this session.");

        var state = GameQuestionState.Create(Id, questionId);
        _questionStates.Add(state);
        return state;
    }

    public void AwardPoints(Guid questionId, Guid? winningTeamId, int points)
    {
        var state = _questionStates.FirstOrDefault(q => q.QuestionId == questionId)
            ?? throw new InvalidOperationException("Question was not revealed in this session yet.");

        state.AwardTo(winningTeamId);

        if (winningTeamId is { } teamId)
        {
            var team = _teams.FirstOrDefault(t => t.Id == teamId)
                ?? throw new InvalidOperationException("Team does not belong to this session.");
            team.AddPoints(points);
        }
    }

    public void Complete()
    {
        Status = GameSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
