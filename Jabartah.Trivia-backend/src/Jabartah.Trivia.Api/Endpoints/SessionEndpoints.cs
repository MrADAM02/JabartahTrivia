using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Sessions.GetMySessions;

namespace Jabartah.Trivia.Api.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/my-sessions", async (ICurrentUserAccessor currentUser, IDispatcher dispatcher, CancellationToken ct) =>
                Results.Ok(await dispatcher.Send(new GetMySessionsQuery(currentUser.UserId!.Value), ct)))
            .RequireAuthorization()
            .WithTags("Sessions");
    }
}
