using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.GameSessions.ActivateTimerDebuff;
using Jabartah.Trivia.Application.GameSessions.AwardPoints;
using Jabartah.Trivia.Application.GameSessions.CreateGameSession;
using Jabartah.Trivia.Application.GameSessions.GetBoard;
using Jabartah.Trivia.Application.GameSessions.RevealAnswer;
using Jabartah.Trivia.Application.GameSessions.SelectQuestion;

namespace Jabartah.Trivia.Api.Endpoints;

public static class GameSessionEndpoints
{
    public static void MapGameSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/game-sessions").WithTags("GameSessions");

        group.MapPost("/", async (CreateGameSessionCommand command, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(command, ct)));

        group.MapGet("/{id:guid}/board", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetBoardQuery(id), ct)));

        group.MapPost("/{id:guid}/questions/{questionId:guid}/select", async (
                Guid id, Guid questionId, SelectQuestionRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new SelectQuestionCommand(id, questionId, body.ActivatingTeamId, body.PowerUp), ct)));

        group.MapPost("/{id:guid}/questions/{questionId:guid}/award", async (
                Guid id, Guid questionId, AwardPointsRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new AwardPointsCommand(id, questionId, body.WinningTeamId), ct)));

        group.MapPost("/{id:guid}/questions/{questionId:guid}/reveal-answer", async (
                Guid id, Guid questionId, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new RevealAnswerCommand(id, questionId), ct)));

        group.MapPost("/{id:guid}/teams/{teamId:guid}/timer-debuff", async (
                Guid id, Guid teamId, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ActivateTimerDebuffCommand(id, teamId), ct)));
    }

    public record SelectQuestionRequest(Guid? ActivatingTeamId, string? PowerUp);
    public record AwardPointsRequest(Guid? WinningTeamId);
}
