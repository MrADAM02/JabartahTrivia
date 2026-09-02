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

// One guess attempt, correct or not. Kept forever (not just successful ones) so the
// session can show each team's full chronological history -- both the discovered-items
// list and the shared "mistakes pile" are just filtered views over this log.
public class Top100Guess
{
    public Guid Id { get; private set; }
    public Guid Top100RoundId { get; private set; }
    public int SequenceNumber { get; private set; }   // 1-based order guessed, across both teams
    public Guid TeamId { get; private set; }
    public string GuessText { get; private set; } = default!;
    public Guid? MatchedItemId { get; private set; }   // null = wrong guess

    private Top100Guess() { } // EF Core

    public static Top100Guess Create(Guid roundId, int sequenceNumber, Guid teamId, string guessText, Guid? matchedItemId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Top100RoundId = roundId,
            SequenceNumber = sequenceNumber,
            TeamId = teamId,
            GuessText = guessText,
            MatchedItemId = matchedItemId
        };
}

// Tracks the single round played per session: which list, whose turn it is, and the full
// guess log. MaxGuesses is a session-level setting (GuessesPerTeam * 2) -- deliberately
// decoupled from how many items the list actually has, since a session now plays exactly
// one (potentially large) list rather than several small ones.
public class Top100Round
{
    public Guid Id { get; private set; }
    public Guid Top100GameSessionId { get; private set; }
    public Guid Top100ListId { get; private set; }
    public Top100RoundStatus Status { get; private set; }
    public Guid CurrentTurnTeamId { get; private set; }
    public int GuessesMade { get; private set; }
    public int MaxGuesses { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private readonly List<Top100Guess> _guesses = new();
    public IReadOnlyCollection<Top100Guess> Guesses => _guesses.AsReadOnly();

    private Top100Round() { } // EF Core

    public static Top100Round Create(Guid sessionId, Guid listId, Guid firstTurnTeamId, int maxGuesses) =>
        new()
        {
            Id = Guid.NewGuid(),
            Top100GameSessionId = sessionId,
            Top100ListId = listId,
            Status = Top100RoundStatus.Pending,
            CurrentTurnTeamId = firstTurnTeamId,
            MaxGuesses = maxGuesses,
            CreatedAt = DateTime.UtcNow
        };

    public (Top100Guess Guess, bool RoundComplete) RecordGuess(Guid guessingTeamId, string guessText, Guid? matchedItemId, Guid otherTeamId, int totalItemCount)
    {
        if (Status != Top100RoundStatus.Pending)
            throw new InvalidOperationException("This round is already complete.");

        GuessesMade++;
        var guess = Top100Guess.Create(Id, GuessesMade, guessingTeamId, guessText, matchedItemId);
        _guesses.Add(guess);

        CurrentTurnTeamId = otherTeamId; // strict alternation, every guess, correct or not

        var claimedCount = _guesses.Count(g => g.MatchedItemId is not null);
        var complete = claimedCount == totalItemCount || GuessesMade >= MaxGuesses;
        if (complete)
        {
            Status = Top100RoundStatus.Completed;
            ResolvedAt = DateTime.UtcNow;
        }

        return (guess, complete);
    }
}

// Aggregate root: two teams alternate individual guesses at a themed ranked list, each
// getting GuessesPerTeam attempts; a correct guess scores points equal to the item's list
// position. Exactly one round is ever played per session -- "rounds" used to mean "how many
// separate lists to play", but now GuessesPerTeam directly controls how many attempts each
// team gets against the one list, so there's nothing left for a second round to do.
public class Top100GameSession
{
    public static readonly int[] AllowedGuessesPerTeam = [3, 4, 5, 6, 7, 8, 9, 10];

    public Guid Id { get; private set; }
    public Top100GameSessionStatus Status { get; private set; }
    public int GuessesPerTeam { get; private set; }
    public Guid? UserId { get; private set; }   // owner, if created while logged in; null for guest play
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<Top100Team> _teams = new();
    public IReadOnlyCollection<Top100Team> Teams => _teams.AsReadOnly();

    private readonly List<Guid> _categoryIds = new();
    public IReadOnlyCollection<Guid> CategoryIds => _categoryIds.AsReadOnly();

    private readonly List<Top100Round> _rounds = new(); // 0 or 1 entries, always
    public IReadOnlyCollection<Top100Round> Rounds => _rounds.AsReadOnly();

    private Top100GameSession() { } // EF Core

    public static Top100GameSession Create(IEnumerable<(string Name, string? Color, string? Icon)> teams, IEnumerable<Guid> categoryIds, int guessesPerTeam)
    {
        var teamsList = teams.ToList();
        var categories = categoryIds.ToList();

        if (teamsList.Count != 2)
            throw new InvalidOperationException("تحدي الـ100 يتطلب فريقين بالضبط.");
        if (categories.Count == 0)
            throw new InvalidOperationException("الجلسة تحتاج فئة واحدة على الأقل.");
        if (!AllowedGuessesPerTeam.Contains(guessesPerTeam))
            throw new InvalidOperationException($"عدد الإجابات لكل فريق يجب أن يكون بين {AllowedGuessesPerTeam.Min()} و {AllowedGuessesPerTeam.Max()}.");

        var session = new Top100GameSession
        {
            Id = Guid.NewGuid(),
            Status = Top100GameSessionStatus.Setup,
            GuessesPerTeam = guessesPerTeam,
            CreatedAt = DateTime.UtcNow
        };

        for (var i = 0; i < teamsList.Count; i++)
            session._teams.Add(Top100Team.Create(session.Id, teamsList[i].Name, i, teamsList[i].Color, teamsList[i].Icon));

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

    public Top100Round StartRound(Guid listId)
    {
        if (Status != Top100GameSessionStatus.InProgress)
            throw new InvalidOperationException("Session is not in progress.");
        if (_rounds.Count > 0)
            throw new InvalidOperationException("This session's round has already started.");

        var firstTeam = _teams.First(t => t.TurnOrder == 0);
        var round = Top100Round.Create(Id, listId, firstTeam.Id, GuessesPerTeam * 2);
        _rounds.Add(round);
        return round;
    }

    public (Guid GuessingTeamId, Guid? MatchedItemId, int PointsAwarded, bool SessionComplete) SubmitGuess(
        Guid roundId, string guessText, Guid? matchedItemId, int pointsIfMatched, int totalItemCount)
    {
        var round = _rounds.FirstOrDefault(r => r.Id == roundId)
            ?? throw new InvalidOperationException("Round does not belong to this session.");
        var guessingTeamId = round.CurrentTurnTeamId; // capture BEFORE RecordGuess flips it
        var otherTeam = _teams.First(t => t.Id != guessingTeamId);

        var (guess, complete) = round.RecordGuess(guessingTeamId, guessText, matchedItemId, otherTeam.Id, totalItemCount);

        var points = 0;
        if (guess.MatchedItemId is not null)
        {
            points = pointsIfMatched;
            _teams.First(t => t.Id == guessingTeamId).AddPoints(points);
        }

        if (complete)
            Complete(); // the session's only round just finished -- the session is done too

        return (guessingTeamId, guess.MatchedItemId, points, complete);
    }

    public void Complete()
    {
        Status = Top100GameSessionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
