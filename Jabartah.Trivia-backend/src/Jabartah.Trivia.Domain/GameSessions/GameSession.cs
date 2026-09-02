using Jabartah.Trivia.Domain.Teams;

namespace Jabartah.Trivia.Domain.GameSessions;

public enum GameSessionStatus
{
    Setup = 0,
    InProgress = 1,
    Completed = 2
}

public enum PowerUpType
{
    DoublePoints = 0,
    TwoAnswers = 1
}

// Tracks which questions have been played in this session, and who won them.
public class GameQuestionState
{
    public Guid Id { get; private set; }
    public Guid GameSessionId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid? WonByTeamId { get; private set; }
    public DateTime RevealedAt { get; private set; }
    public Guid? PowerUpTeamId { get; private set; }
    public PowerUpType? ActivePowerUp { get; private set; }
    public bool AttemptFailed { get; private set; }
    public bool IsResolved { get; private set; }

    private GameQuestionState() { } // EF Core

    public static GameQuestionState Create(Guid gameSessionId, Guid questionId, Guid? powerUpTeamId, PowerUpType? activePowerUp) =>
        new()
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            QuestionId = questionId,
            RevealedAt = DateTime.UtcNow,
            PowerUpTeamId = powerUpTeamId,
            ActivePowerUp = activePowerUp
        };

    // Returns true if a retry is now allowed (question stays unresolved).
    public bool RecordAttempt(Guid? winningTeamId)
    {
        if (IsResolved)
            throw new InvalidOperationException("This question was already resolved.");

        if (winningTeamId is not null)
        {
            WonByTeamId = winningTeamId;
            IsResolved = true;
            return false;
        }

        if (ActivePowerUp == PowerUpType.TwoAnswers && !AttemptFailed)
        {
            AttemptFailed = true;
            return true;
        }

        IsResolved = true;
        return false;
    }

    public bool AwardsDoublePoints(Guid teamId) => ActivePowerUp == PowerUpType.DoublePoints && PowerUpTeamId == teamId;
}

// Aggregate root: owns Teams and QuestionStates for consistency (e.g. can't award
// points to a team that isn't in this session, can't reveal the same question twice).
public class GameSession
{
    public Guid Id { get; private set; }
    public GameSessionStatus Status { get; private set; }
    public Guid? UserId { get; private set; }   // owner, if created while logged in; null for guest play
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
        if (categories.Count != 6)
            throw new InvalidOperationException("A trivia session needs exactly 6 categories.");

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

    public void AttachOwner(Guid? userId) => UserId = userId;

    public GameQuestionState RevealQuestion(Guid questionId, Guid? activatingTeamId = null, PowerUpType? powerUp = null)
    {
        if (Status != GameSessionStatus.InProgress)
            throw new InvalidOperationException("Session is not in progress.");
        if (_questionStates.Any(q => q.QuestionId == questionId))
            throw new InvalidOperationException("This question was already used in this session.");
        if ((activatingTeamId is null) != (powerUp is null))
            throw new InvalidOperationException("A power-up requires an activating team, and vice versa.");

        if (powerUp is not null)
        {
            var team = _teams.FirstOrDefault(t => t.Id == activatingTeamId)
                ?? throw new InvalidOperationException("Team does not belong to this session.");
            if (powerUp == PowerUpType.DoublePoints) team.UseDoublePoints();
            else team.UseTwoAnswers();
        }

        var state = GameQuestionState.Create(Id, questionId, activatingTeamId, powerUp);
        _questionStates.Add(state);
        return state;
    }

    // Returns the retry team id if a retry is now pending, else null (question fully resolved).
    public Guid? AwardPoints(Guid questionId, Guid? winningTeamId, int points)
    {
        var state = _questionStates.FirstOrDefault(q => q.QuestionId == questionId)
            ?? throw new InvalidOperationException("Question was not revealed in this session yet.");

        var canRetry = state.RecordAttempt(winningTeamId);

        if (winningTeamId is { } teamId)
        {
            var team = _teams.FirstOrDefault(t => t.Id == teamId)
                ?? throw new InvalidOperationException("Team does not belong to this session.");
            team.AddPoints(state.AwardsDoublePoints(teamId) ? points * 2 : points);
        }

        return canRetry ? state.PowerUpTeamId : null;
    }

    public void Complete()
    {
        Status = GameSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
