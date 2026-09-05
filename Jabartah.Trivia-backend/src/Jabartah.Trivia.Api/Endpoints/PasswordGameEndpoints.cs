using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.PasswordGame.CreatePasswordGameSession;
using Jabartah.Trivia.Application.PasswordGame.EndGame;
using Jabartah.Trivia.Application.PasswordGame.GetSession;
using Jabartah.Trivia.Application.PasswordGame.IssueRevealToken;
using Jabartah.Trivia.Application.PasswordGame.ResolveRound;
using Jabartah.Trivia.Application.PasswordGame.StartNextRound;
using Jabartah.Trivia.Application.PasswordGame.UseExtraTime;

namespace Jabartah.Trivia.Api.Endpoints;

public static class PasswordGameEndpoints
{
    public static void MapPasswordGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/password-sessions").WithTags("PasswordGame");

        group.MapPost("/", async (CreatePasswordGameSessionCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(command, ct)));

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetPasswordSessionQuery(id), ct)));

        group.MapPost("/{id:guid}/rounds/next", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new StartNextPasswordRoundCommand(id), ct)));

        group.MapPost("/{id:guid}/rounds/{roundId:guid}/reveal-token", async (
                Guid id, Guid roundId, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new IssueRevealTokenCommand(id, roundId), ct)));

        group.MapPost("/{id:guid}/rounds/{roundId:guid}/resolve", async (
                Guid id, Guid roundId, ResolveRoundRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ResolvePasswordRoundCommand(id, roundId, body.Correct), ct)));

        group.MapPost("/{id:guid}/teams/{teamId:guid}/extra-time", async (
                Guid id, Guid teamId, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UseExtraTimeCommand(id, teamId), ct)));

        group.MapPost("/{id:guid}/end", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new EndPasswordGameSessionCommand(id), ct)));
    }

    public record ResolveRoundRequest(bool Correct);
}
