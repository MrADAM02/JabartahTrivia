namespace Jabartah.Trivia.Application.Sessions.GetMySessions;

// Backend port of the same draw-aware logic as the frontend's useWinner.ts composable,
// scoped to this one reporting query -- live gameplay endpoints never compute a winner.
public static class WinnerCalculator
{
    public static (List<string> WinnerNames, bool IsDraw) Calculate(List<MyTeamDto> teams)
    {
        if (teams.Count == 0) return ([], false);

        var topScore = teams.Max(t => t.Score);
        var winners = teams.Where(t => t.Score == topScore).Select(t => t.Name).ToList();
        return (winners, winners.Count > 1);
    }
}
