namespace Jabartah.Trivia.Domain.Top100Game;

public enum Top100GameSessionStatus
{
    Setup = 0,
    InProgress = 1,
    Completed = 2
}

public enum Top100RoundStatus
{
    Pending = 0,
    Completed = 1
}

// Tracks one played round: which list, whose turn it is, and which items have been claimed.
public class Top100Round
{
    public Guid Id { get; private set; }
    public Guid Top100GameSessionId { get; private set; }
    public int RoundNumber { get; private set; }
    public Guid Top100ListId { get; private set; }
    public Top100RoundStatus Status { get; private set; }
    public Guid CurrentTurnTeamId { get; private set; }
    public int GuessesMade { get; private set; }
    public int MaxGuesses { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private readonly List<Guid> _guessedItemIds = new();
    public IReadOnlyCollection<Guid> GuessedItemIds => _guessedItemIds.AsReadOnly();

    private Top100Round() { } // EF Core

    public static Top100Round Create(Guid sessionId, int roundNumber, Guid listId, Guid firstTurnTeamId, int itemCount) =>
        new()
        {
            Id = Guid.NewGuid(),
            Top100GameSessionId = sessionId,
            RoundNumber = roundNumber,
            Top100ListId = listId,
            Status = Top100RoundStatus.Pending,
            CurrentTurnTeamId = firstTurnTeamId,
            MaxGuesses = itemCount * 2,
            CreatedAt = DateTime.UtcNow
        };

    public (Guid? MatchedItemId, bool RoundComplete) RecordGuess(Guid? matchedItemId, Guid otherTeamId, int totalItemCount)
    {
        if (Status != Top100RoundStatus.Pending)
            throw new InvalidOperationException("This round is already complete.");

        GuessesMade++;

        if (matchedItemId is { } id)
        {
            if (_guessedItemIds.Contains(id))
                throw new InvalidOperationException("This item was already guessed.");
            _guessedItemIds.Add(id);
        }

        CurrentTurnTeamId = otherTeamId; // strict alternation, every guess, correct or not

        var complete = _guessedItemIds.Count == totalItemCount || GuessesMade >= MaxGuesses;
        if (complete)
        {
            Status = Top100RoundStatus.Completed;
            ResolvedAt = DateTime.UtcNow;
        }

        return (matchedItemId, complete);
    }
}

// Aggregate root: two teams alternate individual guesses at a themed ranked list;
// a correct guess scores points equal to the item's list position.
public class Top100GameSession
{
    public static readonly int[] AllowedRoundsPerTeam = [1, 2, 3];

    public Guid Id { get; private set; }
    public Top100GameSessionStatus Status { get; private set; }
    public int RoundsPerTeam { get; private set; }
    public Guid? UserId { get; private set; }   // owner, if created while logged in; null for guest play
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<Top100Team> _teams = new();
    public IReadOnlyCollection<Top100Team> Teams => _teams.AsReadOnly();

    private readonly List<Guid> _categoryIds = new();
    public IReadOnlyCollection<Guid> CategoryIds => _categoryIds.AsReadOnly();

    private readonly List<Top100Round> _rounds = new();
    public IReadOnlyCollection<Top100Round> Rounds => _rounds.AsReadOnly();

    public int MaxRounds => RoundsPerTeam * 2;

    private Top100GameSession() { } // EF Core

    public static Top100GameSession Create(IEnumerable<string> teamNames, IEnumerable<Guid> categoryIds, int roundsPerTeam)
    {
        var names = teamNames.ToList();
        var categories = categoryIds.ToList();

        if (names.Count != 2)
            throw new InvalidOperationException("تحدي الـ100 يتطلب فريقين بالضبط.");
        if (categories.Count == 0)
            throw new InvalidOperationException("الجلسة تحتاج فئة واحدة على الأقل.");
        if (!AllowedRoundsPerTeam.Contains(roundsPerTeam))
            throw new InvalidOperationException($"عدد الجولات لكل فريق يجب أن يكون أحد القيم التالية: {string.Join(", ", AllowedRoundsPerTeam)}.");

        var session = new Top100GameSession
        {
            Id = Guid.NewGuid(),
            Status = Top100GameSessionStatus.Setup,
            RoundsPerTeam = roundsPerTeam,
            CreatedAt = DateTime.UtcNow
        };

        for (var i = 0; i < names.Count; i++)
            session._teams.Add(Top100Team.Create(session.Id, names[i], i));

        session._categoryIds.AddRange(categories);
        return session;
    }

    public void Start()
    {
        if (Status != Top100GameSessionStatus.Setup)
            throw new InvalidOperationException("Only a session in Setup can be started.");
        Status = Top100GameSessionStatus.InProgress;
    }

    public void AttachOwner(Guid? userId) => UserId = userId;

    public Top100Round StartNextRound(Guid listId, int itemCount)
    {
        if (Status != Top100GameSessionStatus.InProgress)
            throw new InvalidOperationException("Session is not in progress.");
        if (_rounds.Count >= MaxRounds)
            throw new InvalidOperationException("All rounds have already been played.");
        if (_rounds.Any(r => r.Status == Top100RoundStatus.Pending))
            throw new InvalidOperationException("Complete the current round before starting the next one.");

        var roundNumber = _rounds.Count + 1;
        var turnOrder = (roundNumber - 1) % _teams.Count;
        var firstTeam = _teams.First(t => t.TurnOrder == turnOrder); // strict alternation, exactly 2 teams
        var round = Top100Round.Create(Id, roundNumber, listId, firstTeam.Id, itemCount);
        _rounds.Add(round);
        return round;
    }

    public (Guid GuessingTeamId, Guid? MatchedItemId, int PointsAwarded, bool RoundComplete) SubmitGuess(
        Guid roundId, Guid? matchedItemId, int pointsIfMatched, int totalItemCount)
    {
        var round = _rounds.FirstOrDefault(r => r.Id == roundId)
            ?? throw new InvalidOperationException("Round does not belong to this session.");
        var guessingTeamId = round.CurrentTurnTeamId; // capture BEFORE RecordGuess flips it
        var otherTeam = _teams.First(t => t.Id != guessingTeamId);

        var (matched, complete) = round.RecordGuess(matchedItemId, otherTeam.Id, totalItemCount);

        var points = 0;
        if (matched is not null)
        {
            points = pointsIfMatched;
            _teams.First(t => t.Id == guessingTeamId).AddPoints(points);
        }

        if (_rounds.Count == MaxRounds && _rounds.All(r => r.Status == Top100RoundStatus.Completed))
            Complete();

        return (guessingTeamId, matched, points, complete);
    }

    public void Complete()
    {
        Status = Top100GameSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
