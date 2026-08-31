using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.PasswordGame.ConsumeRevealToken;

namespace Jabartah.Trivia.Api.Endpoints;

public static class RevealEndpoints
{
    public static void MapRevealEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/reveal/{token}", async (string token, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ConsumeRevealTokenCommand(token), ct)))
            .WithTags("Reveal");
    }
}
