using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.ListCategories;

namespace Jabartah.Trivia.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListCategoriesQuery(), ct)))
            .WithTags("Categories");
    }
}
