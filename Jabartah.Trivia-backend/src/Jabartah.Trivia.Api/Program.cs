using Jabartah.Trivia.Api.Endpoints;
using Jabartah.Trivia.Application;
using Jabartah.Trivia.Infrastructure;
using Jabartah.Trivia.Infrastructure.Persistence;
using Jabartah.Trivia.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:3030")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Dev convenience only: applies pending migrations + seeds Arabic sample content.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
    await PasswordDatabaseSeeder.SeedAsync(db);
    await RankingDatabaseSeeder.SeedAsync(db);
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        await Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest)
            .ExecuteAsync(context);
    }
    catch (KeyNotFoundException ex)
    {
        await Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound)
            .ExecuteAsync(context);
    }
});

app.UseCors();
app.MapGameSessionEndpoints();
app.MapCategoryEndpoints();
app.MapPasswordGameEndpoints();
app.MapPasswordCategoryEndpoints();
app.MapRevealEndpoints();
app.MapRankingGameEndpoints();
app.MapRankingCategoryEndpoints();

app.Run();
