using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.ListRankingCategories;

namespace Jabartah.Trivia.Api.Endpoints;

public static class RankingCategoryEndpoints
{
    public static void MapRankingCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/ranking-categories", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListRankingCategoriesQuery(), ct)))
            .WithTags("RankingCategories");
    }
}
