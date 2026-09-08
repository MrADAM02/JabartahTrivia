using Jabartah.Trivia.Application.Abstractions;
using Jabartah.Trivia.Application.Categories.CreateRankingCategory;
using Jabartah.Trivia.Application.Categories.DeleteRankingCategory;
using Jabartah.Trivia.Application.Categories.ListRankingCategoriesForAdmin;
using Jabartah.Trivia.Application.Categories.UpdateRankingCategory;
using Jabartah.Trivia.Application.RankingLists.CreateRankingList;
using Jabartah.Trivia.Application.RankingLists.CreateRankingListItem;
using Jabartah.Trivia.Application.RankingLists.DeleteRankingList;
using Jabartah.Trivia.Application.RankingLists.DeleteRankingListItem;
using Jabartah.Trivia.Application.RankingLists.GetRankingListForAdmin;
using Jabartah.Trivia.Application.RankingLists.ListRankingListsByCategory;
using Jabartah.Trivia.Application.RankingLists.UpdateRankingList;
using Jabartah.Trivia.Application.RankingLists.UpdateRankingListItem;

namespace Jabartah.Trivia.Api.Endpoints;

public static class AdminRankingEndpoints
{
    public static void MapAdminRankingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/ranking").WithTags("AdminRanking")
            .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapGet("/categories", async (IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListRankingCategoriesForAdminQuery(), ct)));

        group.MapPost("/categories", async (CategoryRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreateRankingCategoryCommand(body.Name, body.Icon), ct)));

        group.MapPut("/categories/{id:guid}", async (Guid id, CategoryRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdateRankingCategoryCommand(id, body.Name, body.Icon), ct)));

        group.MapDelete("/categories/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteRankingCategoryCommand(id), ct)));

        group.MapGet("/categories/{id:guid}/lists", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new ListRankingListsByCategoryQuery(id), ct)));

        group.MapPost("/categories/{id:guid}/lists", async (Guid id, ListRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreateRankingListCommand(id, body.Title), ct)));

        group.MapGet("/lists/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new GetRankingListForAdminQuery(id), ct)));

        group.MapPut("/lists/{id:guid}", async (Guid id, ListRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdateRankingListCommand(id, body.Title), ct)));

        group.MapDelete("/lists/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteRankingListCommand(id), ct)));

        group.MapPost("/lists/{id:guid}/items", async (Guid id, ItemRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new CreateRankingListItemCommand(id, body.Label, body.CorrectPosition), ct)));

        group.MapPut("/items/{id:guid}", async (Guid id, ItemRequest body, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new UpdateRankingListItemCommand(id, body.Label, body.CorrectPosition), ct)));

        group.MapDelete("/items/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            Results.Ok(await dispatcher.Send(new DeleteRankingListItemCommand(id), ct)));
    }

    public record CategoryRequest(string Name, string? Icon);
    public record ListRequest(string Title);
    public record ItemRequest(string Label, int CorrectPosition);
}
