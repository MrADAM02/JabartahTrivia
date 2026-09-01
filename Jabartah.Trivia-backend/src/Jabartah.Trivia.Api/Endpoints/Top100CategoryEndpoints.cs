using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.ListTop100Categories;

namespace Jabartah.Trivia.Api.Endpoints;

public static class Top100CategoryEndpoints
{
    public static void MapTop100CategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/top100-categories", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListTop100CategoriesQuery(), ct)))
            .WithTags("Top100Categories");
    }
}
