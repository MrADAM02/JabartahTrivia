using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.ListPasswordCategories;

namespace Jabartah.Trivia.Api.Endpoints;

public static class PasswordCategoryEndpoints
{
    public static void MapPasswordCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/password-categories", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListPasswordCategoriesQuery(), ct)))
            .WithTags("PasswordCategories");
    }
}
