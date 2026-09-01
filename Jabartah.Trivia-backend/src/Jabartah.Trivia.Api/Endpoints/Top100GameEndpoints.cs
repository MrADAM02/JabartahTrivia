using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Top100Game.CreateTop100GameSession;
using Jabartah.Trivia.Application.Top100Game.GetSession;
using Jabartah.Trivia.Application.Top100Game.StartNextRound;
using Jabartah.Trivia.Application.Top100Game.SubmitGuess;

namespace Jabartah.Trivia.Api.Endpoints;

public static class Top100GameEndpoints
{
    public static void MapTop100GameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/top100-sessions").WithTags("Top100Game");

        group.MapPost("/", async (CreateTop100GameSessionCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(command, ct)));

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetTop100SessionQuery(id), ct)));

        group.MapPost("/{id:guid}/rounds/next", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new StartNextTop100RoundCommand(id), ct)));

        group.MapPost("/{id:guid}/rounds/{roundId:guid}/guess", async (
                Guid id, Guid roundId, SubmitGuessRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new SubmitGuessCommand(id, roundId, body.GuessText), ct)));
    }

    public record SubmitGuessRequest(string GuessText);
}
