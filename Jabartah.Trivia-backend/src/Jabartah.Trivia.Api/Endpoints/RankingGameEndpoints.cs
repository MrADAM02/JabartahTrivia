using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.RankingGame.CreateRankingGameSession;
using Jabartah.Trivia.Application.RankingGame.GetSession;
using Jabartah.Trivia.Application.RankingGame.RevealPosition;
using Jabartah.Trivia.Application.RankingGame.StartNextRound;
using Jabartah.Trivia.Application.RankingGame.SubmitRound;

namespace Jabartah.Trivia.Api.Endpoints;

public static class RankingGameEndpoints
{
    public static void MapRankingGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ranking-sessions").WithTags("RankingGame");

        group.MapPost("/", async (CreateRankingGameSessionCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(command, ct)));

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetRankingSessionQuery(id), ct)));

        group.MapPost("/{id:guid}/rounds/next", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new StartNextRankingRoundCommand(id), ct)));

        group.MapPost("/{id:guid}/rounds/{roundId:guid}/submit", async (
                Guid id, Guid roundId, SubmitRoundRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new SubmitRankingRoundCommand(id, roundId, body.OrderedItemIds), ct)));

        group.MapPost("/{id:guid}/rounds/{roundId:guid}/reveal-position", async (
                Guid id, Guid roundId, RevealPositionRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new RevealRankingPositionCommand(id, roundId, body.TeamId), ct)));
    }

    public record RevealPositionRequest(Guid TeamId);

    public record SubmitRoundRequest(List<Guid> OrderedItemIds);
}
