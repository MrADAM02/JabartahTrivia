using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.CreateCustomCategory;
using Jabartah.Trivia.Application.Categories.DeleteCustomCategory;
using Jabartah.Trivia.Application.Categories.GetMyCategory;
using Jabartah.Trivia.Application.Categories.ListMyCategories;
using Jabartah.Trivia.Application.Categories.UpdateCustomCategory;

namespace Jabartah.Trivia.Api.Endpoints;

public static class MyCategoryEndpoints
{
    public static void MapMyCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/my-categories").WithTags("MyCategories").RequireAuthorization();

        group.MapGet("/", async (ICurrentUserAccessor currentUser, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListMyCategoriesQuery(currentUser.UserId!.Value), ct)));

        group.MapPost("/", async (
                CreateCustomCategoryRequest body, ICurrentUserAccessor currentUser, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(
                new CreateCustomCategoryCommand(currentUser.UserId!.Value, body.Name, body.Icon, body.Questions), ct)));

        group.MapGet("/{id:guid}", async (Guid id, ICurrentUserAccessor currentUser, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetMyCategoryQuery(currentUser.UserId!.Value, id), ct)));

        group.MapPut("/{id:guid}", async (
                Guid id, UpdateCustomCategoryRequest body, ICurrentUserAccessor currentUser, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(
                new UpdateCustomCategoryCommand(currentUser.UserId!.Value, id, body.Name, body.Icon, body.Questions), ct)));

        group.MapDelete("/{id:guid}", async (Guid id, ICurrentUserAccessor currentUser, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteCustomCategoryCommand(currentUser.UserId!.Value, id), ct)));
    }

    public record CreateCustomCategoryRequest(string Name, string? Icon, List<CustomQuestionInput> Questions);
    public record UpdateCustomCategoryRequest(string Name, string? Icon, List<CustomQuestionUpdateInput> Questions);
}
